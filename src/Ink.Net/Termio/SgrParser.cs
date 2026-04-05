// -----------------------------------------------------------------------
// <copyright file="SgrParser.cs" company="Ink.Net">
//   Port from Ink (JS) termio/sgr.ts — SGR parameter parser
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Termio;

/// <summary>
/// SGR (Select Graphic Rendition) parameter parser.
/// Parses SGR parameters and applies them to a TextStyle.
/// </summary>
public static class SgrParser
{
    private static readonly NamedColor[] NamedColors =
    {
        NamedColor.Black, NamedColor.Red, NamedColor.Green, NamedColor.Yellow,
        NamedColor.Blue, NamedColor.Magenta, NamedColor.Cyan, NamedColor.White,
        NamedColor.BrightBlack, NamedColor.BrightRed, NamedColor.BrightGreen, NamedColor.BrightYellow,
        NamedColor.BrightBlue, NamedColor.BrightMagenta, NamedColor.BrightCyan, NamedColor.BrightWhite,
    };

    private static readonly UnderlineStyle[] UnderlineStyles =
    {
        UnderlineStyle.None, UnderlineStyle.Single, UnderlineStyle.Double,
        UnderlineStyle.Curly, UnderlineStyle.Dotted, UnderlineStyle.Dashed,
    };

    /// <summary>Apply SGR parameters to a style, returning a new style.</summary>
    public static TextStyle Apply(string paramStr, TextStyle style)
    {
        var s = style.Clone();
        var codes = ParseCodes(paramStr);
        int i = 0;

        while (i < codes.Count)
        {
            int code = codes[i];
            switch (code)
            {
                case 0: s = TextStyle.CreateDefault(); break;
                case 1: s.Bold = true; break;
                case 2: s.Dim = true; break;
                case 3: s.Italic = true; break;
                case 4: s.Underline = UnderlineStyle.Single; break;
                case 5 or 6: s.Blink = true; break;
                case 7: s.Inverse = true; break;
                case 8: s.Hidden = true; break;
                case 9: s.Strikethrough = true; break;
                case 21: s.Underline = UnderlineStyle.Double; break;
                case 22: s.Bold = false; s.Dim = false; break;
                case 23: s.Italic = false; break;
                case 24: s.Underline = UnderlineStyle.None; break;
                case 25: s.Blink = false; break;
                case 27: s.Inverse = false; break;
                case 28: s.Hidden = false; break;
                case 29: s.Strikethrough = false; break;
                case 53: s.Overline = true; break;
                case 55: s.Overline = false; break;
                case >= 30 and <= 37: s.Fg = new TermColor.Named(NamedColors[code - 30]); break;
                case 39: s.Fg = new TermColor.Default(); break;
                case >= 40 and <= 47: s.Bg = new TermColor.Named(NamedColors[code - 40]); break;
                case 49: s.Bg = new TermColor.Default(); break;
                case >= 90 and <= 97: s.Fg = new TermColor.Named(NamedColors[code - 90 + 8]); break;
                case >= 100 and <= 107: s.Bg = new TermColor.Named(NamedColors[code - 100 + 8]); break;
                case 38:
                    if (TryParseExtendedColor(codes, i + 1, out var fgColor, out int fgSkip))
                    { s.Fg = fgColor; i += fgSkip; }
                    break;
                case 48:
                    if (TryParseExtendedColor(codes, i + 1, out var bgColor, out int bgSkip))
                    { s.Bg = bgColor; i += bgSkip; }
                    break;
                case 58:
                    if (TryParseExtendedColor(codes, i + 1, out var ulColor, out int ulSkip))
                    { s.UnderlineColor = ulColor; i += ulSkip; }
                    break;
                case 59: s.UnderlineColor = new TermColor.Default(); break;
            }
            i++;
        }
        return s;
    }

    private static List<int> ParseCodes(string paramStr)
    {
        if (string.IsNullOrEmpty(paramStr)) return new List<int> { 0 };
        var result = new List<int>();
        foreach (var part in paramStr.Split(';', ':'))
        {
            result.Add(int.TryParse(part, out int v) ? v : 0);
        }
        return result;
    }

    private static bool TryParseExtendedColor(List<int> codes, int idx, out TermColor color, out int skip)
    {
        color = new TermColor.Default();
        skip = 0;
        if (idx >= codes.Count) return false;

        if (codes[idx] == 5 && idx + 1 < codes.Count) // 256-color
        {
            color = new TermColor.Indexed(codes[idx + 1]);
            skip = 2;
            return true;
        }
        if (codes[idx] == 2 && idx + 3 < codes.Count) // RGB
        {
            color = new TermColor.Rgb((byte)codes[idx + 1], (byte)codes[idx + 2], (byte)codes[idx + 3]);
            skip = 4;
            return true;
        }
        return false;
    }
}
