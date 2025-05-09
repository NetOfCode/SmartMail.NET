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
    var smtpSettings = new SmtpProviderSettings(
        host: "smtp.gmail.com",
        port: 587,
        username: "your-email@gmail.com",
        password: "your-app-password",
        defaultFrom: "your-email@gmail.com",
        enableSsl: true
    );

    var providerConfig = new ProviderConfig("Gmail SMTP", 1, smtpSettings)
        .WithHourlyQuota(100)
        .WithEnabled(true);

    config.AddProvider<SmtpEmailProvider>(providerConfig);

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
app.MapPost("/send-email", async (IEmailService emailService) =>
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
