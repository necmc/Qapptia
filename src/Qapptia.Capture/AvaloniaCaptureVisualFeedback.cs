using System.Diagnostics;
using Avalonia.Threading;
using Qapptia.Core.Abstractions;
using Qapptia.Core.Configuration;
using Qapptia.UI.Components.Overlay;
using Serilog;

namespace Qapptia.Capture;

public sealed class AvaloniaCaptureVisualFeedback(
    ICaptureIndicatorSurface surface, IConfigReader config, ILogger logger) : ICaptureVisualFeedback, IDisposable
{
    private static readonly TimeSpan s_frameInterval = TimeSpan.FromMilliseconds(16);
    private readonly object _stateLock = new();
    private DispatcherTimer? _timer;
    private Task? _pendingHide;
    private CaptureIndicatorArtwork? _artwork;
    private CaptureIndicatorArtwork.Palette _palette;
    private long _revision;
    private long _started;
    private long _queuedAt;
    private long _animationRevision;
    private bool _mayBeVisible;
    private bool _disabled;
    private bool _disposed;
    private int _left;
    private int _top;

    public IReadOnlyList<CaptureDisplay> GetDisplays()
    {
        try { return surface.GetDisplays(); }
        catch (Exception ex)
        {
            logger.Warning(ex, "No se pudieron consultar los monitores para el indicador de captura.");
            return [];
        }
    }

    // La lectura del escritorio solo espera la retirada efectiva de la ventana, sin esperar la animación.
    public Task HideBeforeCaptureAsync(CancellationToken ct) => RequestHide().WaitAsync(ct);

    public void Enqueue(CaptureFeedbackTarget target)
    {
        long revision;
        lock (_stateLock) revision = ++_revision;
        long queuedAt = Stopwatch.GetTimestamp();
        Dispatcher.UIThread.Post(() => StartOnUiThread(revision, target, queuedAt), DispatcherPriority.Normal);
    }

    public void Dismiss() => _ = DismissAsync();

    private async Task DismissAsync()
    {
        try { await RequestHide().ConfigureAwait(false); }
        catch (Exception ex) { logger.Warning(ex, "No se pudo ocultar el indicador visual de captura."); }
    }

    private Task RequestHide()
    {
        lock (_stateLock)
        {
            _revision++;
            if (!_mayBeVisible) return Task.CompletedTask;
            if (_pendingHide is { IsCompleted: false }) return _pendingHide;

            _pendingHide = Dispatcher.UIThread.InvokeAsync(HideOnUiThread, DispatcherPriority.Send).GetTask();
            return _pendingHide;
        }
    }

    private bool IsCurrent(long revision)
    {
        lock (_stateLock) return revision == _revision;
    }

    private void StartOnUiThread(long revision, CaptureFeedbackTarget target, long queuedAt)
    {
        if (_disposed || _disabled || !config.Current.Capture.ShowCaptureIndicator || !IsCurrent(revision)) return;

        try
        {
            HideOnUiThread();
            if (CaptureIndicatorPlacement.SelectDisplay(target) is not { } selected) return;
            var display = surface.GetDisplays().FirstOrDefault(candidate => candidate.Id == selected.Id);
            if (display.Bounds.Width <= 0 || display.Bounds.Height <= 0) return;

            _palette = CaptureIndicatorArtwork.GetPalette(config.Current.General.Theme);
            int width = Math.Clamp((int)Math.Round(CaptureIndicatorArtwork.Width * display.Scale), 1, display.Bounds.Width);
            int height = Math.Clamp((int)Math.Round(CaptureIndicatorArtwork.Height * display.Scale), 1, display.Bounds.Height);
            if (_artwork?.PixelWidth != width || _artwork.PixelHeight != height)
            {
                var oldArtwork = _artwork;
                _artwork = new CaptureIndicatorArtwork(width, height);
                oldArtwork?.Dispose();
            }

            lock (_stateLock)
            {
                if (revision != _revision) return;
                _mayBeVisible = true;
            }

            _left = CaptureIndicatorPlacement.GetLeft(display.Bounds, width);
            _top = display.Bounds.Y;
            _animationRevision = revision;
            _started = Stopwatch.GetTimestamp();
            _queuedAt = queuedAt;
            if (_timer == null)
            {
                _timer = new DispatcherTimer(DispatcherPriority.Render) { Interval = s_frameInterval };
                _timer.Tick += (_, _) => RenderFrame();
            }
            _timer.Start();
            RenderFrame();
        }
        catch (Exception ex) { DisableOnUiThread(ex); }
    }

    private void RenderFrame()
    {
        try
        {
            double elapsedMs = Stopwatch.GetElapsedTime(_started).TotalMilliseconds;
            if (elapsedMs >= CaptureIndicatorArtwork.DurationMs || !IsCurrent(_animationRevision))
            {
                HideOnUiThread();
                return;
            }

            var artwork = _artwork!;
            // El primer fotograma comienza visible y conserva el ritmo de las fases siguientes.
            double frameTime = Math.Max(elapsedMs, s_frameInterval.TotalMilliseconds);
            if (!surface.TryShowFrame(_left, _top, artwork.PixelWidth, artwork.PixelHeight, artwork.Render(frameTime, _palette)))
            {
                DisableOnUiThread();
            }
            else if (_queuedAt != 0)
            {
                logger.Debug("Primer fotograma visible del indicador enviado en {ElapsedMs:F1} ms.",
                    Stopwatch.GetElapsedTime(_queuedAt).TotalMilliseconds);
                _queuedAt = 0;
            }
        }
        catch (Exception ex) { DisableOnUiThread(ex); }
    }

    private void HideOnUiThread()
    {
        _timer?.Stop();
        surface.Hide();
        lock (_stateLock) _mayBeVisible = false;
    }

    private void DisableOnUiThread(Exception? ex = null)
    {
        _disabled = true;
        try { HideOnUiThread(); }
        catch (Exception hideError) { logger.Warning(hideError, "No se pudo ocultar el indicador visual de captura."); }
        logger.Warning(ex, "El indicador visual de captura no está disponible en esta sesión.");
    }

    public void Dispose()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dismiss();
            return;
        }
        if (_disposed) return;

        _disposed = true;
        _timer?.Stop();
        lock (_stateLock) _mayBeVisible = false;
        _artwork?.Dispose();
        surface.Dispose();
    }
}
