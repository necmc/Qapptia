using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Qapptia.Core.Ipc;
using Qapptia.Core.Launcher;
using Qapptia.Launcher.Views;

namespace Qapptia.Launcher;

public partial class App : Application
{
    public static LaunchPlan? Plan { get; set; }
    public static string? BaseDirectory { get; set; }
    public static IProcessLauncher? Launcher { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var splash = new SplashWindow();
            desktop.MainWindow = splash;

            var plan = Plan;
            var baseDir = BaseDirectory ?? AppContext.BaseDirectory;
            var launcher = Launcher ?? new DefaultProcessLauncher();

            if (plan != null)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await splash.TrackLaunchFlowAsync(plan, baseDir, launcher).ConfigureAwait(false);
                    }
                    catch
                    {
                        // En caso de cualquier excepción en segundo plano, garantizar el cierre del splash
                    }
                    finally
                    {
                        Dispatcher.UIThread.Post(() => splash.Close());
                    }
                });
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
