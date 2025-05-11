using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartMail.NET.Core.Models;

namespace SmartMail.NET.Core.Pipeline.Steps
{
    public class ValidationStep : IEmailPipelineStep
    {
        private readonly ILogger<ValidationStep> _logger;

        public ValidationStep(ILogger<ValidationStep> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<EmailContext> ExecuteAsync(EmailContext context)
        {
            _logger.LogDebug("Validating email message");

            if (string.IsNullOrEmpty(context.Message.To))
                throw new ArgumentException("To address is required");

            if (string.IsNullOrEmpty(context.Message.Subject))
                throw new ArgumentException("Subject is required");

            if (string.IsNullOrEmpty(context.Message.Body))
                throw new ArgumentException("Body is required");

            return context;
        }
    }
} 