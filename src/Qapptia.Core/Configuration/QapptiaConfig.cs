using System.Text.Json.Serialization;

namespace Qapptia.Core.Configuration;

/// <summary>
/// Configuración de Qapptia agrupada por función.
/// </summary>
public sealed class QapptiaConfig
{
    private const int SchemaVersion = 1;

    [JsonPropertyName("version")]
    public int Version { get; } = SchemaVersion;

    [JsonPropertyName("general")]
    public GeneralConfig General { get; set; } = new();

    [JsonPropertyName("capture")]
    public CaptureConfig Capture { get; set; } = new();

    [JsonPropertyName("editor")]
    public EditorConfig Editor { get; set; } = new();
}
