using System.Threading.Tasks;
using SendMail.NET.Core.Models;
using SendMail.NET.Core.Pipeline;

namespace SendMail.NET.Core.Services
{
    public interface IEmailService
    {
        Task<SendResult> SendAsync(EmailMessage message);
    }
} 