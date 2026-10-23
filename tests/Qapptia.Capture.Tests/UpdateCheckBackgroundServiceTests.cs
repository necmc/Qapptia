using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Moq;
using Qapptia.Capture;
using Qapptia.Core.Abstractions;
using Qapptia.Core.Services;
using Xunit;

namespace Qapptia.Capture.Tests;

public sealed class UpdateCheckBackgroundServiceTests : IDisposable
{
    private readonly Mock<IUpdateCheckService> _updateServiceMock;
    private readonly Mock<ITrayIconService> _trayIconMock;
    private readonly Mock<IHostApplicationLifetime> _appLifetimeMock;
    private readonly CancellationTokenSource _appStartedCts;

    public UpdateCheckBackgroundServiceTests()
    {
        _updateServiceMock = new Mock<IUpdateCheckService>();
        _trayIconMock = new Mock<ITrayIconService>();
        _appLifetimeMock = new Mock<IHostApplicationLifetime>();
        _appStartedCts = new CancellationTokenSource();

        _appLifetimeMock.Setup(l => l.ApplicationStarted).Returns(_appStartedCts.Token);
        InMemoryUpdateStateStore.Clear();
    }

    public void Dispose()
    {
        InMemoryUpdateStateStore.Clear();
        _appStartedCts.Dispose();
    }

    [Fact]
    public async Task ExecuteAsyncWhenStoppedBeforeStartupShouldTerminateCleanly()
    {
        using var stoppingCts = new CancellationTokenSource();
        stoppingCts.Cancel();

        using var service = new UpdateCheckBackgroundService(
            _updateServiceMock.Object,
            _trayIconMock.Object,
            _appLifetimeMock.Object);

        var executeTask = service.StartAsync(stoppingCts.Token);
        var completed = await Task.WhenAny(executeTask, Task.Delay(1000, TestContext.Current.CancellationToken));

        completed.Should().Be(executeTask);
        executeTask.IsCompletedSuccessfully.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsyncWhenApplicationStartedSignalsShouldStartAndStopCleanly()
    {
        using var service = new UpdateCheckBackgroundService(
            _updateServiceMock.Object,
            _trayIconMock.Object,
            _appLifetimeMock.Object);

        using var stoppingCts = new CancellationTokenSource();
        var startTask = service.StartAsync(stoppingCts.Token);

        // Señalizar el inicio nativo de la aplicación
        _appStartedCts.Cancel();

        // El servicio debe haber iniciado la orquestación en segundo plano sin bloquear el arranque
        startTask.IsCompletedSuccessfully.Should().BeTrue();

        // Parada limpia
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public void OnCheckCompletedWhenUpdateAvailableShouldUpdateStoreAndNotifyTray()
    {
        var release = new ReleaseInfoDto { Version = "3.0.0", SiteDownload = "https://example.com/dl" };
        var checkResult = UpdateCheckResult.UpdateAvailable("2.0.0", release);

        using var service = new UpdateCheckBackgroundService(
            _updateServiceMock.Object,
            _trayIconMock.Object,
            _appLifetimeMock.Object);

        var method = typeof(UpdateCheckBackgroundService).GetMethod(
            "OnCheckCompleted",
            BindingFlags.NonPublic | BindingFlags.Instance);

        method?.Invoke(service, new object[] { checkResult });

        InMemoryUpdateStateStore.HasUpdate.Should().BeTrue();
        InMemoryUpdateStateStore.Current?.Version.Should().Be("3.0.0");
        InMemoryUpdateStateStore.Current?.DownloadUrl.Should().Be("https://example.com/dl");

        _trayIconMock.Verify(t => t.ShowNotification(
            It.IsAny<string>(),
            It.Is<string>(m => m.Contains("3.0.0")),
            TrayNotificationType.Info,
            It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void OnCheckCompletedWhenUpToDateShouldClearStore()
    {
        InMemoryUpdateStateStore.SetUpdate("2.5.0", "https://example.com/file");
        InMemoryUpdateStateStore.HasUpdate.Should().BeTrue();

        using var service = new UpdateCheckBackgroundService(
            _updateServiceMock.Object,
            _trayIconMock.Object,
            _appLifetimeMock.Object);

        var method = typeof(UpdateCheckBackgroundService).GetMethod(
            "OnCheckCompleted",
            BindingFlags.NonPublic | BindingFlags.Instance);

        method?.Invoke(service, new object[] { UpdateCheckResult.UpToDate("2.0.0") });

        InMemoryUpdateStateStore.HasUpdate.Should().BeFalse();
    }

    [Fact]
    public void OnCheckCompletedWhenErrorShouldRetainExistingStateWithoutDesalertingUser()
    {
        InMemoryUpdateStateStore.SetUpdate("2.5.0", "https://example.com/file");
        InMemoryUpdateStateStore.HasUpdate.Should().BeTrue();

        using var service = new UpdateCheckBackgroundService(
            _updateServiceMock.Object,
            _trayIconMock.Object,
            _appLifetimeMock.Object);

        var method = typeof(UpdateCheckBackgroundService).GetMethod(
            "OnCheckCompleted",
            BindingFlags.NonPublic | BindingFlags.Instance);

        method?.Invoke(service, new object[] { UpdateCheckResult.ServerUnavailable("2.0.0", "HTTP 503") });

        // El estado no debe limpiarse ante errores transitorios de red o servidor
        InMemoryUpdateStateStore.HasUpdate.Should().BeTrue();
        InMemoryUpdateStateStore.Current?.Version.Should().Be("2.5.0");
    }
}
