using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;

namespace Qapptia.Core.Configuration;

/// <summary>
/// Persiste las preferencias agrupadas en global_config.json.
/// </summary>
public sealed class JsonConfigService : IConfigService
{
    private const int ReplaceRetryCount = 3;
    private const int ReplaceRetryDelayMs = 25;
    private const int WindowsAccessDenied = unchecked((int)0x80070005);
    private const int WindowsSharingViolation = unchecked((int)0x80070020);
    private const int WindowsLockViolation = unchecked((int)0x80070021);

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = null,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    private static readonly Qapptia.Core.Serialization.QapptiaJsonContext s_context = new(s_jsonOptions);

    private readonly string _path;
    private readonly ILogger _logger;
    private readonly object _gate = new();
    private QapptiaConfig _current;

    public JsonConfigService(string path, ILogger? logger = null)
    {
        _path = path;
        _logger = logger ?? Serilog.Log.Logger;
        _current = LoadOrNew(path);
    }

    public QapptiaConfig Current => _current;

    public void Save()
    {
        lock (_gate)
        {
            try
            {
                SaveAtomically();
                _logger.Information("Config guardado en {Path}", _path);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error guardando config en {Path}", _path);
                throw;
            }
        }
    }

    private void SaveAtomically()
    {
        var path = Path.GetFullPath(_path);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporaryPath = $"{path}.{Guid.NewGuid():N}.tmp";
        bool ownsTemporary = false;

        try
        {
            using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                ownsTemporary = true;
                JsonSerializer.Serialize(stream, _current, s_context.QapptiaConfig);
                stream.Flush(flushToDisk: true);
            }

            ReplaceFromTemporary(temporaryPath, path);
            ownsTemporary = false;
        }
        finally
        {
            if (ownsTemporary)
            {
                try { File.Delete(temporaryPath); }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    _logger.Warning(ex, "No se pudo eliminar el archivo temporal de configuración en {Path}", temporaryPath);
                }
            }
        }
    }

    private static void ReplaceFromTemporary(string temporaryPath, string path)
    {
        for (int retry = 0; ; retry++)
        {
            try
            {
                File.Move(temporaryPath, path, overwrite: true);
                return;
            }
            catch (Exception ex) when (retry < ReplaceRetryCount && OperatingSystem.IsWindows() &&
                ex.HResult is WindowsAccessDenied or WindowsSharingViolation or WindowsLockViolation)
            {
                // Windows puede devolver acceso denegado mientras un lector mantiene abierto el destino.
                Thread.Sleep(ReplaceRetryDelayMs);
            }
        }
    }

    public void Reload()
    {
        lock (_gate)
        {
            _current = LoadOrNew(_path);
            _logger.Information("Config recargado desde {Path}", _path);
        }
    }

    private QapptiaConfig LoadOrNew(string path)
    {
        if (!File.Exists(path))
        {
            _logger.Information("No se encontró la configuración en {Path}. Se usarán los valores predeterminados", path);
            return new QapptiaConfig();
        }

        try
        {
            var json = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize(json, s_context.QapptiaConfig);
            if (config is null || config.General is null || config.Capture is null || config.Editor is null)
            {
                _logger.Warning("Configuración vacía o inválida en {Path}. Se usarán los valores predeterminados", path);
                return new QapptiaConfig();
            }
            _logger.Information("Config cargado desde {Path}", path);
            return config;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al leer la configuración en {Path}. Se usarán los valores predeterminados", path);
            return new QapptiaConfig();
        }
    }
}

