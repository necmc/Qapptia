using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Principal;
using Microsoft.Win32;
using Qapptia.Core;
using Qapptia.Core.Abstractions;
using Qapptia.Core.Launcher;
using Serilog;

namespace Qapptia.Platform.Windows;

/// <summary>
/// Identifica la instalación por su ruta y administra únicamente el autoarranque de su ámbito.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsAutoStartService : IAutoStartService
{
    private readonly ILogger? _logger;
    private readonly RegistryKey? _currentUser;
    private readonly RegistryKey? _localMachine;
    private readonly string _launcherPath;
    private readonly Func<bool, CancellationToken, Task<bool>> _elevate;

    public WindowsAutoStartService(ILogger? logger = null)
    {
        _logger = logger?.ForContext<WindowsAutoStartService>();
        _launcherPath = LauncherOrchestrator.ResolveLauncherPath();
        _elevate = RunElevatedAsync;
    }

    internal WindowsAutoStartService(RegistryKey currentUser, RegistryKey localMachine, string launcherPath,
        Func<bool, CancellationToken, Task<bool>>? elevate = null)
    {
        _currentUser = currentUser;
        _localMachine = localMachine;
        _launcherPath = launcherPath;
        _elevate = elevate ?? RunElevatedAsync;
    }

    private RegistryKey CurrentUser => _currentUser ?? Registry.CurrentUser;
    private RegistryKey LocalMachine => _localMachine ?? Registry.LocalMachine;
    public bool IsSupported => OperatingSystem.IsWindows();

    public AutoStartState GetState()
    {
        if (!IsSupported) return new(AutoStartScope.Unsupported, false);
        try
        {
            string directory = Path.GetDirectoryName(Path.GetFullPath(_launcherPath))!;
            bool userMatches = MatchesInstallation(CurrentUser, directory);
            bool machineMatches = MatchesInstallation(LocalMachine, directory);
            if (userMatches && machineMatches)
                return new(AutoStartScope.Unknown, false, AutoStartError.AmbiguousScope);
            if (!userMatches && !machineMatches) return new(AutoStartScope.Portable, false);

            var scope = userMatches ? AutoStartScope.CurrentUser : AutoStartScope.AllUsers;
            using var key = (userMatches ? CurrentUser : LocalMachine).OpenSubKey(AutoStartConstants.RunRegistryKey);
            string? command = key?.GetValue(Constants.LauncherAppName) as string;
            if (string.IsNullOrWhiteSpace(command)) return new(scope, false);
            if (!SamePath(ExtractExecutable(command), _launcherPath))
                return new(scope, false, AutoStartError.OtherCopy);
            return new(scope, true);
        }
        catch (Exception ex)
        {
            _logger?.Warning(ex, "Error al consultar el ámbito y el autoarranque de Qapptia.");
            return new(AutoStartScope.Unknown, false, AutoStartError.RegistryReadFailed);
        }
    }

    public bool IsAutoStartEnabled() => GetState().IsEnabled;

    public bool SetAutoStartEnabled(bool enabled) => SetEnabledCore(enabled, out _);

    private bool SetEnabledCore(bool enabled, out bool requiresElevation)
    {
        requiresElevation = false;
        var state = GetState();
        if (!state.CanConfigure) return false;
        try
        {
            var root = state.Scope == AutoStartScope.CurrentUser ? CurrentUser : LocalMachine;
            if (enabled)
            {
                using var key = root.CreateSubKey(AutoStartConstants.RunRegistryKey);
                if (key == null) return false;
                key.SetValue(Constants.LauncherAppName, $"\"{_launcherPath}\" {Constants.ArgCapture}");
            }
            else
            {
                using var key = root.OpenSubKey(AutoStartConstants.RunRegistryKey, writable: true);
                key?.DeleteValue(Constants.LauncherAppName, throwOnMissingValue: false);
            }
            var updated = GetState();
            return updated.CanConfigure && updated.Scope == state.Scope && updated.IsEnabled == enabled;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or SecurityException)
        {
            requiresElevation = state.Scope == AutoStartScope.AllUsers;
            _logger?.Warning(ex, "No hay permisos para modificar el autoarranque de ámbito {Scope}.", state.Scope);
            return false;
        }
        catch (Exception ex)
        {
            _logger?.Warning(ex, "No se pudo modificar el autoarranque de ámbito {Scope}.", state.Scope);
            return false;
        }
    }

    public async Task<AutoStartChangeResult> SetEnabledAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        try
        {
            var state = await Task.Run(GetState, cancellationToken).ConfigureAwait(false);
            if (!state.CanConfigure) return new(false, false, state);
            if (state.IsEnabled == enabled) return new(true, false, state);
            var attempt = await Task.Run(() =>
            {
                bool saved = SetEnabledCore(enabled, out bool needsElevation);
                return (Saved: saved, NeedsElevation: needsElevation);
            }, cancellationToken).ConfigureAwait(false);
            bool succeeded = attempt.Saved;
            if (!succeeded && attempt.NeedsElevation)
                succeeded = await _elevate(enabled, cancellationToken).ConfigureAwait(false);
            var updated = await Task.Run(GetState, cancellationToken).ConfigureAwait(false);
            return new(succeeded && updated.CanConfigure && updated.Scope == state.Scope && updated.IsEnabled == enabled, false, updated);
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            return new(false, true, await Task.Run(GetState).ConfigureAwait(false));
        }
        catch (Exception ex) when (ex is TimeoutException or OperationCanceledException or Win32Exception)
        {
            _logger?.Warning(ex, "La operación de autoarranque no pudo completarse.");
            return new(false, ex is OperationCanceledException, await Task.Run(GetState).ConfigureAwait(false));
        }
    }

    private static bool MatchesInstallation(RegistryKey root, string directory)
    {
        foreach (string keyName in new[] { AutoStartConstants.InstallRegistryKey, AutoStartConstants.LegacyInstallRegistryKey })
        {
            using var key = root.OpenSubKey(keyName);
            if (key == null) continue;
            string? location = key.GetValue(AutoStartConstants.InstallLocationRegistryValue) as string;
            if (string.IsNullOrWhiteSpace(location))
            {
                string? uninstall = key.GetValue(AutoStartConstants.UninstallCommandRegistryValue) as string;
                if (!string.IsNullOrWhiteSpace(uninstall)) location = Path.GetDirectoryName(ExtractExecutable(uninstall));
            }
            if (!string.IsNullOrWhiteSpace(location) && SamePath(location, directory)) return true;
        }
        return false;
    }

    private static string ExtractExecutable(string command)
    {
        string text = Environment.ExpandEnvironmentVariables(command).Trim();
        if (text.StartsWith('"'))
        {
            int end = text.IndexOf('"', 1);
            return end > 1 ? text[1..end] : string.Empty;
        }
        int space = text.IndexOf(' ');
        return space < 0 ? text : text[..space];
    }

    private static bool SamePath(string left, string right) =>
        !string.IsNullOrWhiteSpace(left) && string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(Environment.ExpandEnvironmentVariables(left.Trim('"')))),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(Environment.ExpandEnvironmentVariables(right.Trim('"')))),
            StringComparison.OrdinalIgnoreCase);

    private static async Task<bool> RunElevatedAsync(bool enabled, CancellationToken cancellationToken)
    {
        // El consentimiento UAC y la espera del proceso no se ejecutan en el hilo de interfaz.
        using var process = await Task.Run(() => Process.Start(new ProcessStartInfo(
            Path.Combine(AppContext.BaseDirectory, Constants.ConfigExecutableName),
            enabled ? AutoStartConstants.AutoStartEnableArgument : AutoStartConstants.AutoStartDisableArgument)
        {
            UseShellExecute = true,
            Verb = "runas",
            WorkingDirectory = AppContext.BaseDirectory
        }), cancellationToken).ConfigureAwait(false);
        if (process == null) return false;
        await process.WaitForExitAsync(cancellationToken)
            .WaitAsync(TimeSpan.FromMilliseconds(AutoStartConstants.AutoStartHelperTimeoutMs), cancellationToken).ConfigureAwait(false);
        return process.ExitCode == 0;
    }

    public static bool TryExecuteCommand(string[] args, out int exitCode)
    {
        exitCode = 1;
        if (!args.Any(a => a == AutoStartConstants.AutoStartEnableArgument || a == AutoStartConstants.AutoStartDisableArgument)) return false;
        if (args.Length != 1 || !OperatingSystem.IsWindows()) return true;
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            if (!new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator)) return true;
            var service = new WindowsAutoStartService();
            if (service.GetState().Scope != AutoStartScope.AllUsers) return true;
            exitCode = service.SetAutoStartEnabled(args[0] == AutoStartConstants.AutoStartEnableArgument) ? 0 : 1;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "No se pudo ejecutar la operación elevada de autoarranque.");
        }
        return true;
    }
}
