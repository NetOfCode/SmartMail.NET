using SendMail.NET.Core.Providers;
using SendMail.NET.Dashboard.Extensions;
using SendMail.NET.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SendMail.NET services
builder.Services.AddSendMail(config =>
{
    // Configure AWS SES provider with rate limiting
    config.AddProvider<AwsSesEmailProvider>(options =>
    {
        options.Name = "AWS SES";
        options.Priority = 2;
        options.HourlyQuota = 1000;
        options.RequestsPerSecond = 14;  // AWS SES sandbox limit
        options.Settings["Region"] = "eu-central-1";
        options.Settings["AccessKey"] = "AKIASUDQS7VUQLWJGMJK";
        options.Settings["SecretKey"] = "slaMyy14OWjWZ3UDbRN00sSiqYox3CwHOZf5aXyT";
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

// Add the dashboard
builder.Services.AddSendMailDashboard();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add the dashboard middleware
app.UseSendMailDashboard("/sendmail", "admin", "admin");

app.MapControllers();

app.Run(); 