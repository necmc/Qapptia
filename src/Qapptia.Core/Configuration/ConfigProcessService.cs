using System.Diagnostics;
using Qapptia.Core.Abstractions;
using Qapptia.Core.Ipc;
using Qapptia.Core.Services;

namespace Qapptia.Core.Configuration;

/// <summary>Centraliza el IPC y el lanzamiento de procesos sin depender de la interfaz.</summary>
public sealed class ConfigProcessService : IConfigProcessService
{
    public void NotifyPreferencesChanged(string theme)
    {
        // Difundir notificación de tema y refresco a Capture y Editor en caliente.
        Broadcast(new ThemeChangedNotification { Theme = theme });
        Broadcast(new RefreshTrayIconRequest());
    }

    public void PublishUpdate(ReleaseInfoDto? release)
    {
        if (release != null)
        {
            var url = string.IsNullOrWhiteSpace(release.SiteDownload) ? Constants.DefaultDownloadUrl : release.SiteDownload;
            InMemoryUpdateStateStore.SetUpdate(release.Version, url);
            Broadcast(new UpdateAvailableNotification { Version = release.Version, DownloadUrl = url }, Constants.QuickIpcTimeoutMs);
        }
        else
        {
            InMemoryUpdateStateStore.Clear();
            Broadcast(new UpdateClearedNotification(), Constants.QuickIpcTimeoutMs);
        }
    }

    private static void Broadcast(IpcMessage message, int timeoutMs = Constants.DefaultIpcTimeoutMs)
    {
        foreach (string channel in new[] { IpcChannels.Capture, IpcChannels.Editor })
        {
            _ = Task.Run(async () =>
            {
                try { await QapptiaIpcClient.SendAsync(channel, message, timeoutMs: timeoutMs).ConfigureAwait(false); }
                catch { /* Si la aplicación no está en ejecución, se omite silenciosamente. */ }
            });
        }
    }

    public async Task<bool> OpenEditorHelpAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await QapptiaIpcClient.SendAsync(IpcChannels.Editor, new ShowHelpRequest(),
                timeoutMs: Constants.QuickIpcTimeoutMs, ct: cancellationToken).ConfigureAwait(false);
            if (response is Ack) return true;
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            // Fallback si el Editor no está abierto o no responde a tiempo.
        }

        cancellationToken.ThrowIfCancellationRequested();
        string path = Path.Combine(AppContext.BaseDirectory, Constants.EditorExecutableName);
        if (!File.Exists(path)) return false;
        using var process = Process.Start(new ProcessStartInfo(path, Constants.ArgHelp) { UseShellExecute = true });
        return true;
    }
}
