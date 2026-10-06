using Qapptia.Core.Launcher;

namespace Qapptia.Launcher.Common;

/// <summary>
/// Constantes de texto, interfaz y parámetros específicos del lanzador principal y pantalla de bienvenida.
/// </summary>
public static class Constants
{
    // Mensajes de progreso y estado visual de carga (propios del Launcher)
    public const string ProgressStarting = "Iniciando Qapptia...";
    public const string ProgressCapture = "Iniciando servicio de captura...";
    public const string ProgressCaptureReady = "Servicio de captura activo";
    public const string ProgressEditor = "Iniciando editor...";
    public const string ProgressEditorLoading = "Cargando componentes del editor...";
    public const string ProgressServices = "Cargando servicios y configuración...";
    public const string ProgressConfig = "Abriendo configuración...";
    public const string ProgressUi = "Inicializando interfaz gráfica...";
    public const string ProgressPreparingUi = "Preparando interfaz visual...";
    public const string ProgressReady = "¡Listo!";

    // Textos e interfaz de la ventana de bienvenida (Splash Screen)
    public const string SplashSubtitle = "Captura, Edición y Productividad";
    public const string VersionBadgeFormat = "v{0}";
    public const string InitialPercentText = "5%";
    public const string PercentFormat = "{0}%";
    public const string LogFileName = "launcher_error.log";

    // Dimensiones y métricas visuales
    public const double SplashWindowWidth = 480;
    public const double SplashWindowHeight = 270;
    public const double InitialProgressPercent = 5;
    public const double CaptureProgressPercent = 35;
    public const double CaptureReadyProgressPercent = 45;
    public const double EditorProgressPercent = 55;
    public const double EditorLoadingProgressPercent = 65;
    public const double ServicesLoadingProgressPercent = 78;
    public const double InterfaceLoadingProgressPercent = 88;
    public const double PreparingUiProgressPercent = 94;
    public const double CompleteProgressPercent = 100;

    // Parámetros de temporización (milisegundos)
    public const int SplashMinimumDisplayMs = 800;
    public const int SplashWaitTimeoutMs = 20000;
    public const int SplashPollIntervalMs = 40;
    public const int SplashIpcTimeoutMs = 100;
    public const int SplashExitDelayMs = 250;
    public const int AnimationTimerIntervalMs = 16;
    public const int StagePacingDelayMs = 90;
    public const int CaptureCheckPollIntervalMs = 30;
    public const int CaptureCheckMaxAttempts = 5;

    // Parámetros de suavizado de animación (interpolación visual fluida)
    public const double SmoothLerpFactor = 0.25;
    public const double LerpThreshold = 0.2;

    /// <summary>
    /// Resuelve el texto visual correspondiente al hito de despacho emitido por el orquestador.
    /// </summary>
    public static string GetStageText(LaunchStage stage) => stage switch
    {
        LaunchStage.Starting => ProgressStarting,
        LaunchStage.Capture => ProgressCapture,
        LaunchStage.CaptureReady => ProgressCaptureReady,
        LaunchStage.Editor => ProgressEditor,
        LaunchStage.EditorLoading => ProgressEditorLoading,
        LaunchStage.Config => ProgressConfig,
        LaunchStage.Ui => ProgressUi,
        LaunchStage.Ready => ProgressReady,
        _ => ProgressStarting,
    };
}
