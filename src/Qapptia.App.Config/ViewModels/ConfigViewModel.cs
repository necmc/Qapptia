using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Qapptia.Core;
using Qapptia.Core.Abstractions;
using Qapptia.Core.Configuration;
using Qapptia.UI.Components.Services;
using Qapptia.UI.Components.Theme;
using ConfigConstants = Qapptia.App.Config.Common.Constants;
using ConfigPresentation = Qapptia.App.Config.Common.ConfigPresentation;

namespace Qapptia.App.Config.ViewModels;

public sealed partial class ConfigViewModel : ObservableObject, IDisposable
{
    private static readonly System.Text.CompositeFormat s_updateAvailableFormat = System.Text.CompositeFormat.Parse(ConfigConstants.UpdateStatusAvailableFormat);
    private static readonly System.Text.CompositeFormat s_updateUpToDateFormat = System.Text.CompositeFormat.Parse(ConfigConstants.UpdateStatusUpToDateFormat);

    private readonly IConfigSettingsService _settingsService;
    private readonly IConfigProcessService _processService;
    private readonly IUpdateCheckService _updateCheckService;
    private readonly IShellService _shellService;
    private readonly SponsorVisualInfo _sponsorVisual;
    private AutoStartState _autoStartState = new(AutoStartScope.Unsupported, false);
    private readonly CancellationTokenSource _lifetimeCts = new();
    private bool _disposed;

    public IReadOnlyList<string> ThemeOptions { get; } = ConfigPresentation.ThemeNames;

    [ObservableProperty]
    private int _selectedTabIndex;

    public string SponsorText => _sponsorVisual.Text;
    public string SponsorIconKey => _sponsorVisual.IconKey;
    public string SponsorStyleKey => _sponsorVisual.StyleKey;
    public bool IsSponsorStyleA => _sponsorVisual.StyleKey == SponsorVisualHelper.StyleA;
    public bool IsSponsorStyleB => _sponsorVisual.StyleKey == SponsorVisualHelper.StyleB;
    public bool IsSponsorStyleC => _sponsorVisual.StyleKey == SponsorVisualHelper.StyleC;
    public bool IsSponsorStyleD => _sponsorVisual.StyleKey == SponsorVisualHelper.StyleD;
    public IImage? SponsorIcon => Application.Current?.TryGetResource(SponsorIconKey, null, out var res) == true ? res as IImage : null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSponsor))]
    private bool _isSponsorCooldown;

    public bool CanSponsor => !IsSponsorCooldown;


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanExecuteUpdateAction))]
    private bool _isCheckingUpdate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanExecuteUpdateAction))]
    private bool _isUpdateCooldown;

    public bool CanExecuteUpdateAction => !IsCheckingUpdate && !IsUpdateCooldown;

    [ObservableProperty]
    private string _updateStatusMessage = ConfigConstants.UpdateStatusPrompt;

    [ObservableProperty]
    private bool _isUpdateAvailable;

    [ObservableProperty]
    private string _updateButtonText = ConfigConstants.UpdateButtonCheck;

    [ObservableProperty]
    private string? _downloadUrl;

    [ObservableProperty]
    private bool _autoStart;

    public bool IsAutoStartSupported => _settingsService.IsAutoStartSupported;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEditAutoStart))]
    [NotifyPropertyChangedFor(nameof(AutoStartStatusMessage))]
    private bool _isLoadingAutoStart;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEditAutoStart))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _isSaving;

    public bool CanEditAutoStart => _autoStartState.CanConfigure && !IsLoadingAutoStart && !IsSaving;
    public string AutoStartStatusMessage => IsLoadingAutoStart ? ConfigConstants.AutoStartLoading : ConfigPresentation.GetAutoStartStatusMessage(_autoStartState);

    private bool CanSave => !IsSaving;

    [ObservableProperty]
    private string _selectedTheme = ConfigConstants.ThemeSystem;

    [ObservableProperty]
    private string _savePath = string.Empty;

    [ObservableProperty]
    private string _filenameFormat = string.Empty;

    [ObservableProperty]
    private bool _subfolderMonth;

    [ObservableProperty]
    private bool _subfolderDay;

    [ObservableProperty]
    private bool _subfolderHour;

    [ObservableProperty]
    private bool _showMouse;

    [ObservableProperty]
    private bool _highlightMouse;

    [ObservableProperty]
    private int _manualTimer;

    [ObservableProperty]
    private string _shortcutScreen = string.Empty;

    [ObservableProperty]
    private string _shortcutArea = string.Empty;

    [ObservableProperty]
    private bool _copyToClipboardScreen;

    [ObservableProperty]
    private bool _copyToClipboardArea;

    [ObservableProperty]
    private bool _captureAllScreens;

    [ObservableProperty]
    private bool _showCaptureIndicator;

    [ObservableProperty]
    private string _footerMessage = string.Empty;

    [ObservableProperty]
    private bool _isFooterError;

    public Action? RequestClose { get; set; }
    public Func<Task<string?>>? RequestBrowsePath { get; set; }

    public ConfigViewModel(
        IConfigSettingsService settingsService,
        IUpdateCheckService updateCheckService,
        IShellService shellService,
        IConfigProcessService processService,
        bool startInAboutTab = false)
    {
        _settingsService = settingsService;
        _updateCheckService = updateCheckService;
        _shellService = shellService;
        _processService = processService;
        _sponsorVisual = SponsorVisualHelper.GetRandomCombination();
        _selectedTabIndex = startInAboutTab ? 3 : 0;
        LoadFromConfig();
    }

    private void LoadFromConfig()
    {
        var config = _settingsService.Current;
        AutoStart = false;
        SelectedTheme = ConfigPresentation.ToThemeName(config.Theme);
        SavePath = config.SavePath;
        FilenameFormat = config.FilenameFormat;
        SubfolderMonth = config.SubfolderMonth;
        SubfolderDay = config.SubfolderDay;
        SubfolderHour = config.SubfolderHour;
        ShowMouse = config.ShowMouse;
        HighlightMouse = config.HighlightMouse;
        ManualTimer = config.ManualTimer;
        ShortcutScreen = config.ShortcutScreen;
        ShortcutArea = config.ShortcutArea;
        CopyToClipboardScreen = config.CopyToClipboardScreen;
        CopyToClipboardArea = config.CopyToClipboardArea;
        CaptureAllScreens = config.CaptureAllScreens;
        ShowCaptureIndicator = config.ShowCaptureIndicator;

        LoadAutoStartFromSystem();
    }

    private void LoadAutoStartFromSystem()
    {
        if (!_settingsService.IsAutoStartSupported) return;

        IsLoadingAutoStart = true;
        _ = LoadAutoStartAsync();
    }

    private async Task LoadAutoStartAsync()
    {
        try
        {
            var state = await _settingsService.GetAutoStartStateAsync(_lifetimeCts.Token);
            if (_disposed) return;
            _autoStartState = state;
            AutoStart = state.IsEnabled;
            IsLoadingAutoStart = false;
            if (state.HasError) ShowFooter(ConfigConstants.FooterAutoStartQueryError, isError: true);
        }
        catch (OperationCanceledException) when (_disposed) { }
    }

    [RelayCommand]
    private async Task BrowsePathAsync()
    {
        if (RequestBrowsePath != null)
        {
            var result = await RequestBrowsePath();
            if (!string.IsNullOrWhiteSpace(result))
            {
                SavePath = result;
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        bool requestedAutoStart = AutoStart;
        IsSaving = true;
        FooterMessage = ConfigConstants.FooterConfigSaving;
        IsFooterError = false;
        try
        {
            var changes = new ConfigChanges(
                ConfigPresentation.FromThemeName(SelectedTheme), SavePath, FilenameFormat,
                SubfolderMonth, SubfolderDay, SubfolderHour, ShowMouse, HighlightMouse, ManualTimer,
                ShortcutScreen, ShortcutArea, CopyToClipboardScreen, CopyToClipboardArea, CaptureAllScreens)
            {
                ShowCaptureIndicator = ShowCaptureIndicator
            };
            var saved = _settingsService.SavePreferences(changes, IsLoadingAutoStart ? null : _autoStartState, requestedAutoStart);
            if (!saved.IsValid)
            {
                ShowFooter(ConfigConstants.FooterInvalidSavePath, isError: true);
                return;
            }

            // Aplicar el nuevo tema a la propia ventana de Configuración al guardar.
            ThemeManager.ApplyTheme(saved.Theme);
            if (saved.ChangeAutoStart)
            {
                var result = await _settingsService.SetAutoStartAsync(requestedAutoStart, _lifetimeCts.Token);
                if (_disposed) return;
                _autoStartState = result.State;
                AutoStart = result.State.IsEnabled;
                OnPropertyChanged(nameof(CanEditAutoStart));
                OnPropertyChanged(nameof(AutoStartStatusMessage));
                if (!result.Succeeded)
                {
                    ShowFooter(result.Cancelled ? ConfigConstants.FooterAutoStartCancelled : ConfigConstants.FooterAutoStartSaveError, isError: true);
                    return;
                }
            }
            ShowFooter(ConfigConstants.FooterConfigSaved, isError: false);
        }
        catch (Exception ex)
        {
            ShowFooter($"{ConfigConstants.FooterSaveErrorPrefix}{ex.Message}", isError: true);
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void Close()
    {
        RequestClose?.Invoke();
    }

    private void ShowFooter(string message, bool isError)
    {
        FooterMessage = message;
        IsFooterError = isError;

        if (!isError)
        {
            Task.Delay(ConfigConstants.FooterMessageDurationMs).ContinueWith(_ =>
            {
                if (FooterMessage == message) FooterMessage = string.Empty;
            });
        }
    }

    [RelayCommand]
    private void Sponsor()
    {
        if (!CanSponsor) return;

        IsSponsorCooldown = true;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(Constants.SponsorCooldownMs).ConfigureAwait(false);
            }
            catch
            {
                // Ignore task failures
            }
            finally
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    IsSponsorCooldown = false;
                });
            }
        });

        try
        {
            _shellService.OpenUrl(Constants.DefaultSponsorUrl);
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Error al abrir enlace de apoyo.");
        }
    }


    [RelayCommand]
    private async Task CheckUpdatesAsync()
    {
        if (IsCheckingUpdate) return;

        IsCheckingUpdate = true;
        UpdateStatusMessage = ConfigConstants.UpdateStatusChecking;

        try
        {
            var result = await _updateCheckService.CheckForUpdatesAsync().ConfigureAwait(false);

            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                if (result.Status == UpdateCheckStatus.UpdateAvailable && result.LatestRelease != null)
                {
                    IsUpdateAvailable = true;
                    DownloadUrl = !string.IsNullOrWhiteSpace(result.LatestRelease.SiteDownload)
                        ? result.LatestRelease.SiteDownload
                        : Constants.DefaultDownloadUrl;
                    UpdateStatusMessage = string.Format(System.Globalization.CultureInfo.InvariantCulture, s_updateAvailableFormat, result.LatestRelease.Version);
                    UpdateButtonText = ConfigConstants.UpdateButtonDownload;

                    _processService.PublishUpdate(result.LatestRelease);
                }
                else if (result.Status == UpdateCheckStatus.UpToDate)
                {
                    IsUpdateAvailable = false;
                    DownloadUrl = null;
                    UpdateStatusMessage = string.Format(System.Globalization.CultureInfo.InvariantCulture, s_updateUpToDateFormat, Constants.AppVersion);
                    UpdateButtonText = ConfigConstants.UpdateButtonCheck;

                    _processService.PublishUpdate(null);
                }
                else
                {
                    UpdateStatusMessage = ConfigPresentation.GetUpdateErrorMessage(result);
                    UpdateButtonText = ConfigConstants.UpdateButtonRetry;
                }
            });
        }
        catch (Exception ex)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                UpdateStatusMessage = ConfigConstants.UpdateStatusServerError;
                UpdateButtonText = ConfigConstants.UpdateButtonRetry;
            });
            Serilog.Log.Error(ex, "Error comprobando actualizaciones desde Config");
        }
        finally
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                IsCheckingUpdate = false;
                // Si no hay actualización disponible (al día o error), activamos cooldown
                // para evitar clics compulsivos hacia el servidor de comprobación.
                // Si sí hay actualización disponible, el botón queda inmediatamente activo.
                if (!IsUpdateAvailable)
                {
                    StartUpdateCooldown();
                }
            });
        }
    }

    private void StartUpdateCooldown()
    {
        IsUpdateCooldown = true;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(Constants.UpdateTimingManualCooldownMs).ConfigureAwait(false);
            }
            catch
            {
                // Ignore task failures
            }
            finally
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    IsUpdateCooldown = false;
                });
            }
        });
    }

    [RelayCommand]
    private void DownloadUpdate()
    {
        if (!CanExecuteUpdateAction) return;

        // Evita aperturas compulsivas de pestañas en el navegador
        StartUpdateCooldown();

        try
        {
            var targetUrl = !string.IsNullOrWhiteSpace(DownloadUrl) ? DownloadUrl : Constants.DefaultDownloadUrl;
            _shellService.OpenUrl(targetUrl);
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Error al abrir enlace de descarga de actualización.");
        }
    }

    [RelayCommand]
    private async Task UpdateActionAsync()
    {
        if (!CanExecuteUpdateAction) return;

        if (IsUpdateAvailable)
        {
            DownloadUpdate();
        }
        else
        {
            await CheckUpdatesAsync();
        }
    }

    [RelayCommand]
    private async Task OpenEditorHelpAsync()
    {
        try
        {
            bool opened = await _processService.OpenEditorHelpAsync(_lifetimeCts.Token);
            if (!_disposed && !opened) ShowFooter(ConfigConstants.FooterEditorNotFound, isError: true);
        }
        catch (OperationCanceledException) when (_disposed) { }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Error al lanzar el Editor con argumento de ayuda desde Configuración.");
            if (!_disposed) ShowFooter(ConfigConstants.FooterHelpError, isError: true);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _lifetimeCts.Cancel();
        _lifetimeCts.Dispose();
        if (_updateCheckService is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
