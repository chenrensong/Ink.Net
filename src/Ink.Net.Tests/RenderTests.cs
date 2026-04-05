// -----------------------------------------------------------------------
// RenderTests.cs — Tests for rendering behavior: rerender, lifecycle, throttle.
// Aligned with ink/test/render.tsx (subset applicable to C# model)
// -----------------------------------------------------------------------

using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Dom;
using Ink.Net.Terminal;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>
/// Tests for rendering, rerendering, and lifecycle behavior.
/// <para>Aligned with JS <c>test/render.tsx</c> where applicable to the C# model.</para>
/// </summary>
public class RenderTests
{
    // ─── Basic rendering ──────────────────────────────────────────

    [Fact]
    public void RenderText()
    {
        var stdout = new StringWriter();
        var app = InkApplication.Create(b => new[]
        {
            b.Text("Hello World")
        }, new InkApplicationOptions
        {
            Stdout = stdout,
            Stderr = new StringWriter(),
            Columns = 100,
        });

        var output = stdout.ToString();
        Assert.Contains("Hello World", output);
        app.Dispose();
    }

    [Fact]
    public void Rerender_UpdatesOutput()
    {
        var stdout = new StringWriter();
        var app = InkApplication.Create(b => new[]
        {
            b.Text("Hello")
        }, new InkApplicationOptions
        {
            Stdout = stdout,
            Stderr = new StringWriter(),
            Columns = 100,
        });

        app.Rerender(b => new[]
        {
            b.Text("World")
        });

        var output = stdout.ToString();
        Assert.Contains("World", output);
        app.Dispose();
    }

    [Fact]
    public void RenderWithDebugMode()
    {
        var stdout = new StringWriter();
        var app = InkApplication.Create(b => new[]
        {
            b.Text("Debug Test")
        }, new InkApplicationOptions
        {
            Stdout = stdout,
            Stderr = new StringWriter(),
            Debug = true,
            Columns = 100,
        });

        var output = stdout.ToString();
        Assert.Contains("Debug Test", output);
        app.Dispose();
    }

    // ─── Exit and WaitUntilExit ──────────────────────────────────

    [Fact]
    public async Task WaitUntilExit_ResolvesOnDispose()
    {
        var stdout = new StringWriter();
        var app = InkApplication.Create(b => new[]
        {
            b.Text("Hello")
        }, new InkApplicationOptions
        {
            Stdout = stdout,
            Stderr = new StringWriter(),
        });

        var exitTask = app.WaitUntilExit();
        app.Lifecycle.Exit();

        var result = await exitTask;
        Assert.Null(result);
    }

    [Fact]
    public async Task WaitUntilExit_WithValue()
    {
        var stdout = new StringWriter();
        var app = InkApplication.Create(b => new[]
        {
            b.Text("Hello")
        }, new InkApplicationOptions
        {
            Stdout = stdout,
            Stderr = new StringWriter(),
        });

        var exitTask = app.WaitUntilExit();
        app.Lifecycle.Exit("result_value");

        var result = await exitTask;
        Assert.Equal("result_value", result);
    }

    [Fact]
    public async Task WaitUntilExit_ThrowsOnError()
    {
        // Aligned with JS: "should reject waitUntilExit when app exits
        //   during synchronous render error handling"
        var stdout = new StringWriter();
        var app = InkApplication.Create(b => new[]
        {
            b.Text("Hello")
        }, new InkApplicationOptions
        {
            Stdout = stdout,
            Stderr = new StringWriter(),
        });

        var exitTask = app.WaitUntilExit();
        app.Lifecycle.Exit(new Exception("Synchronous render error"));

        await Assert.ThrowsAsync<Exception>(() => exitTask);
    }

    // ─── Ctrl+C handling ─────────────────────────────────────────

    [Fact]
    public void CtrlC_ExitsApplication()
    {
        var stdout = new StringWriter();
        var app = InkApplication.Create(b => new[]
        {
            b.Text("Hello")
        }, new InkApplicationOptions
        {
            Stdout = stdout,
            Stderr = new StringWriter(),
            ExitOnCtrlC = true,
        });

        app.HandleInput("\x03"); // Ctrl+C

        Assert.True(app.Lifecycle.HasExited);
        app.Dispose();
    }

    [Fact]
    public void CtrlC_DoesNotExitWhenDisabled()
    {
        var stdout = new StringWriter();
        var app = InkApplication.Create(b => new[]
        {
            b.Text("Hello")
        }, new InkApplicationOptions
        {
            Stdout = stdout,
            Stderr = new StringWriter(),
            ExitOnCtrlC = false,
        });

        app.HandleInput("\x03"); // Ctrl+C

        Assert.False(app.Lifecycle.HasExited);
        app.Dispose();
    }

    // ─── Input handling ──────────────────────────────────────────

    [Fact]
    public void HandleInput_PassesToInputHandler()
    {
        var stdout = new StringWriter();
        var app = InkApplication.Create(b => new[]
        {
            b.Text("Hello")
        }, new InkApplicationOptions
        {
            Stdout = stdout,
            Stderr = new StringWriter(),
            ExitOnCtrlC = false,
        });

        var received = new List<string>();
        app.Input.Register((input, key) => received.Add(input));

        app.HandleInput("a");
        app.HandleInput("b");
        app.HandleInput("c");

        Assert.Contains("a", received);
        Assert.Contains("b", received);
        Assert.Contains("c", received);

        app.Dispose();
    }

    // ─── Clear output ────────────────────────────────────────────

    [Fact]
    public void Clear_ClearsOutput()
    {
        // Aligned with JS: "clear output"
        var stdout = new StringWriter();
        var app = InkApplication.Create(b => new[]
        {
            b.Text("Hello World")
        }, new InkApplicationOptions
        {
            Stdout = stdout,
            Stderr = new StringWriter(),
            Columns = 100,
        });

        app.Clear();
        // Clear should not throw
        app.Dispose();
    }

    // ─── Alternate screen rendering ──────────────────────────────

    [Fact]
    public void AlternateScreen_EnterAndExitOnLifecycle()
    {
        var sb = new System.Text.StringBuilder();
        var stdout = new StringWriter(sb);
        var app = InkApplication.Create(b => new[]
        {
            b.Text("Hello")
        }, new InkApplicationOptions
        {
            Stdout = stdout,
            Stderr = new StringWriter(),
            AlternateScreen = true,
            Columns = 100,
        });

        var output = sb.ToString();
        Assert.Contains(AlternateScreen.EnterAlternateScreenEscape, output);

        sb.Clear();
        app.Dispose();

        output = sb.ToString();
        Assert.Contains(AlternateScreen.ExitAlternateScreenEscape, output);
    }

    // ─── StdoutProvider write preservation ─────────────────────────

    [Fact]
    public void StdoutWrite_PreservesInkOutput()
    {
        // Aligned with JS: "useStdout - write to stdout"
        var sb = new System.Text.StringBuilder();
        var stdout = new StringWriter(sb);
        var app = InkApplication.Create(b => new[]
        {
            b.Text("Hello World")
        }, new InkApplicationOptions
        {
            Stdout = stdout,
            Stderr = new StringWriter(),
            Columns = 100,
        });

        sb.Clear();
        app.Stdout.Write("external write\n");

        var output = sb.ToString();
        Assert.Contains("external write", output);

        app.Dispose();
    }

    // ─── InkApp (non-interactive) rendering ─────────────────────

    [Fact]
    public void InkApp_RenderAndUnmount()
    {
        var stdout = new StringWriter();
        var app = InkApp.Render(b => new[]
        {
            b.Text("Static render")
        }, new RenderOptions
        {
            Stdout = stdout,
            Columns = 100,
        });

        var output = stdout.ToString();
        Assert.Contains("Static render", output);

        app.Unmount();
    }

    [Fact]
    public void InkApp_RerenderUpdatesOutput()
    {
        var stdout = new StringWriter();
        var app = InkApp.Render(b => new[]
        {
            b.Text("First")
        }, new RenderOptions
        {
            Stdout = stdout,
            Columns = 100,
        });

        app.Rerender(b => new[]
        {
            b.Text("Second")
        });

        var output = stdout.ToString();
        Assert.Contains("Second", output);

        app.Unmount();
    }

    // ─── Screen reader mode ──────────────────────────────────────

    [Fact]
    public void ScreenReaderEnabled_AffectsRendering()
    {
        var stdout = new StringWriter();
        var app = InkApplication.Create(b => new[]
        {
            b.Text("Accessible text")
        }, new InkApplicationOptions
        {
            Stdout = stdout,
            Stderr = new StringWriter(),
            IsScreenReaderEnabled = true,
            Columns = 100,
        });

        Assert.True(app.IsScreenReaderEnabled);
        var output = stdout.ToString();
        Assert.Contains("Accessible text", output);

        app.Dispose();
    }

    // ─── Dispose idempotency ─────────────────────────────────────

    [Fact]
    public void DoubleDispose_DoesNotThrow()
    {
        var stdout = new StringWriter();
        var app = InkApplication.Create(b => new[]
        {
            b.Text("Hello")
        }, new InkApplicationOptions
        {
            Stdout = stdout,
            Stderr = new StringWriter(),
        });

        app.Dispose();
        app.Dispose(); // Should not throw
    }

    [Fact]
    public void RerenderAfterDispose_DoesNotThrow()
    {
        var stdout = new StringWriter();
        var app = InkApplication.Create(b => new[]
        {
            b.Text("Hello")
        }, new InkApplicationOptions
        {
            Stdout = stdout,
            Stderr = new StringWriter(),
        });

        app.Dispose();

        // Should silently do nothing
        app.Rerender(b => new[] { b.Text("World") });
    }

    [Fact]
    public void HandleInputAfterDispose_DoesNotThrow()
    {
        var stdout = new StringWriter();
        var app = InkApplication.Create(b => new[]
        {
            b.Text("Hello")
        }, new InkApplicationOptions
        {
            Stdout = stdout,
            Stderr = new StringWriter(),
        });

        app.Dispose();

        // Should silently do nothing
        app.HandleInput("a");
    }
}
