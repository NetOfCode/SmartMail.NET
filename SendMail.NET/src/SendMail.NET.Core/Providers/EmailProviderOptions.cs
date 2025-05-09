using System;
using System.Collections.Generic;

namespace SendMail.NET.Core.Providers
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

        /// <summary>
        /// Adds a provider configuration to the options.
        /// </summary>
        /// <param name="config">The provider configuration to add.</param>
        public void AddProvider(ProviderConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            Providers.Add(config);
        }
    }

    /// <summary>
    /// Base class for provider-specific settings.
    /// </summary>
    public abstract class ProviderSettings
    {
        /// <summary>
        /// Gets the type of the provider settings.
        /// </summary>
        public abstract string ProviderType { get; }
    }

    /// <summary>
    /// Settings specific to SMTP email providers.
    /// </summary>
    public class SmtpProviderSettings : ProviderSettings
    {
        /// <summary>
        /// Gets the SMTP server host.
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// Gets the SMTP server port.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Gets whether SSL is enabled.
        /// </summary>
        public bool EnableSsl { get; private set; }

        /// <summary>
        /// Gets the SMTP username.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Gets the SMTP password.
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        /// Gets the default sender email address.
        /// </summary>
        public string DefaultFrom { get; private set; }

        /// <summary>
        /// Gets the provider type.
        /// </summary>
        public override string ProviderType => "SMTP";

        /// <summary>
        /// Initializes a new instance of the SmtpProviderSettings class.
        /// </summary>
        /// <param name="host">The SMTP server host.</param>
        /// <param name="port">The SMTP server port.</param>
        /// <param name="username">The SMTP username.</param>
        /// <param name="password">The SMTP password.</param>
        /// <param name="defaultFrom">The default sender email address.</param>
        /// <param name="enableSsl">Whether SSL is enabled.</param>
        /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
        /// <exception cref="ArgumentException">Thrown when parameters are invalid.</exception>
        public SmtpProviderSettings(
            string host,
            int port,
            string username,
            string password,
            string defaultFrom,
            bool enableSsl = true)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("Host cannot be empty.", nameof(host));
            
            if (port <= 0 || port > 65535)
                throw new ArgumentException("Port must be between 1 and 65535.", nameof(port));
            
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty.", nameof(username));
            
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.", nameof(password));
            
            if (string.IsNullOrWhiteSpace(defaultFrom))
                throw new ArgumentException("DefaultFrom cannot be empty.", nameof(defaultFrom));

            Host = host;
            Port = port;
            Username = username;
            Password = password;
            DefaultFrom = defaultFrom;
            EnableSsl = enableSsl;
        }
    }

    /// <summary>
    /// Configuration for a specific email provider.
    /// </summary>
    public class ProviderConfig
    {
        /// <summary>
        /// Gets the name of the provider.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the priority of the provider (lower numbers have higher priority).
        /// </summary>
        public int Priority { get; private set; }

        /// <summary>
        /// Gets the maximum number of emails that can be sent per hour.
        /// </summary>
        public int? HourlyQuota { get; private set; }

        /// <summary>
        /// Gets the maximum number of emails that can be sent per day.
        /// </summary>
        public int? DailyQuota { get; private set; }

        /// <summary>
        /// Gets the maximum number of emails that can be sent per month.
        /// </summary>
        public int? MonthlyQuota { get; private set; }

        /// <summary>
        /// Gets whether the provider is enabled.
        /// </summary>
        public bool IsEnabled { get; private set; }

        /// <summary>
        /// Gets the provider-specific settings.
        /// </summary>
        public ProviderSettings Settings { get; private set; }

        /// <summary>
        /// Initializes a new instance of the ProviderConfig class.
        /// </summary>
        /// <param name="name">The name of the provider.</param>
        /// <param name="priority">The priority of the provider (lower numbers have higher priority).</param>
        /// <param name="settings">Required provider settings.</param>
        /// <exception cref="ArgumentNullException">Thrown when name or settings is null.</exception>
        /// <exception cref="ArgumentException">Thrown when name is empty or priority is negative.</exception>
        public ProviderConfig(string name, int priority, ProviderSettings settings)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Provider name cannot be empty.", nameof(name));
            
            if (priority < 0)
                throw new ArgumentException("Priority cannot be negative.", nameof(priority));
            
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            Name = name;
            Priority = priority;
            Settings = settings;
            IsEnabled = true;
        }

        /// <summary>
        /// Sets the hourly quota for the provider.
        /// </summary>
        /// <param name="quota">The maximum number of emails that can be sent per hour.</param>
        /// <returns>The current instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when quota is negative.</exception>
        public ProviderConfig WithHourlyQuota(int quota)
        {
            if (quota < 0)
                throw new ArgumentException("Quota cannot be negative.", nameof(quota));
            
            HourlyQuota = quota;
            return this;
        }

        /// <summary>
        /// Sets the daily quota for the provider.
        /// </summary>
        /// <param name="quota">The maximum number of emails that can be sent per day.</param>
        /// <returns>The current instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when quota is negative.</exception>
        public ProviderConfig WithDailyQuota(int quota)
        {
            if (quota < 0)
                throw new ArgumentException("Quota cannot be negative.", nameof(quota));
            
            DailyQuota = quota;
            return this;
        }

        /// <summary>
        /// Sets the monthly quota for the provider.
        /// </summary>
        /// <param name="quota">The maximum number of emails that can be sent per month.</param>
        /// <returns>The current instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when quota is negative.</exception>
        public ProviderConfig WithMonthlyQuota(int quota)
        {
            if (quota < 0)
                throw new ArgumentException("Quota cannot be negative.", nameof(quota));
            
            MonthlyQuota = quota;
            return this;
        }

        /// <summary>
        /// Sets whether the provider is enabled.
        /// </summary>
        /// <param name="isEnabled">Whether the provider is enabled.</param>
        /// <returns>The current instance for method chaining.</returns>
        public ProviderConfig WithEnabled(bool isEnabled)
        {
            IsEnabled = isEnabled;
            return this;
        }
    }
} 