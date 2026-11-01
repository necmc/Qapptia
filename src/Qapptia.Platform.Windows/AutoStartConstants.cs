namespace Qapptia.Platform.Windows;

/// <summary>Contrato nativo de instalación, autoarranque y elevación en Windows.</summary>
internal static class AutoStartConstants
{
    public const string InstallerAppId = "{8B45E1B4-3F25-4A92-A746-3C62077DA820}";
    public const string InstallRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + InstallerAppId + "_is1";
    public const string LegacyInstallRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + InstallerAppId + "}_is1";
    public const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    public const string InstallLocationRegistryValue = "InstallLocation";
    public const string UninstallCommandRegistryValue = "UninstallString";
    public const string AutoStartEnableArgument = "--set-auto-start=1";
    public const string AutoStartDisableArgument = "--set-auto-start=0";
    public const int AutoStartHelperTimeoutMs = 30000;
}
