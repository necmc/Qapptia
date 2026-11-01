using SkiaSharp;

namespace Qapptia.Core;

public static class Constants
{
    public const string AppName = "Qapptia";
    public const string AppVersion = "2.0.0-beta";
    public const string ConfigFileName = "config.json";
    public const string EditorStateFileName = "editor_state.json";
    public const string ShortcutCopyClipboard = "Ctrl+C";
    public const string ShortcutCopyFile = "Ctrl+F";
    public const string DrawingExtension = ".dibujo";
    public static readonly string[] SupportedImageExtensions = { ".png", ".jpg", ".jpeg" };

    // Propiedades de serialización JSON en archivos de estado (.dibujo)
    public const string MetadataPropertyMediaId = "Qapptia.mediaId";
    public const string MetadataPropertyMediaType = "Qapptia.mediaType";

    // Constantes de namespaces y cabeceras XMP (ISO 16684-1)
    public const string XmpNamespaceMediaManagement = "http://ns.adobe.com/xap/1.0/mm/";
    public const string XmpNamespaceDublinCore = "http://purl.org/dc/elements/1.1/";
    public const string XmpNamespaceAdobeBasic = "http://ns.adobe.com/xap/1.0/";
    public const string PngChunkXmpKeyword = "XML:com.adobe.xmp";
    public const string JpegXmpHeader = "http://ns.adobe.com/xap/1.0/\0";

    // Constantes de persistencia y buffers de lectura rápida
    public const string JsonFileExtension = ".json";
    public const string JsonSearchPattern = "*.json";
    public const int JsonHeaderBufferSize = 512;

    // Tipos MIME estándar (IANA / HTTP Content-Type)
    public const string MediaTypePng = "image/png";
    public const string MediaTypeJpeg = "image/jpeg";
    public const string DefaultMediaType = MediaTypePng;

    // Formatos de fecha para subcarpetas de almacenamiento de capturas
    public const string SubfolderMonthFormat = "yyyy-MM";
    public const string SubfolderDayFormat = "yyyy-MM-dd";
    public const string SubfolderHourFormat = "HH'h'";

    // Atajos predeterminados de captura
    public const string DefaultShortcutScreen = "ctrl+shift+q";
    public const string DefaultShortcutArea = "ctrl+shift+a";

    // Formato por defecto para nombres de archivo de captura
    public const string DefaultFilenameFormat = "Qapptia_YYYYMMDD_HHmmSS";

    // Color del halo de resaltado del cursor (amarillo vibrante perceptible a primera vista)
    public static readonly SKColor CursorHaloColor = new(255, 235, 59, 160);

    public static string ResolveMediaType(string filePath)
    {
        string ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".png" => MediaTypePng,
            ".jpg" or ".jpeg" => MediaTypeJpeg,
            _ => MediaTypePng
        };
    }

    // Nombres de aplicaciones de la suite
    public const string LauncherAppName = "Qapptia";
    public const string CaptureAppName = "Qapptia Capture";
    public const string EditorAppName = "Qapptia Editor";
    public const string ConfigAppName = "Qapptia Config";

    // Nombres de ejecutables de la suite
    public const string LauncherExecutableName = "Qapptia.exe";
    public const string CaptureExecutableName = "QapptiaCapture.exe";
    public const string EditorExecutableName = "QapptiaEditor.exe";
    public const string ConfigExecutableName = "QapptiaConfig.exe";

    // Argumentos de línea de comandos
    public const string ArgEditor = "--editor";
    public const string ArgConfig = "--config";
    public const string ArgCapture = "--capture";
    public const string ArgRestart = "--restart";
    public const string ArgAbout = "--about";
    public const string ArgHelp = "--help";

    // URLs de servicios web, descargas y apoyo al proyecto
#if DEBUG
    public const string DefaultSponsorUrl = "http://localhost:5050/sponsor";
    public const string DefaultDownloadUrl = "http://localhost:5050/download";
    public const string DefaultVersionApiUrl = "http://localhost:8080/api/v1/version";
#else
    public const string DefaultSponsorUrl = "https://qapptia.com/sponsor";
    public const string DefaultDownloadUrl = "https://qapptia.com/download";
    public const string DefaultVersionApiUrl = "https://qapptia.com/api/v1/version";
#endif
    public const int DefaultHttpTimeoutMs = 15000;
    public const int DefaultIpcTimeoutMs = 1000;
    public const int QuickIpcTimeoutMs = 800;
    public const int SponsorCooldownMs = 2000;

    // Temporización y políticas de comprobación de actualizaciones 
    public const int UpdateTimingStartupDelayMs = 3000;
    public const int UpdateTimingStartupMaxJitterMs = 15000;
    public const int UpdateTimingManualCooldownMs = 2000;
    public static readonly TimeSpan UpdateTimingPeriodicInterval = TimeSpan.FromHours(24);
    public static readonly TimeSpan UpdateTimingPeriodicMaxJitter = TimeSpan.FromMinutes(30);
    public static readonly int[] UpdateTimingServerUnavailableBackoffMinutes = [30, 60, 120, 240, 480];
    public static readonly TimeSpan UpdateTimingServerUnavailableRetryMaxJitter = TimeSpan.FromMinutes(5);
    public static readonly int[] UpdateTimingNoInternetBackoffHours = [1, 2, 4, 8];
    public static readonly TimeSpan UpdateTimingNoInternetMaxJitter = TimeSpan.FromMinutes(5);

    // Notificaciones del sistema
    public const int NotificationDurationMs = 5000;

    // Nombres de recursos y carpetas de assets
    public const string AssetsDirectoryName = "Assets";
    public const string TrayIconFileName = "tray_icon.ico";
    public const string AppIconFileName = "app_icon.ico";

    // Intervalo entre descargas solicitadas por el usuario
    public const int UpdateDownloadCooldownMs = 2000;

#if DEBUG
    public static string DefaultConfigPath => Path.Combine(AppContext.BaseDirectory, ConfigFileName);

    public static string DefaultLogDirectory => AppContext.BaseDirectory;
#else
    public static string DefaultConfigPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppName, 
        ConfigFileName);

    public static string DefaultLogDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppName);
#endif

    public static string DefaultSavePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
        AppName);
}
