namespace Qapptia.UI.Components.Common;

/// <summary>Textos y recursos visuales compartidos por las aplicaciones.</summary>
public static class Constants
{
    // Claves de iconos del sistema
    public const string IconInfo = "IconInfo";
    public const string IconDownload = "IconDownload";

    // Textos de apoyo y contribución compartidos (numerados para estabilidad ante cambios de textos)
    public const string SponsorText1 = "Invítame un café";
    public const string SponsorText2 = "Apoyar proyecto";
    public const string SponsorText3 = "Contribuir";
    public const string SponsorText4 = "Impulsar desarrollo";
    public const string SponsorDefaultActionTitle = "Apoyar";

    // Claves de iconos vectoriales de apoyo (numeradas para estabilidad)
    public const string SponsorIcon1 = "IconSponsorCoffeeHeart";
    public const string SponsorIcon2 = "IconSponsorRamenHeart";
    public const string SponsorIcon3 = "IconSponsorCoffeeCup";
    public const string SponsorIcon4 = "IconSponsorHeartBadge";

    // Identificadores del set de estilos de apoyo (definidos en MaterialTheme.axaml)
    public const string SponsorStyleA = "SponsorStyleA";
    public const string SponsorStyleB = "SponsorStyleB";
    public const string SponsorStyleC = "SponsorStyleC";
    public const string SponsorStyleD = "SponsorStyleD";

    // Notificaciones del sistema
    public const string NotificationTitleCapture = Qapptia.Core.Constants.CaptureAppName;
    public const string NotificationTitleEditor = Qapptia.Core.Constants.EditorAppName;
    public const string NotificationTitleConfig = Qapptia.Core.Constants.ConfigAppName;
    public const string NotificationMessageCaptureStarted = "El capturador está activo en segundo plano.";
    public const string NotificationMessageCaptureRestarted = "El capturador se ha reiniciado correctamente.";
    public const string NotificationMessageCaptureActive = "El capturador ya se encuentra activo en la bandeja del sistema.";
    public const string NotificationTitleUpdateAvailable = "Actualización disponible";
    public const string NotificationMessageUpdateAvailableFormat = "Qapptia v{0} está disponible para descargar.";

    // Textos de actualización
    public const string ToolTipAbout = "Acerca de Qapptia";
    public const string ToolTipUpdateAvailableFormat = "Nueva versión v{0} disponible. Haz clic para descargar";
    public const string TrayMenuDownloadUpdateFormat = "[↓] Descargar actualización (v{0})";
}
