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
