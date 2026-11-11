using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Qapptia.Core.Configuration;

/// <summary>
/// Preferencias de captura de pantalla y atajos.
/// </summary>
public sealed class CaptureConfig
{
    // === Cursor ===

    [JsonPropertyName("show_mouse")]
    public bool ShowMouse { get; set; } = true;

    [JsonPropertyName("highlight_mouse")]
    public bool HighlightMouse { get; set; } = false;

    // === Temporizador ===

    [JsonPropertyName("manual_timer")]
    [Range(0, 999)]
    public int ManualTimer { get; set; } = 0;

    // === Atajos (formato keyboard: ctrl+shift+q) ===

    [JsonPropertyName("shortcut_screen")]
    public string ShortcutScreen { get; set; } = Constants.DefaultShortcutScreen;

    [JsonPropertyName("shortcut_area")]
    public string ShortcutArea { get; set; } = Constants.DefaultShortcutArea;

    [JsonPropertyName("shortcut_flow")]
    public string ShortcutFlow { get; set; } = "ctrl+shift+f";

    [JsonPropertyName("shortcut_flow_pause")]
    public string ShortcutFlowPause { get; set; } = "ctrl+shift";

    // === Copiado al portapapeles ===

    [JsonPropertyName("copy_to_clipboard_screen")]
    public bool CopyToClipboardScreen { get; set; } = false;

    [JsonPropertyName("copy_to_clipboard_area")]
    public bool CopyToClipboardArea { get; set; } = false;

    // === Comportamiento de captura ===

    [JsonPropertyName("capture_all_screens")]
    public bool CaptureAllScreens { get; set; } = false;

    [JsonPropertyName("show_capture_indicator")]
    public bool ShowCaptureIndicator { get; set; } = true;

    // === Captura con desplazamiento ===

    [JsonPropertyName("enable_scroll_capture")]
    public bool EnableScrollCapture { get; set; } = false;
}
