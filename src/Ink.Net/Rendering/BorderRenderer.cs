// -----------------------------------------------------------------------
// <copyright file="BorderRenderer.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) render-border.ts
// </copyright>
// -----------------------------------------------------------------------

using Ink.Net.Dom;
using static Facebook.Yoga.YGNodeLayoutAPI;

namespace Ink.Net.Rendering;

/// <summary>
/// Renders borders for a DOM node to the output buffer.
/// <para>1:1 port of Ink JS <c>render-border.ts</c>.</para>
/// </summary>
public static class BorderRenderer
{
    /// <summary>
    /// Render borders for a node.
    /// <para>Corresponds to JS <c>renderBorder(x, y, node, output)</c>.</para>
    /// </summary>
    public static void Render(int x, int y, InkNode node, Output output)
    {
        if (string.IsNullOrEmpty(node.Style.BorderStyle))
            return;

        var yogaNode = node.YogaNode;
        if (yogaNode is null) return;

        int width = (int)YGNodeLayoutGetWidth(yogaNode);
        int height = (int)YGNodeLayoutGetHeight(yogaNode);

        // Look up box style by name
        var box = CliBoxes.Get(node.Style.BorderStyle);
        if (box is null) return;

        // Resolve border colors (per-side falls back to generic)
        string? topBorderColor = node.Style.BorderTopColor ?? node.Style.BorderColor;
        string? bottomBorderColor = node.Style.BorderBottomColor ?? node.Style.BorderColor;
        string? leftBorderColor = node.Style.BorderLeftColor ?? node.Style.BorderColor;
        string? rightBorderColor = node.Style.BorderRightColor ?? node.Style.BorderColor;

        bool dimTopBorder = node.Style.BorderTopDimColor ?? node.Style.BorderDimColor ?? false;
        bool dimBottomBorder = node.Style.BorderBottomDimColor ?? node.Style.BorderDimColor ?? false;
        bool dimLeftBorder = node.Style.BorderLeftDimColor ?? node.Style.BorderDimColor ?? false;
        bool dimRightBorder = node.Style.BorderRightDimColor ?? node.Style.BorderDimColor ?? false;

        bool showTopBorder = node.Style.BorderTop != false;
        bool showBottomBorder = node.Style.BorderBottom != false;
        bool showLeftBorder = node.Style.BorderLeft != false;
        bool showRightBorder = node.Style.BorderRight != false;

        int contentWidth = width - (showLeftBorder ? 1 : 0) - (showRightBorder ? 1 : 0);

        // ── Top border ───────────────────────────────────────────────
        if (showTopBorder)
        {
            string topBorder = Colorizer.Colorize(
                (showLeftBorder ? box.TopLeft : "") +
                RepeatString(box.Top, contentWidth) +
                (showRightBorder ? box.TopRight : ""),
                topBorderColor,
                ColorType.Foreground);

            if (dimTopBorder)
                topBorder = Colorizer.Dim(topBorder);

            output.Write(x, y, topBorder);
        }

        // ── Vertical borders ─────────────────────────────────────────
        int verticalBorderHeight = height;
        if (showTopBorder) verticalBorderHeight--;
        if (showBottomBorder) verticalBorderHeight--;

        int offsetY = showTopBorder ? 1 : 0;

        if (showLeftBorder)
        {
            string leftBorder = RepeatWithNewline(
                Colorizer.Colorize(box.Left, leftBorderColor, ColorType.Foreground),
                verticalBorderHeight);

            if (dimLeftBorder)
                leftBorder = Colorizer.Dim(leftBorder);

            output.Write(x, y + offsetY, leftBorder);
        }

        if (showRightBorder)
        {
            string rightBorder = RepeatWithNewline(
                Colorizer.Colorize(box.Right, rightBorderColor, ColorType.Foreground),
                verticalBorderHeight);

            if (dimRightBorder)
                rightBorder = Colorizer.Dim(rightBorder);

            output.Write(x + width - 1, y + offsetY, rightBorder);
        }

        // ── Bottom border ────────────────────────────────────────────
        if (showBottomBorder)
        {
            string bottomBorder = Colorizer.Colorize(
                (showLeftBorder ? box.BottomLeft : "") +
                RepeatString(box.Bottom, contentWidth) +
                (showRightBorder ? box.BottomRight : ""),
                bottomBorderColor,
                ColorType.Foreground);

            if (dimBottomBorder)
                bottomBorder = Colorizer.Dim(bottomBorder);

            output.Write(x, y + height - 1, bottomBorder);
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────────

    private static string RepeatString(string s, int count)
    {
        if (count <= 0) return "";
        if (s.Length == 1 && count < 1024)
            return new string(s[0], count);
        return string.Concat(Enumerable.Repeat(s, count));
    }

    /// <summary>
    /// Repeat a string with newline separators (matches JS: <c>(str + '\n').repeat(count)</c>).
    /// </summary>
    private static string RepeatWithNewline(string s, int count)
    {
        if (count <= 0) return "";
        return string.Join('\n', Enumerable.Repeat(s, count));
    }
}
