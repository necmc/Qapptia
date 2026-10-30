using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Qapptia.Capture;
using Qapptia.Core;
using Qapptia.Core.Abstractions;
using Qapptia.Core.Configuration;
using Serilog;
using Xunit;

namespace Qapptia.Capture.Tests;

public sealed class CaptureWorkerResilienceTests
{
    private readonly Mock<IHotkeyRegistrar> _hotkeysMock = new();
    private readonly Mock<IFullscreenCaptureService> _fullscreenMock = new();
    private readonly Mock<IAreaCaptureService> _areaMock = new();
    private readonly Mock<IConfigService> _configMock = new();
    private readonly Mock<IPowerEvents> _powerEventsMock = new();
    private readonly Mock<IShutterSoundService> _shutterSoundMock = new();
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly Mock<ITrayIconService> _trayIconMock = new();

    private CaptureWorker CreateWorker()
    {
        _configMock.Setup(c => c.Current).Returns(new QapptiaConfig());
        return new CaptureWorker(
            _hotkeysMock.Object,
            _fullscreenMock.Object,
            _areaMock.Object,
            _configMock.Object,
            _powerEventsMock.Object,
            _shutterSoundMock.Object,
            _loggerMock.Object,
            _trayIconMock.Object);
    }

    [Fact]
    public async Task HandleWakeUpAsyncCallsRefreshIconAndShowsNotification()
    {
        var worker = CreateWorker();

        await worker.HandleWakeUpAsync(CancellationToken.None);

        _trayIconMock.Verify(t => t.RefreshIcon(), Times.Once);
        _trayIconMock.Verify(t => t.ShowNotification(
            Constants.NotificationTitleCapture,
            Constants.NotificationMessageCaptureActive,
            TrayNotificationType.Info,
            Constants.NotificationDurationMs), Times.Once);
    }

    [Fact]
    public async Task HandleRefreshTrayAsyncCallsRefreshIcon()
    {
        var worker = CreateWorker();

        await worker.HandleRefreshTrayAsync(CancellationToken.None);

        _trayIconMock.Verify(t => t.RefreshIcon(), Times.Once);
    }

    [Fact]
    public async Task PowerResumeTriggersRefreshIconOnTrayService()
    {
        _powerEventsMock.SetupGet(p => p.RequiresHotkeyReRegistrationAfterResume).Returns(true);
        var subscription = new TaskCompletionSource<EventHandler<PowerMode>>(TaskCreationOptions.RunContinuationsAsynchronously);
        _powerEventsMock.SetupAdd(p => p.PowerModeChanged += It.IsAny<EventHandler<PowerMode>>())
            .Callback<EventHandler<PowerMode>>(handler => subscription.TrySetResult(handler));
        var worker = CreateWorker();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        try
        {
            await worker.StartAsync(cts.Token);
            var handler = await subscription.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            handler(_powerEventsMock.Object, PowerMode.Resume);
            _trayIconMock.Verify(t => t.RefreshIcon(), Times.Once);
        }
        finally
        {
            cts.Cancel();
            await worker.StopAsync(TestContext.Current.CancellationToken);
        }
    }
}
