using System.Reflection;
using Microsoft.Extensions.Options;
using SendMail.NET.Core.Providers;
using SendMail.NET.Core.Services;
using SendMail.NET.Dashboard.Extensions;

namespace SendMail.NET.Dashboard.Services;

public class DashboardService
{
    private readonly IEmailProviderManager _providerManager;
    private readonly DashboardOptions _options;
    private readonly Dictionary<string, byte[]> _staticFiles;

    public DashboardService(
        IEmailProviderManager providerManager,
        IOptions<DashboardOptions> options)
    {
        _providerManager = providerManager;
        _options = options.Value;
        _staticFiles = LoadStaticFiles();
    }

    public string GetDashboardHtml()
    {
        return @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>SendMail.NET Dashboard</title>
    <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"" rel=""stylesheet"">
    <link href=""https://cdn.jsdelivr.net/npm/bootstrap-icons@1.7.2/font/bootstrap-icons.css"" rel=""stylesheet"">
    <style>
        .provider-card {
            transition: all 0.3s ease;
        }
        .provider-card:hover {
            transform: translateY(-5px);
            box-shadow: 0 4px 15px rgba(0,0,0,0.1);
        }
        .status-indicator {
            width: 10px;
            height: 10px;
            border-radius: 50%;
            display: inline-block;
            margin-right: 5px;
        }
        .status-active { background-color: #28a745; }
        .status-inactive { background-color: #dc3545; }
    </style>
</head>
<body>
    <nav class=""navbar navbar-expand-lg navbar-dark bg-dark"">
        <div class=""container-fluid"">
            <a class=""navbar-brand"" href=""#"">SendMail.NET Dashboard</a>
        </div>
    </nav>

    <div class=""container mt-4"">
        <div class=""row"">
            <div class=""col-12"">
                <div class=""card mb-4"">
                    <div class=""card-header"">
                        <h5 class=""card-title mb-0"">Email Providers</h5>
                    </div>
                    <div class=""card-body"">
                        <div id=""providers-container"" class=""row"">
                            <!-- Providers will be loaded here -->
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class=""row"">
            <div class=""col-md-6"">
                <div class=""card"">
                    <div class=""card-header"">
                        <h5 class=""card-title mb-0"">Email Statistics</h5>
                    </div>
                    <div class=""card-body"">
                        <div id=""stats-container"">
                            <!-- Stats will be loaded here -->
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script>
        function updateProviders() {
            fetch('/sendmail/api/providers')
                .then(response => response.json())
                .then(providers => {
                    const container = document.getElementById('providers-container');
                    container.innerHTML = providers.map(provider => `
                        <div class=""col-md-4 mb-3"">
                            <div class=""card provider-card"">
                                <div class=""card-body"">
                                    <h5 class=""card-title"">
                                        <span class=""status-indicator ${provider.isActive ? 'status-active' : 'status-inactive'}""></span>
                                        ${provider.name}
                                    </h5>
                                    <p class=""card-text"">
                                        <small class=""text-muted"">Type: ${provider.type}</small><br>
                                        <small class=""text-muted"">Quota: ${provider.quotaUsed}/${provider.quotaLimit}</small>
                                    </p>
                                </div>
                            </div>
                        </div>
                    `).join('');
                });
        }

        function updateStats() {
            fetch('/sendmail/api/stats')
                .then(response => response.json())
                .then(stats => {
                    const container = document.getElementById('stats-container');
                    container.innerHTML = `
                        <div class=""row"">
                            <div class=""col-6"">
                                <h6>Total Emails Sent</h6>
                                <p class=""h3"">${stats.totalSent}</p>
                            </div>
                            <div class=""col-6"">
                                <h6>Success Rate</h6>
                                <p class=""h3"">${stats.successRate}%</p>
                            </div>
                        </div>
                    `;
                });
        }

        // Update every 5 seconds
        setInterval(() => {
            updateProviders();
            updateStats();
        }, 5000);

        // Initial load
        updateProviders();
        updateStats();
    </script>
</body>
</html>";
    }

    public byte[]? GetStaticFile(string path)
    {
        return _staticFiles.TryGetValue(path, out var file) ? file : null;
    }

    private Dictionary<string, byte[]> LoadStaticFiles()
    {
        var files = new Dictionary<string, byte[]>();
        var assembly = Assembly.GetExecutingAssembly();
        var resourcePath = $"{assembly.GetName().Name}.wwwroot";

        foreach (var resourceName in assembly.GetManifestResourceNames())
        {
            if (resourceName.StartsWith(resourcePath))
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    using var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    var path = resourceName.Replace(resourcePath + ".", "").Replace(".", "/");
                    files[path] = ms.ToArray();
                }
            }
        }

        return files;
    }
} 