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
            return new(false, Current.Theme, false);

        bool canConfigure = observedState?.CanConfigure == true;
        if (canConfigure) Current.AutoStart = observedState!.IsEnabled;
        Current.Theme = changes.Theme;
        Current.SavePath = changes.SavePath;
        Current.FilenameFormat = string.IsNullOrWhiteSpace(changes.FilenameFormat) ? Constants.DefaultFilenameFormat : changes.FilenameFormat;
        Current.SubfolderMonth = changes.SubfolderMonth;
        Current.SubfolderDay = changes.SubfolderDay;
        Current.SubfolderHour = changes.SubfolderHour;
        Current.ShowMouse = changes.ShowMouse;
        Current.HighlightMouse = changes.HighlightMouse && changes.ShowMouse;
        Current.ManualTimer = changes.ManualTimer;
        Current.ShortcutScreen = string.IsNullOrWhiteSpace(changes.ShortcutScreen) ? Constants.DefaultShortcutScreen : changes.ShortcutScreen;
        Current.ShortcutArea = string.IsNullOrWhiteSpace(changes.ShortcutArea) ? Constants.DefaultShortcutArea : changes.ShortcutArea;
        Current.CopyToClipboardScreen = changes.CopyToClipboardScreen;
        Current.CopyToClipboardArea = changes.CopyToClipboardArea;
        Current.CaptureAllScreens = changes.CaptureAllScreens;
        configService.Save();
        processes.NotifyPreferencesChanged(Current.Theme);
        return new(true, Current.Theme, canConfigure && requestedAutoStart != observedState!.IsEnabled);
    }

    public async Task<AutoStartChangeResult> SetAutoStartAsync(bool enabled, CancellationToken cancellationToken)
    {
        var result = await autoStartService.SetEnabledAsync(enabled, cancellationToken).ConfigureAwait(false);
        if (result.State.CanConfigure)
        {
            Current.AutoStart = result.State.IsEnabled;
            configService.Save();
        }
        return result;
    }
}
