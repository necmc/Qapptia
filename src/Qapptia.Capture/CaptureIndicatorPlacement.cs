using Qapptia.Core.Abstractions;
using Qapptia.Core.Capture;

namespace Qapptia.Capture;

internal static class CaptureIndicatorPlacement
{
    private const double HorizontalAnchor = 0.75;

    public static CaptureDisplay? SelectDisplay(CaptureFeedbackTarget target)
    {
        if (target.Cursor is { } cursor)
        {
            foreach (var display in target.Displays)
            {
                if (Contains(display.Bounds, cursor.X, cursor.Y)) return display;
            }
        }

        var bounds = target.Bounds;
        var centerX = bounds.X + bounds.Width / 2.0;
        var centerY = bounds.Y + bounds.Height / 2.0;
        return target.Displays
            .Select(display => (Display: display, Overlap: GetOverlap(display.Bounds, bounds)))
            .Where(candidate => candidate.Overlap > 0)
            .OrderByDescending(candidate => candidate.Overlap)
            .ThenByDescending(candidate => Contains(candidate.Display.Bounds, centerX, centerY))
            .ThenByDescending(candidate => candidate.Display.Bounds.IsPrimary)
            .ThenBy(candidate => candidate.Display.Bounds.X)
            .ThenBy(candidate => candidate.Display.Bounds.Y)
            .Select(candidate => (CaptureDisplay?)candidate.Display)
            .FirstOrDefault();
    }

    public static int GetLeft(MonitorInfo monitor, int width)
    {
        int desiredLeft = (int)Math.Round(monitor.Width * HorizontalAnchor - width / 2.0);
        return monitor.X + Math.Clamp(desiredLeft, 0, Math.Max(0, monitor.Width - width));
    }

    private static bool Contains(MonitorInfo monitor, double x, double y) =>
        x >= monitor.X && x < (long)monitor.X + monitor.Width &&
        y >= monitor.Y && y < (long)monitor.Y + monitor.Height;

    private static long GetOverlap(MonitorInfo monitor, AreaInfo bounds)
    {
        long width = Math.Min((long)monitor.X + monitor.Width, (long)bounds.X + bounds.Width) - Math.Max(monitor.X, bounds.X);
        long height = Math.Min((long)monitor.Y + monitor.Height, (long)bounds.Y + bounds.Height) - Math.Max(monitor.Y, bounds.Y);
        return Math.Max(0, width) * Math.Max(0, height);
    }
}
