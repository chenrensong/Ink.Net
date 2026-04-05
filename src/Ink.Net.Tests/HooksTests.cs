// -----------------------------------------------------------------------
// HooksTests.cs — Tests for hook-like subsystems (useInput, useStdout, etc.)
// Aligned with ink/test/hooks.tsx
// -----------------------------------------------------------------------

using Ink.Net.Input;
using Ink.Net.Terminal;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>
/// Tests for hook-like subsystems in Ink.Net.
/// <para>Aligned with JS <c>test/hooks.tsx</c>.</para>
/// </summary>
public class HooksTests
{
    // ─── useInput (InputHandler) ────────────────────────────────────

    [Fact]
    public void UseInput_IgnoreInputIfNotActive()
    {
        // Aligned with JS: "useInput - ignore input if not active"
        var handler = new InputHandler();
        var received = new List<string>();

        // Register an active handler
        var reg1 = handler.Register((input, key) => received.Add(input));

        // Dispose (deactivate) the handler
        reg1.Dispose();

        // Input should not be received
        handler.HandleData("x");
        Assert.Empty(received);
    }

    [Fact]
    public void UseInput_MultipleHandlers_AllReceiveInput()
    {
        var handler = new InputHandler { ExitOnCtrlC = false };
        var received1 = new List<string>();
        var received2 = new List<string>();

        var reg1 = handler.Register((input, key) => received1.Add(input));
        var reg2 = handler.Register((input, key) => received2.Add(input));

        handler.HandleData("a");

        Assert.Contains("a", received1);
        Assert.Contains("a", received2);

        reg1.Dispose();
        reg2.Dispose();
        handler.Dispose();
    }

    [Fact]
    public void UseInput_HandleCtrlC_ExitOnCtrlCTrue()
    {
        var handler = new InputHandler { ExitOnCtrlC = true };
        bool ctrlCFired = false;
        handler.CtrlCPressed += () => ctrlCFired = true;

        handler.HandleData("\x03");

        Assert.True(ctrlCFired);
        handler.Dispose();
    }

    [Fact]
    public void UseInput_HandleCtrlC_ExitOnCtrlCFalse()
    {
        // Aligned with JS: "useInput - handle Ctrl+C when exitOnCtrlC is false"
        var handler = new InputHandler { ExitOnCtrlC = false };
        bool ctrlCFired = false;
        var received = new List<string>();

        handler.CtrlCPressed += () => ctrlCFired = true;
        handler.Register((input, key) => received.Add(input));

        handler.HandleData("\x03");

        // Ctrl+C should reach handlers when exitOnCtrlC is false
        Assert.False(ctrlCFired);
        handler.Dispose();
    }

    [Fact]
    public void UseInput_NoMaxListenersWarning_WithManyHandlers()
    {
        // Aligned with JS: "useInput - no MaxListenersExceededWarning with many useInput hooks"
        var handler = new InputHandler { ExitOnCtrlC = false };
        var registrations = new List<InputRegistration>();

        // Register more than the typical 10 listener limit
        for (int i = 0; i < 50; i++)
        {
            registrations.Add(handler.Register((input, key) => { }));
        }

        // Should not throw or produce warnings
        handler.HandleData("a");

        foreach (var reg in registrations) reg.Dispose();
        handler.Dispose();
    }

    // ─── useStdout (StdoutProvider) ─────────────────────────────────

    [Fact]
    public void UseStdout_WritesToStream()
    {
        // Aligned with JS: "useStdout - write to stdout"
        var sw = new StringWriter();
        var provider = new StdoutProvider(sw);

        provider.Write("Hello from Ink to stdout");

        Assert.Contains("Hello from Ink to stdout", sw.ToString());
    }

    [Fact]
    public void UseStdout_WriterProperty()
    {
        var sw = new StringWriter();
        var provider = new StdoutProvider(sw);

        Assert.Same(sw, provider.Writer);
    }

    [Fact]
    public void UseStdout_WriteRequested_FiresWhenSubscribed()
    {
        var sw = new StringWriter();
        var provider = new StdoutProvider(sw);
        string? intercepted = null;

        provider.WriteRequested += data => intercepted = data;

        provider.Write("Hello");

        Assert.Equal("Hello", intercepted);
        // When WriteRequested is handled, direct write should not happen
        Assert.Equal("", sw.ToString());
    }

    // ─── useStderr (StderrProvider) ─────────────────────────────────

    [Fact]
    public void UseStderr_WritesToStream()
    {
        var sw = new StringWriter();
        var provider = new StderrProvider(sw);

        provider.Write("Error message");

        Assert.Contains("Error message", sw.ToString());
    }

    [Fact]
    public void UseStderr_WriterProperty()
    {
        var sw = new StringWriter();
        var provider = new StderrProvider(sw);

        Assert.Same(sw, provider.Writer);
    }

    // ─── useStdin (StdinProvider) ───────────────────────────────────

    [Fact]
    public void UseStdin_IsRawModeSupported()
    {
        var provider = new StdinProvider(isRawModeSupported: true);
        Assert.True(provider.IsRawModeSupported);

        var provider2 = new StdinProvider(isRawModeSupported: false);
        Assert.False(provider2.IsRawModeSupported);
    }

    [Fact]
    public void UseStdin_SetRawMode()
    {
        var provider = new StdinProvider(isRawModeSupported: true);
        bool? rawModeChanged = null;

        provider.RawModeChanged += mode => rawModeChanged = mode;

        provider.SetRawMode(true);
        Assert.True(provider.IsRawMode);
        Assert.True(rawModeChanged);

        provider.SetRawMode(false);
        Assert.False(provider.IsRawMode);
        Assert.False(rawModeChanged);
    }

    [Fact]
    public void UseStdin_SetRawMode_Ignored_WhenNotSupported()
    {
        var provider = new StdinProvider(isRawModeSupported: false);
        bool eventFired = false;
        provider.RawModeChanged += _ => eventFired = true;

        provider.SetRawMode(true);
        Assert.False(provider.IsRawMode);
        Assert.False(eventFired);
    }

    [Fact]
    public void UseStdin_EmitData_FiresDataReceived()
    {
        var provider = new StdinProvider(isRawModeSupported: true);
        string? received = null;

        provider.DataReceived += data => received = data;
        provider.EmitData("test input");

        Assert.Equal("test input", received);
    }

    [Fact]
    public void UseStdin_Dispose_DisablesRawMode()
    {
        var provider = new StdinProvider(isRawModeSupported: true);
        bool? lastRawMode = null;
        provider.RawModeChanged += mode => lastRawMode = mode;

        provider.SetRawMode(true);
        provider.Dispose();

        Assert.False(provider.IsRawMode);
        Assert.False(lastRawMode);
    }

    // ─── usePaste (PasteHandler) ────────────────────────────────────

    [Fact]
    public void UsePaste_RegisterEnablesBracketedPasteMode()
    {
        var sw = new StringWriter();
        var paste = new PasteHandler(sw);

        var reg = paste.Register(text => { });

        Assert.True(paste.IsBracketedPasteModeEnabled);
        Assert.Contains("\u001B[?2004h", sw.ToString());

        reg.Dispose();
        paste.Dispose();
    }

    [Fact]
    public void UsePaste_UnregisterDisablesBracketedPasteMode()
    {
        var sw = new StringWriter();
        var paste = new PasteHandler(sw);

        var reg = paste.Register(text => { });
        sw.GetStringBuilder().Clear();

        reg.Dispose();

        Assert.False(paste.IsBracketedPasteModeEnabled);
        Assert.Contains("\u001B[?2004l", sw.ToString());
        paste.Dispose();
    }

    [Fact]
    public void UsePaste_HandlePaste_DispatchesToHandlers()
    {
        var sw = new StringWriter();
        var paste = new PasteHandler(sw);
        string? received = null;

        var reg = paste.Register(text => received = text);
        paste.HandlePaste("pasted text");

        Assert.Equal("pasted text", received);

        reg.Dispose();
        paste.Dispose();
    }

    [Fact]
    public void UsePaste_MultipleRegistrations_RefCounted()
    {
        var sw = new StringWriter();
        var paste = new PasteHandler(sw);

        var reg1 = paste.Register(text => { });
        var reg2 = paste.Register(text => { });

        Assert.True(paste.IsBracketedPasteModeEnabled);

        reg1.Dispose();
        Assert.True(paste.IsBracketedPasteModeEnabled); // still one handler

        reg2.Dispose();
        Assert.False(paste.IsBracketedPasteModeEnabled); // all handlers removed

        paste.Dispose();
    }

    // ─── InkApplication integrated hooks ─────────────────────────────

    [Fact]
    public void InkApplication_ExposesAllSubsystems()
    {
        var app = InkApplication.Create(b => new[]
        {
            b.Text("Hello")
        }, new InkApplicationOptions
        {
            Stdout = new StringWriter(),
            Stderr = new StringWriter()
        });

        Assert.NotNull(app.Lifecycle);
        Assert.NotNull(app.Input);
        Assert.NotNull(app.Paste);
        Assert.NotNull(app.Focus);
        Assert.NotNull(app.Cursor);
        Assert.NotNull(app.WindowSize);
        Assert.NotNull(app.Stdin);
        Assert.NotNull(app.Stdout);
        Assert.NotNull(app.Stderr);
        Assert.NotNull(app.Screen);

        app.Dispose();
    }

    [Fact]
    public void InkApplication_IsScreenReaderEnabled_ReflectsOption()
    {
        var app1 = InkApplication.Create(b => new[] { b.Text("Hello") },
            new InkApplicationOptions
            {
                Stdout = new StringWriter(),
                Stderr = new StringWriter(),
                IsScreenReaderEnabled = true
            });
        Assert.True(app1.IsScreenReaderEnabled);
        app1.Dispose();

        var app2 = InkApplication.Create(b => new[] { b.Text("Hello") },
            new InkApplicationOptions
            {
                Stdout = new StringWriter(),
                Stderr = new StringWriter(),
                IsScreenReaderEnabled = false
            });
        Assert.False(app2.IsScreenReaderEnabled);
        app2.Dispose();
    }
}
