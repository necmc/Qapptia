using Qapptia.Core.Ipc;

namespace Qapptia.Core.Launcher;

/// <summary>
/// Orquestador del despacho de inicio para la suite de aplicaciones Qapptia.
/// </summary>
public static class LauncherOrchestrator
{
    /// <summary>
    /// Interpreta los argumentos de línea de comandos y determina el plan de despacho correspondiente.
    /// </summary>
    public static LaunchPlan ResolveLaunchPlan(string[]? args)
    {
        if (args == null || args.Length == 0)
        {
            return new LaunchPlan(
                LaunchTarget.Suite,
                AdditionalArguments: null,
                ShouldEnsureCapture: true,
                ShouldEnsureEditor: true,
                ShouldEnsureConfig: false);
        }

        bool hasCapture = false;
        bool hasEditor = false;
        bool hasConfig = false;
        var otherArgs = new List<string>();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (string.Equals(arg, Constants.ArgCapture, StringComparison.OrdinalIgnoreCase))
            {
                hasCapture = true;
            }
            else if (string.Equals(arg, Constants.ArgEditor, StringComparison.OrdinalIgnoreCase))
            {
                hasEditor = true;
            }
            else if (string.Equals(arg, Constants.ArgConfig, StringComparison.OrdinalIgnoreCase))
            {
                hasConfig = true;
            }
            else
            {
                otherArgs.Add(arg);
            }
        }

        string? childArgs = otherArgs.Count > 0 ? string.Join(" ", otherArgs.Select(FormatArgument)) : null;

        if (hasConfig)
        {
            return new LaunchPlan(LaunchTarget.ConfigOnly, childArgs, false, false, true);
        }

        if (hasCapture && !hasEditor)
        {
            return new LaunchPlan(LaunchTarget.CaptureOnly, childArgs, true, false, false);
        }

        if (hasEditor && !hasCapture)
        {
            return new LaunchPlan(LaunchTarget.EditorOnly, childArgs, false, true, false);
        }

        return new LaunchPlan(LaunchTarget.Suite, childArgs, true, true, false);
    }

    /// <summary>
    /// Resuelve la ruta absoluta al ejecutable hermano, priorizando la subcarpeta interna 'app' antes del directorio base.
    /// </summary>
    public static string ResolveExecutablePath(string baseDirectory, string executableName)
    {
        string resolvedName = executableName;
        if (!OperatingSystem.IsWindows() && resolvedName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            resolvedName = resolvedName[..^4];
        }

        string appSubdir = Path.Combine(baseDirectory, "app");
        string appPath = Path.Combine(appSubdir, resolvedName);
        if (File.Exists(appPath))
        {
            return appPath;
        }

        string flatPath = Path.Combine(baseDirectory, resolvedName);
        if (File.Exists(flatPath))
        {
            return flatPath;
        }

        string alternativeName = resolvedName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? resolvedName[..^4]
            : resolvedName + ".exe";

        string altAppPath = Path.Combine(appSubdir, alternativeName);
        if (File.Exists(altAppPath))
        {
            return altAppPath;
        }

        string altFlatPath = Path.Combine(baseDirectory, alternativeName);
        if (File.Exists(altFlatPath))
        {
            return altFlatPath;
        }

        return Directory.Exists(appSubdir) ? appPath : flatPath;
    }

    /// <summary>
    /// Resuelve la ruta absoluta al ejecutable del Lanzador principal (Qapptia / Qapptia.exe).
    /// Contempla ejecuciones desde la raíz, subcarpeta interna 'app' o entorno de desarrollo.
    /// </summary>
    public static string ResolveLauncherPath(string? baseDirectory = null)
    {
        string baseDir = baseDirectory ?? AppContext.BaseDirectory;
        string exeName = OperatingSystem.IsWindows() ? Constants.LauncherExecutableName : Constants.LauncherAppName;

        // 1. Mismo directorio (despliegue plano o bin/Debug)
        string flatPath = Path.Combine(baseDir, exeName);
        if (File.Exists(flatPath))
        {
            return flatPath;
        }

        // 2. Directorio padre (si la app actual corre dentro de 'app/')
        var parent = Directory.GetParent(baseDir);
        if (parent != null)
        {
            string parentPath = Path.Combine(parent.FullName, exeName);
            if (File.Exists(parentPath))
            {
                return parentPath;
            }
        }

        // 3. Revisar ruta del proceso actual si existe
        string? currentProc = Environment.ProcessPath;
        if (!string.IsNullOrEmpty(currentProc))
        {
            string? procDir = Path.GetDirectoryName(currentProc);
            if (!string.IsNullOrEmpty(procDir))
            {
                string procFlat = Path.Combine(procDir, exeName);
                if (File.Exists(procFlat)) return procFlat;

                var procParent = Directory.GetParent(procDir);
                if (procParent != null)
                {
                    string procParentPath = Path.Combine(procParent.FullName, exeName);
                    if (File.Exists(procParentPath)) return procParentPath;
                }
            }
        }

        return flatPath;
    }

    public static bool StartSiblingProcess(
        string baseDirectory, IProcessLauncher launcher, string executableName, string? arguments)
    {
        string executablePath = ResolveExecutablePath(baseDirectory, executableName);
        string workingDirectory = Path.GetDirectoryName(executablePath) ?? baseDirectory;
        return launcher.StartProcess(executablePath, arguments, workingDirectory);
    }

    /// <summary>
    /// Activa la instancia del Editor o lo inicia si está permitido. Un clic con el Editor cerrado se considera atendido.
    /// </summary>
    public static Task<bool> ActivateEditorAsync(
        string baseDirectory, IProcessLauncher launcher, bool launchIfClosed, string? arguments = null)
    {
        if (launcher.IsInstanceActive(IpcChannels.Editor))
        {
            return launcher.SendWakeUpAsync(IpcChannels.Editor);
        }

        return Task.FromResult(!launchIfClosed || StartSiblingProcess(baseDirectory, launcher, Constants.EditorExecutableName, arguments));
    }

    /// <summary>
    /// Ejecuta el despacho del plan asegurando o despertando los procesos correspondientes.
    /// </summary>
    public static Task DispatchPlanAsync(LaunchPlan plan, string baseDirectory, IProcessLauncher launcher)
    {
        return DispatchPlanAsync(plan, baseDirectory, launcher, progress: null);
    }

    /// <summary>
    /// Ejecuta el despacho del plan asegurando o despertando los procesos correspondientes, reportando hitos de avance.
    /// </summary>
    public static async Task DispatchPlanAsync(
        LaunchPlan plan,
        string baseDirectory,
        IProcessLauncher launcher,
        IProgress<LaunchProgressReport>? progress)
    {
        progress?.Report(new LaunchProgressReport(LaunchStage.Starting, 15));
        if (plan.ShouldEnsureCapture)
        {
            progress?.Report(new LaunchProgressReport(LaunchStage.Capture, 35));
            if (!launcher.IsInstanceActive(IpcChannels.Capture))
            {
                StartSiblingProcess(baseDirectory, launcher, Constants.CaptureExecutableName, null);
            }
            else if (plan.Target == LaunchTarget.CaptureOnly)
            {
                await launcher.SendWakeUpAsync(IpcChannels.Capture).ConfigureAwait(false);
            }
        }

        if (plan.ShouldEnsureEditor)
        {
            progress?.Report(new LaunchProgressReport(LaunchStage.Editor, 60));
            await ActivateEditorAsync(baseDirectory, launcher, launchIfClosed: true, plan.AdditionalArguments).ConfigureAwait(false);
        }

        if (plan.ShouldEnsureConfig)
        {
            progress?.Report(new LaunchProgressReport(LaunchStage.Config, 80));
            if (!launcher.IsInstanceActive(IpcChannels.Config))
            {
                StartSiblingProcess(baseDirectory, launcher, Constants.ConfigExecutableName, plan.AdditionalArguments);
            }
            else
            {
                await launcher.SendWakeUpAsync(IpcChannels.Config).ConfigureAwait(false);
            }
        }

        progress?.Report(new LaunchProgressReport(LaunchStage.Ui, 90));
    }

    private static string FormatArgument(string arg)
    {
        if (string.IsNullOrEmpty(arg)) return "\"\"";
        if (arg.Contains(' ') && !arg.StartsWith('"'))
        {
            return $"\"{arg}\"";
        }
        return arg;
    }
}
