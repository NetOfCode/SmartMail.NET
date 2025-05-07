# 📨 SendMail.NET

**SendMail.NET** is a powerful and extensible email delivery pipeline for .NET.  
It supports multiple providers, automatic failover, and delivery tracking — all managed through an elegant embedded dashboard.

---

## ✨ Features

- ✅ **Provider-agnostic** – Integrate SMTP, SendGrid, Mailgun, Amazon SES, and more
- 🔁 **Failover support** – Automatically switch to backup providers on failure
- ⚙️ **Pluggable architecture** – Add custom providers via NuGet packages
- 📦 **Pipeline design** – From creation → templating → routing → sending → result tracking
- 📊 **Built-in dashboard** – Monitor sent emails, errors, retries, and metrics
- 🧪 **Test-friendly & production-ready** – Clean interfaces and extensibility built-in

---

## 📦 Install (Coming Soon)

```bash
dotnet add package SendMail.NET
```

> Provider-specific integrations will be available as separate packages:
> - `SendMail.NET.Smtp`
> - `SendMail.NET.SendGrid`
> - `SendMail.NET.Mailgun`
> - And more...

---

## 🛠️ Getting Started

1. Register the core pipeline and dashboard in your `Program.cs` or `Startup.cs`
2. Configure your providers in `appsettings.json`
3. Use the fluent API or injected services to queue and send emails
4. Open `/sendmail-dashboard` to monitor jobs

```csharp
services.AddSendMail(config =>
{
    config.UseDashboard("/sendmail-dashboard");
    config.AddProvider<SendGridProvider>("SendGrid", options =>
    {
        options.ApiKey = "...";
    });
});
```

---

## 📊 Dashboard

Track your email pipeline in real time:

- Recent sends / failures
- Provider usage stats
- Retry history
- Searchable logs

> The dashboard is embedded like Hangfire and protected by your auth layer.

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
Create Email
    ↓
Compile Template
    ↓
Select Provider (priority/failover)
    ↓
Send Email
    ↓
Track + Visualize Result
```

---

## 📌 Roadmap

- [ ] Core pipeline and interfaces
- [ ] SMTP provider
- [ ] Dashboard UI
- [ ] Webhook support for delivery confirmations
- [ ] SQL Server / PostgreSQL logging
- [ ] Hangfire/Quartz integration
- [ ] Blazor-based UI plugin support

---

## 🤝 Contributing

Contributions are welcome! Whether it's a bug fix, a new provider, or dashboard improvements — please open a PR or issue.

---

## 📄 License

MIT
