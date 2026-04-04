// -----------------------------------------------------------------------
// <copyright file="Colorizer.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) colorize.ts
//   Replaces chalk with raw ANSI escape sequences. AOT compatible.
// </copyright>
// -----------------------------------------------------------------------

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Ink.Net.Rendering;

/// <summary>
/// Color type for the colorize function.
/// </summary>
public enum ColorType : byte
{
    Foreground,
    Background,
}

/// <summary>
/// ANSI color helper — replaces JS <c>chalk</c> library.
/// <para>
/// Supports named colors (ANSI 16), hex (#rrggbb), rgb(r,g,b), and ansi256(n).
/// 1:1 port of Ink JS <c>colorize.ts</c>.
/// </para>
/// </summary>
public static class Colorizer
{
    // Corresponds to JS: /^rgb\(\s?(\d+),\s?(\d+),\s?(\d+)\s?\)$/
    private static readonly Regex RgbRegex = new(
        @"^rgb\(\s?(\d+),\s?(\d+),\s?(\d+)\s?\)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // Corresponds to JS: /^ansi256\(\s?(\d+)\s?\)$/
    private static readonly Regex Ansi256Regex = new(
        @"^ansi256\(\s?(\d+)\s?\)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // ─── Named ANSI color mapping ────────────────────────────────────

    // Standard 16 named colors, matching chalk/ansi-styles
    // Foreground: 30-37, 90-97; Background: 40-47, 100-107
    private static readonly Dictionary<string, (int Fg, int Bg)> NamedColors = new(StringComparer.OrdinalIgnoreCase)
    {
        ["black"] = (30, 40),
        ["red"] = (31, 41),
        ["green"] = (32, 42),
        ["yellow"] = (33, 43),
        ["blue"] = (34, 44),
        ["magenta"] = (35, 45),
        ["cyan"] = (36, 46),
        ["white"] = (37, 47),
        ["blackBright"] = (90, 100),
        ["gray"] = (90, 100),
        ["grey"] = (90, 100),
        ["redBright"] = (91, 101),
        ["greenBright"] = (92, 102),
        ["yellowBright"] = (93, 103),
        ["blueBright"] = (94, 104),
        ["magentaBright"] = (95, 105),
        ["cyanBright"] = (96, 106),
        ["whiteBright"] = (97, 107),
    };

    /// <summary>
    /// Apply a color to a string.
    /// <para>1:1 corresponds to JS <c>colorize(str, color, type)</c>.</para>
    /// </summary>
    /// <param name="str">The string to colorize.</param>
    /// <param name="color">Color specification (named, #hex, rgb(), ansi256()).</param>
    /// <param name="type">Foreground or background.</param>
    /// <returns>The string wrapped in ANSI color codes.</returns>
    public static string Colorize(string str, string? color, ColorType type)
    {
        if (string.IsNullOrEmpty(color))
            return str;

        // ── Named color ──────────────────────────────────────────────
        if (NamedColors.TryGetValue(color, out var named))
        {
            int code = type == ColorType.Foreground ? named.Fg : named.Bg;
            return $"\x1B[{code}m{str}\x1B[{(type == ColorType.Foreground ? 39 : 49)}m";
        }

        // ── Hex color (#rrggbb or #rgb) ──────────────────────────────
        if (color.StartsWith('#'))
        {
            var (r, g, b) = ParseHexColor(color);
            return type == ColorType.Foreground
                ? $"\x1B[38;2;{r};{g};{b}m{str}\x1B[39m"
                : $"\x1B[48;2;{r};{g};{b}m{str}\x1B[49m";
        }

        // ── ansi256(n) ───────────────────────────────────────────────
        if (color.StartsWith("ansi256", StringComparison.Ordinal))
        {
            var match = Ansi256Regex.Match(color);
            if (!match.Success)
                return str;

            int value = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            return type == ColorType.Foreground
                ? $"\x1B[38;5;{value}m{str}\x1B[39m"
                : $"\x1B[48;5;{value}m{str}\x1B[49m";
        }

        // ── rgb(r, g, b) ─────────────────────────────────────────────
        if (color.StartsWith("rgb", StringComparison.Ordinal))
        {
            var match = RgbRegex.Match(color);
            if (!match.Success)
                return str;

            int r = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            int g = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            int b = int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);

            return type == ColorType.Foreground
                ? $"\x1B[38;2;{r};{g};{b}m{str}\x1B[39m"
                : $"\x1B[48;2;{r};{g};{b}m{str}\x1B[49m";
        }

        return str;
    }

    /// <summary>
    /// Wrap a string with the ANSI dim attribute.
    /// <para>Corresponds to JS <c>chalk.dim(str)</c>.</para>
    /// </summary>
    public static string Dim(string str)
    {
        return $"\x1B[2m{str}\x1B[22m";
    }

    // ─── Helpers ─────────────────────────────────────────────────────

    private static (int R, int G, int B) ParseHexColor(string hex)
    {
        ReadOnlySpan<char> span = hex.AsSpan(1); // skip '#'

        if (span.Length == 3)
        {
            // #rgb → #rrggbb
            int r = ParseHexNibble(span[0]) * 17;
            int g = ParseHexNibble(span[1]) * 17;
            int b = ParseHexNibble(span[2]) * 17;
            return (r, g, b);
        }

        if (span.Length >= 6)
        {
            int r = int.Parse(span[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            int g = int.Parse(span[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            int b = int.Parse(span[4..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return (r, g, b);
        }

        return (0, 0, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ParseHexNibble(char c) => c switch
    {
        >= '0' and <= '9' => c - '0',
        >= 'a' and <= 'f' => c - 'a' + 10,
        >= 'A' and <= 'F' => c - 'A' + 10,
        _ => 0,
    };
}
