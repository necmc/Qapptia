using Qapptia.Core.Abstractions;
using Qapptia.Core.Capture;

namespace Qapptia.Capture;

public sealed record CaptureFeedbackTarget(
    AreaInfo Bounds,
    (int X, int Y)? Cursor,
    IReadOnlyList<CaptureDisplay> Displays);

public interface ICaptureVisualFeedback
{
    IReadOnlyList<CaptureDisplay> GetDisplays();
    Task HideBeforeCaptureAsync(CancellationToken ct);
    void Enqueue(CaptureFeedbackTarget target);
    void Dismiss();
}
