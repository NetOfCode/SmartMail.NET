using System.Collections.Generic;

namespace SendMail.NET.Core.Providers
{
    public class EmailProviderOptions
    {
        public List<ProviderConfig> Providers { get; set; } = new();
        public bool EnableFallback { get; set; } = true;
        public int MaxRetries { get; set; } = 3;
    }

    public class ProviderConfig
    {
        public string Name { get; set; }
        public int Priority { get; set; }
        public int HourlyQuota { get; set; }
        public int DailyQuota { get; set; }
        public int MonthlyQuota { get; set; }
        public bool IsEnabled { get; set; } = true;
        public Dictionary<string, string> Settings { get; set; } = new();
    }
} 