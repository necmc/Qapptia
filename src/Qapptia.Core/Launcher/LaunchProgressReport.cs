namespace Qapptia.Core.Launcher;

/// <summary>
/// Hitos de despacho orquestados por el Launcher.
/// </summary>
public enum LaunchStage
{
    Starting,
    Capture,
    CaptureReady,
    Editor,
    EditorLoading,
    Config,
    Ui,
    Ready
}

/// <summary>
/// Reporte inmutable del hito y avance porcentual durante el despacho del Launcher.
/// </summary>
/// <param name="Stage">Hito o etapa actual del despacho de procesos.</param>
/// <param name="Percent">Valor porcentual entre 0.0 y 100.0.</param>
public readonly record struct LaunchProgressReport(LaunchStage Stage, double Percent);
