using Qapptia.Core.Abstractions;

namespace Qapptia.Core.Configuration;

/// <summary>Comunicación de Configuración con las otras aplicaciones de la suite.</summary>
public interface IConfigProcessService
{
    void NotifyPreferencesChanged(string theme);
    void PublishUpdate(ReleaseInfoDto? release);
    Task<bool> OpenEditorHelpAsync(CancellationToken cancellationToken = default);
}
