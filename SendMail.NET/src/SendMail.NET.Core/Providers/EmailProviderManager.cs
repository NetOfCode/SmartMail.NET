using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SendMail.NET.Core.Providers
{
    public interface IEmailProviderManager
    {
        Task<IEmailProvider> GetNextProviderAsync();
        Task ReportSuccessAsync(IEmailProvider provider);
        Task ReportFailureAsync(IEmailProvider provider);
        IEnumerable<IEmailProvider> GetAllProviders();
    }

    public class EmailProviderManager : IEmailProviderManager
    {
        private readonly IEnumerable<IEmailProvider> _providers;
        private readonly EmailProviderOptions _options;
        private readonly ILogger<EmailProviderManager> _logger;
        private readonly Dictionary<string, ProviderStats> _stats = new();

        public EmailProviderManager(
            IEnumerable<IEmailProvider> providers,
            IOptions<EmailProviderOptions> options,
            ILogger<EmailProviderManager> logger)
        {
            _providers = providers;
            _options = options.Value;
            _logger = logger;

            foreach (var provider in providers)
            {
                _stats[provider.Name] = new ProviderStats();
            }
        }

        public async Task<IEmailProvider> GetNextProviderAsync()
        {
            var availableProviders = _providers
                .Where(p => IsProviderAvailable(p))
                .OrderBy(p => GetProviderConfig(p)?.Priority ?? int.MaxValue)
                .ToList();

            if (!availableProviders.Any())
            {
                var quotaExceededProviders = _providers
                    .Where(p => GetProviderConfig(p)?.IsEnabled == true)
                    .Where(p => IsQuotaExceeded(p))
                    .ToList();

                if (quotaExceededProviders.Any())
                {
                    var quotaDetails = string.Join(", ", quotaExceededProviders.Select(p => 
                    {
                        var config = GetProviderConfig(p);
                        var stats = _stats[p.Name];
                        return $"{p.Name} (Daily: {stats.SuccessfulSends}/{config.DailyQuota}, " +
                               $"Hourly: {stats.SuccessfulSends}/{config.HourlyQuota}, " +
                               $"Monthly: {stats.SuccessfulSends}/{config.MonthlyQuota})";
                    }));

                    throw new InvalidOperationException(
                        $"No available email providers. Quota exceeded for: {quotaDetails}. " +
                        "Please add more providers or increase the quota limits for existing providers.");
                }

                throw new InvalidOperationException("No available email providers. All providers are either disabled or have exceeded their quotas.");
            }

            return availableProviders.First();
        }

        public Task ReportSuccessAsync(IEmailProvider provider)
        {
            if (_stats.TryGetValue(provider.Name, out var stats))
            {
                stats.SuccessfulSends++;
                stats.LastSuccess = DateTime.UtcNow;
            }
            return Task.CompletedTask;
        }

        public Task ReportFailureAsync(IEmailProvider provider)
        {
            if (_stats.TryGetValue(provider.Name, out var stats))
            {
                stats.FailedSends++;
                stats.LastFailure = DateTime.UtcNow;
            }
            return Task.CompletedTask;
        }

        private bool IsProviderAvailable(IEmailProvider provider)
        {
            var config = GetProviderConfig(provider);
            if (config == null || !config.IsEnabled)
                return false;

            var stats = _stats[provider.Name];
            var now = DateTime.UtcNow;
            
            // Check hourly quota
            if (config.HourlyQuota.HasValue && 
                stats.SuccessfulSends >= config.HourlyQuota.Value && 
                stats.LastSuccess?.Hour == now.Hour &&
                stats.LastSuccess?.Date == now.Date)
            {
                _logger.LogDebug("Provider {Provider} has reached hourly quota of {Quota}", 
                    provider.Name, config.HourlyQuota);
                return false;
            }

            // Check daily quota
            if (config.DailyQuota.HasValue && 
                stats.SuccessfulSends >= config.DailyQuota.Value && 
                stats.LastSuccess?.Date == now.Date)
            {
                _logger.LogDebug("Provider {Provider} has reached daily quota of {Quota}", 
                    provider.Name, config.DailyQuota);
                return false;
            }

            // Check monthly quota
            if (config.MonthlyQuota.HasValue && 
                stats.SuccessfulSends >= config.MonthlyQuota.Value && 
                stats.LastSuccess?.Month == now.Month &&
                stats.LastSuccess?.Year == now.Year)
            {
                _logger.LogDebug("Provider {Provider} has reached monthly quota of {Quota}", 
                    provider.Name, config.MonthlyQuota);
                return false;
            }

            return true;
        }

        private bool IsQuotaExceeded(IEmailProvider provider)
        {
            var config = GetProviderConfig(provider);
            if (config == null || !config.IsEnabled)
                return false;

            var stats = _stats[provider.Name];
            var now = DateTime.UtcNow;
            
            // Check hourly quota
            if (config.HourlyQuota.HasValue && 
                stats.SuccessfulSends >= config.HourlyQuota.Value && 
                stats.LastSuccess?.Hour == now.Hour &&
                stats.LastSuccess?.Date == now.Date)
            {
                return true;
            }

            // Check daily quota
            if (config.DailyQuota.HasValue && 
                stats.SuccessfulSends >= config.DailyQuota.Value && 
                stats.LastSuccess?.Date == now.Date)
            {
                return true;
            }

            // Check monthly quota
            if (config.MonthlyQuota.HasValue && 
                stats.SuccessfulSends >= config.MonthlyQuota.Value && 
                stats.LastSuccess?.Month == now.Month &&
                stats.LastSuccess?.Year == now.Year)
            {
                return true;
            }

            return false;
        }

        private ProviderConfig GetProviderConfig(IEmailProvider provider)
        {
            return _options.Providers.FirstOrDefault(p => p.Name == provider.Name);
        }

        public IEnumerable<IEmailProvider> GetAllProviders()
        {
            return _providers;
        }

        private class ProviderStats
        {
            public int SuccessfulSends { get; set; }
            public int FailedSends { get; set; }
            public DateTime? LastSuccess { get; set; }
            public DateTime? LastFailure { get; set; }
        }
    }
} 