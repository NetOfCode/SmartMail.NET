using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendMail.NET.Core.Models;
using SendMail.NET.Core.Pipeline;

namespace SendMail.NET.Core.Providers
{
    public class SmtpEmailProvider : IEmailProvider
    {
        private readonly ProviderConfig _config;
        private readonly ILogger<SmtpEmailProvider> _logger;

        public string Name => "SMTP";

        public SmtpEmailProvider(
            IOptions<EmailProviderOptions> options,
            ILogger<SmtpEmailProvider> logger)
        {
            _config = options.Value.Providers.FirstOrDefault(p => p.Name == "SMTP") 
                ?? throw new ArgumentException("SMTP provider configuration not found");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SendResult> SendAsync(EmailMessage message)
        {
            try
            {
                var host = _config.Settings["Host"];
                var port = int.Parse(_config.Settings["Port"]);
                var enableSsl = bool.Parse(_config.Settings["EnableSsl"]);
                var username = _config.Settings["Username"];
                var password = _config.Settings["Password"];
                var defaultFrom = _config.Settings["DefaultFrom"];

                using var client = new SmtpClient(host, port)
                {
                    EnableSsl = enableSsl,
                    Credentials = new System.Net.NetworkCredential(username, password)
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(message.From ?? defaultFrom),
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

                foreach (var attachment in message.Attachments)
                {
                    mailMessage.Attachments.Add(new Attachment(
                        new System.IO.MemoryStream(attachment.Content),
                        attachment.FileName,
                        attachment.ContentType));
                }

                await client.SendMailAsync(mailMessage);

                return new SendResult
                {
                    Success = true,
                    MessageId = Guid.NewGuid().ToString() // SMTP doesn't provide message IDs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email via SMTP");
                return new SendResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }
    }
} 