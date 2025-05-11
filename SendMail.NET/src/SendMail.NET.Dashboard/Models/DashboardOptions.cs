namespace SendMail.NET.Dashboard.Models;

public class DashboardOptions
{
    public string Path { get; set; } = "/sendmail";
    public int RefreshIntervalSeconds { get; set; } = 5;
} 