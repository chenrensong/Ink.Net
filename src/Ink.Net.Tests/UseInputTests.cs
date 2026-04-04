// Tests ported from hooks-use-input.tsx
// Covers: InputHandler + KeyInfo — keyboard input handling
using Ink.Net.Input;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Input handling tests aligned with JS hooks-use-input.tsx test suite.</summary>
public class UseInputTests
{
    private static (InputHandler handler, List<(string input, KeyInfo key)> events) CreateHandler()
    {
        var handler = new InputHandler { ExitOnCtrlC = false };
        var events = new List<(string input, KeyInfo key)>();
        handler.Register((input, key) => events.Add((input, key)));
        return (handler, events);
    }

    [Fact]
    public void HandleLowercaseCharacter()
    {
        var (handler, events) = CreateHandler();
        handler.HandleData("q");

        Assert.Single(events);
        Assert.Equal("q", events[0].input);
        Assert.False(events[0].key.Shift);
    }

    [Fact]
    public void HandleUppercaseCharacter()
    {
        var (handler, events) = CreateHandler();
        handler.HandleData("Q");

        Assert.Single(events);
        Assert.Equal("Q", events[0].input);
        Assert.True(events[0].key.Shift);
    }

    [Fact]
    public void ReturnShouldNotCountAsUppercase()
    {
        var (handler, events) = CreateHandler();
        handler.HandleData("\r");

        Assert.Single(events);
        Assert.True(events[0].key.Return);
        Assert.False(events[0].key.Shift);
    }

    [Fact]
    public void HandleEscape()
    {
        var (handler, events) = CreateHandler();
        // Need to flush pending since bare ESC is ambiguous
        handler.HandleData("\u001B");
        // Wait for pending flush
        Thread.Sleep(50);

        Assert.Single(events);
        Assert.True(events[0].key.Escape);
        Assert.True(events[0].key.Meta);
    }

    [Fact]
    public void HandleCtrl()
    {
        var (handler, events) = CreateHandler();
        handler.HandleData("\u0006"); // Ctrl+F

        Assert.Single(events);
        Assert.Equal("f", events[0].input);
        Assert.True(events[0].key.Ctrl);
    }

    [Fact]
    public void HandleMeta()
    {
        var (handler, events) = CreateHandler();
        handler.HandleData("\u001Bm");

        Assert.Single(events);
        Assert.Equal("m", events[0].input);
        Assert.True(events[0].key.Meta);
    }

    [Fact]
    public void HandleMetaBackspace()
    {
        var (handler, events) = CreateHandler();
        handler.HandleData("\u001B\u007F");

        Assert.Single(events);
        Assert.True(events[0].key.Backspace);
        Assert.True(events[0].key.Meta);
    }

    [Fact]
    public void HandleTab()
    {
        var (handler, events) = CreateHandler();
        handler.HandleData("\t");

        Assert.Single(events);
        Assert.True(events[0].key.Tab);
    }

    [Fact]
    public void HandleShiftTab()
    {
        var (handler, events) = CreateHandler();
        handler.HandleData("\u001B[Z");

        Assert.Single(events);
        Assert.True(events[0].key.Tab);
        Assert.True(events[0].key.Shift);
    }

    [Fact]
    public void HandleBackspace()
    {
        var (handler, events) = CreateHandler();
        handler.HandleData("\u0008");

        Assert.Single(events);
        Assert.True(events[0].key.Backspace);
    }

    [Fact]
    public void HandleDelete()
    {
        var (handler, events) = CreateHandler();
        handler.HandleData("\u001B[3~");

        Assert.Single(events);
        Assert.True(events[0].key.Delete);
    }

    [Fact]
    public void HandleArrowKeys()
    {
        var (handler, events) = CreateHandler();
        handler.HandleData("\u001B[A"); // Up
        handler.HandleData("\u001B[B"); // Down
        handler.HandleData("\u001B[C"); // Right
        handler.HandleData("\u001B[D"); // Left

        Assert.Equal(4, events.Count);
        Assert.True(events[0].key.UpArrow);
        Assert.True(events[1].key.DownArrow);
        Assert.True(events[2].key.RightArrow);
        Assert.True(events[3].key.LeftArrow);
    }

    [Fact]
    public void HandleReturnMeta()
    {
        var (handler, events) = CreateHandler();
        handler.HandleData("\u001B\r");

        Assert.Single(events);
        Assert.True(events[0].key.Return);
        Assert.True(events[0].key.Meta);
    }

    [Fact]
    public void HandleCtrlF1()
    {
        var (handler, events) = CreateHandler();
        handler.HandleData("\u001B[1;5P");

        Assert.Single(events);
        Assert.True(events[0].key.Ctrl);
        // Should not crash
    }

    [Fact]
    public void HandleUnmappedCtrlSequenceWithoutCrash()
    {
        var (handler, events) = CreateHandler();
        handler.HandleData("\u001B[1;5I");

        Assert.Single(events);
        Assert.True(events[0].key.Ctrl);
    }

    // ── Registration / Unregistration ───────────────────────────────

    [Fact]
    public void UnregisterStopsReceivingEvents()
    {
        var handler = new InputHandler { ExitOnCtrlC = false };
        var events = new List<string>();
        var reg = handler.Register((input, _) => events.Add(input));

        handler.HandleData("a");
        Assert.Single(events);

        reg.Dispose(); // Unregister

        handler.HandleData("b");
        Assert.Single(events); // Still 1 — "b" not received
    }

    [Fact]
    public void MultipleHandlersReceiveEvents()
    {
        var handler = new InputHandler { ExitOnCtrlC = false };
        var events1 = new List<string>();
        var events2 = new List<string>();

        handler.Register((input, _) => events1.Add(input));
        handler.Register((input, _) => events2.Add(input));

        handler.HandleData("a");

        Assert.Single(events1);
        Assert.Single(events2);
    }

    // ── Ctrl+C handling ─────────────────────────────────────────────

    [Fact]
    public void CtrlCTriggersEvent()
    {
        var handler = new InputHandler { ExitOnCtrlC = true };
        bool ctrlCFired = false;
        handler.CtrlCPressed += () => ctrlCFired = true;

        handler.HandleData("\x03");

        Assert.True(ctrlCFired);
    }

    [Fact]
    public void CtrlCDoesNotTriggerInputHandlers()
    {
        var handler = new InputHandler { ExitOnCtrlC = true };
        var events = new List<string>();
        handler.Register((input, _) => events.Add(input));

        handler.HandleData("\x03");

        Assert.Empty(events);
    }

    // ── Bracketed paste via InputHandler ─────────────────────────────

    [Fact]
    public void BracketedPasteTriggersEvent()
    {
        var handler = new InputHandler { ExitOnCtrlC = false };
        string? pastedText = null;
        handler.PasteReceived += text => pastedText = text;

        handler.HandleData("\u001B[200~hello world\u001B[201~");

        Assert.Equal("hello world", pastedText);
    }

    // ── Dispose ─────────────────────────────────────────────────────

    [Fact]
    public void DisposeStopsAllHandling()
    {
        var handler = new InputHandler { ExitOnCtrlC = false };
        var events = new List<string>();
        handler.Register((input, _) => events.Add(input));

        handler.Dispose();
        handler.HandleData("a"); // Should be ignored

        Assert.Empty(events);
    }
}
