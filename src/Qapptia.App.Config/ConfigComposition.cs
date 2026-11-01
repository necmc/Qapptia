using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Qapptia.App.Config.ViewModels;
using Qapptia.Core;
using Qapptia.Core.Abstractions;
using Qapptia.Core.Configuration;
using Qapptia.Core.Services;
#if WINDOWS
using Qapptia.Platform.Windows;
#elif LINUX
using Qapptia.Platform.Linux;
#elif MAC
using Qapptia.Platform.MacOS;
#endif

namespace Qapptia.App.Config;

/// <summary>Construye los servicios concretos en el punto de entrada de la aplicación.</summary>
internal static class ConfigComposition
{
    public static ServiceProvider CreateServices(IConfigService? configService, bool startInAboutTab)
    {
        var services = new ServiceCollection();
        services.AddSingleton<Serilog.ILogger>(Serilog.Log.Logger);
        services.AddSingleton<IConfigService>(configService ?? new JsonConfigService(Constants.DefaultConfigPath));
#if WINDOWS
        // NT 6.1 corresponde a Windows 7.
        if (OperatingSystem.IsWindowsVersionAtLeast(6, 1)) services.AddWindowsPlatform();
#elif LINUX
        if (OperatingSystem.IsLinux()) services.AddLinuxPlatform();
#elif MAC
        if (OperatingSystem.IsMacOS()) services.AddMacOSPlatform();
#endif
        services.TryAddSingleton<IShellService>(NullShellService.Instance);
        services.TryAddSingleton<IAutoStartService>(NullAutoStartService.Instance);
        services.AddSingleton<IConfigProcessService, ConfigProcessService>();
        services.AddSingleton<IConfigSettingsService, ConfigSettingsService>();
        services.AddSingleton<ConfigViewModel>(provider => new ConfigViewModel(
            provider.GetRequiredService<IConfigSettingsService>(),
            new HttpUpdateCheckService(),
            provider.GetRequiredService<IShellService>(),
            provider.GetRequiredService<IConfigProcessService>(),
            startInAboutTab));
        return services.BuildServiceProvider();
    }
}
