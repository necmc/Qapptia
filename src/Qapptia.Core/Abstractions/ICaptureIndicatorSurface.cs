namespace Qapptia.Core.Abstractions;

public readonly record struct CaptureDisplay(nint Id, MonitorInfo Bounds, double Scale);

public interface ICaptureIndicatorSurface : IDisposable
{
    IReadOnlyList<CaptureDisplay> GetDisplays();

    /// <summary>Presenta un fotograma BGRA premultiplicado sin activar la ventana ni interceptar entrada.</summary>
    bool TryShowFrame(int x, int y, int width, int height, ReadOnlySpan<byte> pixels);

    void Hide();
}
