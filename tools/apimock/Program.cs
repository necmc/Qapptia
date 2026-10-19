using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Configuración CORS transversal para permitir consumo desde navegador o clientes locales
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
    context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
    context.Response.Headers.Append("Access-Control-Allow-Headers", "*");
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 204;
        return;
    }
    await next();
});

// Estado en memoria del caso de prueba activo
string? activeCase = null;
object stateLock = new();

// Endpoint estándar de versión API v1:
// - Si ?case=xxxx se especifica: fija xxxx como el caso activo del servidor y responde con él.
//   (usar ?case=reset, ?case=default o ?case=latest para restaurar el comportamiento normal de latest.json).
// - Si ?case no se especifica: responde con el caso activo actual (o latest.json si ninguno está activo).
var handleGetVersion = (string? @case) =>
{
    // Si se pasa un nuevo caso, intentamos cambiar el estado del mock
    if (!string.IsNullOrWhiteSpace(@case))
    {
        var target = @case.Trim();
        if (target.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            target = target[..^5];
        }
        var name = target.ToLowerInvariant();

        if (name is "reset" or "default" or "latest")
        {
            lock (stateLock)
            {
                activeCase = null;
            }
            Console.WriteLine("[apimock] Estado reseteado a 'versions/latest.json'");
        }
        else
        {
            var testCaseFile = Path.Combine(app.Environment.ContentRootPath, "versions", "cases", $"{name}.json");
            var testVersionFile = Path.Combine(app.Environment.ContentRootPath, "versions", $"{name}.json");

            if (File.Exists(testCaseFile) || File.Exists(testVersionFile))
            {
                lock (stateLock)
                {
                    activeCase = name;
                }
                Console.WriteLine($"[apimock] Caso activo fijado a '{name}' (las peticiones sin ?case responderán con este caso)");
            }
            else
            {
                return Results.NotFound(new { error = $"Caso de prueba '{name}' no existe en versions/cases/ ni en versions/." });
            }
        }
    }

    // Determinar qué responder según el caso activo
    string? currentCase;
    lock (stateLock)
    {
        currentCase = activeCase;
    }

    // Si no hay caso activo, responde con latest.json
    if (string.IsNullOrWhiteSpace(currentCase))
    {
        var latestFile = Path.Combine(app.Environment.ContentRootPath, "versions", "latest.json");
        return File.Exists(latestFile)
            ? Results.Content(File.ReadAllText(latestFile), "application/json; charset=utf-8", statusCode: 200)
            : Results.NotFound(new { error = "El archivo 'versions/latest.json' no existe." });
    }

    // 1. Buscar en versions/cases/ (casos de prueba y simulación: update, beta, 500, 503, timeout, etc.)
    var caseFile = Path.Combine(app.Environment.ContentRootPath, "versions", "cases", $"{currentCase}.json");
    if (File.Exists(caseFile))
    {
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(caseFile));
            var root = doc.RootElement;

            int statusCode = root.TryGetProperty("statusCode", out var s) ? s.GetInt32() : 200;

            if (root.TryGetProperty("delayMs", out var d) && d.GetInt32() > 0)
            {
                Thread.Sleep(d.GetInt32());
            }

            if (root.TryGetProperty("rawBody", out var raw))
            {
                return Results.Content(raw.GetString() ?? string.Empty, "application/json; charset=utf-8", statusCode: statusCode);
            }

            if (root.TryGetProperty("body", out var body))
            {
                return Results.Content(body.GetRawText(), "application/json; charset=utf-8", statusCode: statusCode);
            }

            return Results.StatusCode(statusCode);
        }
        catch (JsonException ex)
        {
            return Results.Problem($"El archivo del caso '{currentCase}.json' contiene JSON malformado: {ex.Message}");
        }
    }

    // 2. Buscar en versions/ (por si se consulta una versión específica como v2.0.0 o v2.0.0-beta)
    var versionFile = Path.Combine(app.Environment.ContentRootPath, "versions", $"{currentCase}.json");
    if (File.Exists(versionFile))
    {
        var content = File.ReadAllText(versionFile);
        return Results.Content(content, "application/json; charset=utf-8", statusCode: 200);
    }

    return Results.NotFound(new { error = $"El caso activo '{currentCase}' ya no existe en versions/cases/ ni en versions/." });
};

app.MapGet("/api/v1/version", handleGetVersion);
app.MapGet("/api/v1/case", () => Results.Ok(new { activeCase = activeCase ?? "latest" }));

app.Run();

