using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Qapptia.Core.Ipc;
using Qapptia.Core.Launcher;
using Qapptia.Launcher.Common;

namespace Qapptia.Launcher.Views;

public partial class SplashWindow : Window
{
    private static readonly CompositeFormat PercentCompositeFormat = CompositeFormat.Parse(Constants.PercentFormat);

    private readonly Stopwatch _lifetimeStopwatch = Stopwatch.StartNew();
    private readonly DispatcherTimer _animationTimer;
    private readonly TextBlock? _statusTextBlock;
    private readonly TextBlock? _percentTextBlock;
    private readonly ProgressBar? _loadingProgressBar;

    private double _targetPercent = Constants.InitialProgressPercent;
    private double _currentPercent = Constants.InitialProgressPercent;

    public SplashWindow()
    {
        InitializeComponent();

        _statusTextBlock = this.FindControl<TextBlock>("StatusTextBlock");
        _percentTextBlock = this.FindControl<TextBlock>("PercentTextBlock");
        _loadingProgressBar = this.FindControl<ProgressBar>("LoadingProgressBar");

        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(Constants.AnimationTimerIntervalMs)
        };
        _animationTimer.Tick += OnAnimationTick;
        _animationTimer.Start();

        // Permitir arrastrar la ventana con el puntero
        PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnClosed(EventArgs e)
    {
        _animationTimer.Stop();
        base.OnClosed(e);
    }

    private void OnAnimationTick(object? sender, EventArgs e)
    {
        if (Math.Abs(_currentPercent - _targetPercent) < Constants.LerpThreshold)
        {
            _currentPercent = _targetPercent;
        }
        else
        {
            _currentPercent += (_targetPercent - _currentPercent) * Constants.SmoothLerpFactor;
        }

        if (_loadingProgressBar != null)
        {
            _loadingProgressBar.Value = _currentPercent;
        }

        if (_percentTextBlock != null)
        {
            _percentTextBlock.Text = string.Format(
                CultureInfo.InvariantCulture,
                PercentCompositeFormat,
                (int)Math.Round(_currentPercent));
        }
    }

    /// <summary>
    /// Actualiza de forma segura en el hilo de UI el avance porcentual y texto de estado.
    /// </summary>
    public void UpdateProgress(double percent, string statusMessage, bool forceImmediate = false)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _targetPercent = Math.Clamp(percent, 0, Constants.CompleteProgressPercent);
            if (forceImmediate || percent >= Constants.CompleteProgressPercent)
            {
                _currentPercent = _targetPercent;
                if (_loadingProgressBar != null)
                {
                    _loadingProgressBar.Value = _currentPercent;
                }
                if (_percentTextBlock != null)
                {
                    _percentTextBlock.Text = string.Format(
                        CultureInfo.InvariantCulture,
                        PercentCompositeFormat,
                        (int)Math.Round(_currentPercent));
                }
            }

            if (_statusTextBlock != null && !string.IsNullOrEmpty(statusMessage))
            {
                _statusTextBlock.Text = statusMessage;
            }
        });
    }

    /// <summary>
    /// Ejecuta el flujo integral de lanzamiento orquestado con validación granular de hitos y tiempos perceptibles.
    /// </summary>
    public async Task TrackLaunchFlowAsync(LaunchPlan plan, string baseDirectory, IProcessLauncher launcher)
    {
        // 1. Inicialización en pantalla (5%): La ventana ya está abierta y visible
        UpdateProgress(Constants.InitialProgressPercent, Constants.ProgressStarting);

        // 2. Servicio de captura (35% -> 45%)
        if (plan.ShouldEnsureCapture)
        {
            UpdateProgress(Constants.CaptureProgressPercent, Constants.ProgressCapture);

            if (!launcher.IsInstanceActive(IpcChannels.Capture))
            {
                string capturePath = LauncherOrchestrator.ResolveExecutablePath(baseDirectory, Qapptia.Core.Constants.CaptureExecutableName);
                string workingDir = Path.GetDirectoryName(capturePath) ?? baseDirectory;
                launcher.StartProcess(capturePath, null, workingDir);

                // Verificación activa de arranque de Capture (hasta 150ms)
                for (int i = 0; i < Constants.CaptureCheckMaxAttempts && !launcher.IsInstanceActive(IpcChannels.Capture); i++)
                {
                    await Task.Delay(Constants.CaptureCheckPollIntervalMs).ConfigureAwait(false);
                }
            }
            else if (plan.Target == LaunchTarget.CaptureOnly)
            {
                await launcher.SendWakeUpAsync(IpcChannels.Capture).ConfigureAwait(false);
            }

            UpdateProgress(Constants.CaptureReadyProgressPercent, Constants.ProgressCaptureReady);
            await Task.Delay(Constants.StagePacingDelayMs).ConfigureAwait(false);
        }

        // 3. Proceso del Editor (55%)
        if (plan.ShouldEnsureEditor)
        {
            UpdateProgress(Constants.EditorProgressPercent, Constants.ProgressEditor);

            if (!launcher.IsInstanceActive(IpcChannels.Editor))
            {
                string editorPath = LauncherOrchestrator.ResolveExecutablePath(baseDirectory, Qapptia.Core.Constants.EditorExecutableName);
                string workingDir = Path.GetDirectoryName(editorPath) ?? baseDirectory;
                launcher.StartProcess(editorPath, plan.AdditionalArguments, workingDir);
            }
            else
            {
                await launcher.SendWakeUpAsync(IpcChannels.Editor).ConfigureAwait(false);
            }

            // 4. Sondeo y validación granular de arranque del Editor (70% -> 85% -> 100%)
            await WaitForEditorReadyGranularAsync(IpcChannels.Editor, Constants.SplashWaitTimeoutMs).ConfigureAwait(false);
        }
        else if (plan.ShouldEnsureConfig)
        {
            UpdateProgress(Constants.EditorProgressPercent, Constants.ProgressConfig);
            if (!launcher.IsInstanceActive(IpcChannels.Config))
            {
                string configPath = LauncherOrchestrator.ResolveExecutablePath(baseDirectory, Qapptia.Core.Constants.ConfigExecutableName);
                string workingDir = Path.GetDirectoryName(configPath) ?? baseDirectory;
                launcher.StartProcess(configPath, plan.AdditionalArguments, workingDir);
            }
            else
            {
                await launcher.SendWakeUpAsync(IpcChannels.Config).ConfigureAwait(false);
            }
            UpdateProgress(Constants.CompleteProgressPercent, Constants.ProgressReady, forceImmediate: true);
        }
        else
        {
            UpdateProgress(Constants.CompleteProgressPercent, Constants.ProgressReady, forceImmediate: true);
        }

        // Garantizar tiempo mínimo de permanencia desde que la ventana se abrió
        long elapsedTotal = _lifetimeStopwatch.ElapsedMilliseconds;
        if (elapsedTotal < Constants.SplashMinimumDisplayMs)
        {
            await Task.Delay((int)(Constants.SplashMinimumDisplayMs - elapsedTotal)).ConfigureAwait(false);
        }

        UpdateProgress(Constants.CompleteProgressPercent, Constants.ProgressReady, forceImmediate: true);
        await Task.Delay(Constants.SplashExitDelayMs).ConfigureAwait(false);
    }

    private async Task WaitForEditorReadyGranularAsync(string channel, int timeoutMs)
    {
        var waitSw = Stopwatch.StartNew();
        bool acknowledged = false;

        while (waitSw.ElapsedMilliseconds < timeoutMs)
        {
            try
            {
                var response = await QapptiaIpcClient.SendAsync(channel, new Ping(), timeoutMs: Constants.SplashIpcTimeoutMs).ConfigureAwait(false);
                if (response is Pong)
                {
                    UpdateProgress(Constants.CompleteProgressPercent, Constants.ProgressReady, forceImmediate: true);
                    break;
                }
                else if (response is Ack)
                {
                    if (!acknowledged)
                    {
                        acknowledged = true;
                        // El Editor respondió Ack: runtime .NET cargado, IPC activo, cargando servicios y configuración
                        UpdateProgress(Constants.ServicesLoadingProgressPercent, Constants.ProgressServices);
                    }
                }
            }
            catch
            {
                // El socket aún está en fase de arranque del runtime CLR
            }

            long waitElapsed = waitSw.ElapsedMilliseconds;

            if (!acknowledged)
            {
                // Mientras espera la primera respuesta del runtime, avanza suavemente hacia 65%
                if (waitElapsed > 350 && _targetPercent < Constants.EditorLoadingProgressPercent)
                {
                    UpdateProgress(Constants.EditorLoadingProgressPercent, Constants.ProgressEditorLoading);
                }
            }
            else
            {
                // Mientras el Editor prepara la interfaz Avalonia tras cargar servicios, avanza hacia 88%
                if (waitElapsed > 1800 && _targetPercent < Constants.InterfaceLoadingProgressPercent)
                {
                    UpdateProgress(Constants.InterfaceLoadingProgressPercent, Constants.ProgressUi);
                }
                // Si la máquina requiere más tiempo (e.g. 4GB RAM), avanzar suavemente hacia 94% mientras se completa el renderizado visual
                else if (waitElapsed > 3500 && _targetPercent < Constants.PreparingUiProgressPercent)
                {
                    UpdateProgress(Constants.PreparingUiProgressPercent, Constants.ProgressPreparingUi);
                }
            }

            await Task.Delay(Constants.SplashPollIntervalMs).ConfigureAwait(false);
        }
    }
}
