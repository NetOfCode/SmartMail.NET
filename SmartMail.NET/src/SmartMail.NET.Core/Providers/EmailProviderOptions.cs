using System.Collections.Generic;

namespace SmartMail.NET.Core.Providers
{
    /// <summary>
    /// Configuration options for email providers.
    /// </summary>
    public class EmailProviderOptions
    {
        /// <summary>
        /// Gets or sets the list of configured email providers.
        /// </summary>
        public List<ProviderConfig> Providers { get; set; } = new();

        /// <summary>
        /// Gets or sets whether to enable automatic failover to backup providers.
        /// </summary>
        public bool EnableFallback { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum number of retry attempts for failed sends.
        /// </summary>
        public int MaxRetries { get; set; } = 3;
    }

    /// <summary>
    /// Configuration for a specific email provider.
    /// </summary>
    public class ProviderConfig
    {
        /// <summary>
        /// Gets or sets the name of the provider.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the priority of the provider (lower numbers have higher priority).
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of emails that can be sent per hour.
        /// </summary>
        public int? HourlyQuota { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of emails that can be sent per day.
        /// </summary>
        public int? DailyQuota { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of emails that can be sent per month.
        /// </summary>
        public int? MonthlyQuota { get; set; }

        /// <summary>
        /// Gets or sets whether the provider is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum number of requests allowed per second.
        /// </summary>
        public int? RequestsPerSecond { get; set; }

        /// <summary>
        /// Gets or sets the provider-specific settings.
        /// </summary>
        public Dictionary<string, string> Settings { get; set; } = new();
    }
} 