using System.Threading.Tasks;
using SendMail.NET.Core.Models;

namespace SendMail.NET.Core.Pipeline
{
    public interface IEmailPipelineStep
    {
        Task<EmailContext> ExecuteAsync(EmailContext context);
    }
} 