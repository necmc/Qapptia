using System;
using System.Threading;
using System.Threading.Tasks;
using Qapptia.Core.Abstractions;
using Serilog;

namespace Qapptia.Core.Services;

/// <summary>
/// Coordina la ejecución periódica y resiliente de comprobaciones de actualización en segundo plano.
/// Centraliza la temporización, el retardo de arranque y los reintentos ante fallos para toda la suite de Qapptia.
/// Compatible con TimeProvider para pruebas unitarias instantáneas en 0 ms.
/// </summary>
public sealed class PeriodicUpdateCoordinator : IDisposable
{
    private readonly IUpdateCheckService _updateCheckService;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger _logger;
    private readonly object _syncRoot = new();

    private CancellationTokenSource? _cts;
    private Task? _runningTask;

    /// <summary>
    /// Evento disparado cada vez que finaliza una comprobación de actualización.
    /// </summary>
    public event Action<UpdateCheckResult>? CheckCompleted;

    /// <summary>
    /// Indica si el bucle de comprobaciones periódicas está actualmente activo.
    /// </summary>
    public bool IsRunning
    {
        get
        {
            lock (_syncRoot)
            {
                return _cts != null && !_cts.IsCancellationRequested;
            }
        }
    }

    public PeriodicUpdateCoordinator(
        IUpdateCheckService updateCheckService,
        TimeProvider? timeProvider = null,
        ILogger? logger = null)
    {
        _updateCheckService = updateCheckService ?? throw new ArgumentNullException(nameof(updateCheckService));
        _timeProvider = timeProvider ?? TimeProvider.System;
        _logger = (logger ?? Log.Logger).ForContext<PeriodicUpdateCoordinator>();
    }

    /// <summary>
    /// Inicia el bucle de comprobaciones periódicas con un retardo inicial opcional.
    /// Aplica dispersión aleatoria (Jitter) al retardo de arranque predeterminado para mitigar picos de concurrencia.
    /// Si ya estaba en ejecución, detiene la instancia previa de forma segura.
    /// </summary>
    public void Start(TimeSpan? initialDelay = null)
    {
        lock (_syncRoot)
        {
            StopInternal();
            _cts = new CancellationTokenSource();
            var delay = initialDelay ?? (TimeSpan.FromMilliseconds(Constants.StartupUpdateCheckDelayMs)
                + TimeSpan.FromMilliseconds(Random.Shared.Next(0, Constants.StartupUpdateCheckMaxJitterMs)));
            _runningTask = RunLoopAsync(delay, _cts.Token);
        }
    }

    /// <summary>
    /// Detiene de inmediato el bucle de comprobaciones periódicas.
    /// </summary>
    public void Stop()
    {
        lock (_syncRoot)
        {
            StopInternal();
        }
    }

    private void StopInternal()
    {
        if (_cts != null)
        {
            try
            {
                _cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // Ignorar si ya fue desechado
            }
            _cts.Dispose();
            _cts = null;
        }
    }

    private async Task RunLoopAsync(TimeSpan initialDelay, CancellationToken ct)
    {
        try
        {
            await Task.Delay(initialDelay, _timeProvider, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        while (!ct.IsCancellationRequested)
        {
            TimeSpan nextInterval;
            try
            {
                var result = await _updateCheckService.CheckForUpdatesAsync(ct: ct).ConfigureAwait(false);
                CheckCompleted?.Invoke(result);

                if (result.Status == UpdateCheckStatus.UpdateAvailable || result.Status == UpdateCheckStatus.UpToDate)
                {
                    nextInterval = GetPeriodicIntervalWithJitter();
                }
                else
                {
                    nextInterval = GetRetryIntervalWithJitter();
                    _logger.Information("La comprobación de actualización no fue concluyente ({Status}). Reintentando en {RetryInterval}.",
                        result.Status, nextInterval);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                nextInterval = GetRetryIntervalWithJitter();
                _logger.Warning(ex, "Error durante la comprobación de actualización periódica. Reintentando en {RetryInterval}.", nextInterval);
            }

            try
            {
                await Task.Delay(nextInterval, _timeProvider, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private static TimeSpan GetPeriodicIntervalWithJitter()
    {
        var maxJitterSeconds = (int)Constants.PeriodicUpdateCheckMaxJitter.TotalSeconds;
        var jitterSeconds = Random.Shared.Next(-maxJitterSeconds, maxJitterSeconds + 1);
        return Constants.PeriodicUpdateCheckInterval + TimeSpan.FromSeconds(jitterSeconds);
    }

    private static TimeSpan GetRetryIntervalWithJitter()
    {
        var maxJitterSeconds = (int)Constants.FailedUpdateCheckRetryMaxJitter.TotalSeconds;
        var jitterSeconds = Random.Shared.Next(-maxJitterSeconds, maxJitterSeconds + 1);
        return Constants.FailedUpdateCheckRetryInterval + TimeSpan.FromSeconds(jitterSeconds);
    }

    public void Dispose()
    {
        Stop();
    }
}
