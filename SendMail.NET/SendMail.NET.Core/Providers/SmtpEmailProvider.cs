using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendMail.NET.Core.Models;
using SendMail.NET.Core.Pipeline;

namespace SendMail.NET.Core.Providers
{
    public interface ISmtpClient
    {
        Task SendMailAsync(MailMessage message);
    }

    public class SmtpClientWrapper : ISmtpClient
    {
        private readonly SmtpClient _client;

        public SmtpClientWrapper(SmtpClient client)
        {
            _client = client;
        }

        public Task SendMailAsync(MailMessage message)
        {
            return _client.SendMailAsync(message);
        }
    }

    public class SmtpEmailProvider : IEmailProvider
    {
        private readonly ILogger<SmtpEmailProvider> _logger;
        private readonly ISmtpClient _smtpClient;
        private readonly string _name;
        private readonly string _defaultFrom;

        public string Name => _name;

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