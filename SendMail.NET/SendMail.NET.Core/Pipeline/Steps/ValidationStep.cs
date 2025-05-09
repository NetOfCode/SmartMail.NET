using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SendMail.NET.Core.Pipeline.Steps
{
    public class ValidationStep : IEmailPipelineStep
    {
        private readonly ILogger<ValidationStep> _logger;

        public ValidationStep(ILogger<ValidationStep> logger)
        {
            _logger = logger;
        }

        public Task ExecuteAsync(EmailContext context)
        {
            if (string.IsNullOrEmpty(context.Message.To))
                throw new ArgumentException("Email recipient (To) is required");

            if (string.IsNullOrEmpty(context.Message.Subject))
                throw new ArgumentException("Email subject is required");

            if (string.IsNullOrEmpty(context.Message.Body) && string.IsNullOrEmpty(context.Message.TemplateName))
                throw new ArgumentException("Either email body or template name must be provided");

            _logger.LogDebug("Email validation passed for recipient: {To}", context.Message.To);
            return Task.CompletedTask;
        }
    }
} 