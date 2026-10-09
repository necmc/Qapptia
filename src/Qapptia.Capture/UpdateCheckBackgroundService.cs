using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Qapptia.Core;
using Qapptia.Core.Abstractions;
using Qapptia.Core.Services;
using Serilog;

namespace Qapptia.Capture;

/// <summary>
/// Servicio de fondo que verifica periódicamente si existe una nueva versión de Qapptia mientras el capturador está activo.
/// Delega la temporización, el retardo inicial y los reintentos al componente transversal PeriodicUpdateCoordinator.
/// </summary>
public sealed class UpdateCheckBackgroundService : BackgroundService
{
    private static readonly System.Text.CompositeFormat s_updateMessageFormat = System.Text.CompositeFormat.Parse(Constants.NotificationMessageUpdateAvailableFormat);

    private readonly PeriodicUpdateCoordinator _coordinator;
    private readonly ITrayIconService? _trayIconService;
    private readonly ILogger _logger;

    public UpdateCheckBackgroundService(
        IUpdateCheckService updateCheckService,
        ITrayIconService? trayIconService = null,
        TimeProvider? timeProvider = null,
        ILogger? logger = null)
    {
        _logger = (logger ?? Log.Logger).ForContext<UpdateCheckBackgroundService>();
        _trayIconService = trayIconService;
        _coordinator = new PeriodicUpdateCoordinator(updateCheckService, timeProvider, _logger);
        _coordinator.CheckCompleted += OnCheckCompleted;
    }

    private void OnCheckCompleted(UpdateCheckResult result)
    {
        if (result.Status == UpdateCheckStatus.UpdateAvailable && result.LatestRelease != null)
        {
            _logger.Information("Nueva versión de Qapptia disponible: {Version}", result.LatestRelease.Version);
            InMemoryUpdateStateStore.SetUpdate(result.LatestRelease.Version, result.LatestRelease.DownloadUrl);

            var message = string.Format(System.Globalization.CultureInfo.InvariantCulture, s_updateMessageFormat, result.LatestRelease.Version);
            _trayIconService?.ShowNotification(
                Constants.NotificationTitleUpdateAvailable,
                message,
                TrayNotificationType.Info,
                Constants.NotificationDurationMs);

            _ = Task.Run(async () =>
            {
                try
                {
                    await Qapptia.Core.Ipc.QapptiaIpcClient.SendAsync(
                        Qapptia.Core.Ipc.IpcChannels.Editor,
                        new Qapptia.Core.Ipc.UpdateAvailableNotification
                        {
                            Version = result.LatestRelease.Version,
                            DownloadUrl = result.LatestRelease.DownloadUrl
                        },
                        timeoutMs: Constants.DefaultIpcTimeoutMs).ConfigureAwait(false);
                }
                catch
                {
                    // Si Editor no está en ejecución, se ignora silenciosamente. Editor consultará el estado al abrirse.
                }
            });
        }
        else
        {
            InMemoryUpdateStateStore.Clear();
            _ = Task.Run(async () =>
            {
                try
                {
                    await Qapptia.Core.Ipc.QapptiaIpcClient.SendAsync(
                        Qapptia.Core.Ipc.IpcChannels.Editor,
                        new Qapptia.Core.Ipc.UpdateClearedNotification(),
                        timeoutMs: Constants.DefaultIpcTimeoutMs).ConfigureAwait(false);
                }
                catch
                {
                    // Si Editor no está en ejecución, se ignora silenciosamente
                }
            });
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Information("Servicio de verificación de actualizaciones en segundo plano iniciado.");
        _coordinator.Start();

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Apagado normal del host
        }
        finally
        {
            _coordinator.Stop();
        }
    }

    public override void Dispose()
    {
        _coordinator.CheckCompleted -= OnCheckCompleted;
        _coordinator.Dispose();
        base.Dispose();
    }
}
