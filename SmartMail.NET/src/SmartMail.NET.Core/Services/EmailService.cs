using Microsoft.Extensions.Logging;
using SmartMail.NET.Core.Models;
using SmartMail.NET.Core.Pipeline;
using SmartMail.NET.Core.Providers;

namespace SmartMail.NET.Core.Services
{
    public class SmartMailService : ISmartMailService
    {
        private readonly EmailPipeline _pipeline;
        private readonly IEmailProviderManager _providerManager;
        private readonly ILogger<SmartMailService> _logger;

        public SmartMailService(
            EmailPipeline pipeline,
            IEmailProviderManager providerManager,
            ILogger<SmartMailService> logger)
        {
            _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
            _providerManager = providerManager ?? throw new ArgumentNullException(nameof(providerManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SendResult> SendAsync(EmailMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            _logger.LogDebug("Sending email to {To}", message.To);
            var provider = await _providerManager.GetNextProviderAsync();

            try
            {
                var context = new EmailContext
                {
                    Message = message,
                    Provider = provider
                };

                var result = await _pipeline.ExecuteAsync(context);

                if (result.Success)
                {
                    await _providerManager.ReportSuccessAsync(provider);
                }
                else
                {
                    await _providerManager.ReportFailureAsync(provider);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", message.To);
                await _providerManager.ReportFailureAsync(provider);
                throw;
            }
        }
    }
} 