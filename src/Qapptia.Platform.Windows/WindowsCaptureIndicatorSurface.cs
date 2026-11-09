using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Qapptia.Core.Abstractions;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.HiDpi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Qapptia.Platform.Windows;

public sealed partial class WindowsCaptureIndicatorSurface : ICaptureIndicatorSurface
{
    private const string WindowClass = "STATIC";
    private const WINDOW_EX_STYLE PassiveStyles = WINDOW_EX_STYLE.WS_EX_LAYERED | WINDOW_EX_STYLE.WS_EX_NOACTIVATE |
        WINDOW_EX_STYLE.WS_EX_TRANSPARENT | WINDOW_EX_STYLE.WS_EX_TOOLWINDOW | WINDOW_EX_STYLE.WS_EX_TOPMOST;
    private const double StandardDpi = 96;
    private const byte SourceAlpha = 1;
    private const SET_WINDOW_POS_FLAGS VisibilityFlags = SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE |
        SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE;
    private static readonly HWND s_topmost = new(-1);

    private HWND _window;
    private HDC _bufferDc;
    private HBITMAP _bitmap;
    private HGDIOBJ _previousBitmap;
    private nint _buffer;
    private int _width;
    private int _height;
    private bool _visible;

    public unsafe IReadOnlyList<CaptureDisplay> GetDisplays()
    {
        var displays = new List<CaptureDisplay>();
        _ = PInvoke.EnumDisplayMonitors(default, (RECT?)null, (monitor, _, _, _) =>
        {
            var info = new MONITORINFO { cbSize = (uint)sizeof(MONITORINFO) };
            if (PInvoke.GetMonitorInfo(monitor, ref info))
            {
                var rect = info.rcMonitor;
                var bounds = new MonitorInfo(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top, (info.dwFlags & 1) != 0);
                displays.Add(new CaptureDisplay(monitor.Value, bounds, GetScale(monitor)));
            }
            return true;
        }, default);
        return displays;
    }

    private static unsafe double GetScale(HMONITOR monitor)
    {
        uint dpiX, dpiY;
        if (SupportsPerMonitorDpi &&
            PInvoke.GetDpiForMonitor(monitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, &dpiX, &dpiY).Succeeded && dpiX != 0)
            return dpiX / StandardDpi;

        var dc = PInvoke.GetDC(HWND.Null);
        if (dc.Value == 0) return 1;
        try
        {
            int dpi = PInvoke.GetDeviceCaps(dc, GET_DEVICE_CAPS_INDEX.LOGPIXELSX);
            return dpi > 0 ? dpi / StandardDpi : 1;
        }
        finally { _ = PInvoke.ReleaseDC(HWND.Null, dc); }
    }

    [SupportedOSPlatformGuard("windows8.1")]
    private static bool SupportsPerMonitorDpi => OperatingSystem.IsWindowsVersionAtLeast(6, 3);

    public unsafe bool TryShowFrame(int x, int y, int width, int height, ReadOnlySpan<byte> pixels)
    {
        if (_window.IsNull)
        {
            _window = (HWND)CreateWindowEx((uint)PassiveStyles, WindowClass, null, (uint)WINDOW_STYLE.WS_POPUP,
                x, y, width, height, 0, 0, 0, 0);
        }

        if (_window.IsNull || !EnsureBuffer(width, height)) return false;

        pixels.CopyTo(new Span<byte>((void*)_buffer, width * height * 4));
        var destination = new Point(x, y);
        var source = new Point();
        var size = new SIZE { cx = width, cy = height };
        var blend = new BLENDFUNCTION { SourceConstantAlpha = byte.MaxValue, AlphaFormat = SourceAlpha };
        if (!PInvoke.UpdateLayeredWindow(_window, default, &destination, &size, _bufferDc, &source,
                default, &blend, UPDATE_LAYERED_WINDOW_FLAGS.ULW_ALPHA)) return false;

        if (!_visible)
        {
            _visible = true;
            if (!PInvoke.SetWindowPos(_window, s_topmost, 0, 0, 0, 0,
                    VisibilityFlags | SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW)) return false;
        }
        return true;
    }

    private unsafe bool EnsureBuffer(int width, int height)
    {
        if (_buffer != 0 && width == _width && height == _height) return true;

        ReleaseBuffer();
        _bufferDc = PInvoke.CreateCompatibleDC(default(HDC));
        if (_bufferDc.Value == 0) return false;

        var info = new BITMAPINFO
        {
            bmiHeader = new BITMAPINFOHEADER
            {
                biSize = (uint)sizeof(BITMAPINFOHEADER), biWidth = width, biHeight = -height,
                biPlanes = 1, biBitCount = 32
            }
        };
        void* buffer;
        _bitmap = PInvoke.CreateDIBSection(_bufferDc, &info, DIB_USAGE.DIB_RGB_COLORS, &buffer, HANDLE.Null, 0);
        if (_bitmap.IsNull || buffer == null) return false;

        _previousBitmap = PInvoke.SelectObject(_bufferDc, (HGDIOBJ)_bitmap);
        if (_previousBitmap.IsNull) return false;

        _buffer = (nint)buffer;
        _width = width;
        _height = height;
        return true;
    }

    public void Hide()
    {
        if (!_visible) return;

        _ = PInvoke.SetWindowPos(_window, HWND.Null, 0, 0, 0, 0,
            VisibilityFlags | SET_WINDOW_POS_FLAGS.SWP_HIDEWINDOW | SET_WINDOW_POS_FLAGS.SWP_NOZORDER);
        if (PInvoke.IsWindowVisible(_window))
        {
            if (!PInvoke.DestroyWindow(_window))
                throw new InvalidOperationException("No se pudo retirar la ventana del indicador de captura.");

            _window = default;
        }
        _visible = false;
    }

    private void ReleaseBuffer()
    {
        if (!_previousBitmap.IsNull) _ = PInvoke.SelectObject(_bufferDc, _previousBitmap);
        if (!_bitmap.IsNull) _ = PInvoke.DeleteObject((HGDIOBJ)_bitmap);
        if (_bufferDc.Value != 0) _ = PInvoke.DeleteDC(_bufferDc);
        _previousBitmap = default;
        _bitmap = default;
        _bufferDc = default;
        _buffer = 0;
    }

    public void Dispose()
    {
        if (!_window.IsNull) _ = PInvoke.DestroyWindow(_window);
        _window = default;
        _visible = false;
        ReleaseBuffer();
    }

    [LibraryImport("user32.dll", EntryPoint = "CreateWindowExW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint CreateWindowEx(uint extendedStyle, string className, string? title, uint style,
        int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
}
