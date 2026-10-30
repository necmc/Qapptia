using Avalonia;

namespace Qapptia.UI.Components;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configura el uso de aceleración gráfica por hardware en Windows.
    /// </summary>
    /// <param name="enabled">
    /// true permite la selección predeterminada de Avalonia, con respaldo por software;
    /// false fuerza el renderizado por CPU sin utilizar la GPU para dibujar la interfaz.
    /// </param>
    /// <remarks>
    /// Habilitarla no garantiza que se utilice la GPU: depende del soporte del equipo.
    /// Esta opción no modifica el renderizado en Linux ni macOS.
    /// </remarks>
    public static AppBuilder WithHardwareAcceleration(this AppBuilder builder, bool enabled)
    {
        if (OperatingSystem.IsWindows() && !enabled)
        {
            builder.With(new Win32PlatformOptions
            {
                RenderingMode = new[] { Win32RenderingMode.Software }
            });
        }

        return builder;
    }
}
