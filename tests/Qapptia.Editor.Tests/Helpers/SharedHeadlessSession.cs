using Avalonia;
using Avalonia.Headless;

namespace Qapptia.Editor.Tests.Helpers;

/// <summary>
/// Sesión única y centralizada para pruebas unitarias Headless en Avalonia.
/// Evita colisiones de hilos en Dispatcher y RenderLoop compartidos en el mismo proceso.
/// </summary>
public static class SharedHeadlessSession
{
    public static readonly HeadlessUnitTestSession Session = HeadlessUnitTestSession.StartNew(typeof(SharedHeadlessSession));

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<Qapptia.App.Editor.App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = true });
}
