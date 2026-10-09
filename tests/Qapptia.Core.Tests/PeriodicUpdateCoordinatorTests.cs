using System;
using System.Threading;
using System.Threading.Tasks;
using Qapptia.Core.Abstractions;
using Qapptia.Core.Services;
using Xunit;

namespace Qapptia.Core.Tests;

public sealed class PeriodicUpdateCoordinatorTests
{
    private sealed class MockUpdateCheckService : IUpdateCheckService
    {
        private readonly Func<CancellationToken, Task<UpdateCheckResult>> _handler;

        public MockUpdateCheckService(Func<CancellationToken, Task<UpdateCheckResult>> handler)
        {
            _handler = handler;
        }

        public Task<UpdateCheckResult> CheckForUpdatesAsync(bool forceCheck = false, CancellationToken ct = default)
            => _handler(ct);
    }

    [Fact]
    public async Task StartShouldExecuteInitialCheckAndFireEvent()
    {
        var release = new ReleaseInfoDto { Version = "99.0.0", DownloadUrl = "https://example.com/dl" };
        var expectedResult = UpdateCheckResult.UpdateAvailable("2.0.0", release);

        var service = new MockUpdateCheckService(_ => Task.FromResult(expectedResult));
        using var coordinator = new PeriodicUpdateCoordinator(service);

        var tcs = new TaskCompletionSource<UpdateCheckResult>();
        coordinator.CheckCompleted += result => tcs.TrySetResult(result);

        coordinator.Start(TimeSpan.FromMilliseconds(10));
        Assert.True(coordinator.IsRunning);

        var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(2000, TestContext.Current.CancellationToken));
        Assert.Same(tcs.Task, completedTask);

        var result = await tcs.Task;
        Assert.Equal(UpdateCheckStatus.UpdateAvailable, result.Status);
        Assert.Equal("99.0.0", result.LatestRelease?.Version);

        coordinator.Stop();
        Assert.False(coordinator.IsRunning);
    }

    [Fact]
    public async Task StartWhenFailedCheckShouldFireEventAndContinue()
    {
        var expectedResult = UpdateCheckResult.Failed("2.0.0", "Servidor no disponible (503).");

        var service = new MockUpdateCheckService(_ => Task.FromResult(expectedResult));
        using var coordinator = new PeriodicUpdateCoordinator(service);

        var tcs = new TaskCompletionSource<UpdateCheckResult>();
        coordinator.CheckCompleted += result => tcs.TrySetResult(result);

        coordinator.Start(TimeSpan.FromMilliseconds(10));
        Assert.True(coordinator.IsRunning);

        var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(2000, TestContext.Current.CancellationToken));
        Assert.Same(tcs.Task, completedTask);

        var result = await tcs.Task;
        Assert.Equal(UpdateCheckStatus.ConnectionFailed, result.Status);
        Assert.Equal("Servidor no disponible (503).", result.ErrorMessage);

        coordinator.Stop();
        Assert.False(coordinator.IsRunning);
    }

    [Fact]
    public void StopShouldCancelLoopImmediately()
    {
        var service = new MockUpdateCheckService(_ => Task.FromResult(UpdateCheckResult.UpToDate("2.0.0", null)));
        using var coordinator = new PeriodicUpdateCoordinator(service);

        // Retardo inicial largo de 1 hora
        coordinator.Start(TimeSpan.FromHours(1));
        Assert.True(coordinator.IsRunning);

        coordinator.Stop();
        Assert.False(coordinator.IsRunning);
    }

    [Fact]
    public void StartWhenAlreadyRunningRestartsCleanly()
    {
        var service = new MockUpdateCheckService(_ => Task.FromResult(UpdateCheckResult.UpToDate("2.0.0", null)));
        using var coordinator = new PeriodicUpdateCoordinator(service);

        coordinator.Start(TimeSpan.FromHours(1));
        Assert.True(coordinator.IsRunning);

        // Reinicio mientras está en ejecución
        coordinator.Start(TimeSpan.FromHours(2));
        Assert.True(coordinator.IsRunning);

        coordinator.Stop();
        Assert.False(coordinator.IsRunning);
    }

    [Fact]
    public void DisposeShouldStopCoordinator()
    {
        var service = new MockUpdateCheckService(_ => Task.FromResult(UpdateCheckResult.UpToDate("2.0.0", null)));
        var coordinator = new PeriodicUpdateCoordinator(service);

        coordinator.Start(TimeSpan.FromHours(1));
        Assert.True(coordinator.IsRunning);

        coordinator.Dispose();
        Assert.False(coordinator.IsRunning);
    }

    [Fact]
    public void StartWithoutExplicitDelayShouldStartWithJitterDelay()
    {
        var service = new MockUpdateCheckService(_ => Task.FromResult(UpdateCheckResult.UpToDate("2.0.0", null)));
        using var coordinator = new PeriodicUpdateCoordinator(service);

        coordinator.Start();
        Assert.True(coordinator.IsRunning);

        coordinator.Stop();
        Assert.False(coordinator.IsRunning);
    }
}
