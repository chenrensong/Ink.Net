// 1:1 port of parse-keypress.ts
// Parses raw terminal key sequences into structured ParsedKey objects.

using System.Text.RegularExpressions;

namespace Ink.Net.Input;

/// <summary>
/// Event type for kitty protocol key events.
/// </summary>
public enum KeyEventType
{
    Press = 1,
    Repeat = 2,
    Release = 3,
}

/// <summary>
/// Structured result of parsing a single key sequence.
/// 1:1 port of the ParsedKey type in parse-keypress.ts.
/// </summary>
public sealed class ParsedKey
{
    public string Name { get; set; } = "";
    public bool Ctrl { get; set; }
    public bool Meta { get; set; }
    public bool Shift { get; set; }
    public bool Option { get; set; }
    public string Sequence { get; set; } = "";
    public string? Raw { get; set; }
    public string? Code { get; set; }
    public bool? Super { get; set; }
    public bool? Hyper { get; set; }
    public bool? CapsLock { get; set; }
    public bool? NumLock { get; set; }
    public KeyEventType? EventType { get; set; }
    public bool? IsKittyProtocol { get; set; }
    public string? Text { get; set; }
    /// <summary>
    /// Whether this key represents printable text input.
    /// When false, the key is a control/function/modifier key.
    /// Only set by the kitty protocol parser.
    /// </summary>
    public bool? IsPrintable { get; set; }
}

/// <summary>
/// Parses raw terminal key sequence strings into <see cref="ParsedKey"/> objects.
/// 1:1 port of parse-keypress.ts.
/// </summary>
public static class KeypressParser
{
    // ── regex patterns ───────────────────────────────────────────────
    private static readonly Regex MetaKeyCodeRe = new(@"^(?:\x1b)([a-zA-Z0-9])$", RegexOptions.Compiled);
    private static readonly Regex FnKeyRe = new(
        @"^(?:\x1b+)(O|N|\[|\[\[)(?:(\d+)(?:;(\d+))?([~^$])|(?:1;)?(\d+)?([a-zA-Z]))",
        RegexOptions.Compiled);

    // Kitty keyboard protocol: CSI codepoint ; modifiers [: eventType] [; text-as-codepoints] u
    private static readonly Regex KittyKeyRe = new(
        @"^\x1b\[(\d+)(?:;(\d+)(?::(\d+))?(?:;([\d:]+))?)?u$",
        RegexOptions.Compiled);

    // Kitty-enhanced special keys: CSI number ; modifiers : eventType {letter|~}
    private static readonly Regex KittySpecialKeyRe = new(
        @"^\x1b\[(\d+);(\d+):(\d+)([A-Za-z~])$",
        RegexOptions.Compiled);

    // ── key name lookup tables ───────────────────────────────────────

    private static readonly Dictionary<string, string> KeyNameMap = new()
    {
        /* xterm/gnome ESC O letter */
        ["OP"] = "f1", ["OQ"] = "f2", ["OR"] = "f3", ["OS"] = "f4",
        /* vt220-style ESC [ letter */
        ["[P"] = "f1", ["[Q"] = "f2", ["[R"] = "f3", ["[S"] = "f4",
        /* xterm/rxvt ESC [ number ~ */
        ["[11~"] = "f1", ["[12~"] = "f2", ["[13~"] = "f3", ["[14~"] = "f4",
        /* Cygwin / libuv */
        ["[[A"] = "f1", ["[[B"] = "f2", ["[[C"] = "f3", ["[[D"] = "f4", ["[[E"] = "f5",
        /* common */
        ["[15~"] = "f5", ["[17~"] = "f6", ["[18~"] = "f7", ["[19~"] = "f8",
        ["[20~"] = "f9", ["[21~"] = "f10", ["[23~"] = "f11", ["[24~"] = "f12",
        /* xterm ESC [ letter */
        ["[A"] = "up", ["[B"] = "down", ["[C"] = "right", ["[D"] = "left",
        ["[E"] = "clear", ["[F"] = "end", ["[H"] = "home",
        /* xterm/gnome ESC O letter */
        ["OA"] = "up", ["OB"] = "down", ["OC"] = "right", ["OD"] = "left",
        ["OE"] = "clear", ["OF"] = "end", ["OH"] = "home",
        /* xterm/rxvt ESC [ number ~ */
        ["[1~"] = "home", ["[2~"] = "insert", ["[3~"] = "delete",
        ["[4~"] = "end", ["[5~"] = "pageup", ["[6~"] = "pagedown",
        /* putty */
        ["[[5~"] = "pageup", ["[[6~"] = "pagedown",
        /* rxvt */
        ["[7~"] = "home", ["[8~"] = "end",
        /* rxvt keys with modifiers */
        ["[a"] = "up", ["[b"] = "down", ["[c"] = "right", ["[d"] = "left", ["[e"] = "clear",
        ["[2$"] = "insert", ["[3$"] = "delete", ["[5$"] = "pageup",
        ["[6$"] = "pagedown", ["[7$"] = "home", ["[8$"] = "end",
        ["Oa"] = "up", ["Ob"] = "down", ["Oc"] = "right", ["Od"] = "left", ["Oe"] = "clear",
        ["[2^"] = "insert", ["[3^"] = "delete", ["[5^"] = "pageup",
        ["[6^"] = "pagedown", ["[7^"] = "home", ["[8^"] = "end",
        /* misc */
        ["[Z"] = "tab",
    };

    /// <summary>All known non-alphanumeric key names.</summary>
    public static readonly HashSet<string> NonAlphanumericKeys;

    static KeypressParser()
    {
        NonAlphanumericKeys = new HashSet<string>(KeyNameMap.Values) { "backspace" };
    }

    // ── shift / ctrl code sets ───────────────────────────────────────

    private static readonly HashSet<string> ShiftCodes = new()
    {
        "[a", "[b", "[c", "[d", "[e",
        "[2$", "[3$", "[5$", "[6$", "[7$", "[8$",
        "[Z",
    };

    private static readonly HashSet<string> CtrlCodes = new()
    {
        "Oa", "Ob", "Oc", "Od", "Oe",
        "[2^", "[3^", "[5^", "[6^", "[7^", "[8^",
    };

    // ── kitty special key maps ───────────────────────────────────────

    private static readonly Dictionary<string, string> KittySpecialLetterKeys = new()
    {
        ["A"] = "up", ["B"] = "down", ["C"] = "right", ["D"] = "left",
        ["E"] = "clear", ["F"] = "end", ["H"] = "home",
        ["P"] = "f1", ["Q"] = "f2", ["R"] = "f3", ["S"] = "f4",
    };

    private static readonly Dictionary<int, string> KittySpecialNumberKeys = new()
    {
        [2] = "insert", [3] = "delete", [5] = "pageup", [6] = "pagedown",
        [7] = "home", [8] = "end",
        [11] = "f1", [12] = "f2", [13] = "f3", [14] = "f4",
        [15] = "f5", [17] = "f6", [18] = "f7", [19] = "f8",
        [20] = "f9", [21] = "f10", [23] = "f11", [24] = "f12",
    };

    // Map of special codepoints to key names in kitty protocol
    private static readonly Dictionary<int, string> KittyCodepointNames = new()
    {
        [27] = "escape",
        [9] = "tab",
        [127] = "backspace",
        [8] = "backspace",
        [57358] = "capslock",
        [57359] = "scrolllock",
        [57360] = "numlock",
        [57361] = "printscreen",
        [57362] = "pause",
        [57363] = "menu",
        [57376] = "f13", [57377] = "f14", [57378] = "f15", [57379] = "f16",
        [57380] = "f17", [57381] = "f18", [57382] = "f19", [57383] = "f20",
        [57384] = "f21", [57385] = "f22", [57386] = "f23", [57387] = "f24",
        [57388] = "f25", [57389] = "f26", [57390] = "f27", [57391] = "f28",
        [57392] = "f29", [57393] = "f30", [57394] = "f31", [57395] = "f32",
        [57396] = "f33", [57397] = "f34", [57398] = "f35",
        [57399] = "kp0", [57400] = "kp1", [57401] = "kp2", [57402] = "kp3",
        [57403] = "kp4", [57404] = "kp5", [57405] = "kp6", [57406] = "kp7",
        [57407] = "kp8", [57408] = "kp9",
        [57409] = "kpdecimal", [57410] = "kpdivide", [57411] = "kpmultiply",
        [57412] = "kpsubtract", [57413] = "kpadd", [57414] = "kpenter",
        [57415] = "kpequal", [57416] = "kpseparator",
        [57417] = "kpleft", [57418] = "kpright", [57419] = "kpup", [57420] = "kpdown",
        [57421] = "kppageup", [57422] = "kppagedown", [57423] = "kphome",
        [57424] = "kpend", [57425] = "kpinsert", [57426] = "kpdelete",
        [57427] = "kpbegin",
        [57428] = "mediaplay", [57429] = "mediapause", [57430] = "mediaplaypause",
        [57431] = "mediareverse", [57432] = "mediastop",
        [57433] = "mediafastforward", [57434] = "mediarewind",
        [57435] = "mediatracknext", [57436] = "mediatrackprevious",
        [57437] = "mediarecord",
        [57438] = "lowervolume", [57439] = "raisevolume", [57440] = "mutevolume",
        [57441] = "leftshift", [57442] = "leftcontrol", [57443] = "leftalt",
        [57444] = "leftsuper", [57445] = "lefthyper", [57446] = "leftmeta",
        [57447] = "rightshift", [57448] = "rightcontrol", [57449] = "rightalt",
        [57450] = "rightsuper", [57451] = "righthyper", [57452] = "rightmeta",
        [57453] = "isoLevel3Shift", [57454] = "isoLevel5Shift",
    };

    // ── helpers ──────────────────────────────────────────────────────

    /// <summary>Valid Unicode codepoint range, excluding surrogates.</summary>
    private static bool IsValidCodepoint(int cp)
        => cp >= 0 && cp <= 0x10FFFF && !(cp >= 0xD800 && cp <= 0xDFFF);

    private static string SafeFromCodePoint(int cp)
        => IsValidCodepoint(cp) ? char.ConvertFromUtf32(cp) : "?";

    private static KeyEventType ResolveEventType(int value)
    {
        if (value == 3) return KeyEventType.Release;
        if (value == 2) return KeyEventType.Repeat;
        return KeyEventType.Press;
    }

    private static void ApplyKittyModifiers(ParsedKey key, int modifiers)
    {
        key.Ctrl = (modifiers & KittyModifiers.Ctrl) != 0;
        key.Shift = (modifiers & KittyModifiers.Shift) != 0;
        key.Meta = (modifiers & KittyModifiers.Meta) != 0;
        key.Option = (modifiers & KittyModifiers.Alt) != 0;
        key.Super = (modifiers & KittyModifiers.Super) != 0;
        key.Hyper = (modifiers & KittyModifiers.Hyper) != 0;
        key.CapsLock = (modifiers & KittyModifiers.CapsLock) != 0;
        key.NumLock = (modifiers & KittyModifiers.NumLock) != 0;
    }

    // ── kitty protocol parsers ───────────────────────────────────────

    private static ParsedKey? ParseKittyKeypress(string s)
    {
        var match = KittyKeyRe.Match(s);
        if (!match.Success) return null;

        int codepoint = int.Parse(match.Groups[1].Value);
        int modifiers = match.Groups[2].Success ? Math.Max(0, int.Parse(match.Groups[2].Value) - 1) : 0;
        int eventType = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 1;
        string? textField = match.Groups[4].Success ? match.Groups[4].Value : null;

        if (!IsValidCodepoint(codepoint))
            return null;

        // Parse text-as-codepoints field (colon-separated Unicode codepoints)
        string? text = null;
        if (textField != null)
        {
            text = string.Join("", textField.Split(':')
                .Select(cp => SafeFromCodePoint(int.Parse(cp))));
        }

        // Determine key name from codepoint
        string name;
        bool isPrintable;
        if (codepoint == 32)
        {
            name = "space";
            isPrintable = true;
        }
        else if (codepoint == 13)
        {
            name = "return";
            isPrintable = true;
        }
        else if (KittyCodepointNames.TryGetValue(codepoint, out var knownName))
        {
            name = knownName;
            isPrintable = false;
        }
        else if (codepoint >= 1 && codepoint <= 26)
        {
            // Ctrl+letter comes as codepoint 1-26
            name = ((char)(codepoint + 96)).ToString(); // 'a' is 97
            isPrintable = false;
        }
        else
        {
            name = SafeFromCodePoint(codepoint).ToLowerInvariant();
            isPrintable = true;
        }

        // Default text to the character from the codepoint when not explicitly provided
        if (isPrintable && text == null)
        {
            text = SafeFromCodePoint(codepoint);
        }

        var key = new ParsedKey
        {
            Name = name,
            EventType = ResolveEventType(eventType),
            Sequence = s,
            Raw = s,
            IsKittyProtocol = true,
            IsPrintable = isPrintable,
            Text = text,
        };

        ApplyKittyModifiers(key, modifiers);
        return key;
    }

    private static ParsedKey? ParseKittySpecialKey(string s)
    {
        var match = KittySpecialKeyRe.Match(s);
        if (!match.Success) return null;

        int number = int.Parse(match.Groups[1].Value);
        int modifiers = Math.Max(0, int.Parse(match.Groups[2].Value) - 1);
        int eventType = int.Parse(match.Groups[3].Value);
        string terminator = match.Groups[4].Value;

        string? name;
        if (terminator == "~")
            KittySpecialNumberKeys.TryGetValue(number, out name);
        else
            KittySpecialLetterKeys.TryGetValue(terminator, out name);

        if (name == null) return null;

        var key = new ParsedKey
        {
            Name = name,
            EventType = ResolveEventType(eventType),
            Sequence = s,
            Raw = s,
            IsKittyProtocol = true,
            IsPrintable = false,
        };

        ApplyKittyModifiers(key, modifiers);
        return key;
    }

    // ── main entry point ─────────────────────────────────────────────

    /// <summary>
    /// Parse a single key sequence (string) into a <see cref="ParsedKey"/>.
    /// Handles kitty protocol, legacy function keys, meta keys, and plain characters.
    /// </summary>
    public static ParsedKey Parse(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return new ParsedKey { Sequence = s ?? "", Raw = s };
        }

        // Try kitty keyboard protocol parsers first
        var kittyResult = ParseKittyKeypress(s);
        if (kittyResult != null) return kittyResult;

        var kittySpecialResult = ParseKittySpecialKey(s);
        if (kittySpecialResult != null) return kittySpecialResult;

        // If the input matched the kitty CSI-u pattern but was rejected (e.g.,
        // invalid codepoint), return a safe empty keypress
        if (KittyKeyRe.IsMatch(s))
        {
            return new ParsedKey
            {
                Name = "",
                Sequence = s,
                Raw = s,
                IsKittyProtocol = true,
                IsPrintable = false,
            };
        }

        // Legacy parsing
        var key = new ParsedKey
        {
            Sequence = s,
            Raw = s,
        };

        if (s == "\r" || s == "\x1b\r")
        {
            key.Raw = null;
            key.Name = "return";
            key.Option = s.Length == 2;
        }
        else if (s == "\n")
        {
            key.Name = "enter";
        }
        else if (s == "\t")
        {
            key.Name = "tab";
        }
        else if (s == "\b" || s == "\x1b\b")
        {
            key.Name = "backspace";
            key.Meta = s[0] == '\x1b';
        }
        else if (s == "\x7f" || s == "\x1b\x7f")
        {
            key.Name = "backspace";
            key.Meta = s[0] == '\x1b';
        }
        else if (s == "\x1b" || s == "\x1b\x1b")
        {
            key.Name = "escape";
            key.Meta = s.Length == 2;
        }
        else if (s == " " || s == "\x1b ")
        {
            key.Name = "space";
            key.Meta = s.Length == 2;
        }
        else if (s.Length == 1 && s[0] <= '\x1a')
        {
            // ctrl+letter
            key.Name = ((char)(s[0] + 'a' - 1)).ToString();
            key.Ctrl = true;
        }
        else if (s.Length == 1 && s[0] >= '0' && s[0] <= '9')
        {
            key.Name = "number";
        }
        else if (s.Length == 1 && s[0] >= 'a' && s[0] <= 'z')
        {
            key.Name = s;
        }
        else if (s.Length == 1 && s[0] >= 'A' && s[0] <= 'Z')
        {
            key.Name = s.ToLowerInvariant();
            key.Shift = true;
        }
        else
        {
            Match parts;
            if ((parts = MetaKeyCodeRe.Match(s)).Success)
            {
                key.Name = parts.Groups[1].Value.ToLowerInvariant();
                key.Meta = true;
                key.Shift = parts.Groups[1].Value.All(c => c >= 'A' && c <= 'Z');
            }
            else if ((parts = FnKeyRe.Match(s)).Success)
            {
                // Check for double-escape (option key on macOS)
                var segs = s.EnumerateRunes().ToArray();
                if (segs.Length >= 2
                    && segs[0].Value == '\u001b'
                    && segs[1].Value == '\u001b')
                {
                    key.Option = true;
                }

                // Reassemble the key code leaving out leading \x1b's,
                // the modifier key bitflag and any meaningless "1;" sequence
                var codeParts = new[] { parts.Groups[1].Value, parts.Groups[2].Value, parts.Groups[4].Value, parts.Groups[6].Value };
                string code = string.Join("", codeParts.Where(p => !string.IsNullOrEmpty(p)));

                string modStr = !string.IsNullOrEmpty(parts.Groups[3].Value)
                    ? parts.Groups[3].Value
                    : (!string.IsNullOrEmpty(parts.Groups[5].Value)
                        ? parts.Groups[5].Value
                        : "1");
                int modifier = int.Parse(modStr) - 1;

                key.Ctrl = (modifier & 4) != 0;
                key.Meta = (modifier & 10) != 0;
                key.Shift = (modifier & 1) != 0;
                key.Code = code;

                key.Name = KeyNameMap.GetValueOrDefault(code, "");
                key.Shift = ShiftCodes.Contains(code) || key.Shift;
                key.Ctrl = CtrlCodes.Contains(code) || key.Ctrl;
            }
        }

        return key;
    }

    /// <summary>
    /// Parse from raw bytes (Uint8Array equivalent).
    /// Handles high-byte meta encoding (byte &gt; 127).
    /// </summary>
    public static ParsedKey Parse(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length == 1 && bytes[0] > 127)
        {
            byte adjusted = (byte)(bytes[0] - 128);
            string s = "\x1b" + System.Text.Encoding.UTF8.GetString(new[] { adjusted });
            return Parse(s);
        }

        return Parse(System.Text.Encoding.UTF8.GetString(bytes));
    }
}
