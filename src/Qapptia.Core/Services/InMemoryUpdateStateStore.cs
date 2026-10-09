using System;
using Qapptia.Core;

namespace Qapptia.Core.Services;

/// <summary>
/// Representa la información en memoria sobre una actualización disponible.
/// </summary>
public sealed record UpdateStateInfo(string Version, string? DownloadUrl);

/// <summary>
/// Almacén estático y en memoria del estado de actualizaciones para la sesión actual.
/// Seguro para subprocesos concurrentes y sin persistencia en disco, garantizando datos limpios y fidedignos en cada arranque.
/// </summary>
public static class InMemoryUpdateStateStore
{
    private static readonly object s_lock = new();
    private static UpdateStateInfo? s_current;

    /// <summary>
    /// Evento disparado cuando el estado en memoria cambia durante la sesión actual.
    /// </summary>
    public static event Action<UpdateStateInfo?>? StateChanged;

    /// <summary>
    /// Indica si hay una actualización disponible en memoria.
    /// </summary>
    public static bool HasUpdate
    {
        get
        {
            lock (s_lock)
            {
                return s_current != null;
            }
        }
    }

    /// <summary>
    /// Obtiene el estado actual de la actualización en memoria, si existe.
    /// </summary>
    public static UpdateStateInfo? Current
    {
        get
        {
            lock (s_lock)
            {
                return s_current;
            }
        }
    }

    /// <summary>
    /// Establece o actualiza la versión disponible en memoria.
    /// Valida que la versión sea superior a la versión actual de la aplicación.
    /// </summary>
    public static void SetUpdate(string version, string? downloadUrl)
    {
        if (string.IsNullOrWhiteSpace(version))
            return;

        if (SemanticVersion.TryParse(version, out var remoteVer) &&
            SemanticVersion.TryParse(Constants.AppVersion, out var localVer))
        {
            if (remoteVer.CompareTo(localVer) <= 0)
            {
                Clear();
                return;
            }
        }

        var info = new UpdateStateInfo(version.Trim(), downloadUrl?.Trim());
        lock (s_lock)
        {
            s_current = info;
        }

        StateChanged?.Invoke(info);
    }

    /// <summary>
    /// Limpia el estado en memoria.
    /// </summary>
    public static void Clear()
    {
        lock (s_lock)
        {
            s_current = null;
        }

        StateChanged?.Invoke(null);
    }
}
