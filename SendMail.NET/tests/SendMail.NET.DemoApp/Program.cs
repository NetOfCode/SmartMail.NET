using SendMail.NET.Core.Extensions;
using SendMail.NET.Core.Models;
using SendMail.NET.Core.Pipeline;
using SendMail.NET.Core.Providers;
using SendMail.NET.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add SendMail.NET services
builder.Services.AddSendMail(config =>
{
    // Configure SMTP provider
    config.AddProvider<SmtpEmailProvider>(options =>
    {
        options.Name = "SMTP";
        options.Priority = 1;
        options.HourlyQuota = 100;  // Only set hourly quota, daily and monthly will be null
        options.Settings["Host"] = "smtp.gmail.com";
        options.Settings["Port"] = "587";
        options.Settings["EnableSsl"] = "true";
        options.Settings["Username"] = "your-email@gmail.com";
        options.Settings["Password"] = "your-app-password";
        options.Settings["DefaultFrom"] = "your-email@gmail.com";
    });

    // Configure AWS SES provider with rate limiting
    config.AddProvider<AwsSesEmailProvider>(options =>
    {
        options.Name = "AWS SES";
        options.Priority = 2;
        options.HourlyQuota = 1000;
        options.RequestsPerSecond = 14;  // AWS SES sandbox limit
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

var app = builder.Build();

// Example endpoint to send an email
app.MapPost("/send-email", async (ISendMailService emailService) =>
{
    var message = new EmailMessage
    {
        To = "recipient@example.com",
        Subject = "Test Email",
        Body = "<h1>Hello World!</h1>",
        IsHtml = true
    };

    var result = await emailService.SendAsync(message);
    return result;
});

// Example endpoint to send an email using AWS SES specifically
app.MapPost("/send-email-ses", async (ISendMailService emailService) =>
{
    var message = new EmailMessage
    {
        To = "recipient@example.com",
        Subject = "Test Email via AWS SES",
        Body = "<h1>Hello from AWS SES!</h1><p>This email was sent using AWS SES provider.</p>",
        IsHtml = true,
        From = "your-verified-email@domain.com" // Must be verified in AWS SES
    };

    var result = await emailService.SendAsync(message);
    return result;
});

// Example endpoint to demonstrate rate limiting
app.MapPost("/send-bulk-emails", async (ISendMailService emailService) =>
{
    var tasks = new List<Task<SendResult>>();
    var startTime = DateTime.UtcNow;

    // Send 20 emails concurrently to demonstrate rate limiting
    for (int i = 0; i < 20; i++)
    {
        var message = new EmailMessage
        {
            To = "recipient@example.com",
            Subject = $"Test Email {i + 1}",
            Body = $"<h1>Hello from AWS SES!</h1><p>This is email {i + 1} of 20.</p>",
            IsHtml = true,
            From = "your-verified-email@domain.com"
        };

        tasks.Add(emailService.SendAsync(message));
    }

    var results = await Task.WhenAll(tasks);
    var endTime = DateTime.UtcNow;
    var duration = (endTime - startTime).TotalSeconds;

    return new
    {
        TotalEmails = results.Length,
        SuccessfulEmails = results.Count(r => r.Success),
        FailedEmails = results.Count(r => !r.Success),
        Duration = $"{duration:F2} seconds",
        AverageRate = $"{results.Length / duration:F2} emails per second"
    };
});

app.Run();

// Example of a custom pipeline step
public class CustomLoggingStep : IEmailPipelineStep
{
    private readonly ILogger<CustomLoggingStep> _logger;

    public CustomLoggingStep(ILogger<CustomLoggingStep> logger)
    {
        _logger = logger;
    }

    public Task<EmailContext> ExecuteAsync(EmailContext context)
    {
        _logger.LogInformation("Custom logging step: Sending email to {To} with subject {Subject}",
            context.Message.To,
            context.Message.Subject);
        return Task.FromResult(context);
    }
}
