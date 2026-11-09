using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Qapptia.UI.Components.Theme;
using SkiaSharp;

namespace Qapptia.UI.Components.Overlay;

public sealed class CaptureIndicatorArtwork : IDisposable
{
    public const int Width = 38;
    public const int Height = 34;
    public const int DurationMs = DescentMs + FlashMs + HoldMs + RetreatMs;

    private const int DescentMs = 160;
    private const int FlashMs = 160;
    private const int FlashPeakMs = 32;
    private const int HoldMs = 260;
    private const int RetreatMs = 160;
    private const float CornerRadius = 8;
    private const float OutlineWidth = 1.25f;
    private const float LensX = Width / 2f;
    private const float LensY = 18;
    private const float LensRadius = 4;
    private const float FlashRadius = 18;
    private const string CameraOutline = "M7 12 Q7 10 9 10 H13 L16 7 H22 L25 10 H29 Q31 10 31 12 V24 Q31 26 29 26 H9 Q7 26 7 24 Z";
    private const string FlashOutline = "M19 8 L21.5 15.5 L29 18 L21.5 20.5 L19 28 L16.5 20.5 L9 18 L16.5 15.5 Z";
    private static readonly SKRect s_outline = new(0, -CornerRadius, Width, Height);
    private static readonly SKRect s_interior = new(OutlineWidth, -CornerRadius, Width - OutlineWidth, Height - OutlineWidth);
    private static readonly SKPoint s_lensCenter = new(LensX, LensY);
    private static readonly Lazy<IResourceProvider> s_themeResources = new(() =>
        (IResourceProvider)AvaloniaXamlLoader.Load(new Uri("avares://Qapptia.UI.Components/Theme/MaterialThemeStyles.axaml")));

    private readonly SKBitmap _bitmap;
    private readonly SKCanvas _canvas;
    private readonly SKPaint _paint = new() { IsAntialias = true };
    private readonly SKPath _camera = SKPath.ParseSvgPathData(CameraOutline);
    private readonly SKPath _flash = SKPath.ParseSvgPathData(FlashOutline);

    public readonly record struct Palette(SKColor Surface, SKColor Outline, SKColor Camera, SKColor Accent);

    public static Palette GetPalette(string? theme)
    {
        var variant = ThemeManager.GetThemeVariant(theme);
        if (variant == ThemeVariant.Default)
            variant = Application.Current?.ActualThemeVariant ?? ThemeVariant.Light;

        return new Palette(
            GetThemeColor("M3SurfaceContainerHighBrush", variant),
            GetThemeColor("M3OutlineBrush", variant),
            GetThemeColor("M3OnSurfaceBrush", variant),
            GetThemeColor("M3PrimaryBrush", variant));
    }

    private static SKColor GetThemeColor(string key, ThemeVariant variant)
    {
        if (s_themeResources.Value.TryGetResource(key, variant, out var resource) && resource is ISolidColorBrush brush)
        {
            var color = brush.Color;
            return new SKColor(color.R, color.G, color.B, color.A);
        }

        throw new InvalidOperationException($"Falta el color del indicador de captura: {key}.");
    }

    public CaptureIndicatorArtwork(int width, int height)
    {
        _bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        _canvas = new SKCanvas(_bitmap);
    }

    public int PixelWidth => _bitmap.Width;
    public int PixelHeight => _bitmap.Height;

    public ReadOnlySpan<byte> Render(double elapsedMs, Palette palette)
    {
        _canvas.Clear(SKColors.Transparent);
        _canvas.Save();
        _canvas.Scale((float)PixelWidth / Width, (float)PixelHeight / Height);
        float reveal = elapsedMs < DescentMs
            ? 1 - EaseIn(1 - (float)(elapsedMs / DescentMs))
            : 1 - EaseIn((float)Math.Clamp((elapsedMs - (DurationMs - RetreatMs)) / RetreatMs, 0, 1));
        _canvas.Translate(0, (reveal - 1) * Height);

        double flashTime = elapsedMs - DescentMs;
        float flash = flashTime >= 0 && flashTime < FlashMs
            ? 1 - (float)Math.Clamp((flashTime - FlashPeakMs) / (FlashMs - FlashPeakMs), 0, 1)
            : 0;
        _paint.Color = Blend(palette.Outline, palette.Accent, flash);
        _canvas.DrawRoundRect(s_outline, CornerRadius, CornerRadius, _paint);
        DrawInterior(palette, flash);

        _paint.Color = Blend(palette.Camera, palette.Surface, flash * 0.5f);
        _canvas.DrawPath(_camera, _paint);
        _paint.Color = palette.Surface;
        _canvas.DrawCircle(LensX, LensY, LensRadius, _paint);

        if (flash > 0)
        {
            _paint.Color = palette.Accent.WithAlpha((byte)(byte.MaxValue * flash));
            _paint.Style = SKPaintStyle.Stroke;
            _paint.StrokeWidth = OutlineWidth;
            _canvas.DrawPath(_flash, _paint);
            _paint.Style = SKPaintStyle.Fill;
            _paint.Color = SKColors.White.WithAlpha((byte)(byte.MaxValue * flash));
            _canvas.DrawPath(_flash, _paint);
        }

        _canvas.Restore();
        return _bitmap.GetPixelSpan();
    }

    private void DrawInterior(Palette palette, float flash)
    {
        _paint.Color = palette.Surface;
        using var shader = flash > 0
            ? SKShader.CreateRadialGradient(s_lensCenter, FlashRadius,
                [Blend(palette.Surface, SKColors.White, flash), Blend(palette.Surface, palette.Accent, flash * 0.6f)],
                null, SKShaderTileMode.Clamp)
            : null;
        _paint.Shader = shader;
        _canvas.DrawRoundRect(s_interior, CornerRadius - OutlineWidth, CornerRadius - OutlineWidth, _paint);
        _paint.Shader = null;
    }

    private static SKColor Blend(SKColor start, SKColor end, float amount) => new(
        (byte)(start.Red + (end.Red - start.Red) * amount),
        (byte)(start.Green + (end.Green - start.Green) * amount),
        (byte)(start.Blue + (end.Blue - start.Blue) * amount));

    private static float EaseIn(float progress) => progress * progress * progress;

    public void Dispose()
    {
        _canvas.Dispose();
        _bitmap.Dispose();
        _paint.Dispose();
        _camera.Dispose();
        _flash.Dispose();
    }
}
