using System.Threading.Tasks;
using SmartMail.NET.Core.Models;
using SmartMail.NET.Core.Pipeline;

namespace SmartMail.NET.Core.Providers
{
    /// <summary>
    /// Defines the contract for email providers that can send emails.
    /// </summary>
    public interface IEmailProvider
    {
        /// <summary>
        /// Gets the name of the provider.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Sends an email message using this provider.
        /// </summary>
        /// <param name="message">The email message to send.</param>
        /// <returns>A <see cref="SendResult"/> containing the result of the send operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the message is null.</exception>
        /// <exception cref="Exception">Thrown when the email fails to send.</exception>
        Task<SendResult> SendAsync(EmailMessage message);
    }
} 