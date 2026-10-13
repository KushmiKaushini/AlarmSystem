<#>
.SYNOPSIS
    Ensures a specific port is free by stopping any AlarmSystem process using it.

.DESCRIPTION
    Checks if the specified port is in LISTENING state and owned by an AlarmSystem process.
    If found, stops that process gracefully to free the port.

.PARAMETER Port
    The TCP port number to check and free (default: 5200).

.EXAMPLE
    .\ensure-port-free.ps1 -Port 5200
#>
param(
    [int]$Port = 5200
)

Write-Host "Checking port $Port..." -ForegroundColor Cyan

try {
    $connections = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
    
    foreach ($conn in $connections) {
        $proc = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
        if ($proc -and $proc.ProcessName -eq "AlarmSystem") {
            Write-Host "Found AlarmSystem process (PID: $($proc.Id)) holding port $Port" -ForegroundColor Yellow
            Write-Host "Stopping process..." -ForegroundColor Yellow
            Stop-Process -Id $proc.Id -Force -ErrorAction Stop
            Write-Host "Process stopped. Waiting for port release..." -ForegroundColor Cyan
            Start-Sleep -Seconds 2
            
            # Verify port is free
            $verify = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
            if (-not $verify) {
                Write-Host "Port $Port is now free" -ForegroundColor Green
            } else {
                Write-Host "Warning: Port $Port still in use" -ForegroundColor Red
            }
        } elseif ($proc) {
            Write-Host "Port $Port is held by another process: $($proc.ProcessName) (PID: $($proc.Id))" -ForegroundColor Red
            Write-Host "Manual intervention required." -ForegroundColor Red
            exit 1
        }
    }
    
    if (-not $connections) {
        Write-Host "Port $Port is already free" -ForegroundColor Green
    }
}
catch {
    Write-Host "Error checking port ${Port}: $_" -ForegroundColor Red
    exit 1
}

exit 0