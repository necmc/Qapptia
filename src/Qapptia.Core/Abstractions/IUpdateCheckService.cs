using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Qapptia.Core.Abstractions;

/// <summary>
/// Estados de resultado del proceso de comprobación de actualizaciones.
/// </summary>
public enum UpdateCheckStatus
{
    /// <summary>La aplicación se encuentra en la versión más reciente.</summary>
    UpToDate,

    /// <summary>Existe una nueva versión disponible superior a la actual.</summary>
    UpdateAvailable,

    /// <summary>El servidor remoto no está disponible o respondió con un código de error HTTP (5xx / 4xx).</summary>
    ServerUnavailable,

    /// <summary>El equipo local no cuenta con conexión a Internet activa o la red está inaccesible.</summary>
    NoInternet,

    /// <summary>No se pudo conectar con el servidor o expiró el tiempo de espera.</summary>
    ConnectionFailed,

    /// <summary>La respuesta remota no pudo deserializarse o contenía un SemVer inválido.</summary>
    InvalidPayload
}

/// <summary>
/// DTO estructurado para la respuesta del endpoint remoto de versión.
/// Sigue las buenas prácticas de APIs RESTful con atributos en camelCase e inglés.
/// </summary>
public sealed class ReleaseInfoDto
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; } = true;

    [JsonPropertyName("publishedAt")]
    public string? PublishedAt { get; set; }

    [JsonPropertyName("siteDownload")]
    public string? SiteDownload { get; set; }

    [JsonPropertyName("siteSponsor")]
    public string? SiteSponsor { get; set; }

    [JsonPropertyName("siteRepositoryGH")]
    public string? SiteRepositoryGH { get; set; }

    [JsonPropertyName("changelog")]
    public List<string>? Changelog { get; set; }

    [JsonPropertyName("platform")]
    public PlatformContainerDto? Platform { get; set; }

    [JsonPropertyName("isCritical")]
    public bool? IsCritical { get; set; }

    [JsonPropertyName("minSupportedVersion")]
    public string? MinSupportedVersion { get; set; }
}

/// <summary>
/// Contenedor de plataformas disponibles para la versión.
/// </summary>
public sealed class PlatformContainerDto
{
    [JsonPropertyName("windows")]
    public PlatformDetailDto? Windows { get; set; }

    [JsonPropertyName("linux")]
    public PlatformDetailDto? Linux { get; set; }

    [JsonPropertyName("macos")]
    public PlatformDetailDto? Macos { get; set; }
}

/// <summary>
/// Detalle de disponibilidad y variantes para un sistema operativo.
/// </summary>
public sealed class PlatformDetailDto
{
    [JsonPropertyName("isAvailable")]
    public bool IsAvailable { get; set; }

    [JsonPropertyName("variants")]
    public VariantMapDto? Variants { get; set; }
}

/// <summary>
/// Mapeo de variantes de descarga por plataforma.
/// </summary>
public sealed class VariantMapDto
{
    [JsonPropertyName("installer")]
    public VariantDetailDto? Installer { get; set; }

    [JsonPropertyName("portable")]
    public VariantDetailDto? Portable { get; set; }

    [JsonPropertyName("portableLight")]
    public VariantDetailDto? PortableLight { get; set; }
}

/// <summary>
/// Información detallada de un archivo o variante binaria.
/// </summary>
public sealed class VariantDetailDto
{
    [JsonPropertyName("filename")]
    public string? Filename { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("alternativeMirrorUrl")]
    public string? AlternativeMirrorUrl { get; set; }

    [JsonPropertyName("sizeMB")]
    public double? SizeMB { get; set; }

    [JsonPropertyName("sha256")]
    public string? Sha256 { get; set; }
}

/// <summary>
/// Resultado del proceso de verificación de versiones.
/// </summary>
public sealed class UpdateCheckResult
{
    public UpdateCheckStatus Status { get; init; }
    public ReleaseInfoDto? LatestRelease { get; init; }
    public string CurrentVersion { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }

    public bool IsUpdateAvailable => Status == UpdateCheckStatus.UpdateAvailable;

    public static UpdateCheckResult UpToDate(string currentVersion, ReleaseInfoDto? release = null) =>
        new() { Status = UpdateCheckStatus.UpToDate, CurrentVersion = currentVersion, LatestRelease = release };

    public static UpdateCheckResult UpdateAvailable(string currentVersion, ReleaseInfoDto release) =>
        new() { Status = UpdateCheckStatus.UpdateAvailable, CurrentVersion = currentVersion, LatestRelease = release };

    public static UpdateCheckResult Failed(string currentVersion, string errorMessage) =>
        new() { Status = UpdateCheckStatus.ConnectionFailed, CurrentVersion = currentVersion, ErrorMessage = errorMessage };

    public static UpdateCheckResult ServerUnavailable(string currentVersion, string errorMessage) =>
        new() { Status = UpdateCheckStatus.ServerUnavailable, CurrentVersion = currentVersion, ErrorMessage = errorMessage };

    public static UpdateCheckResult NoInternet(string currentVersion, string errorMessage) =>
        new() { Status = UpdateCheckStatus.NoInternet, CurrentVersion = currentVersion, ErrorMessage = errorMessage };

    public static UpdateCheckResult Invalid(string currentVersion, string errorMessage) =>
        new() { Status = UpdateCheckStatus.InvalidPayload, CurrentVersion = currentVersion, ErrorMessage = errorMessage };
}

/// <summary>
/// Contrato de servicio para verificar actualizaciones remotas del software.
/// </summary>
public interface IUpdateCheckService
{
    /// <summary>
    /// Consulta el endpoint remoto para determinar si existe una nueva versión de Qapptia.
    /// </summary>
    /// <param name="forceCheck">Si es true, ignora marcas de tiempo de caché previa.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Resultado estructurado de la verificación.</returns>
    Task<UpdateCheckResult> CheckForUpdatesAsync(bool forceCheck = false, CancellationToken ct = default);
}
