using Qapptia.Core.Abstractions;
using Serilog;

namespace Qapptia.Core.Configuration;

public sealed class ConfigSettingsService(
    IConfigService configService, IAutoStartService autoStartService, IConfigProcessService processes) : IConfigSettingsService
{
    public QapptiaConfig Current => configService.Current;
    public bool IsAutoStartSupported => autoStartService.IsSupported;

    public Task<AutoStartState> GetAutoStartStateAsync(CancellationToken cancellationToken) => Task.Run(() =>
    {
        try { return autoStartService.GetState(); }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error al consultar el inicio automático desde Configuración.");
            return new AutoStartState(AutoStartScope.Unknown, false, AutoStartError.RegistryReadFailed);
        }
    }, cancellationToken);

    public ConfigSaveResult SavePreferences(ConfigChanges changes, AutoStartState? observedState, bool requestedAutoStart)
    {
        if (string.IsNullOrWhiteSpace(changes.SavePath) || !Directory.Exists(Environment.ExpandEnvironmentVariables(changes.SavePath)))
            return new(false, Current.General.Theme, false);

        bool canConfigure = observedState?.CanConfigure == true;
        if (canConfigure) Current.General.AutoStart = observedState!.IsEnabled;
        Current.General.Theme = changes.Theme;
        Current.General.SavePath = changes.SavePath;
        Current.General.FilenameFormat = string.IsNullOrWhiteSpace(changes.FilenameFormat) ? Constants.DefaultFilenameFormat : changes.FilenameFormat;
        Current.General.SubfolderMonth = changes.SubfolderMonth;
        Current.General.SubfolderDay = changes.SubfolderDay;
        Current.General.SubfolderHour = changes.SubfolderHour;
        Current.Capture.ShowMouse = changes.ShowMouse;
        Current.Capture.HighlightMouse = changes.HighlightMouse && changes.ShowMouse;
        Current.Capture.ManualTimer = changes.ManualTimer;
        Current.Capture.ShortcutScreen = string.IsNullOrWhiteSpace(changes.ShortcutScreen) ? Constants.DefaultShortcutScreen : changes.ShortcutScreen;
        Current.Capture.ShortcutArea = string.IsNullOrWhiteSpace(changes.ShortcutArea) ? Constants.DefaultShortcutArea : changes.ShortcutArea;
        Current.Capture.CopyToClipboardScreen = changes.CopyToClipboardScreen;
        Current.Capture.CopyToClipboardArea = changes.CopyToClipboardArea;
        Current.Capture.CaptureAllScreens = changes.CaptureAllScreens;
        Current.Capture.ShowCaptureIndicator = changes.ShowCaptureIndicator;
        configService.Save();
        processes.NotifyPreferencesChanged(Current.General.Theme);
        return new(true, Current.General.Theme, canConfigure && requestedAutoStart != observedState!.IsEnabled);
    }

    public async Task<AutoStartChangeResult> SetAutoStartAsync(bool enabled, CancellationToken cancellationToken)
    {
        var result = await autoStartService.SetEnabledAsync(enabled, cancellationToken).ConfigureAwait(false);
        if (result.State.CanConfigure)
        {
            Current.General.AutoStart = result.State.IsEnabled;
            configService.Save();
        }
        return result;
    }
}
