using System.Threading.Tasks;
using SendMail.NET.Core.Models;
using SendMail.NET.Core.Pipeline;

namespace SendMail.NET.Core.Providers
{
    public interface IEmailProvider
    {
        string Name { get; }
        Task<SendResult> SendAsync(EmailMessage message);
    }
} 