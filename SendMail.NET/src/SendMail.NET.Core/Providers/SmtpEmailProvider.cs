using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendMail.NET.Core.Models;
using SendMail.NET.Core.Pipeline;

namespace SendMail.NET.Core.Providers
{
    /// <summary>
    /// Interface for SMTP client operations.
    /// </summary>
    public interface ISmtpClient
    {
        /// <summary>
        /// Sends an email message asynchronously.
        /// </summary>
        /// <param name="message">The mail message to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendMailAsync(MailMessage message);
    }

    /// <summary>
    /// Wrapper for System.Net.Mail.SmtpClient to enable testing.
    /// </summary>
    public class SmtpClientWrapper : ISmtpClient
    {
        private readonly SmtpClient _client;

        /// <summary>
        /// Initializes a new instance of the SmtpClientWrapper class.
        /// </summary>
        /// <param name="client">The SMTP client to wrap.</param>
        public SmtpClientWrapper(SmtpClient client)
        {
            _client = client;
        }

        /// <inheritdoc/>
        public Task SendMailAsync(MailMessage message)
        {
            return _client.SendMailAsync(message);
        }
    }

    /// <summary>
    /// Email provider implementation using SMTP.
    /// </summary>
    public class SmtpEmailProvider : IEmailProvider
    {
        private readonly ILogger<SmtpEmailProvider> _logger;
        private readonly ISmtpClient _smtpClient;
        private readonly string _name;
        private readonly string _defaultFrom;

        /// <summary>
        /// Gets the name of the provider.
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Initializes a new instance of the SmtpEmailProvider class.
        /// </summary>
        /// <param name="options">The provider options.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="smtpClient">Optional SMTP client for testing.</param>
        /// <exception cref="ArgumentNullException">Thrown when options or logger is null.</exception>
        public SmtpEmailProvider(
            IOptions<EmailProviderOptions> options,
            ILogger<SmtpEmailProvider> logger,
            ISmtpClient smtpClient = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _smtpClient = smtpClient ?? CreateSmtpClient(options.Value);
            _name = options.Value.Providers[0].Name;
            _defaultFrom = options.Value.Providers[0].Settings["DefaultFrom"];
        }

        private ISmtpClient CreateSmtpClient(EmailProviderOptions options)
        {
            var config = options.Providers[0];
            var client = new SmtpClient
            {
                Host = config.Settings["Host"],
                Port = int.Parse(config.Settings["Port"]),
                EnableSsl = bool.Parse(config.Settings["EnableSsl"]),
                Credentials = new System.Net.NetworkCredential(
                    config.Settings["Username"],
                    config.Settings["Password"]
                )
            };
            return new SmtpClientWrapper(client);
        }

        /// <inheritdoc/>
        public async Task<SendResult> SendAsync(EmailMessage message)
        {
            try
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_defaultFrom),
                    Subject = message.Subject,
                    Body = message.Body,
                    IsBodyHtml = message.IsHtml
                };

                mailMessage.To.Add(message.To);

                foreach (var cc in message.Cc)
                {
                    mailMessage.CC.Add(cc);
                }

                foreach (var bcc in message.Bcc)
                {
                    mailMessage.Bcc.Add(bcc);
                }

                if (message.Attachments != null)
                {
                    foreach (var attachment in message.Attachments)
                    {
                        var mailAttachment = new Attachment(
                            new System.IO.MemoryStream(attachment.Content),
                            attachment.FileName,
                            attachment.ContentType
                        );
                        mailMessage.Attachments.Add(mailAttachment);
                    }
                }

                await _smtpClient.SendMailAsync(mailMessage);

                return new SendResult
                {
                    Success = true,
                    MessageId = Guid.NewGuid().ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", message.To);
                return new SendResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }
    }
} 