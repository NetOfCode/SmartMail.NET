using System;
using Microsoft.Extensions.Logging;
using SendMail.NET.Core.Models;
using SendMail.NET.Core.Pipeline;

namespace SendMail.NET.Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailPipeline _pipeline;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            EmailPipeline pipeline,
            ILogger<EmailService> logger)
        {
            _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SendResult> SendAsync(EmailMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            _logger.LogDebug("Sending email to {To}", message.To);

            var context = new EmailContext
            {
                Message = message
            };

            return await _pipeline.ExecuteAsync(context);
        }
    }
} 