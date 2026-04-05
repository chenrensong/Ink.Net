// -----------------------------------------------------------------------
// HooksUseInputNavigationTests.cs — Tests for arrow/navigation key handling.
// Aligned with ink/test/hooks-use-input-navigation.tsx
// -----------------------------------------------------------------------

using Ink.Net.Input;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>
/// Tests for arrow keys, page up/down, home/end key handling via InputHandler.
/// <para>Aligned with JS <c>test/hooks-use-input-navigation.tsx</c>.</para>
/// </summary>
public class HooksUseInputNavigationTests
{
    private static (string Input, KeyInfo Key) HandleSingleInput(string rawData)
    {
        var handler = new InputHandler { ExitOnCtrlC = false };
        string? input = null;
        KeyInfo? key = null;

        handler.Register((i, k) => { input = i; key = k; });
        handler.HandleData(rawData);
        handler.Dispose();

        Assert.NotNull(key);
        return (input!, key!);
    }

    [Fact]
    public void HandleUpArrow()
    {
        // Aligned with JS: "useInput - handle up arrow" (ESC[A)
        var (input, key) = HandleSingleInput("\u001B[A");
        Assert.True(key.UpArrow);
        Assert.False(key.DownArrow);
        Assert.False(key.LeftArrow);
        Assert.False(key.RightArrow);
    }

    [Fact]
    public void HandleDownArrow()
    {
        // Aligned with JS: "useInput - handle down arrow" (ESC[B)
        var (input, key) = HandleSingleInput("\u001B[B");
        Assert.True(key.DownArrow);
        Assert.False(key.UpArrow);
    }

    [Fact]
    public void HandleLeftArrow()
    {
        // Aligned with JS: "useInput - handle left arrow" (ESC[D)
        var (input, key) = HandleSingleInput("\u001B[D");
        Assert.True(key.LeftArrow);
        Assert.False(key.RightArrow);
    }

    [Fact]
    public void HandleRightArrow()
    {
        // Aligned with JS: "useInput - handle right arrow" (ESC[C)
        var (input, key) = HandleSingleInput("\u001B[C");
        Assert.True(key.RightArrow);
        Assert.False(key.LeftArrow);
    }

    [Fact]
    public void HandleRapidArrowsAndEnterInOneChunk()
    {
        // Aligned with JS: "useInput - handles rapid arrows and enter in one chunk"
        // ESC[B ESC[B ESC[B CR
        var handler = new InputHandler { ExitOnCtrlC = false };
        var keys = new List<KeyInfo>();

        handler.Register((i, k) => keys.Add(k));
        handler.HandleData("\u001B[B\u001B[B\u001B[B\r");
        handler.Dispose();

        Assert.Equal(4, keys.Count);
        Assert.True(keys[0].DownArrow);
        Assert.True(keys[1].DownArrow);
        Assert.True(keys[2].DownArrow);
        Assert.True(keys[3].Return);
    }

    [Fact]
    public void HandleMetaUpArrow()
    {
        // Aligned with JS: "useInput - handle meta + up arrow" (ESC ESC[A)
        var (input, key) = HandleSingleInput("\u001B\u001B[A");
        Assert.True(key.UpArrow);
        Assert.True(key.Meta);
    }

    [Fact]
    public void HandleMetaDownArrow()
    {
        // Aligned with JS: "useInput - handle meta + down arrow" (ESC ESC[B)
        var (input, key) = HandleSingleInput("\u001B\u001B[B");
        Assert.True(key.DownArrow);
        Assert.True(key.Meta);
    }

    [Fact]
    public void HandleMetaLeftArrow()
    {
        // Aligned with JS: "useInput - handle meta + left arrow" (ESC ESC[D)
        var (input, key) = HandleSingleInput("\u001B\u001B[D");
        Assert.True(key.LeftArrow);
        Assert.True(key.Meta);
    }

    [Fact]
    public void HandleMetaRightArrow()
    {
        // Aligned with JS: "useInput - handle meta + right arrow" (ESC ESC[C)
        var (input, key) = HandleSingleInput("\u001B\u001B[C");
        Assert.True(key.RightArrow);
        Assert.True(key.Meta);
    }

    [Fact]
    public void HandleCtrlUpArrow()
    {
        // Aligned with JS: "useInput - handle ctrl + up arrow" (ESC[1;5A)
        var (input, key) = HandleSingleInput("\u001B[1;5A");
        Assert.True(key.UpArrow);
        Assert.True(key.Ctrl);
    }

    [Fact]
    public void HandleCtrlDownArrow()
    {
        // Aligned with JS: "useInput - handle ctrl + down arrow" (ESC[1;5B)
        var (input, key) = HandleSingleInput("\u001B[1;5B");
        Assert.True(key.DownArrow);
        Assert.True(key.Ctrl);
    }

    [Fact]
    public void HandleCtrlLeftArrow()
    {
        // Aligned with JS: "useInput - handle ctrl + left arrow" (ESC[1;5D)
        var (input, key) = HandleSingleInput("\u001B[1;5D");
        Assert.True(key.LeftArrow);
        Assert.True(key.Ctrl);
    }

    [Fact]
    public void HandleCtrlRightArrow()
    {
        // Aligned with JS: "useInput - handle ctrl + right arrow" (ESC[1;5C)
        var (input, key) = HandleSingleInput("\u001B[1;5C");
        Assert.True(key.RightArrow);
        Assert.True(key.Ctrl);
    }

    [Fact]
    public void HandlePageDown()
    {
        // Aligned with JS: "useInput - handle page down" (ESC[6~)
        var (input, key) = HandleSingleInput("\u001B[6~");
        Assert.True(key.PageDown);
    }

    [Fact]
    public void HandlePageUp()
    {
        // Aligned with JS: "useInput - handle page up" (ESC[5~)
        var (input, key) = HandleSingleInput("\u001B[5~");
        Assert.True(key.PageUp);
    }

    [Fact]
    public void HandleHome()
    {
        // Aligned with JS: "useInput - handle home" (ESC[H)
        var (input, key) = HandleSingleInput("\u001B[H");
        Assert.True(key.Home);
    }

    [Fact]
    public void HandleEnd()
    {
        // Aligned with JS: "useInput - handle end" (ESC[F)
        var (input, key) = HandleSingleInput("\u001B[F");
        Assert.True(key.End);
    }
}
