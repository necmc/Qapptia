using System.Threading.Tasks;
using Avalonia.Controls;
using Qapptia.App.Config.ViewModels;
using Qapptia.Core.Configuration;

namespace Qapptia.App.Config;

public partial class MainWindow : Window
{
    public MainWindow() : this(false, null) { }

    public MainWindow(bool startInAboutTab, JsonConfigService? configService = null)
    {
        InitializeComponent();

        var vm = new ConfigViewModel(
            configService: configService,
            startInAboutTab: startInAboutTab);
        vm.RequestClose = Close;
        vm.RequestBrowsePath = ShowFolderDialogAsync;

        DataContext = vm;
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
