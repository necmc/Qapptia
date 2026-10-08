using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string activeScenario = "update";

// 1. Verificación de versiones con despacho basado en archivos en scenarios/{case}.json
app.MapGet("/api/version", (string? @case) =>
{
    var name = (@case ?? activeScenario).Trim().ToLowerInvariant();
    var file = Path.Combine(app.Environment.ContentRootPath, "scenarios", $"{name}.json");

    if (!File.Exists(file))
    {
        return Results.NotFound(new { error = $"Escenario '{name}' no existe en scenarios/." });
    }

    try
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(file));
        var root = doc.RootElement;

        int statusCode = root.TryGetProperty("statusCode", out var s) ? s.GetInt32() : 200;

        if (root.TryGetProperty("delayMs", out var d) && d.GetInt32() > 0)
        {
            Thread.Sleep(d.GetInt32());
        }

        if (root.TryGetProperty("rawBody", out var raw))
        {
            return Results.Content(raw.GetString() ?? string.Empty, "application/json", statusCode: statusCode);
        }

        if (root.TryGetProperty("body", out var body))
        {
            return Results.Content(body.GetRawText(), "application/json", statusCode: statusCode);
        }

        return Results.StatusCode(statusCode);
    }
    catch (JsonException ex)
    {
        return Results.Problem($"El archivo de escenario '{name}.json' contiene JSON malformado: {ex.Message}");
    }
});

// Permite cambiar el escenario activo global para llamadas sin parámetros de Qapptia
app.MapPost("/api/version", (string @case) =>
{
    var name = @case.Trim().ToLowerInvariant();
    var file = Path.Combine(app.Environment.ContentRootPath, "scenarios", $"{name}.json");

    if (!File.Exists(file))
    {
        return Results.NotFound(new { error = $"Escenario '{name}' no existe en scenarios/." });
    }

    activeScenario = name;
    return Results.Ok(new { message = $"Escenario activo cambiado a '{activeScenario}'." });
});

// 2. Página web de descarga de actualizaciones (HTML básico)
app.MapGet("/download", () =>
{
    var file = Path.Combine(app.Environment.ContentRootPath, "pages", "download.html");
    return File.Exists(file)
        ? Results.Content(File.ReadAllText(file), "text/html; charset=utf-8")
        : Results.NotFound("Página 'pages/download.html' no encontrada.");
});

// 3. Página web de patrocinio (HTML básico)
app.MapGet("/sponsor", () =>
{
    var file = Path.Combine(app.Environment.ContentRootPath, "pages", "sponsor.html");
    return File.Exists(file)
        ? Results.Content(File.ReadAllText(file), "text/html; charset=utf-8")
        : Results.NotFound("Página 'pages/sponsor.html' no encontrada.");
});

app.Run();
