using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SendMail.NET.Core.Services;
using SendMail.NET.Dashboard.Middleware;
using SendMail.NET.Dashboard.Services;

namespace SendMail.NET.Dashboard.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSendMailDashboard(this IServiceCollection services, Action<DashboardOptions>? configure = null)
    {
        services.Configure<DashboardOptions>(options =>
        {
            configure?.Invoke(options);
        });

        services.AddScoped<DashboardService>();
        services.AddScoped<ProviderMonitorService>();

        return services;
    }

    public static IApplicationBuilder UseSendMailDashboard(
        this IApplicationBuilder app,
        string path = "/sendmail",
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
    public string Path { get; set; } = "/sendmail";
} 