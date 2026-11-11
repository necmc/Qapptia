using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Qapptia.Core.Theme;

namespace Qapptia.Core.Configuration;

/// <summary>
/// Preferencias generales de la aplicación y de guardado de capturas.
/// </summary>
public sealed class GeneralConfig
{
    [JsonPropertyName("theme")]
    public string Theme { get; set; } = ThemeConstants.System;

    [JsonPropertyName("auto_start")]
    public bool AutoStart { get; set; } = false;

    [JsonPropertyName("save_path")]
    [Required]
    public string SavePath { get; set; } = Constants.DefaultSavePath;

    [JsonPropertyName("filename_format")]
    [Required]
    public string FilenameFormat { get; set; } = Constants.DefaultFilenameFormat;

    [JsonPropertyName("subfolder_month")]
    public bool SubfolderMonth { get; set; } = true;

    [JsonPropertyName("subfolder_day")]
    public bool SubfolderDay { get; set; } = true;

    [JsonPropertyName("subfolder_hour")]
    public bool SubfolderHour { get; set; } = false;
}
