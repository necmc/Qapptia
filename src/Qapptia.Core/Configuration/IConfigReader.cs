namespace Qapptia.Core.Configuration;

/// <summary>Acceso de lectura a la configuración global.</summary>
public interface IConfigReader
{
    /// <summary>Configuración actual en memoria.</summary>
    QapptiaConfig Current { get; }

    /// <summary>Recarga global_config.json desde disco y reemplaza la configuración en memoria.</summary>
    void Reload();
}
