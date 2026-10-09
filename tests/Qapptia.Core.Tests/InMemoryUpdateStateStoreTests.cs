using Qapptia.Core.Services;
using Xunit;

namespace Qapptia.Core.Tests;

public sealed class InMemoryUpdateStateStoreTests
{
    [Fact]
    public void InitialOrClearedStateShouldBeEmpty()
    {
        InMemoryUpdateStateStore.Clear();

        Assert.False(InMemoryUpdateStateStore.HasUpdate);
        Assert.Null(InMemoryUpdateStateStore.Current);
    }

    [Fact]
    public void SetUpdateWithHigherVersionShouldStoreAndTriggerEvent()
    {
        InMemoryUpdateStateStore.Clear();

        bool eventFired = false;
        UpdateStateInfo? capturedInfo = null;

        void Handler(UpdateStateInfo? info)
        {
            eventFired = true;
            capturedInfo = info;
        }

        InMemoryUpdateStateStore.StateChanged += Handler;
        try
        {
            InMemoryUpdateStateStore.SetUpdate("99.0.0", "https://example.com/download");

            Assert.True(InMemoryUpdateStateStore.HasUpdate);
            Assert.NotNull(InMemoryUpdateStateStore.Current);
            Assert.Equal("99.0.0", InMemoryUpdateStateStore.Current.Version);
            Assert.Equal("https://example.com/download", InMemoryUpdateStateStore.Current.DownloadUrl);
            Assert.True(eventFired);
            Assert.NotNull(capturedInfo);
            Assert.Equal("99.0.0", capturedInfo.Version);
        }
        finally
        {
            InMemoryUpdateStateStore.StateChanged -= Handler;
            InMemoryUpdateStateStore.Clear();
        }
    }

    [Fact]
    public void SetUpdateWithLowerOrEqualVersionShouldNotStore()
    {
        InMemoryUpdateStateStore.Clear();

        // 0.1.0 es inferior a la versión actual de la aplicación
        InMemoryUpdateStateStore.SetUpdate("0.1.0", "https://example.com/download");

        Assert.False(InMemoryUpdateStateStore.HasUpdate);
        Assert.Null(InMemoryUpdateStateStore.Current);
    }

    [Fact]
    public void ClearShouldResetStateAndNotify()
    {
        InMemoryUpdateStateStore.SetUpdate("99.0.0", "https://example.com/download");
        Assert.True(InMemoryUpdateStateStore.HasUpdate);

        bool eventFired = false;
        UpdateStateInfo? capturedInfo = "dummy" != null ? new UpdateStateInfo("dummy", null) : null;

        void Handler(UpdateStateInfo? info)
        {
            eventFired = true;
            capturedInfo = info;
        }

        InMemoryUpdateStateStore.StateChanged += Handler;
        try
        {
            InMemoryUpdateStateStore.Clear();

            Assert.False(InMemoryUpdateStateStore.HasUpdate);
            Assert.Null(InMemoryUpdateStateStore.Current);
            Assert.True(eventFired);
            Assert.Null(capturedInfo);
        }
        finally
        {
            InMemoryUpdateStateStore.StateChanged -= Handler;
        }
    }
}
