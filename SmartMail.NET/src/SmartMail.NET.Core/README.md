# 📨 SmartMail.NET.Core

**SmartMail.NET.Core** is a powerful and extensible email delivery pipeline for .NET applications.  
It provides a flexible architecture for managing multiple email providers, handling quotas, and implementing fallback mechanisms.

---

## ✨ Features

- ✅ **Multiple Provider Support** – Currently supports SMTP and AWS SES with more providers coming soon
- 🔁 **Smart Failover** – Automatic switching to backup providers on failure
- ⚙️ **Extensible Architecture** – Add custom providers via NuGet packages
- 📊 **Quota Management** – Hourly, daily, and monthly quota tracking
- 🔄 **Runtime Configuration** – Update provider settings without restart
- 🧪 **Test-friendly** – Clean interfaces and extensibility built-in

---

## 📦 Installation

```bash
dotnet add package SmartMail.NET.Core
```

---

## 🛠️ Getting Started

1. Register the core pipeline in your `Program.cs` or `Startup.cs`
2. Configure your providers
3. Use the injected services to send emails

```csharp
services.AddSmartMail(config =>
{
    // Configure SMTP provider
    config.AddProvider<SmtpEmailProvider>(options =>
    {
        options.Name = "SMTP";
        options.Priority = 1;
        options.HourlyQuota = 100;
        options.DailyQuota = 1000;
        options.MonthlyQuota = 10000;
        options.Settings["Host"] = "smtp.gmail.com";
        options.Settings["Port"] = "587";
        options.Settings["EnableSsl"] = "true";
        options.Settings["Username"] = "your-email@gmail.com";
        options.Settings["Password"] = "your-app-password";
        options.Settings["DefaultFrom"] = "your-email@gmail.com";
    });

    // Configure AWS SES provider
    config.AddProvider<AwsSesEmailProvider>(options =>
    {
        options.Name = "AWS SES";
        options.Priority = 2;
        options.HourlyQuota = 200;
        options.DailyQuota = 2000;
        options.MonthlyQuota = 20000;
        options.Settings["Region"] = "us-west-2";
        options.Settings["AccessKey"] = "your-access-key";
        options.Settings["SecretKey"] = "your-secret-key";
    });

    // Configure provider behavior
    config.ConfigureProviders(options =>
    {
        options.EnableFallback = true;
        options.MaxRetries = 3;
    });
});
```

---

## 📦 Provider Architecture

Every email provider implements the shared `IEmailProvider` interface, allowing:

- Custom retry logic
- Load balancing
- Metrics collection
- Easy fallback switching

Want to build your own provider? Just implement the interface and register it.

---

## 🧱 Architecture

```plaintext
Validate Email
    ↓
Select Provider (priority/failover)
    ↓
Check Quotas
    ↓
Send Email
    ↓
Track Result
```

---

## 📌 Roadmap

- [x] Core pipeline and interfaces
- [x] SMTP provider
- [x] AWS SES provider
- [ ] SendGrid provider
- [ ] Mailgun provider
- [ ] Webhook support for delivery confirmations
- [ ] SQL Server / PostgreSQL logging
- [ ] Hangfire/Quartz integration

---

## 🤝 Contributing

This project is owned and maintained by the Net of Code team. All contributions become the property of Net of Code.

Contributions are welcome! Whether it's a bug fix, a new provider, or improvements — please open a PR or issue.

---

## 📄 License

MIT 