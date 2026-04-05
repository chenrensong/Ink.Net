// -----------------------------------------------------------------------
// <copyright file="TermColor.cs" company="Ink.Net">
//   Port from Ink (JS) termio/types.ts — Color types
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Termio;

/// <summary>Named colors from the 16-color palette.</summary>
public enum NamedColor : byte
{
    Black, Red, Green, Yellow, Blue, Magenta, Cyan, White,
    BrightBlack, BrightRed, BrightGreen, BrightYellow,
    BrightBlue, BrightMagenta, BrightCyan, BrightWhite,
}

/// <summary>Color specification — named, indexed (256), RGB, or default.</summary>
public abstract record TermColor
{
    public sealed record Named(NamedColor Name) : TermColor;
    public sealed record Indexed(int Index) : TermColor;
    public sealed record Rgb(byte R, byte G, byte B) : TermColor;
    public sealed record Default : TermColor;
}
