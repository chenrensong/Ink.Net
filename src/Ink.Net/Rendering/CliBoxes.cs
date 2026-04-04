// -----------------------------------------------------------------------
// <copyright file="CliBoxes.cs" company="Ink.Net">
//   Inlined from npm cli-boxes package.
//   Box drawing character definitions for border rendering.
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Rendering;

/// <summary>
/// A set of box-drawing characters for rendering borders.
/// Corresponds to a single entry from JS <c>cli-boxes</c> package.
/// </summary>
public sealed class BoxStyle
{
    public string TopLeft { get; }
    public string Top { get; }
    public string TopRight { get; }
    public string Right { get; }
    public string BottomRight { get; }
    public string Bottom { get; }
    public string BottomLeft { get; }
    public string Left { get; }

    public BoxStyle(
        string topLeft, string top, string topRight,
        string right, string bottomRight, string bottom,
        string bottomLeft, string left)
    {
        TopLeft = topLeft;
        Top = top;
        TopRight = topRight;
        Right = right;
        BottomRight = bottomRight;
        Bottom = bottom;
        BottomLeft = bottomLeft;
        Left = left;
    }
}

/// <summary>
/// Built-in box styles, inlined from the <c>cli-boxes</c> npm package.
/// <para>Used by <see cref="BorderRenderer"/> to look up border characters by name.</para>
/// </summary>
public static class CliBoxes
{
    public static readonly BoxStyle Single = new("┌", "─", "┐", "│", "┘", "─", "└", "│");
    public static readonly BoxStyle Double = new("╔", "═", "╗", "║", "╝", "═", "╚", "║");
    public static readonly BoxStyle Round = new("╭", "─", "╮", "│", "╯", "─", "╰", "│");
    public static readonly BoxStyle Bold = new("┏", "━", "┓", "┃", "┛", "━", "┗", "┃");
    public static readonly BoxStyle SingleDouble = new("╓", "─", "╖", "║", "╜", "─", "╙", "║");
    public static readonly BoxStyle DoubleSingle = new("╒", "═", "╕", "│", "╛", "═", "╘", "│");
    public static readonly BoxStyle Classic = new("+", "-", "+", "|", "+", "-", "+", "|");
    public static readonly BoxStyle Arrow = new("↘", "↓", "↙", "←", "↖", "↑", "↗", "→");

    private static readonly Dictionary<string, BoxStyle> StyleMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["single"] = Single,
        ["double"] = Double,
        ["round"] = Round,
        ["bold"] = Bold,
        ["singleDouble"] = SingleDouble,
        ["doubleSingle"] = DoubleSingle,
        ["classic"] = Classic,
        ["arrow"] = Arrow,
    };

    /// <summary>
    /// Look up a box style by name.
    /// <para>Corresponds to JS <c>cliBoxes[node.style.borderStyle]</c>.</para>
    /// </summary>
    public static BoxStyle? Get(string name)
    {
        return StyleMap.GetValueOrDefault(name);
    }
}
