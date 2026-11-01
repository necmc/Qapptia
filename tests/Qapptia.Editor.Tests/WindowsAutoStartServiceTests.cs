#if WINDOWS
using System.ComponentModel;
using System.Runtime.Versioning;
using Microsoft.Win32;
using Qapptia.Core;
using Qapptia.Core.Abstractions;
using Qapptia.Platform.Windows;

namespace Qapptia.Editor.Tests;

[SupportedOSPlatform("windows")]
public sealed class WindowsAutoStartServiceTests : IDisposable
{
    private const string LauncherPath = @"D:\Aplicaciones de prueba\Qapptia\Qapptia.exe";
    private const string OtherPath = @"C:\Otra copia\Qapptia.exe";
    private readonly string _testKeyPath = @"Software\Qapptia_AutoStart_Test_" + Guid.NewGuid().ToString("N");
    private readonly RegistryKey _user;
    private readonly RegistryKey _machine;
    private readonly WindowsAutoStartService _service;

    public WindowsAutoStartServiceTests()
    {
        using var root = Registry.CurrentUser.CreateSubKey(_testKeyPath);
        _user = root.CreateSubKey("User");
        _machine = root.CreateSubKey("Machine");
        _service = new WindowsAutoStartService(_user, _machine, LauncherPath);
    }

    [Theory]
    [InlineData(false, AutoStartScope.CurrentUser)]
    [InlineData(true, AutoStartScope.AllUsers)]
    public void InstallationScopeIsDetectedWithoutStartupInArbitraryDirectory(bool global, AutoStartScope expected)
    {
        RegisterInstallation(global ? _machine : _user);
        var state = _service.GetState();
        state.Scope.Should().Be(expected);
        state.IsEnabled.Should().BeFalse();
        state.CanConfigure.Should().BeTrue();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EnablingDisablingAndReenablingAlwaysUsesInstallationScope(bool global)
    {
        var ownRoot = global ? _machine : _user;
        var otherRoot = global ? _user : _machine;
        RegisterInstallation(ownRoot);
        RegisterStartup(otherRoot, OtherPath);

        _service.SetAutoStartEnabled(true).Should().BeTrue();
        ReadStartup(ownRoot).Should().Be(Command(LauncherPath));
        _service.SetAutoStartEnabled(false).Should().BeTrue();
        ReadStartup(ownRoot).Should().BeNull();
        _service.GetState().CanConfigure.Should().BeTrue();
        _service.SetAutoStartEnabled(true).Should().BeTrue();
        ReadStartup(ownRoot).Should().Be(Command(LauncherPath));
        ReadStartup(otherRoot).Should().Be(Command(OtherPath));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PortableDoesNotAdoptAnotherInstallationOrModifyItsStartup(bool enable)
    {
        RegisterInstallation(_machine, OtherPath);
        RegisterStartup(_machine, OtherPath);
        RegisterStartup(_user, OtherPath);

        var state = _service.GetState();
        state.Scope.Should().Be(AutoStartScope.Portable);
        state.CanConfigure.Should().BeFalse();
        state.IsEnabled.Should().BeFalse();
        _service.SetAutoStartEnabled(enable).Should().BeFalse();
        (await _service.SetEnabledAsync(enable, TestContext.Current.CancellationToken)).Succeeded.Should().BeFalse();
        ReadStartup(_machine).Should().Be(Command(OtherPath));
        ReadStartup(_user).Should().Be(Command(OtherPath));
    }

    [Fact]
    public void InstallationInBothScopesReportsConflictRatherThanGuessing()
    {
        RegisterInstallation(_user);
        RegisterInstallation(_machine);
        var state = _service.GetState();
        state.Scope.Should().Be(AutoStartScope.Unknown);
        state.Error.Should().Be(AutoStartError.AmbiguousScope);
        state.CanConfigure.Should().BeFalse();
    }

    [Fact]
    public void AnotherCopyInStartupIsNotSilentlyReplacedOrDeleted()
    {
        RegisterInstallation(_user);
        RegisterStartup(_user, OtherPath);
        _service.GetState().Error.Should().Be(AutoStartError.OtherCopy);
        _service.SetAutoStartEnabled(false).Should().BeFalse();
        ReadStartup(_user).Should().Be(Command(OtherPath));
    }

    [Fact]
    public void RegistryReadFailureIsNotReportedAsPortable()
    {
        using var unavailable = Registry.CurrentUser.OpenSubKey(_testKeyPath + @"\User")!;
        unavailable.Dispose();
        var service = new WindowsAutoStartService(unavailable, _machine, LauncherPath);
        service.GetState().Scope.Should().Be(AutoStartScope.Unknown);
        service.GetState().Error.Should().Be(AutoStartError.RegistryReadFailed);
    }

    [Fact]
    public void LegacyInstallationWithMissingUninstallerFileStillIdentifiesItsScope()
    {
        using var key = _machine.CreateSubKey(AutoStartConstants.LegacyInstallRegistryKey);
        key.SetValue(AutoStartConstants.UninstallCommandRegistryValue,
            "\"" + Path.Combine(Path.GetDirectoryName(LauncherPath)!, "unins000.exe") + "\"");
        _service.GetState().Scope.Should().Be(AutoStartScope.AllUsers);
    }

    [Fact]
    public async Task ElevationCancellationPreservesDisabledMachineState()
    {
        RegisterInstallation(_machine);
        using var readOnlyMachine = Registry.CurrentUser.OpenSubKey(_testKeyPath + @"\Machine")!;
        var service = new WindowsAutoStartService(_user, readOnlyMachine, LauncherPath,
            (_, _) => throw new Win32Exception(1223));

        var result = await service.SetEnabledAsync(true, TestContext.Current.CancellationToken);
        result.Succeeded.Should().BeFalse();
        result.Cancelled.Should().BeTrue();
        result.State.IsEnabled.Should().BeFalse();
        ReadStartup(_machine).Should().BeNull();
    }

    [Fact]
    public async Task ElevatedOperationIsVerifiedByReadingMachineState()
    {
        RegisterInstallation(_machine);
        using var readOnlyMachine = Registry.CurrentUser.OpenSubKey(_testKeyPath + @"\Machine")!;
        var service = new WindowsAutoStartService(_user, readOnlyMachine, LauncherPath, (enabled, _) =>
        {
            enabled.Should().BeTrue();
            RegisterStartup(_machine, LauncherPath);
            return Task.FromResult(true);
        });
        var result = await service.SetEnabledAsync(true, TestContext.Current.CancellationToken);
        result.Succeeded.Should().BeTrue();
        result.State.Scope.Should().Be(AutoStartScope.AllUsers);
        ReadStartup(_user).Should().BeNull();
    }

    [Fact]
    public void HelperRejectsAdditionalArgumentsBeforeAccessingProductionRegistry()
    {
        WindowsAutoStartService.TryExecuteCommand([AutoStartConstants.AutoStartEnableArgument, "unexpected"], out int code).Should().BeTrue();
        code.Should().NotBe(0);
        WindowsAutoStartService.TryExecuteCommand([], out _).Should().BeFalse();
    }

    private static void RegisterInstallation(RegistryKey root, string launcher = LauncherPath)
    {
        using var key = root.CreateSubKey(AutoStartConstants.InstallRegistryKey);
        key.SetValue(AutoStartConstants.InstallLocationRegistryValue, Path.GetDirectoryName(launcher)!.ToUpperInvariant() + @"\");
    }

    private static string Command(string path) => $"\"{path}\" {Constants.ArgCapture}";
    private static void RegisterStartup(RegistryKey root, string path)
    {
        using var key = root.CreateSubKey(AutoStartConstants.RunRegistryKey);
        key.SetValue(Constants.LauncherAppName, Command(path));
    }

    private static object? ReadStartup(RegistryKey root)
    {
        using var key = root.OpenSubKey(AutoStartConstants.RunRegistryKey);
        return key?.GetValue(Constants.LauncherAppName);
    }

    public void Dispose()
    {
        _user.Dispose();
        _machine.Dispose();
        Registry.CurrentUser.DeleteSubKeyTree(_testKeyPath, throwOnMissingSubKey: false);
    }
}
#endif
