using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SmartMail.NET.Dashboard.Middleware;
using SmartMail.NET.Dashboard.Models;
using SmartMail.NET.Dashboard.Services;

namespace SmartMail.NET.Dashboard.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSmartMailDashboard(this IServiceCollection services, Action<DashboardOptions>? configure = null)
    {
        // Configure options with proper defaults
        services.Configure<DashboardOptions>(options =>
        {
            // Apply user configuration if provided
            configure?.Invoke(options);
        });

        services.AddScoped<DashboardService>();
        services.AddScoped<ProviderMonitorService>();

        return services;
    }

    public static IApplicationBuilder UseSmartMailDashboard(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<IOptions<DashboardOptions>>().Value;

        // Add Basic Auth middleware if BasicAuth is configured
        if (options.BasicAuth != null && 
            !string.IsNullOrEmpty(options.BasicAuth.Username) && 
            !string.IsNullOrEmpty(options.BasicAuth.Password))
        {
            app.UseMiddleware<BasicAuthMiddleware>(
                options.BasicAuth.Username, 
                options.BasicAuth.Password);
        }

        app.UseMiddleware<DashboardMiddleware>();

        return app;
    }
} 