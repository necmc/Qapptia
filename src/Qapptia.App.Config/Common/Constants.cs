namespace Qapptia.App.Config.Common;

/// <summary>
/// Constantes de texto e interfaz específicas de la aplicación de configuración.
/// </summary>
public static class Constants
{
    // Textos de ayuda y acerca de
    public const string AppDescription = "Herramienta de captura de pantalla, recortes y anotaciones. Mantiene un enfoque en la agilidad para documentar evidencias en el proceso de pruebas de software (QA) y practicidad para propósitos generales.";
    public const string AboutSupportHeader = "Apoyar el proyecto";
    public const string AboutSupportDescription = "Este es un proyecto libre e independiente. Tu apoyo impulsa la creación de nuevas características y su desarrollo continuo.";
    public const string AboutUpdatesHeader = "Actualizaciones de software";
    public const string AboutHelpHeader = "Guía del Editor";
    public const string AboutHelpDescription = "Accede a la guía visual y manual rápido de herramientas, funciones del Capturador y opciones del Editor.";
    public const string AboutHelpButtonText = "Ver guía del Editor";
    public const string AboutHelpToolTip = "Abrir la guía de uso del Editor (F1)";

    // Mensajes de estado de comprobación de actualizaciones
    public const string UpdateStatusPrompt = "Comprueba si existe una versión más reciente.";
    public const string UpdateStatusChecking = "Comprobando actualizaciones con el servidor...";
    public const string UpdateStatusAvailableFormat = "¡Nueva versión v{0} disponible para descargar!";
    public const string UpdateStatusUpToDateFormat = "Tienes instalada la versión más reciente (v{0}).";
    public const string UpdateStatusConnectError = "No se pudo conectar con el servidor.";
    public const string UpdateStatusServerError = "Ocurrió un error al contactar el servidor.";

    // Mensajes de fallos de actualización
    public const string UpdateErrorServerUnavailableFormat = "Servidor no disponible ({0}).";
    public const string UpdateErrorEmptyResponse = "El servidor no devolvió información.";
    public const string UpdateErrorMissingVersion = "La respuesta del servidor no contiene información de versión válida.";
    public const string UpdateErrorInvalidVersionFormat = "El formato de la versión recibida no es válido.";
    public const string UpdateErrorCheckCancelled = "Comprobación cancelada.";
    public const string UpdateErrorTimeout = "El servidor no responde, tiempo de espera agotado.";
    public const string UpdateErrorUnrecognizedResponse = "No se pudo interpretar la respuesta del servidor.";
    public const string UpdateErrorConnectionFailed = "No se pudo conectar con el servidor.";
    public const string UpdateErrorNoInternet = "Sin conexión a Internet.";

    // Textos de botones de actualización
    public const string UpdateButtonCheck = "Comprobar actualización";
    public const string UpdateButtonDownload = "Descargar actualización";
    public const string UpdateButtonRetry = "Reintentar comprobación";

    // Mensajes de la barra de estado inferior (Footer)
    public const string FooterConfigSaving = "Guardando…";
    public const string FooterConfigSaved = "Configuración guardada exitosamente.";
    public const string FooterSaveErrorPrefix = "Error al guardar: ";
    public const string FooterInvalidSavePath = "Error: La ruta de guardado es inválida o no existe.";
    public const string FooterAutoStartSaveError = "Guardado; falló el autoarranque.";
    public const string FooterAutoStartCancelled = "Guardado; autoarranque cancelado.";
    public const string FooterAutoStartQueryError = "No se pudo leer el autoarranque.";
    public const string FooterEditorNotFound = Qapptia.Core.Constants.EditorAppName + " no encontrado";
    public const string FooterHelpError = "Error al abrir la guía del Editor";

    // Mensajes de inicio automático
    public const string AutoStartLoading = "Comprobando autoarranque…";
    public const string AutoStartErrorQuery = "No se pudo consultar el autoarranque.";
    public const string AutoStartErrorAmbiguousScope = "La ruta está registrada como local y global.";
    public const string AutoStartErrorOtherCopy = "El autoarranque apunta a otra copia.";
    public const string AutoStartUserDescription = "Aplicable al usuario actual.";
    public const string AutoStartMachineDescription = "Todos los usuarios. Puede requerir permisos de administrador.";
    public const string AutoStartPortableDescription = "Disponible únicamente en versiones instaladas.";

    // Nombres visuales para la interfaz de usuario
    public const string ThemeSystem = "Sistema";
    public const string ThemeDark = "Oscuro";
    public const string ThemeLight = "Claro";
    public const int FooterMessageDurationMs = 5000;

    // Diálogos del sistema
    public const string DialogSelectFolderTitle = "Selecciona carpeta de guardado";
}
