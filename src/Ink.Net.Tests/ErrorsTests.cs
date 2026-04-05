// -----------------------------------------------------------------------
// ErrorsTests.cs — Tests for error handling scenarios.
// Aligned with ink/test/errors.tsx
// -----------------------------------------------------------------------

using Ink.Net;
using Ink.Net.Terminal;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>
/// Tests for error handling in Ink.Net.
/// <para>Aligned with JS <c>test/errors.tsx</c>.</para>
/// </summary>
public class ErrorsTests
{
    [Fact]
    public async Task ExitWithError_WaitUntilExit_Throws()
    {
        // Aligned with JS: "catch and display error"
        // In C#, errors are propagated through AppLifecycle.Exit(exception)
        var lifecycle = new AppLifecycle();
        var exitTask = lifecycle.WaitUntilExit();

        var error = new Exception("Oh no");
        lifecycle.Exit(error);

        var thrown = await Assert.ThrowsAsync<Exception>(() => exitTask);
        Assert.Equal("Oh no", thrown.Message);
    }

    [Fact]
    public void ExitWithError_HasExited_IsTrue()
    {
        var lifecycle = new AppLifecycle();
        lifecycle.Exit(new Exception("Boom"));
        Assert.True(lifecycle.HasExited);
    }

    [Fact]
    public async Task CleanUpRawModeWhenErrorIsThrown()
    {
        // Aligned with JS: "clean up raw mode when error is thrown"
        var rawModeChanges = new List<bool>();
        var stdin = new StdinProvider(isRawModeSupported: true);
        stdin.RawModeChanged += mode => rawModeChanges.Add(mode);

        // Simulate enabling raw mode
        stdin.SetRawMode(true);
        Assert.True(stdin.IsRawMode);

        // Simulate cleanup on error (Dispose disables raw mode)
        stdin.Dispose();

        // Verify raw mode was enabled then disabled
        Assert.Contains(true, rawModeChanges);
        Assert.Contains(false, rawModeChanges);
        Assert.False(stdin.IsRawMode);
    }

    [Fact]
    public async Task WaitUntilExit_ResolvesFirstExitValue_WhenDuplicateExitsDuringTeardown()
    {
        // Aligned with JS: "waitUntilExit resolves first exit value when
        //   duplicate exits happen during teardown"
        var lifecycle = new AppLifecycle();
        var exitTask = lifecycle.WaitUntilExit();

        // First exit
        lifecycle.Exit("first");

        // Second exit should be ignored (already exited)
        lifecycle.Exit("second");

        var result = await exitTask;
        Assert.Equal("first", result);
    }

    [Fact]
    public async Task WaitUntilExit_ResolvesFirstExitValue_WhenExitIsReEntered()
    {
        // Aligned with JS: "waitUntilExit resolves first exit value when
        //   exit is re-entered during unmount writes"
        var lifecycle = new AppLifecycle();
        var exitTask = lifecycle.WaitUntilExit();

        bool didReenterExit = false;

        lifecycle.Exiting += _ =>
        {
            // During the first exit handler, try to exit again
            if (!didReenterExit)
            {
                didReenterExit = true;
                lifecycle.Exit("second"); // Should be ignored
            }
        };

        lifecycle.Exit("first");

        var result = await exitTask;
        Assert.True(didReenterExit);
        Assert.Equal("first", result);
    }

    [Fact]
    public async Task WaitUntilRenderFlush_ResolvesAfterExitWithError()
    {
        // Aligned with JS: "waitUntilRenderFlush resolves after exit with error"
        var lifecycle = new AppLifecycle();

        lifecycle.Exit(new Exception("boom"));

        // WaitUntilExit should throw
        await Assert.ThrowsAsync<Exception>(() => lifecycle.WaitUntilExit());

        // WaitUntilRenderFlush should resolve (not throw) even after error exit
        await lifecycle.WaitUntilRenderFlush();
    }

    [Fact]
    public async Task RenderFlush_NotifiesBeforeExit()
    {
        var lifecycle = new AppLifecycle();
        var flushTask = lifecycle.WaitUntilRenderFlush();

        lifecycle.NotifyRenderFlushed();

        await flushTask; // Should resolve without exception
    }

    [Fact]
    public async Task ExitRejects_WithExceptionArgument()
    {
        // Aligned with JS: "exit rejects on cross-realm Error"
        var lifecycle = new AppLifecycle();
        var exitTask = lifecycle.WaitUntilExit();

        lifecycle.Exit(new InvalidOperationException("boom"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => exitTask);
    }

    [Fact]
    public void InkApplication_Dispose_CleansUpResources()
    {
        // Verify InkApplication properly cleans up on dispose
        var stdout = new StringWriter();
        var stderr = new StringWriter();

        var app = InkApplication.Create(b => new[]
        {
            b.Text("Hello")
        }, new InkApplicationOptions
        {
            Stdout = stdout,
            Stderr = stderr,
            ExitOnCtrlC = false,
            IsRawModeSupported = false
        });

        // Should not throw
        app.Dispose();

        // Double dispose should also not throw
        app.Dispose();
    }

    [Fact]
    public void InkApplication_AlternateScreen_ExitsOnDispose()
    {
        var sb = new System.Text.StringBuilder();
        var stdout = new StringWriter(sb);
        var stderr = new StringWriter();

        var app = InkApplication.Create(b => new[]
        {
            b.Text("Hello")
        }, new InkApplicationOptions
        {
            Stdout = stdout,
            Stderr = stderr,
            AlternateScreen = true,
            ExitOnCtrlC = false,
            IsRawModeSupported = false
        });

        // Verify alternate screen was entered
        var output = sb.ToString();
        Assert.Contains(AlternateScreen.EnterAlternateScreenEscape, output);

        sb.Clear();
        app.Dispose();

        // Verify alternate screen was exited on dispose
        output = sb.ToString();
        Assert.Contains(AlternateScreen.ExitAlternateScreenEscape, output);
    }
}
