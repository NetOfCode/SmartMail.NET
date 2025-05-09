using System.Collections.Generic;
using SendMail.NET.Core.Models;
using SendMail.NET.Core.Providers;

namespace SendMail.NET.Core.Pipeline
{
    public class EmailContext
    {
        public EmailMessage Message { get; set; }
        public string CompiledBody { get; set; }
        public IEmailProvider Provider { get; set; }
        public SendResult Result { get; set; }
        public List<string> Logs { get; set; } = new();
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    public class SendResult
    {
        public bool Success { get; set; }
        public string MessageId { get; set; }
        public string Error { get; set; }
    }
} 