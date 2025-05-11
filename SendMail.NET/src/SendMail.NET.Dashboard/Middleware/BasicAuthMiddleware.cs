using Microsoft.AspNetCore.Http;
using System.Text;
using System.Threading.Tasks;

namespace SendMail.NET.Dashboard.Middleware;

public class BasicAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _username;
    private readonly string _password;

    public BasicAuthMiddleware(RequestDelegate next, string username, string password)
    {
        _next = next;
        _username = username;
        _password = password;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/sendmail"))
        {
            string authHeader = context.Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Basic "))
            {
                var encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
                var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));
                var parts = decodedUsernamePassword.Split(':');
                if (parts.Length == 2 && parts[0] == _username && parts[1] == _password)
                {
                    await _next(context);
                    return;
                }
            }
            context.Response.Headers["WWW-Authenticate"] = "Basic";
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }
        await _next(context);
    }
} 