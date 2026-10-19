<#
.SYNOPSIS
    Gestión de certificado SSL de desarrollo para pruebas HTTPS:
      - Añadir / Confiar:  dotnet dev-certs https --trust
      - Quitar / Limpiar:  dotnet dev-certs https --clean
#>

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$port = 8080

# 1. Verificar si el puerto 8080 está ocupado y liberar el socket
$activeConnections = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
if ($activeConnections) {
    $owningPids = $activeConnections | Select-Object -ExpandProperty OwningProcess -Unique
    foreach ($procId in $owningPids) {
        if ($procId -and $procId -ne $PID) {
            $proc = Get-Process -Id $procId -ErrorAction SilentlyContinue
            if ($proc) {
                Write-Host "Puerto $port ocupado. Cerrando proceso previo (PID: $procId - $($proc.ProcessName))..." -ForegroundColor Yellow
                Stop-Process -Id $procId -Force -ErrorAction SilentlyContinue
            }
        }
    }
    Start-Sleep -Milliseconds 600
}

# 2. Asegurar que no queden procesos residuales de ApiMock
$residualApiMock = Get-Process -Name "ApiMock" -ErrorAction SilentlyContinue
if ($residualApiMock) {
    foreach ($p in $residualApiMock) {
        Write-Host "Cerrando instancia residual de ApiMock (PID: $($p.Id))..." -ForegroundColor Yellow
        Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue
    }
    Start-Sleep -Milliseconds 400
}

Write-Host "Mock Server activo:" -ForegroundColor Cyan
Write-Host "  > http://localhost:8080/api/v1/version`n" -ForegroundColor Green

dotnet run --project (Join-Path $scriptDir "apimock.csproj")
