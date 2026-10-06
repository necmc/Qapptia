<#
.SYNOPSIS
    Script de compilación, prueba, empaquetado y ejecución para Qapptia.

.DESCRIPTION
    - Sin parámetros: Compila la solución y ejecuta las pruebas unitarias.
    - Con -Clean: Limpia los directorios bin, obj y dist.
    - Con -Run: Ejecuta la aplicación tras la compilación/empaquetado.
    - Con -Package: Genera el paquete ligero dependiente del framework (portable-light.zip).
    - Con -PackageFull: Genera el paquete autocontenido (portable.zip).
    - Con -Installer: Genera el instalador de Windows (setup.exe) con Inno Setup.
    - Con -PackageAll: Genera todos los formatos de distribución (portable-light, portable e instalador).

.PARAMETER Clean
    Limpia los directorios bin, obj y dist antes de la compilación o como tarea única.

.PARAMETER Package
    Genera el paquete ligero dependiente del framework (.NET en el sistema).

.PARAMETER PackageFull
    Genera el paquete autocontenido con runtime embebido.

.PARAMETER Installer
    Genera el instalador ejecutable para Windows mediante Inno Setup.

.PARAMETER PackageAll
    Genera todos los formatos de empaquetado disponibles (incluyendo el instalador).

.PARAMETER Run
    Inicia la aplicación al finalizar el proceso.

.PARAMETER Configuration
    Configuración de compilación (Release por defecto).

.PARAMETER Runtime
    Identificador de runtime de destino (win-x64 por defecto).

.PARAMETER NoTest
    Omite la ejecución de las pruebas unitarias.
#>

[CmdletBinding()]
param (
    [switch]$Clean,
    [switch]$Package,
    [switch]$PackageFull,
    [switch]$Installer,
    [switch]$PackageAll,
    [switch]$Run,
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$NoTest
)

$ErrorActionPreference = "Stop"
$rootDir = $PSScriptRoot
$distDir = Join-Path $rootDir "dist"
$distAppDir = Join-Path $distDir "Qapptia"
$distSubAppDir = Join-Path $distAppDir "app"
$generatedPackages = @()

function Stop-ExistingQapptiaProcesses {
    $runningProcesses = @(Get-Process -Name "*Qapptia*" -ErrorAction SilentlyContinue)
    if ($runningProcesses.Count -gt 0) {
        $procList = ($runningProcesses | ForEach-Object { "$($_.ProcessName) (PID: $($_.Id))" }) -join ", "
        Write-Host "Procesos previos de Qapptia detectados en ejecución: $procList. Cerrando..." -ForegroundColor Yellow
        $runningProcesses | Stop-Process -Force -ErrorAction SilentlyContinue
        Start-Sleep -Milliseconds 150
    }
}

# 1. Resolver versión desde Directory.Build.props
$propsPath = Join-Path $rootDir "Directory.Build.props"
if (-not (Test-Path $propsPath)) {
    Write-Error "No se encontró Directory.Build.props en $rootDir."
    exit 1
}

[xml]$propsXml = Get-Content $propsPath
$versionPrefix = $propsXml.Project.PropertyGroup.VersionPrefix | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -First 1
$versionSuffix = $propsXml.Project.PropertyGroup.VersionSuffix | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -First 1
$appVersion = if ($versionSuffix) { "$versionPrefix-$versionSuffix" } else { $versionPrefix }

$buildFramework = $Package -or $PackageAll
$buildZipSelfContained = $PackageFull -or $PackageAll
$buildInstaller = $Installer -or $PackageAll
$buildSelfContained = $buildZipSelfContained -or $buildInstaller
$isPackaging = $buildFramework -or $buildSelfContained -or $buildInstaller

Write-Host "Qapptia Build - Versión: $appVersion ($Configuration)" -ForegroundColor Cyan

# Limpieza previa de bin, obj y dist al empaquetar o si se invoca -Clean
if ($isPackaging -or $Clean) {
    Write-Host "`nLimpiando artefactos previos (bin, obj, dist)..." -ForegroundColor Yellow
    Stop-ExistingQapptiaProcesses
    & dotnet build-server shutdown | Out-Null
    Start-Sleep -Milliseconds 100

    Get-ChildItem -Path $rootDir -Include "bin", "obj" -Recurse -Directory -Force -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -notmatch '[\\/]\.(git|agents|vs)([\\/]|$)' } |
        Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

    if (Test-Path $distDir) {
        Remove-Item -Path (Join-Path $distDir "*") -Recurse -Force -ErrorAction SilentlyContinue
    }

    if ($Clean -and -not $isPackaging -and -not $Run) {
        Write-Host "Limpieza completada exitosamente." -ForegroundColor Green
        exit 0
    }
}

# 2. Compilar solución
Write-Host "`n[1/3] Compilando solución..." -ForegroundColor Yellow
Stop-ExistingQapptiaProcesses
dotnet build (Join-Path $rootDir "Qapptia.slnx") -c $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Error "Falló la compilación de la solución."
    exit $LASTEXITCODE
}

# 3. Pruebas unitarias
if (-not $NoTest) {
    Write-Host "`n[2/3] Ejecutando pruebas unitarias..." -ForegroundColor Yellow
    dotnet run --project (Join-Path $rootDir "tests/Qapptia.Core.Tests/Qapptia.Core.Tests.csproj") -c $Configuration --no-build
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Pruebas unitarias de Core fallidas."
        exit $LASTEXITCODE
    }
    dotnet run --project (Join-Path $rootDir "tests/Qapptia.Editor.Tests/Qapptia.Editor.Tests.csproj") -c $Configuration --no-build
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Pruebas unitarias de Editor fallidas."
        exit $LASTEXITCODE
    }
} else {
    Write-Host "`n[2/3] Pruebas unitarias omitidas." -ForegroundColor DarkGray
}

function Find-InnoSetupCompiler {
    $cmd = Get-Command "iscc" -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    $candidatePaths = @(
        "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
        "$env:LOCALAPPDATA\Programs\Inno Setup 7\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 7\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 7\ISCC.exe"
    )

    foreach ($path in $candidatePaths) {
        if (-not [string]::IsNullOrWhiteSpace($path) -and (Test-Path $path)) {
            return $path
        }
    }

    return $null
}

function Publish-AppFiles {
    param (
        [bool]$SelfContained,
        [string]$VariantName
    )

    Write-Host "`n--- Publicando binarios: $VariantName ---" -ForegroundColor Yellow

    Stop-ExistingQapptiaProcesses
    Start-Sleep -Milliseconds 100

    if (Test-Path $distAppDir) {
        Remove-Item -Recurse -Force $distAppDir
    }

    $publishLauncherArgs = @(
        "publish", (Join-Path $rootDir "src/Qapptia.Launcher/Qapptia.Launcher.csproj"),
        "-c", $Configuration,
        "-r", $Runtime,
        "--self-contained", $SelfContained.ToString().ToLower(),
        "-p:PublishSingleFile=true",
        "-p:IncludeNativeLibrariesForSelfExtract=true",
        "-p:DebugType=none",
        "-o", $distAppDir
    )
    if ($SelfContained) {
        $publishLauncherArgs += "-p:PublishTrimmed=true"
    }

    & dotnet $publishLauncherArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error al publicar Qapptia.Launcher ($VariantName)."
        exit $LASTEXITCODE
    }

    $rootAssets = Join-Path $distAppDir "Assets"
    if (Test-Path $rootAssets) {
        Remove-Item -Recurse -Force $rootAssets
    }

    $subProjects = @(
        "src/Qapptia.App.Capture/Qapptia.App.Capture.csproj",
        "src/Qapptia.App.Editor/Qapptia.App.Editor.csproj",
        "src/Qapptia.App.Config/Qapptia.App.Config.csproj"
    )

    foreach ($proj in $subProjects) {
        & dotnet publish (Join-Path $rootDir $proj) `
            -c $Configuration -r $Runtime `
            --self-contained $SelfContained `
            -p:DebugType=none `
            -o $distSubAppDir
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Error al publicar $proj ($VariantName)."
            exit $LASTEXITCODE
        }
    }

    # Depuración de símbolos PDB y ensamblados ajenos al sistema operativo de destino
    Get-ChildItem -Path $distAppDir -Filter "*.pdb" -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force
    if ($Runtime -like "win*") {
        $prunePatterns = @(
            "Avalonia.X11*.dll",
            "Avalonia.FreeDesktop*.dll",
            "Avalonia.Native*.dll",
            "Tmds.DBus*.dll",
            "Microsoft.DiaSymReader*.dll",
            "mscordaccore*.dll",
            "mscordbi.dll"
        )
        foreach ($pattern in $prunePatterns) {
            Get-ChildItem -Path $distSubAppDir -Filter $pattern -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force
        }
    }
}

function Build-PackageZip {
    param (
        [string]$VariantName,
        [string]$VariantSuffix
    )

    $zipFileName = "Qapptia-v$appVersion-$Runtime-$VariantSuffix.zip"
    $zipFilePath = Join-Path $distDir $zipFileName
    Compress-Archive -Path (Join-Path $distAppDir "*") -DestinationPath $zipFilePath -Force

    $zipItem = Get-Item $zipFilePath
    $zipSizeMb = [math]::Round($zipItem.Length / 1MB, 2)

    return [PSCustomObject]@{
        Tipo    = $VariantName
        Archivo = $zipFileName
        Tamano  = "$zipSizeMb MB"
    }
}

function Build-InstallerPackage {
    param (
        [string]$SourceDir
    )

    Write-Host "`n--- Generando Instalador (Inno Setup) ---" -ForegroundColor Yellow

    $iscc = Find-InnoSetupCompiler
    if (-not $iscc) {
        Write-Error "No se encontró el compilador de Inno Setup (ISCC.exe). Instálalo con: winget install JRSoftware.InnoSetup"
        exit 1
    }

    $issPath = Join-Path $rootDir "qapptia.iss"
    if (-not (Test-Path $issPath)) {
        Write-Error "No se encontró el archivo de script de Inno Setup en: $issPath"
        exit 1
    }

    $setupBaseName = "Qapptia-v$appVersion-$Runtime-setup"
    $setupExeName = "$setupBaseName.exe"
    $setupFilePath = Join-Path $distDir $setupExeName

    if (Test-Path $setupFilePath) {
        Remove-Item $setupFilePath -Force
    }

    Write-Host "Compilando instalador con ISCC..." -ForegroundColor DarkGray
    & $iscc /Q `
        "/DAppVersion=$appVersion" `
        "/DSourceDir=$SourceDir" `
        "/DOutputDir=$distDir" `
        "/DOutputBaseFilename=$setupBaseName" `
        $issPath

    if ($LASTEXITCODE -ne 0 -or -not (Test-Path $setupFilePath)) {
        Write-Error "Error al compilar el instalador con Inno Setup."
        exit 1
    }

    $setupItem = Get-Item $setupFilePath
    $setupSizeMb = [math]::Round($setupItem.Length / 1MB, 2)

    return [PSCustomObject]@{
        Tipo    = "Instalador (Inno Setup)"
        Archivo = $setupExeName
        Tamano  = "$setupSizeMb MB"
    }
}

# 4. Pipeline de empaquetado
if ($isPackaging) {
    Write-Host "`n[3/3] Generando artefactos de distribución..." -ForegroundColor Yellow

    if (-not (Test-Path $distDir)) {
        New-Item -ItemType Directory -Path $distDir | Out-Null
    }

    # Paquete dependiente del Framework (portable-light)
    if ($buildFramework) {
        Publish-AppFiles -SelfContained $false -VariantName "Framework-Dependent"
        $generatedPackages += Build-PackageZip -VariantName "Framework-Dependent" -VariantSuffix "portable-light"
    }

    # Publicación autocontenida (necesaria para portable y/o instalador)
    if ($buildSelfContained) {
        Publish-AppFiles -SelfContained $true -VariantName "Self-Contained"

        if ($buildZipSelfContained) {
            $generatedPackages += Build-PackageZip -VariantName "Self-Contained" -VariantSuffix "portable"
        }

        if ($buildInstaller) {
            $generatedPackages += Build-InstallerPackage -SourceDir $distAppDir
        }
    }

    Write-Host "`nPaquetes generados:" -ForegroundColor Green
    $generatedPackages | Format-Table Tipo, Archivo, Tamano -AutoSize | Out-String | Write-Host
} else {
    Write-Host "`n[3/3] Para empaquetar use: -Package, -PackageFull, -Installer o -PackageAll" -ForegroundColor DarkGray
}

# 5. Ejecución (-Run)
if ($Run) {
    Write-Host "`n[Run] Iniciando aplicación..." -ForegroundColor Cyan
    if ($isPackaging) {
        $exeToRun = Join-Path $distAppDir "Qapptia.exe"
    } else {
        $localExeRoot = Join-Path $rootDir "bin/$Configuration/Qapptia.exe"
        $localExeWin = Join-Path $rootDir "bin/$Configuration/win-x64/Qapptia.exe"
        $exeToRun = if (Test-Path $localExeRoot) { $localExeRoot } else { $localExeWin }
    }

    if (Test-Path $exeToRun) {
        Start-Process $exeToRun
    } else {
        Write-Error "No se encontró el ejecutable en: $exeToRun"
    }
}
