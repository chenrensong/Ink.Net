// Tests for Input/InputParser.cs — 1:1 port of input-parser.ts
using Ink.Net.Input;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Tests for the InputParser (ported from input-parser.ts).</summary>
public class InputParserTests
{
    [Fact]
    public void PushReturnsEventsForPlainText()
    {
        var parser = new InputParser();
        var events = parser.Push("abc");

        Assert.Single(events);
        Assert.False(events[0].IsPaste);
        Assert.Equal("abc", events[0].Value);
    }

    [Fact]
    public void PushSplitsBackspaceBytes()
    {
        var parser = new InputParser();
        var events = parser.Push("a\u007Fb");

        Assert.Equal(3, events.Count);
        Assert.Equal("a", events[0].Value);
        Assert.Equal("\u007F", events[1].Value);
        Assert.Equal("b", events[2].Value);
    }

    [Fact]
    public void PushSplitsCtrlHBackspace()
    {
        var parser = new InputParser();
        var events = parser.Push("a\u0008b");

        Assert.Equal(3, events.Count);
        Assert.Equal("a", events[0].Value);
        Assert.Equal("\u0008", events[1].Value);
        Assert.Equal("b", events[2].Value);
    }

    [Fact]
    public void PushHandlesEscapeSequence()
    {
        var parser = new InputParser();
        // ESC [ A = cursor up
        var events = parser.Push("\u001B[A");

        Assert.Single(events);
        Assert.Equal("\u001B[A", events[0].Value);
        Assert.False(events[0].IsPaste);
    }

    [Fact]
    public void PushHandlesSs3Sequence()
    {
        var parser = new InputParser();
        // ESC O A = SS3 cursor up
        var events = parser.Push("\u001BOA");

        Assert.Single(events);
        Assert.Equal("\u001BOA", events[0].Value);
    }

    [Fact]
    public void PushHandlesDoubleEscapeSequence()
    {
        var parser = new InputParser();
        // ESC ESC [ A = option + cursor up
        var events = parser.Push("\u001B\u001B[A");

        Assert.Single(events);
        Assert.Equal("\u001B\u001B[A", events[0].Value);
    }

    [Fact]
    public void PushBuffersPendingEscape()
    {
        var parser = new InputParser();
        // Lone escape should be buffered
        var events = parser.Push("\u001B");

        Assert.Empty(events);
        Assert.True(parser.HasPendingEscape());
    }

    [Fact]
    public void PushCompletesPendingOnNextChunk()
    {
        var parser = new InputParser();
        parser.Push("\u001B");
        var events = parser.Push("[A");

        Assert.Single(events);
        Assert.Equal("\u001B[A", events[0].Value);
        Assert.False(parser.HasPendingEscape());
    }

    [Fact]
    public void FlushPendingEscapeReturnsPendingSequence()
    {
        var parser = new InputParser();
        parser.Push("\u001B");

        var flushed = parser.FlushPendingEscape();
        Assert.Equal("\u001B", flushed);
        Assert.False(parser.HasPendingEscape());
    }

    [Fact]
    public void FlushPendingEscapeReturnsNullWhenNoPending()
    {
        var parser = new InputParser();
        parser.Push("abc");

        var flushed = parser.FlushPendingEscape();
        Assert.Null(flushed);
    }

    [Fact]
    public void PushHandlesBracketedPaste()
    {
        var parser = new InputParser();
        // Bracketed paste: ESC[200~ content ESC[201~
        var events = parser.Push("\u001B[200~Hello World\u001B[201~");

        Assert.Single(events);
        Assert.True(events[0].IsPaste);
        Assert.Equal("Hello World", events[0].Value);
    }

    [Fact]
    public void PushBuffersIncompletePaste()
    {
        var parser = new InputParser();
        // Start of paste without end marker
        var events = parser.Push("\u001B[200~Hello World");

        Assert.Empty(events);
        // The pending starts with paste start, so HasPendingEscape is false
        Assert.False(parser.HasPendingEscape());
    }

    [Fact]
    public void PushCompletesPasteOnNextChunk()
    {
        var parser = new InputParser();
        parser.Push("\u001B[200~Hello");
        var events = parser.Push(" World\u001B[201~");

        Assert.Single(events);
        Assert.True(events[0].IsPaste);
        Assert.Equal("Hello World", events[0].Value);
    }

    [Fact]
    public void PushHandlesMixedEventsAndEscapes()
    {
        var parser = new InputParser();
        var events = parser.Push("a\u001B[Ab");

        Assert.Equal(3, events.Count);
        Assert.Equal("a", events[0].Value);
        Assert.Equal("\u001B[A", events[1].Value);
        Assert.Equal("b", events[2].Value);
    }

    [Fact]
    public void ResetClearsPendingBuffer()
    {
        var parser = new InputParser();
        parser.Push("\u001B");
        Assert.True(parser.HasPendingEscape());

        parser.Reset();
        Assert.False(parser.HasPendingEscape());
    }

    [Fact]
    public void PushHandlesMultipleSequencesInOneChunk()
    {
        var parser = new InputParser();
        // Two arrow key sequences
        var events = parser.Push("\u001B[A\u001B[B");

        Assert.Equal(2, events.Count);
        Assert.Equal("\u001B[A", events[0].Value);
        Assert.Equal("\u001B[B", events[1].Value);
    }

    [Fact]
    public void PushHandlesEscapedCodePoint()
    {
        var parser = new InputParser();
        // ESC followed by 'a' = meta+a
        var events = parser.Push("\u001Ba");

        Assert.Single(events);
        Assert.Equal("\u001Ba", events[0].Value);
    }

    [Fact]
    public void HasPendingEscapeIsFalseDuringPasteAssembly()
    {
        var parser = new InputParser();
        // Partial paste start marker
        parser.Push("\u001B[200");
        Assert.False(parser.HasPendingEscape());
    }

    [Fact]
    public void InputEventToString()
    {
        var keyEvent = InputEvent.Key("a");
        Assert.Equal("Key(a)", keyEvent.ToString());

        var pasteEvent = InputEvent.Paste("hello");
        Assert.Equal("Paste(hello)", pasteEvent.ToString());
    }
}
