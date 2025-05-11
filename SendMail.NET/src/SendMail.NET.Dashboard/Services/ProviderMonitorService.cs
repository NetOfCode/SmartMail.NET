using SendMail.NET.Core.Providers;
using System.Text.Json.Serialization;

namespace SendMail.NET.Dashboard.Services;

public class ProviderMonitorService
{
    private readonly IEmailProviderManager _providerManager;
    private readonly EmailProviderOptions _providerOptions;

    public ProviderMonitorService(IEmailProviderManager providerManager, Microsoft.Extensions.Options.IOptions<EmailProviderOptions> providerOptions)
    {
        _providerManager = providerManager;
        _providerOptions = providerOptions.Value;
    }

    public async Task<IEnumerable<ProviderStatus>> GetProvidersStatus()
    {
        var providers = _providerManager.GetAllProviders();
        var status = new List<ProviderStatus>();

        foreach (var provider in providers)
        {
            var config = _providerOptions.Providers.FirstOrDefault(p => p.Name == provider.Name);
            status.Add(new ProviderStatus
            {
                Name = provider.Name ?? "Unknown",
                Type = provider.GetType().Name ?? "Unknown",
                IsActive = config?.IsEnabled ?? true,
                QuotaUsed = 0, // Set to 0 if not tracked
                QuotaLimit = config?.HourlyQuota ?? config?.DailyQuota ?? config?.MonthlyQuota ?? 0
            });
        }

        return status;
    }

    public async Task<EmailStats> GetEmailStats()
    {
        // Provide default values for now
        return new EmailStats
        {
            TotalSent = 0,
            SuccessRate = 100.0
        };
    }
}

public class ProviderStatus
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
    [JsonPropertyName("quotaUsed")]
    public int QuotaUsed { get; set; }
    [JsonPropertyName("quotaLimit")]
    public int QuotaLimit { get; set; }
}

public class EmailStats
{
    [JsonPropertyName("totalSent")]
    public int TotalSent { get; set; }
    [JsonPropertyName("successRate")]
    public double SuccessRate { get; set; }
} 