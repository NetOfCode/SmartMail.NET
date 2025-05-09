using Microsoft.Extensions.Logging;

namespace SendMail.NET.Core.Pipeline
{
    public class EmailPipeline
    {
        private readonly IEnumerable<IEmailPipelineStep> _steps;
        private readonly ILogger<EmailPipeline> _logger;

        public EmailPipeline(
            IEnumerable<IEmailPipelineStep> steps,
            ILogger<EmailPipeline> logger)
        {
            _steps = steps ?? throw new ArgumentNullException(nameof(steps));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SendResult> ExecuteAsync(EmailContext context)
        {
            try
            {
                foreach (var step in _steps)
                {
                    _logger.LogDebug("Executing pipeline step: {StepType}", step.GetType().Name);
                    await step.ExecuteAsync(context);
                }

                return context.Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing email pipeline");
                return new SendResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }
    }
} 