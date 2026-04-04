// 1:1 port of kitty-keyboard.ts
// Kitty keyboard protocol flags and modifiers.
// @see https://sw.kovidgoyal.net/kitty/keyboard-protocol/

namespace Ink.Net.Input;

/// <summary>
/// Kitty keyboard protocol flags.
/// </summary>
public static class KittyFlags
{
    public const int DisambiguateEscapeCodes = 1;
    public const int ReportEventTypes = 2;
    public const int ReportAlternateKeys = 4;
    public const int ReportAllKeysAsEscapeCodes = 8;
    public const int ReportAssociatedText = 16;

    /// <summary>
    /// Valid flag names for the kitty keyboard protocol.
    /// </summary>
    public enum FlagName
    {
        DisambiguateEscapeCodes,
        ReportEventTypes,
        ReportAlternateKeys,
        ReportAllKeysAsEscapeCodes,
        ReportAssociatedText,
    }

    /// <summary>
    /// Converts an array of flag names to the corresponding bitmask value.
    /// </summary>
    public static int ResolveFlags(params FlagName[] flags)
    {
        int result = 0;
        foreach (var flag in flags)
        {
            result |= flag switch
            {
                FlagName.DisambiguateEscapeCodes => DisambiguateEscapeCodes,
                FlagName.ReportEventTypes => ReportEventTypes,
                FlagName.ReportAlternateKeys => ReportAlternateKeys,
                FlagName.ReportAllKeysAsEscapeCodes => ReportAllKeysAsEscapeCodes,
                FlagName.ReportAssociatedText => ReportAssociatedText,
                _ => 0,
            };
        }

        return result;
    }
}

/// <summary>
/// Kitty keyboard modifier bits.
/// These are used in the modifier parameter of CSI u sequences.
/// Note: The actual modifier value is (modifiers - 1) as per the protocol.
/// </summary>
public static class KittyModifiers
{
    public const int Shift = 1;
    public const int Alt = 2;
    public const int Ctrl = 4;
    public const int Super = 8;
    public const int Hyper = 16;
    public const int Meta = 32;
    public const int CapsLock = 64;
    public const int NumLock = 128;
}

/// <summary>
/// Options for configuring kitty keyboard protocol.
/// </summary>
public class KittyKeyboardOptions
{
    /// <summary>
    /// Mode for kitty keyboard protocol support.
    /// - Auto: Attempt to detect terminal support (default)
    /// - Enabled: Force enable the protocol
    /// - Disabled: Never enable the protocol
    /// </summary>
    public KittyKeyboardMode Mode { get; set; } = KittyKeyboardMode.Auto;

    /// <summary>
    /// Protocol flags to request from the terminal.
    /// </summary>
    public KittyFlags.FlagName[]? Flags { get; set; }
}

public enum KittyKeyboardMode
{
    Auto,
    Enabled,
    Disabled,
}
