namespace Qapptia.Core.Abstractions;

/// <summary>
/// Servicio para consultar y configurar el inicio automático de Qapptia con el sistema operativo
/// (Windows, Linux, macOS).
/// </summary>
public interface IAutoStartService
{
    /// <summary>
    /// Indica si el mecanismo de inicio automático está soportado en la plataforma actual.
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Consulta si el inicio automático está actualmente habilitado en el sistema operativo.
    /// </summary>
    /// <returns>True si está registrado para iniciar con el sistema; en caso contrario, false.</returns>
    bool IsAutoStartEnabled();

    /// <summary>
    /// Habilita o deshabilita el inicio automático en el sistema operativo.
    /// </summary>
    /// <param name="enabled">True para registrar el inicio con el sistema; false para eliminarlo.</param>
    /// <returns>True si la operación tuvo éxito; false en caso de error o falta de permisos.</returns>
    bool SetAutoStartEnabled(bool enabled);

    AutoStartState GetState() => IsSupported
        ? new(AutoStartScope.CurrentUser, IsAutoStartEnabled())
        : new(AutoStartScope.Unsupported, false);

    async Task<AutoStartChangeResult> SetEnabledAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        bool succeeded = await Task.Run(() => SetAutoStartEnabled(enabled), cancellationToken).ConfigureAwait(false);
        return new(succeeded, false, GetState());
    }
}
