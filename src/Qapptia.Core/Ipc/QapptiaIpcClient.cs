using System.IO.Pipes;

namespace Qapptia.Core.Ipc;

/// <summary>
/// Cliente de Named Pipes unificado para comunicación interproceso.
/// Lee el estado persistido del canal o utiliza el nombre de pipe por convención,
/// envía un mensaje de protocolo y espera la respuesta de forma asíncrona.
/// </summary>
public static class QapptiaIpcClient
{
    public static async Task<IpcMessage?> SendAsync(
        string channel,
        IpcMessage request,
        int timeoutMs = 1000,
        CancellationToken ct = default)
    {
        var pipeName = IpcChannels.GetPipeName(channel);
        return await SendToPipeAsync(pipeName, request, timeoutMs, ct).ConfigureAwait(false);
    }

    public static async Task<IpcMessage?> SendToPipeAsync(
        string pipeName,
        IpcMessage request,
        int timeoutMs = 1000,
        CancellationToken ct = default)
    {
        using var client = new NamedPipeClientStream(
            ".",
            pipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous);

        try
        {
            using var timeoutCts = new CancellationTokenSource(timeoutMs);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);
            await client.ConnectAsync(linkedCts.Token).ConfigureAwait(false);

            await IpcWire.WriteFrameAsync(client, request, linkedCts.Token).ConfigureAwait(false);
            return await IpcWire.ReadFrameAsync(client, linkedCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { return null; }
        catch (TimeoutException) { return null; }
        catch (IOException) { return null; }
        catch (Exception) { return null; }
    }
}
