// -----------------------------------------------------------------------
// HooksUseInputKittyTests.cs — Tests for Kitty keyboard protocol handling.
// Aligned with ink/test/hooks-use-input-kitty.tsx
// -----------------------------------------------------------------------

using Ink.Net.Input;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>
/// Tests for Kitty keyboard protocol key handling via InputHandler.
/// <para>Aligned with JS <c>test/hooks-use-input-kitty.tsx</c>.</para>
/// </summary>
public class HooksUseInputKittyTests
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
    public void HandleKittySuperModifier()
    {
        // Aligned with JS: "useInput - handle kitty protocol super modifier"
        // 's' with super modifier (modifier 9 = super(8) + 1)
        var (input, key) = HandleSingleInput("\u001B[115;9u");
        Assert.Equal("s", input);
        Assert.True(key.Super);
    }

    [Fact]
    public void HandleKittyHyperModifier()
    {
        // Aligned with JS: "useInput - handle kitty protocol hyper modifier"
        // 'h' with hyper modifier (modifier 17 = hyper(16) + 1)
        var (input, key) = HandleSingleInput("\u001B[104;17u");
        Assert.Equal("h", input);
        Assert.True(key.Hyper);
    }

    [Fact]
    public void HandleKittyCapsLock()
    {
        // Aligned with JS: "useInput - handle kitty protocol capsLock"
        // 'a' with capsLock (modifier 65 = capsLock(64) + 1)
        var (input, key) = HandleSingleInput("\u001B[97;65u");
        Assert.Equal("a", input);
        Assert.True(key.CapsLock);
    }

    [Fact]
    public void HandleKittyNumLock()
    {
        // Aligned with JS: "useInput - handle kitty protocol numLock"
        // 'a' with numLock (modifier 129 = numLock(128) + 1)
        var (input, key) = HandleSingleInput("\u001B[97;129u");
        Assert.Equal("a", input);
        Assert.True(key.NumLock);
    }

    [Fact]
    public void HandleKittySuperCtrl()
    {
        // Aligned with JS: "useInput - handle kitty protocol super+ctrl"
        // 's' with super+ctrl (modifier 13 = super(8) + ctrl(4) + 1)
        var (input, key) = HandleSingleInput("\u001B[115;13u");
        Assert.Equal("s", input);
        Assert.True(key.Super);
        Assert.True(key.Ctrl);
    }

    [Fact]
    public void HandleKittyPressEvent()
    {
        // Aligned with JS: "useInput - handle kitty protocol press event"
        // 'a' press event (eventType 1 = press)
        var (input, key) = HandleSingleInput("\u001B[97;1:1u");
        Assert.Equal("a", input);
        Assert.Equal(InputEventType.Press, key.EventType);
    }

    [Fact]
    public void HandleKittyRepeatEvent()
    {
        // Aligned with JS: "useInput - handle kitty protocol repeat event"
        // 'a' repeat event (eventType 2 = repeat)
        var (input, key) = HandleSingleInput("\u001B[97;1:2u");
        Assert.Equal("a", input);
        Assert.Equal(InputEventType.Repeat, key.EventType);
    }

    [Fact]
    public void HandleKittyReleaseEvent()
    {
        // Aligned with JS: "useInput - handle kitty protocol release event"
        // 'a' release event (eventType 3 = release)
        var (input, key) = HandleSingleInput("\u001B[97;1:3u");
        Assert.Equal("a", input);
        Assert.Equal(InputEventType.Release, key.EventType);
    }

    [Fact]
    public void HandleKittyEscapeKey()
    {
        // Aligned with JS: "useInput - handle kitty protocol escape key"
        // Escape key
        var (input, key) = HandleSingleInput("\u001B[27u");
        Assert.True(key.Escape);
    }

    [Fact]
    public void HandleKittyBackspace()
    {
        // Aligned with JS: "useInput - handle kitty protocol backspace (codepoint 127)"
        var (input, key) = HandleSingleInput("\u001B[127u");
        Assert.True(key.Backspace);
    }

    [Fact]
    public void HandleKittyDelete()
    {
        // Aligned with JS: "useInput - handle kitty protocol delete"
        var (input, key) = HandleSingleInput("\u001B[3;1:1~");
        Assert.True(key.Delete);
    }

    [Fact]
    public void HandleKittyNonPrintable_CapsLockKey_ProducesEmptyInput()
    {
        // Aligned with JS: "useInput - non-printable kitty key (capslock) produces empty input"
        // CapsLock (codepoint 57358)
        var (input, key) = HandleSingleInput("\u001B[57358u");
        Assert.Equal("", input);
    }

    [Fact]
    public void HandleKittyNonPrintable_F13_ProducesEmptyInput()
    {
        // Aligned with JS: "useInput - non-printable kitty key (f13) produces empty input"
        // F13 (codepoint 57376)
        var (input, key) = HandleSingleInput("\u001B[57376u");
        Assert.Equal("", input);
    }

    [Fact]
    public void HandleKittyNonPrintable_PrintScreen_ProducesEmptyInput()
    {
        // Aligned with JS: "useInput - non-printable kitty key (printscreen) produces empty input"
        // PrintScreen (codepoint 57361)
        var (input, key) = HandleSingleInput("\u001B[57361u");
        Assert.Equal("", input);
    }

    [Fact]
    public void HandleKittySpaceKey()
    {
        // Aligned with JS: "useInput - kitty protocol space key produces space input"
        // Space key (codepoint 32)
        var (input, key) = HandleSingleInput("\u001B[32u");
        Assert.Equal(" ", input);
    }

    [Fact]
    public void HandleKittyReturnKey()
    {
        // Aligned with JS: "useInput - kitty protocol return key produces carriage return input"
        // Return key (codepoint 13)
        var (input, key) = HandleSingleInput("\u001B[13u");
        Assert.True(key.Return);
    }

    [Fact]
    public void HandleKittyCtrlLetterViaCodepoint()
    {
        // Aligned with JS: "useInput - kitty protocol ctrl+letter via codepoint 1-26 produces input"
        // Ctrl+a via codepoint 1 form (modifier 5 = ctrl(4) + 1)
        var (input, key) = HandleSingleInput("\u001B[1;5u");
        Assert.True(key.Ctrl);
        Assert.Equal("a", input);
    }

    [Fact]
    public void HandleKittyCtrlC_ExitOnCtrlCFalse()
    {
        // Aligned with JS: "useInput - handle Ctrl+C via kitty codepoint-3 form when exitOnCtrlC is false"
        // Ctrl+C via kitty codepoint 3 form (modifier 5 = ctrl(4) + 1)
        var handler = new InputHandler { ExitOnCtrlC = false };
        var received = new List<(string Input, KeyInfo Key)>();

        handler.Register((input, key) => received.Add((input, key)));
        handler.HandleData("\u001B[3;5u");
        handler.Dispose();

        Assert.Single(received);
        Assert.True(received[0].Key.Ctrl);
        Assert.Equal("c", received[0].Input);
    }
}
