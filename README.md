# 📨 SendMail.NET

**SendMail.NET** is a powerful and extensible email delivery pipeline for .NET.  
It supports multiple providers, automatic failover, and delivery tracking through a modular pipeline design.

---

## ✨ Current Features

- ✅ **Provider-agnostic** – Currently supports SMTP with more providers coming soon
- 🔁 **Failover support** – Automatically switch to backup providers on failure
- ⚙️ **Pluggable architecture** – Add custom providers via NuGet packages
- 📦 **Pipeline design** – From validation → sending → result tracking
- 🧪 **Test-friendly** – Clean interfaces and extensibility built-in

---

## 📦 Install

```bash
dotnet add package SendMail.NET
```

---

## 🛠️ Getting Started

1. Register the core pipeline in your `Program.cs` or `Startup.cs`
2. Configure your providers
3. Use the injected services to send emails

```csharp
services.AddSendMail(config =>
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

    // Configure provider behavior
    config.ConfigureProviders(options =>
    {
        options.EnableFallback = true;
        options.MaxRetries = 3;
    });

    // Use default pipeline
    config.UseDefaultPipeline();
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
Send Email
    ↓
Track Result
```

---

## 📌 Roadmap

- [x] Core pipeline and interfaces
- [x] SMTP provider
- [ ] SendGrid provider
- [ ] Mailgun provider
- [ ] Amazon SES provider
- [ ] Dashboard UI for monitoring
- [ ] Webhook support for delivery confirmations
- [ ] SQL Server / PostgreSQL logging
- [ ] Hangfire/Quartz integration
- [ ] Blazor-based UI plugin support

---

## 🤝 Contributing

This project is owned and maintained by the Net of Code team. All contributions become the property of Net of Code.

Contributions are welcome! Whether it's a bug fix, a new provider, or improvements — please open a PR or issue.

---

## 📄 License

MIT
