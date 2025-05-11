using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SendMail.NET.Dashboard.Services;
using SendMail.NET.Dashboard.Models;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace SendMail.NET.Dashboard.Middleware;

public class DashboardMiddleware
{
    private readonly RequestDelegate _next;
    private readonly DashboardOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;

    public DashboardMiddleware(
        RequestDelegate next,
        IOptions<DashboardOptions> options,
        IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _options = options.Value;
        _scopeFactory = scopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments(_options.Path))
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value?.Replace(_options.Path, "").TrimStart('/') ?? "";
        
        using var scope = _scopeFactory.CreateScope();
        var dashboardService = scope.ServiceProvider.GetRequiredService<DashboardService>();
        var monitorService = scope.ServiceProvider.GetRequiredService<ProviderMonitorService>();
        
        if (string.IsNullOrEmpty(path))
        {
            // Serve the main dashboard
            await ServeDashboard(context, dashboardService);
        }
        else if (path.StartsWith("api/"))
        {
            // Handle API requests
            await HandleApiRequest(context, path, monitorService);
        }
        else
        {
            // Serve static files
            await ServeStaticFile(context, path, dashboardService);
        }
    }

    private async Task ServeDashboard(HttpContext context, DashboardService dashboardService)
    {
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(dashboardService.GetDashboardHtml());
    }

    private async Task HandleApiRequest(HttpContext context, string path, ProviderMonitorService monitorService)
    {
        context.Response.ContentType = "application/json";

        switch (path)
        {
            case "api/providers":
                var providers = await monitorService.GetProvidersStatus();
                await context.Response.WriteAsync(JsonSerializer.Serialize(providers));
                break;
            case "api/stats":
                var stats = await monitorService.GetEmailStats();
                await context.Response.WriteAsync(JsonSerializer.Serialize(stats));
                break;
            default:
                context.Response.StatusCode = 404;
                break;
        }
    }

    private async Task ServeStaticFile(HttpContext context, string path, DashboardService dashboardService)
    {
        var file = dashboardService.GetStaticFile(path);
        if (file == null)
        {
            context.Response.StatusCode = 404;
            return;
        }

        context.Response.ContentType = GetContentType(path);
        await context.Response.Body.WriteAsync(file);
    }

    private string GetContentType(string path)
    {
        return path switch
        {
            var p when p.EndsWith(".css") => "text/css",
            var p when p.EndsWith(".js") => "application/javascript",
            var p when p.EndsWith(".png") => "image/png",
            var p when p.EndsWith(".jpg") || p.EndsWith(".jpeg") => "image/jpeg",
            var p when p.EndsWith(".svg") => "image/svg+xml",
            _ => "application/octet-stream"
        };
    }
} 