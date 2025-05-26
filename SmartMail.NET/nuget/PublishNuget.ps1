param(
    [string]$ProjectPath = "../src/SmartMail.NET.Dashboard/SmartMail.NET.Dashboard.csproj",
    [string]$Configuration = "Release",
    [string]$NugetApiKey = "<YOUR_NUGET_API_KEY>",
    [string]$NugetSource = "https://api.nuget.org/v3/index.json"
)

Write-Host "Building project..."
dotnet build $ProjectPath -c $Configuration

Write-Host "Packing project..."
dotnet pack $ProjectPath -c $Configuration --no-build

# Find the generated .nupkg file
$package = Get-ChildItem -Path (Split-Path $ProjectPath) -Filter "*.nupkg" -Recurse | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if (-not $package) {
    Write-Error "NuGet package not found!"
    exit 1
}

Write-Host "Pushing package $($package.FullName) to $NugetSource..."
dotnet nuget push $package.FullName --api-key $NugetApiKey --source $NugetSource

Write-Host "Done!" 