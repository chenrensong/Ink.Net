// Tests for Input/KittyKeyboard.cs — 1:1 port of kitty-keyboard.ts
using Ink.Net.Input;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Tests for KittyKeyboard flags and modifiers.</summary>
public class KittyKeyboardTests
{
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
}
