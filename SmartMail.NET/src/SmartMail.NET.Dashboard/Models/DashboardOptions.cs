using System;

namespace SmartMail.NET.Dashboard.Models;

public class DashboardOptions
{
    /// <summary>
    /// The path where the dashboard will be accessible. Default is "/SmartMail"
    /// </summary>
    public string Path { get; set; } = "/SmartMail";

    /// <summary>
    /// The interval in seconds for refreshing the dashboard data. Default is 5 seconds.
    /// </summary>
    public int RefreshIntervalSeconds { get; set; } = 5;

    /// <summary>
    /// Basic authentication configuration
    /// </summary>
    public BasicAuth? BasicAuth { get; set; }
}

public class BasicAuth
{
    /// <summary>
    /// Basic authentication username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Basic authentication password
    /// </summary>
    public string Password { get; set; } = string.Empty;
} 