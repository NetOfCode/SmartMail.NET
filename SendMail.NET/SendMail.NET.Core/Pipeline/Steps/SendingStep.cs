using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SendMail.NET.Core.Pipeline.Steps
{
    public class SendingStep : IEmailPipelineStep
    {
        private readonly ILogger<SendingStep> _logger;

        public SendingStep(ILogger<SendingStep> logger)
        {
            _logger = logger;
        }

        public async Task ExecuteAsync(EmailContext context)
        {
            if (context.Provider == null)
                throw new InvalidOperationException("No email provider selected");

            _logger.LogDebug("Sending email using provider: {Provider}", context.Provider.Name);

            context.Result = await context.Provider.SendAsync(context.Message);

            if (!context.Result.Success)
            {
                _logger.LogError("Failed to send email: {Error}", context.Result.Error);
            }
            else
            {
                _logger.LogInformation("Email sent successfully. MessageId: {MessageId}", context.Result.MessageId);
            }
        }
    }
} 