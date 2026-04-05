// -----------------------------------------------------------------------
// <copyright file="TextStyle.cs" company="Ink.Net">
//   Port from Ink (JS) termio/types.ts — Text style attributes
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Termio;

/// <summary>Underline style variants.</summary>
public enum UnderlineStyle : byte { None, Single, Double, Curly, Dotted, Dashed }

/// <summary>Text style attributes — full styling state at a point in the stream.</summary>
public sealed class TextStyle
{
    public bool Bold { get; set; }
    public bool Dim { get; set; }
    public bool Italic { get; set; }
    public UnderlineStyle Underline { get; set; }
    public bool Blink { get; set; }
    public bool Inverse { get; set; }
    public bool Hidden { get; set; }
    public bool Strikethrough { get; set; }
    public bool Overline { get; set; }
    public TermColor Fg { get; set; } = new TermColor.Default();
    public TermColor Bg { get; set; } = new TermColor.Default();
    public TermColor UnderlineColor { get; set; } = new TermColor.Default();

    public static TextStyle CreateDefault() => new();

    public TextStyle Clone() => new()
    {
        Bold = Bold, Dim = Dim, Italic = Italic, Underline = Underline,
        Blink = Blink, Inverse = Inverse, Hidden = Hidden,
        Strikethrough = Strikethrough, Overline = Overline,
        Fg = Fg, Bg = Bg, UnderlineColor = UnderlineColor,
    };

    public bool Equals(TextStyle? other) => other is not null &&
        Bold == other.Bold && Dim == other.Dim && Italic == other.Italic &&
        Underline == other.Underline && Blink == other.Blink && Inverse == other.Inverse &&
        Hidden == other.Hidden && Strikethrough == other.Strikethrough && Overline == other.Overline &&
        Fg == other.Fg && Bg == other.Bg && UnderlineColor == other.UnderlineColor;
}
