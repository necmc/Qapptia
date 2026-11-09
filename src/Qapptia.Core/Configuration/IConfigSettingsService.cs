using Qapptia.Core.Abstractions;

namespace Qapptia.Core.Configuration;

/// <summary>Cambios editables; las preferencias ocultas del archivo se conservan.</summary>
public sealed record ConfigChanges(
    string Theme, string SavePath, string FilenameFormat,
    bool SubfolderMonth, bool SubfolderDay, bool SubfolderHour,
    bool ShowMouse, bool HighlightMouse, int ManualTimer,
    string ShortcutScreen, string ShortcutArea,
    bool CopyToClipboardScreen, bool CopyToClipboardArea, bool CaptureAllScreens)
{
    public bool ShowCaptureIndicator { get; init; } = true;
}

public sealed record ConfigSaveResult(bool IsValid, string Theme, bool ChangeAutoStart);

/// <summary>Persistencia y autoarranque de Configuración, sin dependencias visuales.</summary>
public interface IConfigSettingsService
{
    QapptiaConfig Current { get; }
    bool IsAutoStartSupported { get; }
    Task<AutoStartState> GetAutoStartStateAsync(CancellationToken cancellationToken);
    ConfigSaveResult SavePreferences(ConfigChanges changes, AutoStartState? observedState, bool requestedAutoStart);
    Task<AutoStartChangeResult> SetAutoStartAsync(bool enabled, CancellationToken cancellationToken);
}
