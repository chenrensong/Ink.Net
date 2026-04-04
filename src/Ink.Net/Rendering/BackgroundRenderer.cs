// -----------------------------------------------------------------------
// <copyright file="BackgroundRenderer.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) render-background.ts
// </copyright>
// -----------------------------------------------------------------------

using Ink.Net.Dom;
using static Facebook.Yoga.YGNodeLayoutAPI;

namespace Ink.Net.Rendering;

/// <summary>
/// Renders the background color of a DOM node to the output buffer.
/// <para>1:1 port of Ink JS <c>render-background.ts</c>.</para>
/// </summary>
public static class BackgroundRenderer
{
    /// <summary>
    /// Render a background fill for a node.
    /// <para>Corresponds to JS <c>renderBackground(x, y, node, output)</c>.</para>
    /// </summary>
    public static void Render(int x, int y, InkNode node, Output output)
    {
        if (string.IsNullOrEmpty(node.Style.BackgroundColor))
            return;

        var yogaNode = node.YogaNode;
        if (yogaNode is null) return;

        float width = YGNodeLayoutGetWidth(yogaNode);
        float height = YGNodeLayoutGetHeight(yogaNode);

        // Calculate the actual content area considering borders
        int leftBorderWidth = (node.Style.BorderStyle is not null && node.Style.BorderLeft != false) ? 1 : 0;
        int rightBorderWidth = (node.Style.BorderStyle is not null && node.Style.BorderRight != false) ? 1 : 0;
        int topBorderHeight = (node.Style.BorderStyle is not null && node.Style.BorderTop != false) ? 1 : 0;
        int bottomBorderHeight = (node.Style.BorderStyle is not null && node.Style.BorderBottom != false) ? 1 : 0;

        int contentWidth = (int)width - leftBorderWidth - rightBorderWidth;
        int contentHeight = (int)height - topBorderHeight - bottomBorderHeight;

        if (contentWidth <= 0 || contentHeight <= 0)
            return;

        // Create background fill for each row
        string backgroundLine = Colorizer.Colorize(
            new string(' ', contentWidth),
            node.Style.BackgroundColor,
            ColorType.Background);

        for (int row = 0; row < contentHeight; row++)
        {
            output.Write(
                x + leftBorderWidth,
                y + topBorderHeight + row,
                backgroundLine);
        }
    }
}
