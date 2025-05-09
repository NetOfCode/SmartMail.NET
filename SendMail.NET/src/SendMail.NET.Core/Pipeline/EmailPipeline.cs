using Microsoft.Extensions.Logging;
using SendMail.NET.Core.Models;

namespace SendMail.NET.Core.Pipeline
{
    public class EmailPipeline
    {
        private readonly IEnumerable<IEmailPipelineStep> _steps;
        private readonly ILogger<EmailPipeline> _logger;

        public EmailPipeline(IEnumerable<IEmailPipelineStep> steps, ILogger<EmailPipeline> logger)
        {
            _steps = steps ?? throw new ArgumentNullException(nameof(steps));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SendResult> ExecuteAsync(EmailContext context)
        {
            _logger.LogDebug("Starting email pipeline execution");

            foreach (var step in _steps)
            {
                try
                {
                    context = await step.ExecuteAsync(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Pipeline step failed");
                    throw;
                }
            }

            return context.Result;
        }
    }
} 