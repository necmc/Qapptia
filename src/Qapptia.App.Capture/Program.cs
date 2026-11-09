using Qapptia.UI.Components.Services;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Qapptia.Capture;
using Qapptia.Core.Abstractions;
using Qapptia.Core.Configuration;
using Qapptia.Core.Ipc;
using Qapptia.Core.Launcher;
using Qapptia.Core.Logging;
using Qapptia.Core.Platform;
using Qapptia.Core.Services;
using Serilog;
using Serilog.Events;
using CaptureConstants = Qapptia.App.Capture.Common.Constants;
using Qapptia.UI.Components;
#if WINDOWS
using Qapptia.Platform.Windows;
#elif LINUX
using Qapptia.Platform.Linux;
#elif MAC
using Qapptia.Platform.MacOS;
#endif

namespace Qapptia.App.Capture;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        var logDir = Qapptia.Core.Constants.DefaultLogDirectory;
#if DEBUG
        var logLevel = LogEventLevel.Debug;
#else
        var logLevel = LogEventLevel.Information;
#endif
        using var _log = LoggingBootstrap.ConfigureGlobal(logDir, logLevel, "capture");

        try
        {
            using var guard = new MutexSingleInstanceGuard(IpcChannels.Capture);
            if (!guard.Acquire())
            {
                Log.Warning("Otra instancia de App.Capture ya está en ejecución.");
                try
                {
                    QapptiaIpcClient.SendAsync(IpcChannels.Capture, new WakeUpRequest(), timeoutMs: 1000).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "No se pudo entregar WakeUpRequest a la instancia de captura existente.");
                }
                return;
            }

            Log.Information("--------------------------------------------------------------------------------");
            Log.Information(">>> INICIO DE INSTANCIA: App.Capture (PID: {ProcessId}, Versión: {Version}) <<<", Environment.ProcessId, Qapptia.Core.Constants.AppVersion);
            using var host = BuildHost(args);
            var appLogger = host.Services.GetRequiredService<Serilog.ILogger>();

            var lifetime = new ClassicDesktopStyleApplicationLifetime
            {
                Args = args,
                ShutdownMode = ShutdownMode.OnExplicitShutdown,
            };
            AppBuilder.Configure<HeadlessCaptureApp>()
                .UsePlatformDetect()
                .WithHardwareAcceleration(enabled: false)
                .AfterSetup(b =>
                {
                    if (b.Instance is HeadlessCaptureApp app)
                    {
                        app.AppHost = host;
                    }
                })
                .SetupWithLifetime(lifetime);

            var hostTask = Task.Run(() => host.StartAsync());
            if (hostTask.IsFaulted) hostTask.GetAwaiter().GetResult();

            lifetime.Start(Array.Empty<string>());
            try
            {
                using var stopCts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                host.StopAsync(stopCts.Token).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                appLogger.Error(ex, "Error deteniendo host");
            }
            finally
            {
                Environment.Exit(0);
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fallo fatal en App.Capture durante el ciclo de vida o arranque");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static IHost BuildHost(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Logging.ClearProviders();
        builder.Services.AddSingleton<Serilog.ILogger>(Log.Logger);
        builder.Logging.AddSerilog(Log.Logger, dispose: false);

        var configPath = Qapptia.Core.Constants.DefaultConfigPath;
        builder.Services.AddSingleton<IConfigService>(_ => new JsonConfigService(configPath));
        builder.Services.AddSingleton<IProcessLauncher, DefaultProcessLauncher>();

#if WINDOWS
        // NT 6.1 corresponde a Windows 7
        if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
        {
            builder.Services.AddWindowsPlatform();
            builder.Services.AddSingleton<ICaptureVisualFeedback, AvaloniaCaptureVisualFeedback>();
        }
#elif LINUX
        if (OperatingSystem.IsLinux()) builder.Services.AddLinuxPlatform();
#elif MAC
        if (OperatingSystem.IsMacOS()) builder.Services.AddMacOSPlatform();
#endif

        builder.Services.AddSingleton<IFullscreenCaptureService, FullscreenCaptureService>();
        builder.Services.AddSingleton<IAreaCaptureService, AvaloniaAreaCaptureService>();

        builder.Services.AddSingleton<ICaptureActionHandler, CaptureWorker>();
        builder.Services.AddHostedService(sp => (CaptureWorker)sp.GetRequiredService<ICaptureActionHandler>());
        builder.Services.AddHostedService<IpcServerHostedService>();

        builder.Services.AddSingleton<IUpdateCheckService, HttpUpdateCheckService>();
        builder.Services.AddHostedService<UpdateCheckBackgroundService>();

        return builder.Build();
    }
}

internal sealed class AppLoggerMarker { }

internal sealed class HeadlessCaptureApp : Application
{
    private static readonly System.Text.CompositeFormat s_trayDownloadFormat = System.Text.CompositeFormat.Parse(Qapptia.UI.Components.Common.Constants.TrayMenuDownloadUpdateFormat);

    public IHost AppHost { get; set; } = null!;

    public override void Initialize()
    {
        // No cargamos FluentTheme porque esta app es headless (solo usa TrayIcon nativo)
        // Esto reduce drásticamente el tiempo de inicio y la memoria.
    }

    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && AppHost?.Services is { } services)
        {
            var logger = services.GetService<Serilog.ILogger>();
            var captureHandler = services.GetService<ICaptureActionHandler>();
            var trayService = services.GetService<ITrayIconService>();
            if (services.GetService<ICaptureVisualFeedback>() is IDisposable indicator)
                desktop.Exit += (_, _) => indicator.Dispose();
            var shellService = services.GetService<IShellService>();
            var launcher = services.GetRequiredService<IProcessLauncher>();

            var menuDef = new TrayMenuDefinition
            {
                SingleClickAction = () => ActivateEditorAsync(launcher, launchIfClosed: false),
                DoubleClickAction = () => ActivateEditorAsync(launcher, launchIfClosed: true)
            };
            var config = services.GetService<IConfigService>();

            menuDef.Items.Add(new TrayMenuActionItem(CaptureConstants.TrayMenuCaptureScreen, () => captureHandler?.HandleFullscreenCaptureAsync(CancellationToken.None), shortcutTextProvider: () => config?.Current.ShortcutScreen));
            menuDef.Items.Add(new TrayMenuActionItem(CaptureConstants.TrayMenuCaptureArea, () => captureHandler?.HandleAreaCaptureAsync(CancellationToken.None), shortcutTextProvider: () => config?.Current.ShortcutArea));
            menuDef.Items.Add(new TrayMenuSeparatorItem());
            menuDef.Items.Add(new TrayMenuActionItem(CaptureConstants.TrayMenuEditor, () => _ = ActivateEditorAsync(launcher, launchIfClosed: true)));
            menuDef.Items.Add(new TrayMenuActionItem(CaptureConstants.TrayMenuConfig, () => LaunchApp(launcher, Qapptia.Core.Constants.ConfigExecutableName, Qapptia.Core.Constants.ArgConfig)));
            menuDef.Items.Add(new TrayMenuSeparatorItem());
            long lastSponsorTicks = 0;
            menuDef.Items.Add(new TrayMenuActionItem(Qapptia.UI.Components.Common.Constants.SponsorDefaultActionTitle, () =>
            {
                var now = Environment.TickCount64;
                if (now - lastSponsorTicks < Qapptia.Core.Constants.SponsorCooldownMs) return;
                lastSponsorTicks = now;
                shellService?.OpenUrl(Qapptia.Core.Constants.DefaultSponsorUrl);
            }, textProvider: () => SponsorVisualHelper.GetRandomCombination().Text));
            long lastUpdateTicks = 0;
            menuDef.Items.Add(new TrayMenuActionItem(
                CaptureConstants.TrayMenuAbout,
                () =>
                {
                    if (Qapptia.Core.Services.InMemoryUpdateStateStore.HasUpdate)
                    {
                        var now = Environment.TickCount64;
                        if (now - lastUpdateTicks < Qapptia.Core.Constants.UpdateDownloadCooldownMs) return;
                        lastUpdateTicks = now;
                        var state = Qapptia.Core.Services.InMemoryUpdateStateStore.Current;
                        var url = !string.IsNullOrWhiteSpace(state?.DownloadUrl) ? state.DownloadUrl : Qapptia.Core.Constants.DefaultDownloadUrl;
                        shellService?.OpenUrl(url);
                    }
                    else
                    {
                        LaunchApp(launcher, Qapptia.Core.Constants.ConfigExecutableName, Qapptia.Core.Constants.ArgAbout);
                    }
                },
                textProvider: () =>
                {
                    var state = Qapptia.Core.Services.InMemoryUpdateStateStore.Current;
                    return (Qapptia.Core.Services.InMemoryUpdateStateStore.HasUpdate && state != null)
                        ? string.Format(System.Globalization.CultureInfo.InvariantCulture, s_trayDownloadFormat, state.Version)
                        : CaptureConstants.TrayMenuAbout;
                }));
            menuDef.Items.Add(new TrayMenuSeparatorItem());
            menuDef.Items.Add(new TrayMenuActionItem(CaptureConstants.TrayMenuRestart, () =>
            {
                var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrEmpty(exePath))
                {
                    Process.Start(new ProcessStartInfo(exePath, Qapptia.Core.Constants.ArgRestart) { UseShellExecute = true });
                }
                try { trayService?.Dispose(); } catch { }
                desktop.Shutdown();
            }, isVisibleProvider: () => false));
            menuDef.Items.Add(new TrayMenuActionItem(CaptureConstants.TrayMenuExit, () =>
            {
                try { trayService?.Dispose(); } catch { }
                desktop.Shutdown();
            }));

            var iconPath = Path.Combine(AppContext.BaseDirectory, Qapptia.Core.Constants.AssetsDirectoryName, Qapptia.Core.Constants.TrayIconFileName);
            trayService?.Initialize(menuDef, iconPath);

            var isRestart = desktop.Args?.Any(a => string.Equals(a, Qapptia.Core.Constants.ArgRestart, StringComparison.OrdinalIgnoreCase)) ?? false;
            var notificationTitle = Qapptia.UI.Components.Common.Constants.NotificationTitleCapture;
            var notificationMessage = isRestart
                ? Qapptia.UI.Components.Common.Constants.NotificationMessageCaptureRestarted
                : Qapptia.UI.Components.Common.Constants.NotificationMessageCaptureStarted;

            trayService?.ShowNotification(
                notificationTitle,
                notificationMessage,
                TrayNotificationType.Info,
                Qapptia.Core.Constants.NotificationDurationMs);

            logger?.Information("TrayIcon asignado a la aplicación de forma limpia.");
        }
    }

    private static async Task ActivateEditorAsync(IProcessLauncher launcher, bool launchIfClosed)
    {
        try
        {
            bool handled = await LauncherOrchestrator.ActivateEditorAsync(
                AppContext.BaseDirectory, launcher, launchIfClosed, Qapptia.Core.Constants.ArgEditor).ConfigureAwait(false);
            if (!handled)
            {
                Log.Warning("No se pudo solicitar la activación del Editor desde la bandeja.");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error al abrir el Editor desde la bandeja.");
        }
    }

    private static void LaunchApp(IProcessLauncher launcher, string exeName, string arguments)
    {
        try
        {
            if (!LauncherOrchestrator.StartSiblingProcess(AppContext.BaseDirectory, launcher, exeName, arguments))
            {
                Log.Error("No se pudo iniciar la aplicación {ExecutableName}.", exeName);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error al lanzar aplicación");
        }
    }
}
