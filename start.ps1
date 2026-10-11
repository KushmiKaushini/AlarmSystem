<#>
.SYNOPSIS
    Starts the AlarmSystem application with automatic port cleanup.

.DESCRIPTION
    Ensures port 5203 is free, then starts the ASP.NET Core application.

.EXAMPLE
    .\start.ps1
#>

# Set error handling
$ErrorActionPreference = "Stop"

# Get script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$projectDir = Join-Path $scriptDir ".."
$projectPath = Join-Path $projectDir "AlarmSystem.csproj"

Write-Host "=== AlarmSystem Startup ===" -ForegroundColor Cyan
Write-Host "Project: $projectPath" -ForegroundColor Gray

# Step 1: Ensure port is free
Write-Host "`nStep 1: Checking port 5203..." -ForegroundColor Cyan
& "$scriptDir\ensure-port-free.ps1" -Port 5203
if ($LASTEXITCODE -ne 0) {
    Write-Host "Port cleanup failed. Exiting." -ForegroundColor Red
    exit $LASTEXITCODE
}

# Step 2: Start application
Write-Host "`nStep 2: Starting AlarmSystem..." -ForegroundColor Cyan
Write-Host "Press Ctrl+C to stop`n" -ForegroundColor Gray

try {
    dotnet run --project "$projectPath"
}
catch {
    Write-Host "`nApplication stopped with error: $_" -ForegroundColor Red
    exit 1
}

Write-Host "`nApplication stopped gracefully" -ForegroundColor Green