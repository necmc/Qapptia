using System.Text.Json;
using Qapptia.Core.Abstractions;
using Qapptia.Core.Configuration;
using Qapptia.Core.Serialization;
using Qapptia.Core.Theme;

namespace Qapptia.Core.Tests;

public sealed class ConfigSettingsServiceTests
{
    [Fact]
    public void SavePreservesUneditedPreferencesAndAppliesCaptureRules()
    {
        var config = new MemoryConfigService();
        config.Current.ShowEditorHelpOnStartup = false;
        config.Current.ShortcutFlow = "alt+f";
        var processes = new RecordingProcesses();
        var service = new ConfigSettingsService(config, new FakeAutoStart(), processes);

        var saved = service.SavePreferences(Changes() with
        {
            ShowMouse = false, HighlightMouse = true, FilenameFormat = " ", ShortcutScreen = "", ShortcutArea = " ",
            ShowCaptureIndicator = false
        }, new(AutoStartScope.CurrentUser, true), true);

        Assert.True(saved.IsValid);
        Assert.False(saved.ChangeAutoStart);
        Assert.False(config.Current.HighlightMouse);
        Assert.True(config.Current.AutoStart);
        Assert.Equal(Constants.DefaultFilenameFormat, config.Current.FilenameFormat);
        Assert.Equal(Constants.DefaultShortcutScreen, config.Current.ShortcutScreen);
        Assert.Equal(Constants.DefaultShortcutArea, config.Current.ShortcutArea);
        Assert.False(config.Current.ShowEditorHelpOnStartup);
        Assert.False(config.Current.ShowCaptureIndicator);
        Assert.Equal("alt+f", config.Current.ShortcutFlow);
        Assert.Equal(1, config.SaveCount);
        Assert.Equal(ThemeConstants.Dark, processes.Theme);
    }

    [Theory]
    [InlineData("{}", true)]
    [InlineData("{\"show_capture_indicator\":false}", false)]
    public void CaptureIndicatorDefaultsOnAndRespectsStoredPreference(string json, bool expected)
    {
        var config = JsonSerializer.Deserialize(json, QapptiaJsonContext.Default.QapptiaConfig);

        Assert.NotNull(config);
        Assert.Equal(expected, config.ShowCaptureIndicator);
    }

    [Fact]
    public void InvalidFolderDoesNotMutateOrPersistConfiguration()
    {
        var config = new MemoryConfigService();
        var processes = new RecordingProcesses();
        var service = new ConfigSettingsService(config, new FakeAutoStart(), processes);

        var saved = service.SavePreferences(Changes() with { SavePath = Path.Combine(Path.GetTempPath(), "Qapptia_Missing_" + Guid.NewGuid().ToString("N")) }, new(AutoStartScope.CurrentUser, false), true);

        Assert.False(saved.IsValid);
        Assert.Equal(ThemeConstants.System, config.Current.Theme);
        Assert.Equal(0, config.SaveCount);
        Assert.Null(processes.Theme);
    }

    private static ConfigChanges Changes() => new(
        ThemeConstants.Dark, Path.GetTempPath(), "Captura", true, false, false, true, false, 0,
        Constants.DefaultShortcutScreen, Constants.DefaultShortcutArea, false, true, false);

    private sealed class MemoryConfigService : IConfigService
    {
        public QapptiaConfig Current { get; } = new();
        public int SaveCount { get; private set; }
        public void Save() => SaveCount++;
        public void Reload() { }
    }

    private sealed class FakeAutoStart : IAutoStartService
    {
        public bool IsSupported => true;
        public bool IsAutoStartEnabled() => false;
        public bool SetAutoStartEnabled(bool enabled) => false;
    }

    private sealed class RecordingProcesses : IConfigProcessService
    {
        public string? Theme { get; private set; }
        public void NotifyPreferencesChanged(string theme) => Theme = theme;
        public void PublishUpdate(ReleaseInfoDto? release) { }
        public Task<bool> OpenEditorHelpAsync(CancellationToken cancellationToken = default) => Task.FromResult(false);
    }
}
