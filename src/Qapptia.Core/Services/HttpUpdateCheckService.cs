using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Authentication;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Qapptia.Core.Abstractions;
using Serilog;

namespace Qapptia.Core.Services;

/// <summary>
/// Implementación de IUpdateCheckService que consulta un endpoint HTTP RESTful.
/// Desacoplado, resiliente ante caídas de red y con timeouts estrictos para no penalizar el hilo de UI.
/// </summary>
public sealed class HttpUpdateCheckService : IUpdateCheckService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;
    private readonly string _endpointUrl;
    private readonly string _currentVersion;
    private readonly ILogger _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private static readonly Qapptia.Core.Serialization.QapptiaJsonContext Context = new(JsonOptions);
    private static readonly System.Text.CompositeFormat s_serverUnavailableFormat = System.Text.CompositeFormat.Parse(Constants.UpdateErrorServerUnavailableFormat);

    public HttpUpdateCheckService(
        HttpClient? httpClient = null,
        string? endpointUrl = null,
        string? currentVersion = null,
        ILogger? logger = null)
    {
        _endpointUrl = string.IsNullOrWhiteSpace(endpointUrl) ? Constants.DefaultVersionApiUrl : endpointUrl;
        _currentVersion = string.IsNullOrWhiteSpace(currentVersion) ? Constants.AppVersion : currentVersion;
        _logger = (logger ?? Log.Logger).ForContext<HttpUpdateCheckService>();

        if (httpClient != null)
        {
            _httpClient = httpClient;
            _disposeHttpClient = false;
        }
        else
        {
            _httpClient = CreateDefaultHttpClient(_currentVersion);
            _disposeHttpClient = true;
        }
    }

    private static HttpClient CreateDefaultHttpClient(string currentVersion)
    {
        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(15),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            ConnectTimeout = TimeSpan.FromMilliseconds(Constants.DefaultHttpTimeoutMs),
            SslOptions = new SslClientAuthenticationOptions
            {
                EnabledSslProtocols = GetSupportedSslProtocols()
            }
        };

        var client = new HttpClient(handler, disposeHandler: true)
        {
            Timeout = TimeSpan.FromMilliseconds(Constants.DefaultHttpTimeoutMs)
        };
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(Constants.AppName, currentVersion));
        return client;
    }

    private static SslProtocols GetSupportedSslProtocols()
    {
        if (OperatingSystem.IsWindows() && !OperatingSystem.IsWindowsVersionAtLeast(10))
        {
            // En Windows 7, 8 y 8.1 (NT 6.1 a 6.3), Schannel no soporta TLS 1.3
            // y falla con SEC_E_UNSUPPORTED_FUNCTION (0x80090302) si se solicita.
            return SslProtocols.Tls12;
        }

        return SslProtocols.Tls12 | SslProtocols.Tls13;
    }

    public async Task<UpdateCheckResult> CheckForUpdatesAsync(bool forceCheck = false, CancellationToken ct = default)
    {
        try
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                _logger.Information("Sin conexión de red activa detectada localmente al consultar actualizaciones.");
                return UpdateCheckResult.NoInternet(_currentVersion, Constants.UpdateErrorNoInternet);
            }
        }
        catch
        {
            // Continuar normalmente si el entorno restringe NetworkInterface
        }

        try
        {
            _logger.Debug("Iniciando consulta de actualización a {Url} (Versión local: {Version})...", _endpointUrl, _currentVersion);

            using var request = new HttpRequestMessage(HttpMethod.Get, _endpointUrl);
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                _logger.Warning("El servidor de actualizaciones respondió con código {StatusCode}", response.StatusCode);
                var errorMsg = string.Format(System.Globalization.CultureInfo.InvariantCulture, s_serverUnavailableFormat, (int)response.StatusCode);
                return UpdateCheckResult.ServerUnavailable(_currentVersion, errorMsg);
            }

            var jsonContent = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                _logger.Warning("La respuesta del servidor de actualizaciones está vacía.");
                return UpdateCheckResult.Invalid(_currentVersion, Constants.UpdateErrorEmptyResponse);
            }

            var releaseDto = JsonSerializer.Deserialize(jsonContent, Context.ReleaseInfoDto);
            if (releaseDto == null || string.IsNullOrWhiteSpace(releaseDto.Version))
            {
                _logger.Warning("El JSON recibido no contiene un atributo de versión válido.");
                return UpdateCheckResult.Invalid(_currentVersion, Constants.UpdateErrorMissingVersion);
            }

            if (!SemanticVersion.TryParse(_currentVersion, out var localSemver) ||
                !SemanticVersion.TryParse(releaseDto.Version, out var remoteSemver))
            {
                _logger.Warning("Fallo al comparar versiones SemVer: Local='{Local}', Remota='{Remote}'", _currentVersion, releaseDto.Version);
                return UpdateCheckResult.Invalid(_currentVersion, Constants.UpdateErrorInvalidVersionFormat);
            }

            if (remoteSemver > localSemver)
            {
                if (!releaseDto.IsEnabled)
                {
                    _logger.Debug("Nueva versión {RemoteVersion} detectada pero se encuentra deshabilitada en el servidor.", remoteSemver);
                    return UpdateCheckResult.UpToDate(_currentVersion, releaseDto);
                }

                if (releaseDto.Platform != null)
                {
                    bool isAvailableForCurrentPlatform = false;
                    if (OperatingSystem.IsWindows())
                    {
                        isAvailableForCurrentPlatform = releaseDto.Platform.Windows?.IsAvailable ?? false;
                    }
                    else if (OperatingSystem.IsLinux())
                    {
                        isAvailableForCurrentPlatform = releaseDto.Platform.Linux?.IsAvailable ?? false;
                    }
                    else if (OperatingSystem.IsMacOS())
                    {
                        isAvailableForCurrentPlatform = releaseDto.Platform.Macos?.IsAvailable ?? false;
                    }

                    if (!isAvailableForCurrentPlatform)
                    {
                        _logger.Information("Nueva versión detectada ({RemoteVersion}), pero aún no está disponible para la plataforma actual.", remoteSemver);
                        return UpdateCheckResult.UpToDate(_currentVersion, releaseDto);
                    }
                }

                _logger.Information("Nueva versión detectada: {NewVersion} (Actual: {LocalVersion})", remoteSemver, localSemver);
                return UpdateCheckResult.UpdateAvailable(_currentVersion, releaseDto);
            }

            _logger.Debug("Qapptia está actualizado (Local: {LocalVersion}, Servidor: {RemoteVersion}).", localSemver, remoteSemver);
            return UpdateCheckResult.UpToDate(_currentVersion, releaseDto);
        }
        catch (TimeoutException ex)
        {
            _logger.Warning(ex, "Tiempo de espera agotado al consultar el servidor de actualizaciones en {Url}.", _endpointUrl);
            return UpdateCheckResult.ServerUnavailable(_currentVersion, Constants.UpdateErrorTimeout);
        }
        catch (OperationCanceledException ex)
        {
            if (!ct.IsCancellationRequested)
            {
                _logger.Warning(ex, "Tiempo de espera agotado al consultar el servidor de actualizaciones en {Url}.", _endpointUrl);
                return UpdateCheckResult.ServerUnavailable(_currentVersion, Constants.UpdateErrorTimeout);
            }

            _logger.Debug("La comprobación de actualización fue cancelada.");
            return UpdateCheckResult.Failed(_currentVersion, Constants.UpdateErrorCheckCancelled);
        }
        catch (JsonException ex)
        {
            _logger.Warning(ex, "Error al deserializar la respuesta JSON de actualización.");
            return UpdateCheckResult.Invalid(_currentVersion, Constants.UpdateErrorUnrecognizedResponse);
        }
        catch (Exception ex)
        {
            if (IsNoInternetException(ex))
            {
                _logger.Warning(ex, "Sin conexión a Internet al consultar {Url}.", _endpointUrl);
                return UpdateCheckResult.NoInternet(_currentVersion, Constants.UpdateErrorNoInternet);
            }

            _logger.Warning(ex, "Error al verificar actualizaciones en {Url}: {Message}", _endpointUrl, ex.Message);
            return UpdateCheckResult.ServerUnavailable(_currentVersion, Constants.UpdateErrorConnectionFailed);
        }
    }

    private static bool IsNoInternetException(Exception ex)
    {
        try
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return true;
            }
        }
        catch
        {
            // Continuar con análisis de sockets si el entorno restringe NetworkInterface
        }

        var current = ex;
        while (current != null)
        {
            if (current is System.Net.Sockets.SocketException socketEx)
            {
                if (socketEx.SocketErrorCode is System.Net.Sockets.SocketError.NetworkDown
                    or System.Net.Sockets.SocketError.NetworkUnreachable
                    or System.Net.Sockets.SocketError.HostUnreachable
                    or System.Net.Sockets.SocketError.HostNotFound
                    or System.Net.Sockets.SocketError.NoData)
                {
                    return true;
                }
            }

            if (current is HttpRequestException httpEx && httpEx.HttpRequestError == HttpRequestError.NameResolutionError)
            {
                return true;
            }

            current = current.InnerException;
        }

        return false;
    }

    public void Dispose()
    {
        if (_disposeHttpClient)
        {
            _httpClient.Dispose();
        }
    }
}
