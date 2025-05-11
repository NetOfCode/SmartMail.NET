using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SmartMail.NET.Core.Services;
using SmartMail.NET.Dashboard.Middleware;
using SmartMail.NET.Dashboard.Services;

namespace SmartMail.NET.Dashboard.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSmartMailDashboard(this IServiceCollection services, Action<DashboardOptions>? configure = null)
    {
        services.Configure<DashboardOptions>(options =>
        {
            configure?.Invoke(options);
        });

        services.AddScoped<DashboardService>();
        services.AddScoped<ProviderMonitorService>();

        return services;
    }

    public static IApplicationBuilder UseSmartMailDashboard(
        this IApplicationBuilder app,
        string path = "/SmartMail",
        string? basicAuthUsername = null,
        string? basicAuthPassword = null)
    {
        var options = app.ApplicationServices.GetRequiredService<IOptions<DashboardOptions>>().Value;
        options.Path = path;

        // Add Basic Auth middleware if credentials are provided
        if (!string.IsNullOrEmpty(basicAuthUsername) && !string.IsNullOrEmpty(basicAuthPassword))
        {
            app.UseMiddleware<BasicAuthMiddleware>(basicAuthUsername, basicAuthPassword);
        }

        app.UseMiddleware<DashboardMiddleware>();

        return app;
    }
}

public class DashboardOptions
{
    public string Path { get; set; } = "/SmartMail";
} 