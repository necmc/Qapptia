namespace Qapptia.Core.Theme;

public static class ThemeConstants
{
    // Códigos internos persistidos en config.json
    public const string Dark = "dark";
    public const string Light = "light";
    public const string System = "system";

    // Normaliza el string de código de tema
    public static string Normalize(string? theme)
    {
        if (string.IsNullOrWhiteSpace(theme)) return System;
        var lower = theme.Trim().ToLowerInvariant();
        return lower switch
        {
            Dark => Dark,
            Light => Light,
            _ => System
        };
    }
}
