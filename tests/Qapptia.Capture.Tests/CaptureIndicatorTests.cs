using Qapptia.Core.Abstractions;
using Qapptia.Core.Capture;
using Qapptia.Core.Configuration;
using Qapptia.UI.Components.Overlay;
using Serilog;
using SkiaSharp;

namespace Qapptia.Capture.Tests;

public sealed class CaptureIndicatorTests
{
    private static readonly CaptureDisplay s_left = new(1, new MonitorInfo(-1600, -200, 1600, 1200, false), 1.5);
    private static readonly CaptureDisplay s_right = new(2, new MonitorInfo(0, 0, 1920, 1080, true), 1);

    [Fact]
    public void ArtworkUsesThemedOutlineAndFlashesAcrossTheNotch()
    {
        const int width = CaptureIndicatorArtwork.Width;
        using var artwork = new CaptureIndicatorArtwork(width, CaptureIndicatorArtwork.Height);
        var palette = new CaptureIndicatorArtwork.Palette(new SKColor(40, 40, 40), new SKColor(107, 114, 128),
            SKColors.White, new SKColor(168, 199, 250));
        const int lens = (18 * width + width / 2) * 4;
        const int body = (18 * width + 4) * 4;
        const int outline = 18 * width * 4;

        Assert.Equal(0, artwork.Render(0, palette)[lens + 3]);
        var flash = artwork.Render(180, palette);
        Assert.Equal(byte.MaxValue, flash[lens]);
        Assert.True(flash[body] > palette.Surface.Blue);
        var hold = artwork.Render(330, palette);
        Assert.Equal(palette.Outline.Blue, hold[outline]);
        Assert.Equal(palette.Surface.Blue, hold[body]);
        Assert.Equal(palette.Surface.Blue, hold[lens]);
        Assert.Equal(0, artwork.Render(CaptureIndicatorArtwork.DurationMs, palette)[lens + 3]);
    }

    [Fact]
    public void FullDesktopUsesCursorFromCaptureAndAnchorsAtThreeQuartersOfThatDisplay()
    {
        var target = new CaptureFeedbackTarget(new AreaInfo(-1600, -200, 3520, 1280), (-2, 50), [s_left, s_right]);

        var selected = CaptureIndicatorPlacement.SelectDisplay(target);

        Assert.Equal(s_left, selected);
        const int scaledWidth = CaptureIndicatorArtwork.Width * 3 / 2;
        Assert.Equal(-400, CaptureIndicatorPlacement.GetLeft(s_left.Bounds, scaledWidth) + scaledWidth / 2);
    }

    [Theory]
    [InlineData(-250, 400, 1)]
    [InlineData(-200, 400, 2)]
    [InlineData(2100, 200, 0)]
    public void CropUsesGreatestOverlapThenCenterAndSkipsUnmatchedAreas(int x, int width, int display)
    {
        var target = new CaptureFeedbackTarget(new AreaInfo(x, 25, width, 100), null, [s_left, s_right]);

        var selected = CaptureIndicatorPlacement.SelectDisplay(target);

        Assert.Equal((nint)display, selected?.Id ?? 0);
    }

    [Fact]
    public async Task FrozenReadWaitsForPriorIndicatorToHide()
    {
        var ct = TestContext.Current.CancellationToken;
        var hide = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var visual = new Mock<ICaptureVisualFeedback>();
        visual.Setup(v => v.HideBeforeCaptureAsync(ct)).Returns(hide.Task);
        visual.Setup(v => v.GetDisplays()).Returns([s_left]);
        var screen = new Mock<IScreenCapture>();
        screen.Setup(s => s.CaptureScreenAsync(false, ct)).ReturnsAsync(new ScreenCaptureResult([], 1, 1, 0, 0));
        var config = new Mock<IConfigService>();
        config.SetupGet(c => c.Current).Returns(new QapptiaConfig());
        var service = new FullscreenCaptureService(screen.Object, Mock.Of<ICursorCapture>(), Mock.Of<IClipboardService>(),
            Mock.Of<IDesktopService>(), config.Object, Mock.Of<ILogger>(), visual.Object);

        var read = service.CaptureFrozenScreenAsync(false, ct);
        screen.Verify(s => s.CaptureScreenAsync(false, ct), Times.Never);
        hide.SetResult();

        Assert.Equal(s_left, Assert.Single((await read).DisplaysAtCapture));
        screen.Verify(s => s.CaptureScreenAsync(false, ct), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ConsecutiveCapturesReplaceTheIndicatorWithoutWaitingForEarlierAudio(bool enabled)
    {
        var ct = TestContext.Current.CancellationToken;
        var target = new CaptureFeedbackTarget(new AreaInfo(0, 0, 1920, 1080), null, [s_right]);
        var firstSoundStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondSoundStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var soundDone = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var firstVisualStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondVisualStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int soundCount = 0;
        int visualCount = 0;
        var config = new Mock<IConfigService>();
        config.SetupGet(c => c.Current).Returns(new QapptiaConfig { Capture = { ShowCaptureIndicator = enabled } });
        var screen = new Mock<IFullscreenCaptureService>();
        screen.Setup(s => s.CaptureAsync(It.IsAny<CaptureJob>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CaptureResult("capture.png", [], 1920, 1080) { FeedbackTarget = target });
        var sound = new Mock<IShutterSoundService>();
        sound.Setup(s => s.PlayAsync(CancellationToken.None)).Returns(() =>
        {
            var started = Interlocked.Increment(ref soundCount) == 1 ? firstSoundStarted : secondSoundStarted;
            started.TrySetResult();
            return soundDone.Task;
        });
        var visual = new Mock<ICaptureVisualFeedback>();
        visual.Setup(v => v.Enqueue(target)).Callback(() =>
        {
            var started = Interlocked.Increment(ref visualCount) == 1 ? firstVisualStarted : secondVisualStarted;
            started.TrySetResult();
        });
        using var worker = new CaptureWorker(Mock.Of<IHotkeyRegistrar>(), screen.Object, Mock.Of<IAreaCaptureService>(),
            config.Object, Mock.Of<IPowerEvents>(), sound.Object, Mock.Of<ILogger>(), visualFeedback: visual.Object);

        await worker.StartAsync(ct);
        try
        {
            await worker.HandleFullscreenCaptureAsync(ct);
            await firstSoundStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), ct);
            if (enabled) await firstVisualStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), ct);

            await worker.HandleFullscreenCaptureAsync(ct);
            await secondSoundStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), ct);
            if (enabled) await secondVisualStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), ct);
            visual.Verify(v => v.Dismiss(), Times.Exactly(2));
        }
        finally
        {
            soundDone.TrySetResult();
            await worker.StopAsync(ct);
        }
        visual.Verify(v => v.Enqueue(target), enabled ? Times.Exactly(2) : Times.Never());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CancelledAreaOrFailedCaptureDoesNotDispatchVisualConfirmation(bool cancelledArea)
    {
        var ct = TestContext.Current.CancellationToken;
        var target = new CaptureFeedbackTarget(new AreaInfo(0, 0, 1920, 1080), null, [s_right]);
        var nextCaptureConfirmed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var config = new Mock<IConfigService>();
        config.SetupGet(c => c.Current).Returns(new QapptiaConfig());
        var screen = new Mock<IFullscreenCaptureService>();
        var area = new Mock<IAreaCaptureService>();
        var successful = new CaptureResult("capture.png", [], 1920, 1080) { FeedbackTarget = target };
        var sequence = screen.SetupSequence(s => s.CaptureAsync(It.IsAny<CaptureJob>(), It.IsAny<CancellationToken>()));
        if (cancelledArea)
        {
            screen.Setup(s => s.CaptureFrozenScreenAsync(true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ScreenCaptureResult([], 1, 1, 0, 0));
            area.Setup(a => a.SelectAreaAsync(It.IsAny<ScreenCaptureResult>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AreaInfo?)null);
        }
        else
        {
            sequence.ThrowsAsync(new IOException("No se pudo guardar la captura."));
        }
        sequence.ReturnsAsync(successful);

        var visual = new Mock<ICaptureVisualFeedback>();
        visual.Setup(v => v.Enqueue(target)).Callback(() => nextCaptureConfirmed.TrySetResult());
        using var worker = new CaptureWorker(Mock.Of<IHotkeyRegistrar>(), screen.Object, area.Object, config.Object,
            Mock.Of<IPowerEvents>(), Mock.Of<IShutterSoundService>(), Mock.Of<ILogger>(), visualFeedback: visual.Object);

        await worker.StartAsync(ct);
        try
        {
            if (cancelledArea) await worker.HandleAreaCaptureAsync(ct);
            else await worker.HandleFullscreenCaptureAsync(ct);
            await worker.HandleFullscreenCaptureAsync(ct);
            await nextCaptureConfirmed.Task.WaitAsync(TimeSpan.FromSeconds(5), ct);
        }
        finally
        {
            await worker.StopAsync(ct);
        }
        visual.Verify(v => v.Enqueue(It.IsAny<CaptureFeedbackTarget>()), Times.Once);
    }
}
