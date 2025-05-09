using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SendMail.NET.Core.Models;

namespace SendMail.NET.Core.Pipeline.Steps
{
    public class SendingStep : IEmailPipelineStep
    {
        private readonly ILogger<SendingStep> _logger;

        public SendingStep(ILogger<SendingStep> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<EmailContext> ExecuteAsync(EmailContext context)
        {
            _logger.LogDebug("Sending email via {Provider}", context.Provider.Name);

            var result = await context.Provider.SendAsync(context.Message);
            context.Result = result;

            if (!result.Success)
            {
                _logger.LogError("Failed to send email: {Error}", result.Error);
                throw new Exception($"Failed to send email: {result.Error}");
            }

            return context;
        }
    }
} 