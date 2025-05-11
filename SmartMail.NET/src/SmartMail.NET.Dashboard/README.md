# 📊 SmartMail.NET.Dashboard

**SmartMail.NET.Dashboard** is a real-time monitoring dashboard for SmartMail.NET.  
It provides a beautiful and intuitive interface to monitor your email providers, track quotas, and view sending statistics.

---

## ✨ Features

- 📈 **Real-time Monitoring** – Live updates of email sending status
- 📊 **Provider Statistics** – Track success rates and quotas
- 🔄 **Status Overview** – Quick view of all provider statuses
- 📱 **Responsive Design** – Works on desktop and mobile devices
- 🎨 **Modern UI** – Clean and intuitive interface
- 🔌 **Easy Integration** – Simple setup with your existing SmartMail.NET installation

---

## 📦 Installation

```bash
dotnet add package SmartMail.NET.Dashboard
```

---

## 🛠️ Getting Started

1. Install the package
2. Add the dashboard to your application
3. Configure the dashboard settings

```csharp
// In Program.cs or Startup.cs
services.AddSmartMailDashboard(options =>
{
    options.Path = "/email-dashboard"; // Custom path for the dashboard
    options.RequireAuthentication = true; // Enable authentication
    options.AllowedRoles = new[] { "Admin", "EmailManager" }; // Optional role restrictions
});
```

---

## 🖥️ Dashboard Features

### Provider Status
- Real-time status of each email provider
- Success/failure rates
- Current quota usage
- Provider health indicators

### Statistics
- Emails sent per provider
- Success/failure ratios
- Quota utilization
- Historical data

### Configuration
- View current provider settings
- Monitor quota limits
- Check provider priorities
- View fallback configurations

---

## 🔒 Security

The dashboard includes built-in security features:
- Optional authentication requirement
- Role-based access control
- Secure configuration viewing
- Audit logging

---

## 📌 Roadmap

- [x] Basic dashboard functionality
- [x] Real-time updates
- [x] Provider status monitoring
- [ ] Advanced analytics
- [ ] Custom widgets
- [ ] Export functionality
- [ ] Email templates management
- [ ] User management interface

---

## 🤝 Contributing

This project is owned and maintained by the Net of Code team. All contributions become the property of Net of Code.

Contributions are welcome! Whether it's a bug fix, a new feature, or improvements — please open a PR or issue.

---

## 📄 License

MIT 