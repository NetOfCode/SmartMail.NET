using System.Threading.Tasks;
using SmartMail.NET.Core.Models;

namespace SmartMail.NET.Core.Pipeline
{
    public interface IEmailPipelineStep
    {
        Task<EmailContext> ExecuteAsync(EmailContext context);
    }
} 