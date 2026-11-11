namespace Qapptia.Core.Configuration;

/// <summary>
/// Servicio central de lectura y escritura de global_config.json.
/// </summary>
public interface IConfigService : IConfigReader
{
    /// <summary>
    /// Persiste los cambios en global_config.json.
    /// </summary>
    void Save();
}
