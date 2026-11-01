using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Qapptia.App.Config.ViewModels;
using Qapptia.Core.Configuration;

namespace Qapptia.App.Config;

public partial class MainWindow : Window
{
    public MainWindow() : this(false, null) { }

    public MainWindow(bool startInAboutTab, IConfigService? configService = null)
    {
        InitializeComponent();

        var services = ConfigComposition.CreateServices(configService, startInAboutTab);
        var vm = services.GetRequiredService<ConfigViewModel>();
        vm.RequestClose = Close;
        vm.RequestBrowsePath = ShowFolderDialogAsync;

        DataContext = vm;
        Closed += (_, _) => services.Dispose();
    }

    private async Task<string?> ShowFolderDialogAsync()
    {
        var topLevel = Avalonia.Controls.TopLevel.GetTopLevel(this);
        if (topLevel == null) return null;

        var result = await topLevel.StorageProvider.OpenFolderPickerAsync(new Avalonia.Platform.Storage.FolderPickerOpenOptions
        {
            Title = Common.Constants.DialogSelectFolderTitle,
            AllowMultiple = false
        });

        return result?.Count > 0 ? result[0].Path.LocalPath : null;
    }
}
