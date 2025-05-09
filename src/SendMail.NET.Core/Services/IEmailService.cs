using System.Threading.Tasks;
using SendMail.NET.Core.Models;
using SendMail.NET.Core.Pipeline;

namespace SendMail.NET.Core.Services
{
    /// <summary>
    /// Defines the contract for the email service that handles sending emails through the pipeline.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email message through the configured pipeline.
        /// </summary>
        /// <param name="message">The email message to send.</param>
        /// <returns>A <see cref="SendResult"/> containing the result of the send operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the message is null.</exception>
        /// <exception cref="Exception">Thrown when the email fails to send.</exception>
        Task<SendResult> SendAsync(EmailMessage message);
    }
} 