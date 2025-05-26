using SmartMail.NET.Core.Providers;
using SmartMail.NET.Dashboard.Extensions;
using SmartMail.NET.Core.Extensions;
using SmartMail.NET.Dashboard.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SmartMail.NET services
builder.Services.AddSmartMail(config =>
{
    // Configure AWS SES provider with rate limiting
    config.AddProvider<AwsSesEmailProvider>(options =>
    {
        options.Name = "AWS SES";
        options.Priority = 2;
        options.HourlyQuota = 1000;
        options.RequestsPerSecond = 14;  // AWS SES sandbox limit
        options.Settings["Region"] = builder.Configuration["AWS:Region"];
        options.Settings["AccessKey"] = builder.Configuration["AWS:AccessKey"];
        options.Settings["SecretKey"] = builder.Configuration["AWS:SecretKey"];
        options.Settings["DefaultFrom"] = builder.Configuration["AWS:DefaultFrom"];
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

// Add the dashboard with configuration
builder.Services.AddSmartMailDashboard(options =>
{
    options.Path = "/dashboard"; // Custom path for the dashboard
    options.RefreshIntervalSeconds = 5; // Refresh every 5 seconds
    
    // Configure basic authentication
    options.BasicAuth = new BasicAuth
    {
        Username = builder.Configuration["Dashboard:Username"] ?? "admin",
        Password = builder.Configuration["Dashboard:Password"] ?? "admin"
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add the dashboard middleware
app.UseSmartMailDashboard();

app.MapControllers();

app.Run();