namespace Qapptia.Core.Configuration;

/// <summary>
/// Servicio central de lectura y escritura de global_config.json.
/// </summary>
public interface IConfigService
{
    /// <summary>
    /// Configuración actual en memoria.
    /// </summary>
    QapptiaConfig Current { get; }

    /// <summary>
    /// Persiste los cambios en global_config.json.
    /// </summary>
    void Save();

    /// <summary>
    /// Recarga global_config.json desde disco y reemplaza la configuración en memoria.
    /// </summary>
    void Reload();
}
