using System;
using System.IO;
using FluentAssertions;
using Moq;
using Qapptia.App.Editor.ViewModels;
using Qapptia.Core;
using Qapptia.Core.Abstractions;
using Qapptia.Core.Services;
using Qapptia.Editor.Core;
using Qapptia.Editor.Services;
using Xunit;

namespace Qapptia.Editor.Tests.ViewModels;

public sealed class EditorViewModelUpdateTests : IDisposable
{
    private readonly string _testDir;
    private readonly Mock<IEditorStateService> _stateServiceMock;
    private readonly Mock<IFontProvider> _fontProviderMock;
    private readonly Mock<IShellService> _shellServiceMock;

    public EditorViewModelUpdateTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "Qapptia_EditorUpdateTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);

        _stateServiceMock = new Mock<IEditorStateService>();
        _stateServiceMock.Setup(s => s.Load()).Returns(new Qapptia.Editor.Models.EditorState());

        _fontProviderMock = new Mock<IFontProvider>();
        _shellServiceMock = new Mock<IShellService>();

        InMemoryUpdateStateStore.Clear();
    }

    public void Dispose()
    {
        InMemoryUpdateStateStore.Clear();
        if (Directory.Exists(_testDir))
        {
            try { Directory.Delete(_testDir, true); } catch { }
        }
    }

    private EditorViewModel CreateViewModel()
    {
        return new EditorViewModel(
            _stateServiceMock.Object,
            _testDir,
            _fontProviderMock.Object,
            shellService: _shellServiceMock.Object,
            uiDispatcher: action => action(),
            queryIpcUpdateOnStartup: false);
    }

    [Fact]
    public void EditorViewModelInitialStateShouldHaveAboutIconAndNoPendingUpdate()
    {
        using var vm = CreateViewModel();

        vm.HasPendingUpdate.Should().BeFalse();
        vm.InfoButtonIconKey.Should().Be(Qapptia.Core.Constants.IconInfo);
        vm.InfoButtonToolTip.Should().Be(Qapptia.Core.Constants.ToolTipAbout);
    }

    [Fact]
    public void EditorViewModelSetPendingUpdateShouldChangeIconToolTipAndProperties()
    {
        using var vm = CreateViewModel();

        vm.SetPendingUpdate("2.0.0", "https://example.com/download");

        vm.HasPendingUpdate.Should().BeTrue();
        vm.PendingUpdateVersion.Should().Be("2.0.0");
        vm.PendingUpdateUrl.Should().Be("https://example.com/download");
        vm.InfoButtonIconKey.Should().Be(Qapptia.Core.Constants.IconDownload);
        vm.InfoButtonToolTip.Should().Contain("2.0.0");
    }

    [Fact]
    public void EditorViewModelClearPendingUpdateShouldRevertIconAndToolTip()
    {
        using var vm = CreateViewModel();

        vm.SetPendingUpdate("2.0.0", "https://example.com/download");
        vm.HasPendingUpdate.Should().BeTrue();

        vm.ClearPendingUpdate();

        vm.HasPendingUpdate.Should().BeFalse();
        vm.InfoButtonIconKey.Should().Be(Qapptia.Core.Constants.IconInfo);
        vm.InfoButtonToolTip.Should().Be(Qapptia.Core.Constants.ToolTipAbout);
    }

    [Fact]
    public void EditorViewModelOpenAboutOrDownloadWhenUpdateAvailableShouldOpenDownloadUrlWithCooldown()
    {
        using var vm = CreateViewModel();
        vm.SetPendingUpdate("2.0.0", "https://example.com/download");

        // Primer clic debe abrir la URL
        vm.OpenAboutOrDownloadCommand.Execute(null);
        _shellServiceMock.Verify(s => s.OpenUrl("https://example.com/download"), Times.Once);

        // Segundo clic inmediato (dentro de cooldown de 2s) debe ser ignorado
        vm.OpenAboutOrDownloadCommand.Execute(null);
        _shellServiceMock.Verify(s => s.OpenUrl("https://example.com/download"), Times.Once);
    }

    [Fact]
    public async Task EditorViewModelAutonomousRelayWhenCaptureInactiveShouldDetectUpdateAndMutateIcon()
    {
        var release = new ReleaseInfoDto { Version = "99.0.0", SiteDownload = "https://example.com/dl" };
        var expectedResult = UpdateCheckResult.UpdateAvailable("2.0.0", release);

        var updateServiceMock = new Mock<IUpdateCheckService>();
        updateServiceMock.Setup(s => s.CheckForUpdatesAsync(It.IsAny<bool>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var captureAppMock = new Mock<ICaptureAppService>();
        captureAppMock.Setup(c => c.IsRunning).Returns(false);

        using var vm = new EditorViewModel(
            _stateServiceMock.Object,
            _testDir,
            _fontProviderMock.Object,
            shellService: _shellServiceMock.Object,
            captureAppService: captureAppMock.Object,
            updateCheckService: updateServiceMock.Object,
            uiDispatcher: action => action(),
            queryIpcUpdateOnStartup: true);

        var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
        void Handler(UpdateStateInfo? info)
        {
            if (info?.Version == "99.0.0") tcs.TrySetResult(true);
        }

        InMemoryUpdateStateStore.StateChanged += Handler;
        try
        {
            var completed = await System.Threading.Tasks.Task.WhenAny(tcs.Task, System.Threading.Tasks.Task.Delay(4000, TestContext.Current.CancellationToken));
            completed.Should().Be(tcs.Task);

            vm.HasPendingUpdate.Should().BeTrue();
            vm.InfoButtonIconKey.Should().Be(Qapptia.Core.Constants.IconDownload);
            vm.PendingUpdateVersion.Should().Be("99.0.0");
        }
        finally
        {
            InMemoryUpdateStateStore.StateChanged -= Handler;
        }
    }

    [Fact]
    public void EditorViewModelWhenUpdateStateClearedShouldRevertToAboutIconImmediately()
    {
        using var vm = CreateViewModel();
        vm.SetPendingUpdate("2.0.0", "https://example.com/download");
        vm.HasPendingUpdate.Should().BeTrue();
        vm.InfoButtonIconKey.Should().Be(Qapptia.Core.Constants.IconDownload);

        // Al limpiar el estado en memoria (por fallo de comprobación o estar al día), el VM reacciona
        InMemoryUpdateStateStore.Clear();

        vm.HasPendingUpdate.Should().BeFalse();
        vm.InfoButtonIconKey.Should().Be(Qapptia.Core.Constants.IconInfo);
        vm.InfoButtonToolTip.Should().Be(Qapptia.Core.Constants.ToolTipAbout);
    }

    [Fact]
    public void EditorViewModelWhenCoordinatorFailsShouldRetainExistingPendingUpdateWithoutClearing()
    {
        InMemoryUpdateStateStore.SetUpdate("2.1.0", "https://example.com/download");

        var updateServiceMock = new Mock<IUpdateCheckService>();
        updateServiceMock.Setup(s => s.CheckForUpdatesAsync(It.IsAny<bool>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(UpdateCheckResult.ServerUnavailable("2.0.0", "Timeout"));

        using var vm = new EditorViewModel(
            _stateServiceMock.Object,
            _testDir,
            _fontProviderMock.Object,
            shellService: _shellServiceMock.Object,
            updateCheckService: updateServiceMock.Object,
            uiDispatcher: action => action(),
            queryIpcUpdateOnStartup: false);

        // Disparamos un chequeo fallido en el coordinador
        var method = typeof(EditorViewModel).GetMethod("OnCoordinatorCheckCompleted", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method?.Invoke(vm, new object[] { UpdateCheckResult.ServerUnavailable("2.0.0", "Timeout") });

        // La alerta debe seguir activa; no se debe haber desalertado al usuario
        vm.HasPendingUpdate.Should().BeTrue();
        InMemoryUpdateStateStore.HasUpdate.Should().BeTrue();
        InMemoryUpdateStateStore.Current?.Version.Should().Be("2.1.0");
    }
}
