using System.Collections.Generic;
using SmartMail.NET.Core.Models;
using SmartMail.NET.Core.Providers;

namespace SmartMail.NET.Core.Pipeline
{
    /// <summary>
    /// Represents the context of an email being processed through the pipeline.
    /// </summary>
    public class EmailContext
    {
        /// <summary>
        /// Gets or sets the email message being processed.
        /// </summary>
        public EmailMessage Message { get; set; }

        /// <summary>
        /// Gets or sets the provider that will be used to send the email.
        /// </summary>
        public IEmailProvider Provider { get; set; }

        /// <summary>
        /// Gets or sets the result of the send operation.
        /// </summary>
        public SendResult Result { get; set; }

        /// <summary>
        /// Gets or sets the list of logs generated during pipeline execution.
        /// </summary>
        public List<string> Logs { get; set; } = new();

        /// <summary>
        /// Gets or sets additional properties that can be used by pipeline steps.
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    /// <summary>
    /// Represents the result of an email send operation.
    /// </summary>
    public class SendResult
    {
        /// <summary>
        /// Gets or sets whether the send operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the sent message.
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets the error message if the send operation failed.
        /// </summary>
        public string Error { get; set; }
    }
} 