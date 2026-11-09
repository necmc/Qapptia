using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Qapptia.Core;
using Qapptia.Core.Abstractions;
using Qapptia.Core.Extensions;
using Serilog;

namespace Qapptia.Platform.Linux;

public sealed class LinuxTrayIconService : ITrayIconService
{
    private static readonly Version s_minimumTrayVersion = new(12, 1, 3);
    private readonly ILogger _logger;
    private readonly bool _enableNativeTray;
    private TrayIcons? _icons;
    private TrayIcon? _icon;
    private TrayMenuDefinition? _definition;
    private bool _disposed;

    public LinuxTrayIconService(ILogger logger, bool enableNativeTray = false)
    {
        _logger = logger;
        _enableNativeTray = enableNativeTray;
    }

    public void Initialize(TrayMenuDefinition menu, string iconPath)
    {
        if (!_enableNativeTray || !OperatingSystem.IsLinux()) return;

        // La cancelación del observador de D-Bus requiere la corrección de cierre de Avalonia.
        if (typeof(TrayIcon).Assembly.GetName().Version is not { } version || version < s_minimumTrayVersion)
        {
            _logger.Warning("La bandeja de Linux requiere Avalonia {Version} o posterior.", s_minimumTrayVersion);
            return;
        }

        Dispatcher.UIThread.Invoke(() => InitializeOnUiThread(menu, iconPath));
    }

    private void InitializeOnUiThread(TrayMenuDefinition definition, string iconPath)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_icon != null || Application.Current is not { } app) return;

        var menu = new NativeMenu();
        PopulateMenu(menu, definition);
        menu.NeedsUpdate += (_, _) => PopulateMenu(menu, definition);

        var icon = new TrayIcon { Icon = new WindowIcon(iconPath), ToolTipText = Constants.CaptureAppName, Menu = menu };
        // StatusNotifierItem no distingue el doble clic. La activación solo enfoca un Editor abierto. La opción Editor del menú permite iniciarlo.
        icon.Clicked += (_, _) =>
        {
            if (definition.SingleClickAction is { } activate)
            {
                _ = Task.Run(activate);
            }
        };

        _definition = definition;
        _icon = icon;
        _icons = TrayIcon.GetIcons(app);
        if (_icons == null)
        {
            _icons = new TrayIcons();
            TrayIcon.SetIcons(app, _icons);
        }
        _icons.Add(icon);
    }

    private static void PopulateMenu(NativeMenu nativeMenu, TrayMenuDefinition definition)
    {
        nativeMenu.Items.Clear();
        foreach (var item in definition.Items)
        {
            if (!item.IsVisible) continue;

            if (item is TrayMenuSeparatorItem)
            {
                nativeMenu.Add(new NativeMenuItemSeparator());
            }
            else if (item is TrayMenuActionItem action)
            {
                string? shortcut = action.ShortcutTextProvider?.Invoke();
                string label = string.IsNullOrWhiteSpace(shortcut) ? action.Text : $"{action.Text}\t{shortcut.ToShortcutTitleCase()}";
                var nativeItem = new NativeMenuItem(label)
                {
                    IsChecked = action.IsChecked,
                    ToggleType = action.IsChecked ? MenuItemToggleType.CheckBox : MenuItemToggleType.None
                };
                nativeItem.Click += (_, _) => Dispatcher.UIThread.Post(action.OnClick);
                nativeMenu.Add(nativeItem);
            }
        }
    }

    public void ShowNotification(string title, string message, TrayNotificationType type = TrayNotificationType.Info, int timeoutMs = Qapptia.Core.Constants.NotificationDurationMs)
    {
        _logger.Information("Notificación del sistema ({Type}): {Title} - {Message}", type, title, message);
    }

    public void RefreshIcon()
    {
        if (!_enableNativeTray) return;

        Dispatcher.UIThread.Post(() =>
        {
            if (_icon?.Menu is { } menu && _definition is { } definition)
            {
                PopulateMenu(menu, definition);
            }
        });
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
        if (_icon is { } icon)
        {
            _icons?.Remove(icon);
            icon.Dispose();
        }
        _icon = null;
        _icons = null;
        _definition = null;
    }
}

