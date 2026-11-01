using Avalonia.Threading;
using Qapptia.App.Config.ViewModels;
using Qapptia.Core.Abstractions;
using Qapptia.Core.Configuration;
using Qapptia.Core.Services;
using Qapptia.Editor.Tests.Helpers;

namespace Qapptia.Editor.Tests.ViewModels;

public sealed class ConfigAutoStartTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SystemQueryDoesNotBlockUiOrSavingOtherPreferences(bool enabled)
    {
        await SharedHeadlessSession.Session.Dispatch(async () =>
        {
            using var releaseRead = new ManualResetEventSlim();
            var readStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var service = CreateService(new(AutoStartScope.CurrentUser, enabled));
            service.Setup(s => s.GetState()).Returns(() =>
            {
                readStarted.TrySetResult();
                if (!releaseRead.Wait(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken))
                    throw new TimeoutException("No se liberó la consulta de prueba.");
                return new AutoStartState(AutoStartScope.CurrentUser, enabled);
            });
            var config = new RecordingConfigService();
            config.Current.AutoStart = !enabled;
            using var vm = CreateViewModel(config, service.Object);
            var loaded = ObserveLoadCompletion(vm);
            try
            {
                await readStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
                vm.CanEditAutoStart.Should().BeFalse();
                vm.SaveCommand.CanExecute(null).Should().BeTrue();
                var responsive = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                Dispatcher.UIThread.Post(() =>
                {
                    vm.FilenameFormat = "Captura_YYYYMMDD";
                    responsive.TrySetResult();
                });
                await responsive.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
                await vm.SaveCommand.ExecuteAsync(null);
                config.Saved.Should().Be((!enabled, "Captura_YYYYMMDD"));
            }
            finally
            {
                releaseRead.Set();
            }
            await loaded.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            vm.AutoStart.Should().Be(enabled);
            vm.CanEditAutoStart.Should().BeTrue();
            config.Current.AutoStart.Should().Be(!enabled);
            service.Verify(s => s.SetEnabledAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        }, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(false, true)]
    public async Task ConfirmationWaitsForSystemChangeAndReportsItsResult(bool succeeded, bool cancelled)
    {
        await SharedHeadlessSession.Session.Dispatch(async () =>
        {
            var state = new AutoStartState(AutoStartScope.AllUsers, false);
            var service = CreateService(state);
            var completion = new TaskCompletionSource<AutoStartChangeResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            service.Setup(s => s.SetEnabledAsync(true, It.IsAny<CancellationToken>()))
                .Returns(completion.Task);
            var config = new RecordingConfigService();
            using var vm = CreateViewModel(config, service.Object);
            await ObserveLoadCompletion(vm).WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            await vm.SaveCommand.ExecuteAsync(null);
            vm.AutoStart = true;
            vm.FilenameFormat = "Otro_formato";
            var saving = vm.SaveCommand.ExecuteAsync(null);

            try
            {
                saving.IsCompleted.Should().BeFalse();
                vm.IsSaving.Should().BeTrue();
                vm.SaveCommand.CanExecute(null).Should().BeFalse();
                vm.FooterMessage.Should().Be(Qapptia.App.Config.Common.Constants.FooterConfigSaving);
                vm.IsFooterError.Should().BeFalse();
                config.Saved.Should().Be((false, "Otro_formato"));
            }
            finally
            {
                completion.TrySetResult(new(succeeded, cancelled, state with { IsEnabled = succeeded }));
                await saving.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            }

            vm.AutoStart.Should().Be(succeeded);
            vm.IsFooterError.Should().Be(!succeeded);
            vm.FooterMessage.Should().Be(succeeded
                ? Qapptia.App.Config.Common.Constants.FooterConfigSaved
                : cancelled ? Qapptia.App.Config.Common.Constants.FooterAutoStartCancelled
                    : Qapptia.App.Config.Common.Constants.FooterAutoStartSaveError);
            config.Saved.Should().Be((succeeded, "Otro_formato"));
            vm.SaveCommand.CanExecute(null).Should().BeTrue();
        }, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PortableAndReadErrorNeverChangeStartupWhenSavingOtherPreferences(bool readError)
    {
        await SharedHeadlessSession.Session.Dispatch(async () =>
        {
            var state = readError ? new AutoStartState(AutoStartScope.Unknown, false, AutoStartError.RegistryReadFailed)
                : new AutoStartState(AutoStartScope.Portable, false);
            var service = CreateService(state);
            var config = new RecordingConfigService();
            config.Current.AutoStart = true;
            using var vm = CreateViewModel(config, service.Object);
            await ObserveLoadCompletion(vm).WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            vm.AutoStart.Should().BeFalse();
            vm.CanEditAutoStart.Should().BeFalse();
            vm.AutoStartStatusMessage.Should().Be(readError
                ? Qapptia.App.Config.Common.Constants.AutoStartErrorQuery
                : Qapptia.App.Config.Common.Constants.AutoStartPortableDescription);
            vm.FilenameFormat = "Portable";
            await vm.SaveCommand.ExecuteAsync(null);
            config.Saved.Should().Be((true, "Portable"));
            config.Current.AutoStart.Should().BeTrue();
            service.Verify(s => s.SetEnabledAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        }, TestContext.Current.CancellationToken);
    }

    private static ConfigViewModel CreateViewModel(IConfigService config, IAutoStartService autoStart)
    {
        var processes = new Mock<IConfigProcessService>().Object;
        return new ConfigViewModel(new ConfigSettingsService(config, autoStart, processes),
            new Mock<IUpdateCheckService>().Object, NullShellService.Instance, processes);
    }

    private static Mock<IAutoStartService> CreateService(AutoStartState state)
    {
        var service = new Mock<IAutoStartService>();
        service.SetupGet(s => s.IsSupported).Returns(true);
        service.Setup(s => s.GetState()).Returns(state);
        return service;
    }

    private sealed class RecordingConfigService : IConfigService
    {
        public QapptiaConfig Current { get; } = new() { SavePath = Path.GetTempPath() };
        public (bool AutoStart, string FilenameFormat)? Saved { get; private set; }
        public void Save() => Saved = (Current.AutoStart, Current.FilenameFormat);
        public void Reload() => throw new NotSupportedException();
    }

    private static Task ObserveLoadCompletion(ConfigViewModel vm)
    {
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.IsLoadingAutoStart) && !vm.IsLoadingAutoStart)
                completion.TrySetResult();
        };
        if (!vm.IsLoadingAutoStart) completion.TrySetResult();
        return completion.Task;
    }

}
