// Tests for Kitty Keyboard protocol — 1:1 port of kitty-keyboard.tsx
using Ink.Net.Input;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Tests for KittyKeyboard flags, modifiers, and protocol parsing.</summary>
public class KittyKeyboardTests
{
    // ── Helper to create kitty protocol CSI u sequences ─────────────
    private static string KittyKey(
        int codepoint,
        int? modifiers = null,
        int? eventType = null,
        int[]? textCodepoints = null)
    {
        var seq = $"\u001b[{codepoint}";
        if (modifiers != null || eventType != null || textCodepoints != null)
        {
            seq += $";{modifiers ?? 1}";
        }

        if (eventType != null || textCodepoints != null)
        {
            seq += $":{eventType ?? 1}";
        }

        if (textCodepoints != null)
        {
            seq += ";" + string.Join(":", textCodepoints);
        }

        seq += "u";
        return seq;
    }

    // ── Flag constants ─────────────────────────────────────────────

    [Fact]
    public void FlagConstants()
    {
        Assert.Equal(1, KittyFlags.DisambiguateEscapeCodes);
        Assert.Equal(2, KittyFlags.ReportEventTypes);
        Assert.Equal(4, KittyFlags.ReportAlternateKeys);
        Assert.Equal(8, KittyFlags.ReportAllKeysAsEscapeCodes);
        Assert.Equal(16, KittyFlags.ReportAssociatedText);
    }

    [Fact]
    public void ModifierConstants()
    {
        Assert.Equal(1, KittyModifiers.Shift);
        Assert.Equal(2, KittyModifiers.Alt);
        Assert.Equal(4, KittyModifiers.Ctrl);
        Assert.Equal(8, KittyModifiers.Super);
        Assert.Equal(16, KittyModifiers.Hyper);
        Assert.Equal(32, KittyModifiers.Meta);
        Assert.Equal(64, KittyModifiers.CapsLock);
        Assert.Equal(128, KittyModifiers.NumLock);
    }

    [Fact]
    public void ResolveFlagsSingleFlag()
    {
        int result = KittyFlags.ResolveFlags(KittyFlags.FlagName.DisambiguateEscapeCodes);
        Assert.Equal(1, result);
    }

    [Fact]
    public void ResolveFlagsMultipleFlags()
    {
        int result = KittyFlags.ResolveFlags(
            KittyFlags.FlagName.DisambiguateEscapeCodes,
            KittyFlags.FlagName.ReportEventTypes,
            KittyFlags.FlagName.ReportAssociatedText);
        Assert.Equal(1 | 2 | 16, result);
    }

    [Fact]
    public void ResolveFlagsEmpty()
    {
        int result = KittyFlags.ResolveFlags();
        Assert.Equal(0, result);
    }

    [Fact]
    public void ResolveFlagsAllFlags()
    {
        int result = KittyFlags.ResolveFlags(
            KittyFlags.FlagName.DisambiguateEscapeCodes,
            KittyFlags.FlagName.ReportEventTypes,
            KittyFlags.FlagName.ReportAlternateKeys,
            KittyFlags.FlagName.ReportAllKeysAsEscapeCodes,
            KittyFlags.FlagName.ReportAssociatedText);
        Assert.Equal(1 | 2 | 4 | 8 | 16, result);
    }

    [Fact]
    public void DefaultKeyboardOptions()
    {
        var opts = new KittyKeyboardOptions();
        Assert.Equal(KittyKeyboardMode.Auto, opts.Mode);
        Assert.Null(opts.Flags);
    }

    // ── Kitty protocol - simple character ───────────────────────────

    [Fact]
    public void KittyProtocol_SimpleCharacter()
    {
        // 'a' key
        var result = KeypressParser.Parse(KittyKey(97));
        Assert.Equal("a", result.Name);
        Assert.False(result.Ctrl);
        Assert.False(result.Shift);
        Assert.False(result.Meta);
        Assert.Equal(KeyEventType.Press, result.EventType);
        Assert.True(result.IsKittyProtocol);
    }

    [Fact]
    public void KittyProtocol_UppercaseCharacterShift()
    {
        // 'A' with shift (modifier 2 = shift + 1)
        var result = KeypressParser.Parse(KittyKey(65, 2));
        Assert.Equal("a", result.Name);
        Assert.True(result.Shift);
        Assert.False(result.Ctrl);
        Assert.Equal(KeyEventType.Press, result.EventType);
    }

    [Fact]
    public void KittyProtocol_CtrlModifier()
    {
        // 'a' with ctrl (modifier 5 = ctrl(4) + 1)
        var result = KeypressParser.Parse(KittyKey(97, 5));
        Assert.Equal("a", result.Name);
        Assert.True(result.Ctrl);
        Assert.False(result.Shift);
        Assert.Equal(KeyEventType.Press, result.EventType);
    }

    [Fact]
    public void KittyProtocol_AltOptionModifier()
    {
        // 'a' with alt (modifier 3 = alt(2) + 1)
        var result = KeypressParser.Parse(KittyKey(97, 3));
        Assert.Equal("a", result.Name);
        Assert.True(result.Option);
        Assert.False(result.Ctrl);
        Assert.Equal(KeyEventType.Press, result.EventType);
    }

    [Fact]
    public void KittyProtocol_SuperModifier()
    {
        // 'a' with super (modifier 9 = super(8) + 1)
        var result = KeypressParser.Parse(KittyKey(97, 9));
        Assert.Equal("a", result.Name);
        Assert.True(result.Super);
        Assert.False(result.Ctrl);
        Assert.Equal(KeyEventType.Press, result.EventType);
    }

    [Fact]
    public void KittyProtocol_HyperModifier()
    {
        // 'a' with hyper (modifier 17 = hyper(16) + 1)
        var result = KeypressParser.Parse(KittyKey(97, 17));
        Assert.Equal("a", result.Name);
        Assert.True(result.Hyper);
        Assert.Equal(false, result.Super);
        Assert.Equal(KeyEventType.Press, result.EventType);
    }

    [Fact]
    public void KittyProtocol_MetaModifier()
    {
        // 'a' with meta (modifier 33 = meta(32) + 1)
        var result = KeypressParser.Parse(KittyKey(97, 33));
        Assert.Equal("a", result.Name);
        Assert.True(result.Meta);
        Assert.Equal(KeyEventType.Press, result.EventType);
    }

    [Fact]
    public void KittyProtocol_CapsLock()
    {
        // 'a' with capsLock (modifier 65 = capsLock(64) + 1)
        var result = KeypressParser.Parse(KittyKey(97, 65));
        Assert.Equal("a", result.Name);
        Assert.True(result.CapsLock);
        Assert.Equal(KeyEventType.Press, result.EventType);
    }

    [Fact]
    public void KittyProtocol_NumLock()
    {
        // 'a' with numLock (modifier 129 = numLock(128) + 1)
        var result = KeypressParser.Parse(KittyKey(97, 129));
        Assert.Equal("a", result.Name);
        Assert.True(result.NumLock);
        Assert.Equal(KeyEventType.Press, result.EventType);
    }

    [Fact]
    public void KittyProtocol_CombinedModifiers_CtrlShift()
    {
        // 'a' with ctrl+shift (modifier 6 = ctrl(4) + shift(1) + 1)
        var result = KeypressParser.Parse(KittyKey(97, 6));
        Assert.Equal("a", result.Name);
        Assert.True(result.Ctrl);
        Assert.True(result.Shift);
        Assert.False(result.Meta);
        Assert.Equal(KeyEventType.Press, result.EventType);
    }

    [Fact]
    public void KittyProtocol_CombinedModifiers_SuperCtrl()
    {
        // 's' with super+ctrl (modifier 13 = super(8) + ctrl(4) + 1)
        var result = KeypressParser.Parse(KittyKey(115, 13));
        Assert.Equal("s", result.Name);
        Assert.True(result.Super);
        Assert.True(result.Ctrl);
        Assert.False(result.Shift);
        Assert.Equal(KeyEventType.Press, result.EventType);
    }

    // ── Special keys ───────────────────────────────────────────────

    [Fact]
    public void KittyProtocol_EscapeKey()
    {
        var result = KeypressParser.Parse(KittyKey(27));
        Assert.Equal("escape", result.Name);
        Assert.Equal(KeyEventType.Press, result.EventType);
    }

    [Fact]
    public void KittyProtocol_ReturnEnterKey()
    {
        var result = KeypressParser.Parse(KittyKey(13));
        Assert.Equal("return", result.Name);
        Assert.Equal(KeyEventType.Press, result.EventType);
    }

    [Fact]
    public void KittyProtocol_TabKey()
    {
        var result = KeypressParser.Parse(KittyKey(9));
        Assert.Equal("tab", result.Name);
        Assert.Equal(KeyEventType.Press, result.EventType);
    }

    [Fact]
    public void KittyProtocol_BackspaceKey()
    {
        var result = KeypressParser.Parse(KittyKey(8));
        Assert.Equal("backspace", result.Name);
        Assert.Equal(KeyEventType.Press, result.EventType);
    }

    [Fact]
    public void KittyProtocol_BackspaceKey_Codepoint127()
    {
        var result = KeypressParser.Parse(KittyKey(127));
        Assert.Equal("backspace", result.Name);
        Assert.Equal(KeyEventType.Press, result.EventType);
    }

    [Fact]
    public void LegacyParser_MetaBackspace()
    {
        var result = KeypressParser.Parse("\u001b\u007f");
        Assert.Equal("backspace", result.Name);
        Assert.True(result.Meta);
    }

    [Fact]
    public void KittyProtocol_SpaceKey()
    {
        var result = KeypressParser.Parse(KittyKey(32));
        Assert.Equal("space", result.Name);
        Assert.Equal(KeyEventType.Press, result.EventType);
    }

    // ── Event types ────────────────────────────────────────────────

    [Fact]
    public void KittyProtocol_EventType_Press()
    {
        var result = KeypressParser.Parse(KittyKey(97, 1, 1));
        Assert.Equal("a", result.Name);
        Assert.Equal(KeyEventType.Press, result.EventType);
    }

    [Fact]
    public void KittyProtocol_EventType_Repeat()
    {
        var result = KeypressParser.Parse(KittyKey(97, 1, 2));
        Assert.Equal("a", result.Name);
        Assert.Equal(KeyEventType.Repeat, result.EventType);
    }

    [Fact]
    public void KittyProtocol_EventType_Release()
    {
        var result = KeypressParser.Parse(KittyKey(97, 1, 3));
        Assert.Equal("a", result.Name);
        Assert.Equal(KeyEventType.Release, result.EventType);
    }

    // ── Number keys and special characters ─────────────────────────

    [Fact]
    public void KittyProtocol_NumberKeys()
    {
        // '1' key
        var result = KeypressParser.Parse(KittyKey(49));
        Assert.Equal("1", result.Name);
        Assert.Equal(KeyEventType.Press, result.EventType);
    }

    [Fact]
    public void KittyProtocol_SpecialCharacter()
    {
        // '@' key
        var result = KeypressParser.Parse(KittyKey(64));
        Assert.Equal("@", result.Name);
        Assert.Equal(KeyEventType.Press, result.EventType);
    }

    // ── Ctrl+letter codepoint 1-26 ────────────────────────────────

    [Fact]
    public void KittyProtocol_CtrlLetterProducesCodepoint1To26()
    {
        // Ctrl+a (codepoint 1, modifier 5 = ctrl + 1)
        var result = KeypressParser.Parse(KittyKey(1, 5));
        Assert.Equal("a", result.Name);
        Assert.True(result.Ctrl);
    }

    // ── Preserves sequence and raw ─────────────────────────────────

    [Fact]
    public void KittyProtocol_PreservesSequenceAndRaw()
    {
        var seq = KittyKey(97, 5);
        var result = KeypressParser.Parse(seq);
        Assert.Equal(seq, result.Sequence);
        Assert.Equal(seq, result.Raw);
    }

    // ── Text-as-codepoints ─────────────────────────────────────────

    [Fact]
    public void KittyProtocol_TextAsCodepoints()
    {
        // 'a' key with text-as-codepoints containing 'A' (shifted)
        var result = KeypressParser.Parse(KittyKey(97, 2, 1, new[] { 65 }));
        Assert.Equal("a", result.Name);
        Assert.Equal("A", result.Text);
        Assert.True(result.Shift);
        Assert.True(result.IsKittyProtocol);
    }

    [Fact]
    public void KittyProtocol_TextAsCodepoints_MultipleCodepoints()
    {
        // Key with text containing multiple codepoints (e.g., composed character)
        var result = KeypressParser.Parse(KittyKey(97, 1, 1, new[] { 72, 101 }));
        Assert.Equal("He", result.Text);
        Assert.True(result.IsKittyProtocol);
    }

    [Fact]
    public void KittyProtocol_SupplementaryUnicodeCodepoint()
    {
        // Emoji: 😀 (U+1F600 = 128512)
        var result = KeypressParser.Parse(KittyKey(128512));
        Assert.Equal("😀", result.Name);
        Assert.True(result.IsKittyProtocol);
    }

    [Fact]
    public void KittyProtocol_TextAsCodepoints_SupplementaryUnicode()
    {
        // Text field with emoji codepoint
        var result = KeypressParser.Parse(KittyKey(97, 1, 1, new[] { 128512 }));
        Assert.Equal("😀", result.Text);
        Assert.True(result.IsKittyProtocol);
    }

    [Fact]
    public void KittyProtocol_TextDefaultsToCharacterFromCodepoint()
    {
        var result = KeypressParser.Parse(KittyKey(97));
        Assert.Equal("a", result.Text);
        Assert.True(result.IsKittyProtocol);
    }

    // ── Kitty-enhanced special key tests ───────────────────────────

    [Fact]
    public void KittyProtocol_ArrowKeys_WithEventType()
    {
        // Up arrow press: CSI 1;1:1 A
        var up = KeypressParser.Parse("\u001b[1;1:1A");
        Assert.Equal("up", up.Name);
        Assert.Equal(KeyEventType.Press, up.EventType);
        Assert.True(up.IsKittyProtocol);

        // Down arrow release: CSI 1;1:3 B
        var down = KeypressParser.Parse("\u001b[1;1:3B");
        Assert.Equal("down", down.Name);
        Assert.Equal(KeyEventType.Release, down.EventType);
        Assert.True(down.IsKittyProtocol);

        // Right arrow repeat: CSI 1;1:2 C
        var right = KeypressParser.Parse("\u001b[1;1:2C");
        Assert.Equal("right", right.Name);
        Assert.Equal(KeyEventType.Repeat, right.EventType);
        Assert.True(right.IsKittyProtocol);

        // Left arrow: CSI 1;1:1 D
        var left = KeypressParser.Parse("\u001b[1;1:1D");
        Assert.Equal("left", left.Name);
        Assert.Equal(KeyEventType.Press, left.EventType);
        Assert.True(left.IsKittyProtocol);
    }

    [Fact]
    public void KittyProtocol_ArrowKeys_WithModifiers()
    {
        // Ctrl+up: CSI 1;5:1 A (modifiers=5 means ctrl(4)+1)
        var result = KeypressParser.Parse("\u001b[1;5:1A");
        Assert.Equal("up", result.Name);
        Assert.True(result.Ctrl);
        Assert.Equal(KeyEventType.Press, result.EventType);
        Assert.True(result.IsKittyProtocol);
    }

    [Fact]
    public void KittyProtocol_HomeAndEndKeys()
    {
        var home = KeypressParser.Parse("\u001b[1;1:1H");
        Assert.Equal("home", home.Name);
        Assert.Equal(KeyEventType.Press, home.EventType);
        Assert.True(home.IsKittyProtocol);

        var end = KeypressParser.Parse("\u001b[1;1:1F");
        Assert.Equal("end", end.Name);
        Assert.Equal(KeyEventType.Press, end.EventType);
        Assert.True(end.IsKittyProtocol);
    }

    [Fact]
    public void KittyProtocol_TildeTerminatedSpecialKeys()
    {
        // Delete: CSI 3;1:1 ~
        var del = KeypressParser.Parse("\u001b[3;1:1~");
        Assert.Equal("delete", del.Name);
        Assert.Equal(KeyEventType.Press, del.EventType);
        Assert.True(del.IsKittyProtocol);

        // Insert: CSI 2;1:1 ~
        var ins = KeypressParser.Parse("\u001b[2;1:1~");
        Assert.Equal("insert", ins.Name);
        Assert.True(ins.IsKittyProtocol);

        // Page up: CSI 5;1:1 ~
        var pgup = KeypressParser.Parse("\u001b[5;1:1~");
        Assert.Equal("pageup", pgup.Name);
        Assert.True(pgup.IsKittyProtocol);

        // F5: CSI 15;1:1 ~
        var f5 = KeypressParser.Parse("\u001b[15;1:1~");
        Assert.Equal("f5", f5.Name);
        Assert.True(f5.IsKittyProtocol);
    }

    [Fact]
    public void KittyProtocol_TildeKeys_WithModifiers()
    {
        // Shift+Delete: CSI 3;2:1 ~ (modifiers=2 means shift(1)+1)
        var result = KeypressParser.Parse("\u001b[3;2:1~");
        Assert.Equal("delete", result.Name);
        Assert.True(result.Shift);
        Assert.Equal(KeyEventType.Press, result.EventType);
        Assert.True(result.IsKittyProtocol);
    }

    // ── Malformed input handling ───────────────────────────────────

    [Fact]
    public void KittyProtocol_InvalidCodepointAboveMaxUnicode()
    {
        // Codepoint 1114112 = 0x110000, one above max Unicode
        var result = KeypressParser.Parse("\u001b[1114112u");
        Assert.Equal("", result.Name);
        Assert.False(result.Ctrl);
        Assert.True(result.IsKittyProtocol);
        Assert.False(result.IsPrintable);
    }

    [Fact]
    public void KittyProtocol_SurrogateCodepoint()
    {
        // Codepoint 0xD800 is a surrogate
        var result = KeypressParser.Parse("\u001b[55296u");
        Assert.Equal("", result.Name);
        Assert.False(result.Ctrl);
        Assert.True(result.IsKittyProtocol);
        Assert.False(result.IsPrintable);
    }

    [Fact]
    public void KittyProtocol_InvalidTextCodepointReplacedWithFallback()
    {
        // Valid primary codepoint, but text field has an invalid codepoint
        var result = KeypressParser.Parse(KittyKey(97, 1, 1, new[] { 1114112 }));
        Assert.Equal("a", result.Name);
        Assert.Equal("?", result.Text);
        Assert.True(result.IsKittyProtocol);
    }

    [Fact]
    public void KittyProtocol_MalformedModifier0()
    {
        // Malformed sequence with modifier 0 (should clamp to 0, not become -1)
        var result = KeypressParser.Parse("\u001b[97;0u");
        Assert.Equal("a", result.Name);
        Assert.False(result.Ctrl);
        Assert.False(result.Shift);
        Assert.False(result.Option);
        Assert.Equal(false, result.Super ?? false);
        Assert.True(result.IsKittyProtocol);
    }

    // ── Legacy fallback ────────────────────────────────────────────

    [Fact]
    public void NonKittySequences_FallBackToLegacyParsing()
    {
        // Regular escape sequence (not kitty protocol) - Up arrow key
        var result = KeypressParser.Parse("\u001b[A");
        Assert.Equal("up", result.Name);
        Assert.Null(result.IsKittyProtocol);
    }

    [Fact]
    public void NonKittySequences_CtrlC()
    {
        // Ctrl+c
        var result = KeypressParser.Parse("\u0003");
        Assert.Equal("c", result.Name);
        Assert.True(result.Ctrl);
        Assert.Null(result.IsKittyProtocol);
    }

    // ── isPrintable field tests ────────────────────────────────────

    [Fact]
    public void KittyProtocol_IsPrintable_TrueForRegularCharacters()
    {
        var result = KeypressParser.Parse(KittyKey(97));
        Assert.True(result.IsPrintable);
    }

    [Fact]
    public void KittyProtocol_IsPrintable_TrueForDigits()
    {
        var result = KeypressParser.Parse(KittyKey(49));
        Assert.True(result.IsPrintable);
    }

    [Fact]
    public void KittyProtocol_IsPrintable_TrueForSymbols()
    {
        var result = KeypressParser.Parse(KittyKey(64));
        Assert.True(result.IsPrintable);
    }

    [Fact]
    public void KittyProtocol_IsPrintable_TrueForEmoji()
    {
        var result = KeypressParser.Parse(KittyKey(128512));
        Assert.True(result.IsPrintable);
    }

    [Fact]
    public void KittyProtocol_IsPrintable_FalseForEscape()
    {
        var result = KeypressParser.Parse(KittyKey(27));
        Assert.False(result.IsPrintable);
    }

    [Fact]
    public void KittyProtocol_IsPrintable_TrueForReturn()
    {
        var result = KeypressParser.Parse(KittyKey(13));
        Assert.True(result.IsPrintable);
    }

    [Fact]
    public void KittyProtocol_IsPrintable_FalseForTab()
    {
        var result = KeypressParser.Parse(KittyKey(9));
        Assert.False(result.IsPrintable);
    }

    [Fact]
    public void KittyProtocol_IsPrintable_TrueForSpace()
    {
        var result = KeypressParser.Parse(KittyKey(32));
        Assert.True(result.IsPrintable);
    }

    [Fact]
    public void KittyProtocol_IsPrintable_FalseForBackspace()
    {
        var result = KeypressParser.Parse(KittyKey(8));
        Assert.False(result.IsPrintable);
    }

    [Fact]
    public void KittyProtocol_IsPrintable_FalseForCtrlLetter()
    {
        // Ctrl+a (codepoint 1)
        var result = KeypressParser.Parse(KittyKey(1, 5));
        Assert.False(result.IsPrintable);
    }

    [Fact]
    public void KittyProtocol_IsPrintable_FalseForSpecialKeys_Arrows()
    {
        // Up arrow via kitty enhanced special key format
        var result = KeypressParser.Parse("\u001b[1;1:1A");
        Assert.False(result.IsPrintable);
    }

    // ── Non-printable key suppression tests ────────────────────────

    [Fact]
    public void KittyProtocol_Capslock_IsNonPrintable()
    {
        var result = KeypressParser.Parse("\u001b[57358u");
        Assert.Equal("capslock", result.Name);
        Assert.False(result.IsPrintable);
        Assert.True(result.IsKittyProtocol);
    }

    [Fact]
    public void KittyProtocol_Printscreen_IsNonPrintable()
    {
        var result = KeypressParser.Parse("\u001b[57361u");
        Assert.Equal("printscreen", result.Name);
        Assert.False(result.IsPrintable);
        Assert.True(result.IsKittyProtocol);
    }

    [Fact]
    public void KittyProtocol_F13_IsNonPrintable()
    {
        var result = KeypressParser.Parse("\u001b[57376u");
        Assert.Equal("f13", result.Name);
        Assert.False(result.IsPrintable);
        Assert.True(result.IsKittyProtocol);
    }

    [Fact]
    public void KittyProtocol_MediaKey_IsNonPrintable()
    {
        var result = KeypressParser.Parse("\u001b[57428u");
        Assert.Equal("mediaplay", result.Name);
        Assert.False(result.IsPrintable);
        Assert.True(result.IsKittyProtocol);
    }

    [Fact]
    public void KittyProtocol_LeftShift_IsNonPrintable()
    {
        var result = KeypressParser.Parse("\u001b[57441u");
        Assert.Equal("leftshift", result.Name);
        Assert.False(result.IsPrintable);
        Assert.True(result.IsKittyProtocol);
    }

    [Fact]
    public void KittyProtocol_LeftControl_IsNonPrintable()
    {
        var result = KeypressParser.Parse("\u001b[57442u");
        Assert.Equal("leftcontrol", result.Name);
        Assert.False(result.IsPrintable);
        Assert.True(result.IsKittyProtocol);
    }

    [Fact]
    public void KittyProtocol_Kp0_IsNonPrintable()
    {
        var result = KeypressParser.Parse("\u001b[57399u");
        Assert.Equal("kp0", result.Name);
        Assert.False(result.IsPrintable);
        Assert.True(result.IsKittyProtocol);
    }

    [Fact]
    public void KittyProtocol_ScrollLock_IsNonPrintable()
    {
        var result = KeypressParser.Parse("\u001b[57359u");
        Assert.Equal("scrolllock", result.Name);
        Assert.False(result.IsPrintable);
        Assert.True(result.IsKittyProtocol);
    }

    [Fact]
    public void KittyProtocol_NumLockKey_IsNonPrintable()
    {
        var result = KeypressParser.Parse("\u001b[57360u");
        Assert.Equal("numlock", result.Name);
        Assert.False(result.IsPrintable);
        Assert.True(result.IsKittyProtocol);
    }

    [Fact]
    public void KittyProtocol_Pause_IsNonPrintable()
    {
        var result = KeypressParser.Parse("\u001b[57362u");
        Assert.Equal("pause", result.Name);
        Assert.False(result.IsPrintable);
        Assert.True(result.IsKittyProtocol);
    }

    [Fact]
    public void KittyProtocol_VolumeKeys_AreNonPrintable()
    {
        // Lower volume (57438)
        var lower = KeypressParser.Parse("\u001b[57438u");
        Assert.Equal("lowervolume", lower.Name);
        Assert.False(lower.IsPrintable);

        // Raise volume (57439)
        var raise = KeypressParser.Parse("\u001b[57439u");
        Assert.Equal("raisevolume", raise.Name);
        Assert.False(raise.IsPrintable);

        // Mute volume (57440)
        var mute = KeypressParser.Parse("\u001b[57440u");
        Assert.Equal("mutevolume", mute.Name);
        Assert.False(mute.IsPrintable);
    }

    // ── Space and return text input tests ──────────────────────────

    [Fact]
    public void KittyProtocol_SpaceKey_HasTextFieldSetToSpaceCharacter()
    {
        var result = KeypressParser.Parse(KittyKey(32));
        Assert.Equal(" ", result.Text);
    }

    [Fact]
    public void KittyProtocol_ReturnKey_HasTextFieldSetToCarriageReturn()
    {
        var result = KeypressParser.Parse(KittyKey(13));
        Assert.Equal("\r", result.Text);
    }
}
