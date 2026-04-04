// Tests for Input/KeypressParser.cs — 1:1 port of parse-keypress.ts
using Ink.Net.Input;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Tests for KeypressParser (ported from parse-keypress.ts).</summary>
public class KeypressParserTests
{
    // ── Basic characters ────────────────────────────────────────────

    [Fact]
    public void ParsesLowercaseLetter()
    {
        var key = KeypressParser.Parse("a");
        Assert.Equal("a", key.Name);
        Assert.False(key.Ctrl);
        Assert.False(key.Meta);
        Assert.False(key.Shift);
    }

    [Fact]
    public void ParsesUppercaseLetterAsShift()
    {
        var key = KeypressParser.Parse("A");
        Assert.Equal("a", key.Name);
        Assert.True(key.Shift);
        Assert.False(key.Ctrl);
    }

    [Fact]
    public void ParsesNumber()
    {
        var key = KeypressParser.Parse("5");
        Assert.Equal("number", key.Name);
    }

    // ── Control keys ────────────────────────────────────────────────

    [Fact]
    public void ParsesReturn()
    {
        var key = KeypressParser.Parse("\r");
        Assert.Equal("return", key.Name);
        Assert.Null(key.Raw);
    }

    [Fact]
    public void ParsesEnter()
    {
        var key = KeypressParser.Parse("\n");
        Assert.Equal("enter", key.Name);
    }

    [Fact]
    public void ParsesTab()
    {
        var key = KeypressParser.Parse("\t");
        Assert.Equal("tab", key.Name);
    }

    [Fact]
    public void ParsesBackspace_0x7F()
    {
        var key = KeypressParser.Parse("\u007F");
        Assert.Equal("backspace", key.Name);
        Assert.False(key.Meta);
    }

    [Fact]
    public void ParsesBackspace_0x08()
    {
        var key = KeypressParser.Parse("\b");
        Assert.Equal("backspace", key.Name);
    }

    [Fact]
    public void ParsesEscape()
    {
        var key = KeypressParser.Parse("\u001B");
        Assert.Equal("escape", key.Name);
        Assert.False(key.Meta);
    }

    [Fact]
    public void ParsesDoubleEscapeAsMetaEscape()
    {
        var key = KeypressParser.Parse("\u001B\u001B");
        Assert.Equal("escape", key.Name);
        Assert.True(key.Meta);
    }

    [Fact]
    public void ParsesSpace()
    {
        var key = KeypressParser.Parse(" ");
        Assert.Equal("space", key.Name);
        Assert.False(key.Meta);
    }

    [Fact]
    public void ParsesMetaSpace()
    {
        var key = KeypressParser.Parse("\u001B ");
        Assert.Equal("space", key.Name);
        Assert.True(key.Meta);
    }

    // ── Ctrl+letter ─────────────────────────────────────────────────

    [Fact]
    public void ParsesCtrlA()
    {
        var key = KeypressParser.Parse("\u0001"); // Ctrl+A
        Assert.Equal("a", key.Name);
        Assert.True(key.Ctrl);
    }

    [Fact]
    public void ParsesCtrlC()
    {
        var key = KeypressParser.Parse("\u0003"); // Ctrl+C
        Assert.Equal("c", key.Name);
        Assert.True(key.Ctrl);
    }

    [Fact]
    public void ParsesCtrlZ()
    {
        var key = KeypressParser.Parse("\u001A"); // Ctrl+Z
        Assert.Equal("z", key.Name);
        Assert.True(key.Ctrl);
    }

    // ── Meta keys ───────────────────────────────────────────────────

    [Fact]
    public void ParsesMetaLetter()
    {
        var key = KeypressParser.Parse("\u001Ba"); // ESC a = meta+a
        Assert.Equal("a", key.Name);
        Assert.True(key.Meta);
        Assert.False(key.Shift);
    }

    [Fact]
    public void ParsesMetaBackspace()
    {
        var key = KeypressParser.Parse("\u001B\u007F");
        Assert.Equal("backspace", key.Name);
        Assert.True(key.Meta);
    }

    // ── Arrow keys ──────────────────────────────────────────────────

    [Fact]
    public void ParsesUpArrow()
    {
        var key = KeypressParser.Parse("\u001B[A");
        Assert.Equal("up", key.Name);
    }

    [Fact]
    public void ParsesDownArrow()
    {
        var key = KeypressParser.Parse("\u001B[B");
        Assert.Equal("down", key.Name);
    }

    [Fact]
    public void ParsesRightArrow()
    {
        var key = KeypressParser.Parse("\u001B[C");
        Assert.Equal("right", key.Name);
    }

    [Fact]
    public void ParsesLeftArrow()
    {
        var key = KeypressParser.Parse("\u001B[D");
        Assert.Equal("left", key.Name);
    }

    // ── Function keys ───────────────────────────────────────────────

    [Fact]
    public void ParsesF1_EscOP()
    {
        var key = KeypressParser.Parse("\u001BOP");
        Assert.Equal("f1", key.Name);
    }

    [Fact]
    public void ParsesF5()
    {
        var key = KeypressParser.Parse("\u001B[15~");
        Assert.Equal("f5", key.Name);
    }

    [Fact]
    public void ParsesF12()
    {
        var key = KeypressParser.Parse("\u001B[24~");
        Assert.Equal("f12", key.Name);
    }

    // ── Navigation keys ─────────────────────────────────────────────

    [Fact]
    public void ParsesHome()
    {
        var key = KeypressParser.Parse("\u001B[H");
        Assert.Equal("home", key.Name);
    }

    [Fact]
    public void ParsesEnd()
    {
        var key = KeypressParser.Parse("\u001B[F");
        Assert.Equal("end", key.Name);
    }

    [Fact]
    public void ParsesInsert()
    {
        var key = KeypressParser.Parse("\u001B[2~");
        Assert.Equal("insert", key.Name);
    }

    [Fact]
    public void ParsesDelete()
    {
        var key = KeypressParser.Parse("\u001B[3~");
        Assert.Equal("delete", key.Name);
    }

    [Fact]
    public void ParsesPageUp()
    {
        var key = KeypressParser.Parse("\u001B[5~");
        Assert.Equal("pageup", key.Name);
    }

    [Fact]
    public void ParsesPageDown()
    {
        var key = KeypressParser.Parse("\u001B[6~");
        Assert.Equal("pagedown", key.Name);
    }

    // ── Modifiers ───────────────────────────────────────────────────

    [Fact]
    public void ParsesShiftTab()
    {
        var key = KeypressParser.Parse("\u001B[Z");
        Assert.Equal("tab", key.Name);
        Assert.True(key.Shift);
    }

    [Fact]
    public void ParsesCtrlUpArrow()
    {
        // CSI 1;5 A = Ctrl+Up
        var key = KeypressParser.Parse("\u001B[1;5A");
        Assert.Equal("up", key.Name);
        Assert.True(key.Ctrl);
    }

    [Fact]
    public void ParsesShiftRightArrow()
    {
        // CSI 1;2 C = Shift+Right
        var key = KeypressParser.Parse("\u001B[1;2C");
        Assert.Equal("right", key.Name);
        Assert.True(key.Shift);
    }

    // ── Kitty keyboard protocol ─────────────────────────────────────

    [Fact]
    public void ParsesKittyLetterA()
    {
        // CSI 97 u = 'a' (codepoint 97)
        var key = KeypressParser.Parse("\u001B[97u");
        Assert.Equal("a", key.Name);
        Assert.True(key.IsKittyProtocol == true);
        Assert.Equal(KeyEventType.Press, key.EventType);
    }

    [Fact]
    public void ParsesKittySpace()
    {
        // CSI 32 u = space (codepoint 32)
        var key = KeypressParser.Parse("\u001B[32u");
        Assert.Equal("space", key.Name);
        Assert.True(key.IsPrintable == true);
    }

    [Fact]
    public void ParsesKittyReturn()
    {
        // CSI 13 u = return (codepoint 13)
        var key = KeypressParser.Parse("\u001B[13u");
        Assert.Equal("return", key.Name);
        Assert.True(key.IsPrintable == true);
    }

    [Fact]
    public void ParsesKittyEscape()
    {
        // CSI 27 u = escape (codepoint 27)
        var key = KeypressParser.Parse("\u001B[27u");
        Assert.Equal("escape", key.Name);
        Assert.True(key.IsPrintable == false);
    }

    [Fact]
    public void ParsesKittyWithModifiers()
    {
        // CSI 97;5 u = Ctrl+a (modifiers=5 means bit pattern 4=ctrl set)
        var key = KeypressParser.Parse("\u001B[97;5u");
        Assert.Equal("a", key.Name);
        Assert.True(key.Ctrl);
        Assert.True(key.IsKittyProtocol == true);
    }

    [Fact]
    public void ParsesKittyWithShift()
    {
        // CSI 97;2 u = Shift+a (modifiers=2 means shift bit set)
        var key = KeypressParser.Parse("\u001B[97;2u");
        Assert.Equal("a", key.Name);
        Assert.True(key.Shift);
    }

    [Fact]
    public void ParsesKittyRepeatEvent()
    {
        // CSI 97;1:2 u = 'a' repeat (eventType=2)
        var key = KeypressParser.Parse("\u001B[97;1:2u");
        Assert.Equal("a", key.Name);
        Assert.Equal(KeyEventType.Repeat, key.EventType);
    }

    [Fact]
    public void ParsesKittyReleaseEvent()
    {
        // CSI 97;1:3 u = 'a' release (eventType=3)
        var key = KeypressParser.Parse("\u001B[97;1:3u");
        Assert.Equal("a", key.Name);
        Assert.Equal(KeyEventType.Release, key.EventType);
    }

    [Fact]
    public void ParsesKittyWithTextAsCodepoints()
    {
        // CSI 97;1:1;97 u = 'a' press with text 'a'
        var key = KeypressParser.Parse("\u001B[97;1:1;97u");
        Assert.Equal("a", key.Name);
        Assert.Equal("a", key.Text);
    }

    [Fact]
    public void ParsesKittySpecialUpArrow()
    {
        // CSI 1;1:1 A = up arrow press (kitty enhanced)
        var key = KeypressParser.Parse("\u001B[1;1:1A");
        Assert.Equal("up", key.Name);
        Assert.True(key.IsKittyProtocol == true);
        Assert.Equal(KeyEventType.Press, key.EventType);
    }

    [Fact]
    public void ParsesKittySpecialDeleteRelease()
    {
        // CSI 3;1:3 ~ = delete release (kitty enhanced)
        var key = KeypressParser.Parse("\u001B[3;1:3~");
        Assert.Equal("delete", key.Name);
        Assert.Equal(KeyEventType.Release, key.EventType);
    }

    [Fact]
    public void ParsesKittyCtrlLetter()
    {
        // CSI 1 u = Ctrl+a (codepoint 1)
        var key = KeypressParser.Parse("\u001B[1u");
        Assert.Equal("a", key.Name);
        Assert.True(key.IsPrintable == false);
    }

    [Fact]
    public void ParsesKittyBackspace()
    {
        // CSI 127 u = backspace
        var key = KeypressParser.Parse("\u001B[127u");
        Assert.Equal("backspace", key.Name);
        Assert.True(key.IsPrintable == false);
    }

    [Fact]
    public void ParsesKittyTab()
    {
        // CSI 9 u = tab
        var key = KeypressParser.Parse("\u001B[9u");
        Assert.Equal("tab", key.Name);
        Assert.True(key.IsPrintable == false);
    }

    // ── Edge cases ──────────────────────────────────────────────────

    [Fact]
    public void ParsesEmptyString()
    {
        var key = KeypressParser.Parse("");
        Assert.Equal("", key.Name);
    }

    [Fact]
    public void ParsesNull()
    {
        var key = KeypressParser.Parse((string)null!);
        Assert.Equal("", key.Name);
    }

    [Fact]
    public void OptionReturn()
    {
        var key = KeypressParser.Parse("\u001B\r");
        Assert.Equal("return", key.Name);
        Assert.True(key.Option);
    }

    // ── NonAlphanumericKeys set ─────────────────────────────────────

    [Fact]
    public void NonAlphanumericKeysContainsExpectedKeys()
    {
        Assert.Contains("backspace", KeypressParser.NonAlphanumericKeys);
        Assert.Contains("up", KeypressParser.NonAlphanumericKeys);
        Assert.Contains("down", KeypressParser.NonAlphanumericKeys);
        Assert.Contains("left", KeypressParser.NonAlphanumericKeys);
        Assert.Contains("right", KeypressParser.NonAlphanumericKeys);
        Assert.Contains("home", KeypressParser.NonAlphanumericKeys);
        Assert.Contains("end", KeypressParser.NonAlphanumericKeys);
        Assert.Contains("f1", KeypressParser.NonAlphanumericKeys);
        Assert.Contains("f12", KeypressParser.NonAlphanumericKeys);
    }
}
