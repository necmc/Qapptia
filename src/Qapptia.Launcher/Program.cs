using System;
using System.IO;
using Avalonia;
using Qapptia.Core;
using Qapptia.Core.Ipc;
using Qapptia.Core.Launcher;

namespace Qapptia.Launcher;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            var baseDir = AppContext.BaseDirectory;
            var plan = LauncherOrchestrator.ResolveLaunchPlan(args);
            var launcher = new DefaultProcessLauncher();

            bool isEditorActive = launcher.IsInstanceActive(IpcChannels.Editor);
            bool shouldShowSplash = (plan.Target == LaunchTarget.Suite || plan.Target == LaunchTarget.EditorOnly) &&
                                    !isEditorActive;

            if (shouldShowSplash)
            {
                App.Plan = plan;
                App.BaseDirectory = baseDir;
                App.Launcher = launcher;

                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            else
            {
                LauncherOrchestrator.DispatchPlanAsync(plan, baseDir, launcher).GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            try
            {
                var logDir = Constants.DefaultLogDirectory;
                Directory.CreateDirectory(logDir);
                File.AppendAllText(Path.Combine(logDir, Common.Constants.LogFileName), $"[{DateTime.UtcNow:O}] {ex}\n");
            }
            catch
            {
                // Failsafe silencioso si el entorno restringe el almacenamiento local
            }
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont();
}
