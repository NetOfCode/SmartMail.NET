# SendMail.NET

A flexible and extensible email sending pipeline for .NET applications.

## Features

- Multiple email provider support
  - SMTP (Gmail, Office 365, etc.)
  - AWS SES (Amazon Simple Email Service)
- Quota management
- Environment-specific configurations
- Runtime configuration reloading
- Fallback mechanisms
- Retry policies

## Installation

```bash
dotnet add package SendMail.NET.Core
```

## Version History

### 1.1.0
- Added AWS SES provider support
- Improved configuration validation
- Enhanced error handling

### 1.0.0
- Initial release
- SMTP provider support
- Basic pipeline functionality

## Quick Start

```csharp
// Add SendMail.NET services
builder.Services.AddSendMail(config =>
{
    // Configure SMTP provider
    config.AddProvider<SmtpEmailProvider>(options =>
    {
        options.Name = "SMTP";
        options.Priority = 1;
        options.HourlyQuota = 100;
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
        options.HourlyQuota = 1000;
        options.RequestsPerSecond = 14;
        options.Settings["Region"] = "us-east-1";
        options.Settings["AccessKey"] = "your-access-key";
        options.Settings["SecretKey"] = "your-secret-key";
        options.Settings["DefaultFrom"] = "your-verified-email@domain.com";
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

// Inject and use the service
public class YourService
{
    private readonly IEmailService _emailService;

    public YourService(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendEmailAsync()
    {
        var email = new EmailMessage
        {
            To = "recipient@example.com",
            Subject = "Test Email",
            Body = "Hello from SendMail.NET!"
        };

        await _emailService.SendAsync(email);
    }
}
```

## Configuration

### appsettings.json

```json
{
  "SendMail": {
    "Providers": [
      {
        "Name": "SMTP",
        "Priority": 1,
        "HourlyQuota": 100,
        "Settings": {
          "Host": "smtp.gmail.com",
          "Port": "587",
          "EnableSsl": "true",
          "Username": "your-email@gmail.com",
          "Password": "your-app-password",
          "DefaultFrom": "your-email@gmail.com"
        }
      },
      {
        "Name": "AWS SES",
        "Priority": 2,
        "HourlyQuota": 1000,
        "RequestsPerSecond": 14,
        "Settings": {
          "Region": "us-east-1",
          "AccessKey": "your-access-key",
          "SecretKey": "your-secret-key",
          "DefaultFrom": "your-verified-email@domain.com"
        }
      }
    ],
    "EnableFallback": true,
    "MaxRetries": 3
  }
}
```

### Rate Limiting

The library supports rate limiting for email providers to prevent hitting service limits. For example, AWS SES sandbox has a limit of 14 requests per second. You can configure this using the `RequestsPerSecond` property in the provider configuration:

```json
{
  "Name": "AWS SES",
  "Priority": 2,
  "RequestsPerSecond": 14,  // Limits requests to 14 per second
  "Settings": {
    // ... other settings ...
  }
}
```

When rate limiting is enabled, the provider will automatically throttle requests to stay within the specified limit.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details. 