using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendMail.NET.Core.Models;
using SendMail.NET.Core.Pipeline;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace SendMail.NET.Core.Providers
{
    /// <summary>
    /// Email provider implementation using AWS SES.
    /// </summary>
    public class AwsSesEmailProvider : IEmailProvider
    {
        private readonly ILogger<AwsSesEmailProvider> _logger;
        private readonly IAmazonSimpleEmailService _sesClient;
        private readonly string _name;
        private readonly string _defaultFrom;
        private readonly SemaphoreSlim _rateLimiter;
        private readonly int? _requestsPerSecond;

        /// <summary>
        /// Gets the name of the provider.
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Initializes a new instance of the AwsSesEmailProvider class.
        /// </summary>
        /// <param name="options">The provider options.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="sesClient">Optional SES client for testing.</param>
        /// <exception cref="ArgumentNullException">Thrown when options or logger is null.</exception>
        public AwsSesEmailProvider(
            IOptions<EmailProviderOptions> options,
            ILogger<AwsSesEmailProvider> logger,
            IAmazonSimpleEmailService sesClient = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            if (options == null)
                throw new ArgumentNullException(nameof(options));
                
            if (options.Value == null)
                throw new ArgumentException("Options value cannot be null", nameof(options));
                
            if (options.Value.Providers == null || !options.Value.Providers.Any())
                throw new ArgumentException("No providers configured", nameof(options));
                
            var provider = options.Value.Providers[0];
            if (provider.Settings == null || !provider.Settings.ContainsKey("DefaultFrom"))
                throw new ArgumentException("Provider settings must contain DefaultFrom", nameof(options));

            _name = provider.Name;
            _defaultFrom = provider.Settings["DefaultFrom"];
            _requestsPerSecond = provider.RequestsPerSecond;

            // Initialize rate limiter if requests per second is specified
            if (_requestsPerSecond.HasValue && _requestsPerSecond.Value > 0)
            {
                _rateLimiter = new SemaphoreSlim(_requestsPerSecond.Value, _requestsPerSecond.Value);
            }

            if (sesClient == null)
            {
                if (!provider.Settings.ContainsKey("Region") || 
                    !provider.Settings.ContainsKey("AccessKey") || 
                    !provider.Settings.ContainsKey("SecretKey"))
                    throw new ArgumentException("Provider settings must contain Region, AccessKey, and SecretKey", nameof(options));

                var awsConfig = new AmazonSimpleEmailServiceConfig
                {
                    RegionEndpoint = RegionEndpoint.GetBySystemName(provider.Settings["Region"])
                };

                _sesClient = new AmazonSimpleEmailServiceClient(
                    provider.Settings["AccessKey"],
                    provider.Settings["SecretKey"],
                    awsConfig
                );
            }
            else
            {
                _sesClient = sesClient;
            }
        }

        /// <inheritdoc/>
        public async Task<SendResult> SendAsync(EmailMessage message)
        {
            try
            {
                // Apply rate limiting if configured
                if (_rateLimiter != null)
                {
                    var startTime = DateTime.UtcNow;
                    await _rateLimiter.WaitAsync();
                    try
                    {
                        var result = await SendEmailInternalAsync(message);
                        
                        // Calculate the time we should wait to maintain the rate limit
                        var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
                        var targetDelay = 1000.0 / _requestsPerSecond.Value;
                        var remainingDelay = Math.Max(0, targetDelay - elapsed);
                        
                        if (remainingDelay > 0)
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(remainingDelay));
                        }
                        
                        return result;
                    }
                    finally
                    {
                        _rateLimiter.Release();
                    }
                }
                else
                {
                    return await SendEmailInternalAsync(message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email using AWS SES");
                return new SendResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private async Task<SendResult> SendEmailInternalAsync(EmailMessage message)
        {
            var sendRequest = new SendEmailRequest
            {
                Source = message.From ?? _defaultFrom,
                Destination = new Destination
                {
                    ToAddresses = new List<string> { message.To },
                    CcAddresses = message.Cc,
                    BccAddresses = message.Bcc
                },
                Message = new Message
                {
                    Subject = new Content(message.Subject),
                    Body = new Body
                    {
                        Html = message.IsHtml ? new Content { Charset = "UTF-8", Data = message.Body } : null,
                        Text = !message.IsHtml ? new Content { Charset = "UTF-8", Data = message.Body } : null
                    }
                }
            };

            var response = await _sesClient.SendEmailAsync(sendRequest);

            return new SendResult
            {
                Success = true,
                MessageId = response.MessageId
            };
        }
    }
} 