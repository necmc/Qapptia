using System.Text.Json.Serialization;

namespace Qapptia.Core.Configuration;

/// <summary>
/// Configuración específica del Editor de Qapptia.
/// </summary>
public sealed partial class QapptiaConfig
{
    [JsonPropertyName("show_editor_help_on_startup")]
    public bool ShowEditorHelpOnStartup { get; set; } = true;
}
