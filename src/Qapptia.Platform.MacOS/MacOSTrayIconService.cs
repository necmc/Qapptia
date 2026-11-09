using Avalonia.Threading;
using Qapptia.Core.Abstractions;
using Serilog;

namespace Qapptia.Platform.MacOS;

public sealed class MacOSTrayIconService : ITrayIconService
{
    private readonly ILogger _logger;
    private readonly bool _enableNativeTray;
    private AppKitStatusItem? _statusItem;
    private bool _disposed;

    public MacOSTrayIconService(ILogger logger, bool enableNativeTray = false)
    {
        _logger = logger;
        _enableNativeTray = enableNativeTray;
    }

    public void Initialize(TrayMenuDefinition menu, string iconPath)
    {
        if (!_enableNativeTray || !OperatingSystem.IsMacOS()) return;

        Dispatcher.UIThread.Invoke(() =>
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _statusItem ??= new AppKitStatusItem(menu, iconPath, _logger);
        });
    }

    public void ShowNotification(string title, string message, TrayNotificationType type = TrayNotificationType.Info, int timeoutMs = Qapptia.Core.Constants.NotificationDurationMs)
    {
        _logger.Information("Notificación del sistema ({Type}): {Title} - {Message}", type, title, message);
    }

    public void RefreshIcon()
    {
        // AppKit conserva el icono y el menú se construye al abrirlo.
    }

    public void Dispose()
    {
        if (!_enableNativeTray) return;

        if (Dispatcher.UIThread.CheckAccess()) DisposeOnUiThread();
        else Dispatcher.UIThread.Post(DisposeOnUiThread);
    }

    private void DisposeOnUiThread()
    {
        if (_disposed) return;
        _disposed = true;
        _statusItem?.Dispose();
        _statusItem = null;
    }
}

