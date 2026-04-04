// Tests ported from exit.tsx
// Covers: AppLifecycle — exit, waitUntilExit, error handling
using Ink.Net;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Exit / lifecycle tests aligned with JS exit.tsx test suite.</summary>
public class ExitTests
{
    [Fact]
    public void ExitNormally()
    {
        var lifecycle = new AppLifecycle();
        lifecycle.Exit();

        Assert.True(lifecycle.HasExited);
    }

    [Fact]
    public async Task WaitUntilExitResolvesOnExit()
    {
        var lifecycle = new AppLifecycle();
        var task = lifecycle.WaitUntilExit();

        lifecycle.Exit();

        var result = await task;
        Assert.Null(result);
    }

    [Fact]
    public async Task ExitWithResultValue()
    {
        var lifecycle = new AppLifecycle();
        var task = lifecycle.WaitUntilExit();

        lifecycle.Exit("hello from ink");

        var result = await task;
        Assert.Equal("hello from ink", result);
    }

    [Fact]
    public async Task ExitWithObjectResult()
    {
        var lifecycle = new AppLifecycle();
        var task = lifecycle.WaitUntilExit();

        lifecycle.Exit("hello from ink object");

        var result = await task;
        Assert.Equal("hello from ink object", result);
    }

    [Fact]
    public async Task ExitWithErrorThrowsOnWait()
    {
        var lifecycle = new AppLifecycle();
        var task = lifecycle.WaitUntilExit();

        var error = new Exception("test error");
        lifecycle.Exit(error);

        await Assert.ThrowsAsync<Exception>(() => task);
    }

    [Fact]
    public void ExitingEventFires()
    {
        var lifecycle = new AppLifecycle();
        object? received = null;
        lifecycle.Exiting += obj => received = obj;

        lifecycle.Exit("value");

        Assert.Equal("value", received);
    }

    [Fact]
    public void DoubleExitIsIgnored()
    {
        var lifecycle = new AppLifecycle();
        int exitCount = 0;
        lifecycle.Exiting += _ => exitCount++;

        lifecycle.Exit();
        lifecycle.Exit(); // Should be ignored

        Assert.Equal(1, exitCount);
    }

    [Fact]
    public async Task WaitUntilRenderFlushResolves()
    {
        var lifecycle = new AppLifecycle();
        var task = lifecycle.WaitUntilRenderFlush();

        lifecycle.NotifyRenderFlushed();

        await task; // Should complete without exception
    }

    [Fact]
    public void DisposeCallsExit()
    {
        var lifecycle = new AppLifecycle();
        bool exited = false;
        lifecycle.Exiting += _ => exited = true;

        lifecycle.Dispose();

        Assert.True(exited);
        Assert.True(lifecycle.HasExited);
    }

    [Fact]
    public void DisposeAfterExitDoesNotDoubleExit()
    {
        var lifecycle = new AppLifecycle();
        int exitCount = 0;
        lifecycle.Exiting += _ => exitCount++;

        lifecycle.Exit();
        lifecycle.Dispose();

        Assert.Equal(1, exitCount);
    }
}
