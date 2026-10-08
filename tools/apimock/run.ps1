<#
.SYNOPSIS
    Gestión de certificado SSL de desarrollo para pruebas HTTPS:
      - Añadir / Confiar:  dotnet dev-certs https --trust
      - Quitar / Limpiar:  dotnet dev-certs https --clean
#>

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "Mock Server activo:" -ForegroundColor Cyan
Write-Host "  > https://localhost:8080/api/version" -ForegroundColor Green
Write-Host "  > https://localhost:8080/download" -ForegroundColor Green
Write-Host "  > https://localhost:8080/sponsor`n" -ForegroundColor Green

dotnet run --project (Join-Path $scriptDir "apimock.csproj")
