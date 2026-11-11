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
        config.Current.Editor.ShowEditorHelpOnStartup = false;
        config.Current.Capture.ShortcutFlow = "alt+f";
        var processes = new RecordingProcesses();
        var service = new ConfigSettingsService(config, new FakeAutoStart(), processes);

        var saved = service.SavePreferences(Changes() with
        {
            ShowMouse = false, HighlightMouse = true, FilenameFormat = " ", ShortcutScreen = "", ShortcutArea = " ",
            ShowCaptureIndicator = false
        }, new(AutoStartScope.CurrentUser, true), true);

        Assert.True(saved.IsValid);
        Assert.False(saved.ChangeAutoStart);
        Assert.False(config.Current.Capture.HighlightMouse);
        Assert.True(config.Current.General.AutoStart);
        Assert.Equal(Constants.DefaultFilenameFormat, config.Current.General.FilenameFormat);
        Assert.Equal(Constants.DefaultShortcutScreen, config.Current.Capture.ShortcutScreen);
        Assert.Equal(Constants.DefaultShortcutArea, config.Current.Capture.ShortcutArea);
        Assert.False(config.Current.Editor.ShowEditorHelpOnStartup);
        Assert.False(config.Current.Capture.ShowCaptureIndicator);
        Assert.Equal("alt+f", config.Current.Capture.ShortcutFlow);
        Assert.Equal(1, config.SaveCount);
        Assert.Equal(ThemeConstants.Dark, processes.Theme);
    }

    [Theory]
    [InlineData("{}", true)]
    [InlineData("{\"show_capture_indicator\":false}", true)]
    [InlineData("{\"capture\":{\"show_capture_indicator\":false}}", false)]
    public void CaptureIndicatorDefaultsOnAndRespectsStoredPreference(string json, bool expected)
    {
        var config = JsonSerializer.Deserialize(json, QapptiaJsonContext.Default.QapptiaConfig);

        Assert.NotNull(config);
        Assert.Equal(expected, config.Capture.ShowCaptureIndicator);
    }

    [Fact]
    public void GlobalConfigUsesDefaultsWithOnlyOldFileAndPersistsTheThreeSections()
    {
        var directory = Directory.CreateTempSubdirectory("Qapptia_Config_");
        try
        {
            var oldPath = Path.Combine(directory.FullName, "config.json");
            const string oldJson = "{\"theme\":\"light\",\"manual_timer\":10}";
            File.WriteAllText(oldPath, oldJson);
            var path = Path.Combine(directory.FullName, Constants.ConfigFileName);
            var service = new JsonConfigService(path);

            Assert.Equal("global_config.json", Path.GetFileName(path));
            Assert.Equal(ThemeConstants.System, service.Current.General.Theme);
            Assert.Equal(0, service.Current.Capture.ManualTimer);
            Assert.True(service.Current.Editor.ShowEditorHelpOnStartup);

            service.Current.General.Theme = ThemeConstants.Dark;
            service.Current.Capture.ManualTimer = 3;
            service.Current.Editor.ShowEditorHelpOnStartup = false;
            service.Save();

            using var json = JsonDocument.Parse(File.ReadAllText(path));
            var root = json.RootElement;
            AssertJsonProperties(root, "version", "general", "capture", "editor");
            Assert.Equal(1, root.GetProperty("version").GetInt32());

            var general = root.GetProperty("general");
            AssertJsonProperties(general, "theme", "auto_start", "save_path", "filename_format",
                "subfolder_month", "subfolder_day", "subfolder_hour");
            Assert.Equal(ThemeConstants.Dark, general.GetProperty("theme").GetString());

            var capture = root.GetProperty("capture");
            AssertJsonProperties(capture, "show_mouse", "highlight_mouse", "manual_timer", "shortcut_screen",
                "shortcut_area", "shortcut_flow", "shortcut_flow_pause", "copy_to_clipboard_screen",
                "copy_to_clipboard_area", "capture_all_screens", "show_capture_indicator", "enable_scroll_capture");
            Assert.Equal(3, capture.GetProperty("manual_timer").GetInt32());

            var editor = root.GetProperty("editor");
            AssertJsonProperties(editor, "show_editor_help_on_startup");
            Assert.False(editor.GetProperty("show_editor_help_on_startup").GetBoolean());

            service.Current.General.Theme = ThemeConstants.System;
            service.Current.Capture.ManualTimer = 0;
            service.Current.Editor.ShowEditorHelpOnStartup = true;
            service.Reload();

            Assert.Equal(ThemeConstants.Dark, service.Current.General.Theme);
            Assert.Equal(3, service.Current.Capture.ManualTimer);
            Assert.False(service.Current.Editor.ShowEditorHelpOnStartup);
            Assert.Equal(oldJson, File.ReadAllText(oldPath));
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void InvalidFolderDoesNotMutateOrPersistConfiguration()
    {
        var config = new MemoryConfigService();
        var processes = new RecordingProcesses();
        var service = new ConfigSettingsService(config, new FakeAutoStart(), processes);

        var saved = service.SavePreferences(Changes() with { SavePath = Path.Combine(Path.GetTempPath(), "Qapptia_Missing_" + Guid.NewGuid().ToString("N")) }, new(AutoStartScope.CurrentUser, false), true);

        Assert.False(saved.IsValid);
        Assert.Equal(ThemeConstants.System, config.Current.General.Theme);
        Assert.Equal(0, config.SaveCount);
        Assert.Null(processes.Theme);
    }

    private static void AssertJsonProperties(JsonElement element, params string[] expected)
    {
        var actual = element.EnumerateObject().Select(property => property.Name);
        Assert.Equal(expected.Order(StringComparer.Ordinal), actual.Order(StringComparer.Ordinal));
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
