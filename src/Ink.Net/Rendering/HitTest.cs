// -----------------------------------------------------------------------
// <copyright file="HitTest.cs" company="Ink.Net">
//   Hit testing: given screen coordinates, find the deepest DOM element.
//   Corresponds to JS hit-test.ts.
// </copyright>
// -----------------------------------------------------------------------

using Facebook.Yoga;
using Ink.Net.Dom;
using static Facebook.Yoga.YGNodeLayoutAPI;
using static Facebook.Yoga.YGNodeStyleAPI;

namespace Ink.Net.Rendering;

/// <summary>
/// Hit testing: given screen coordinates (x, y), find the deepest DOM element at that position.
/// Corresponds to JS hit-test.ts.
/// </summary>
public static class HitTest
{
    /// <summary>
    /// Find the deepest element at the given screen coordinates.
    /// </summary>
    /// <param name="root">The root DOM element.</param>
    /// <param name="x">Screen column (0-indexed).</param>
    /// <param name="y">Screen row (0-indexed).</param>
    /// <returns>The element at (x, y), or null if none found.</returns>
    public static DomElement? ElementAt(DomElement root, int x, int y)
    {
        return Walk(root, x, y, 0, 0);
    }

    private static DomElement? Walk(DomElement node, int x, int y, float offsetX, float offsetY)
    {
        var yoga = node.YogaNode;
        if (yoga is null) return null;

        var display = YGNodeStyleGetDisplay(yoga);
        if (display == YGDisplay.None) return null;

        float left = offsetX + YGNodeLayoutGetLeft(yoga);
        float top = offsetY + YGNodeLayoutGetTop(yoga);
        float width = YGNodeLayoutGetWidth(yoga);
        float height = YGNodeLayoutGetHeight(yoga);

        // Check if point is within this node's bounds
        if (x < left || x >= left + width || y < top || y >= top + height)
            return null;

        // Check children in reverse order (last child = highest z-order)
        for (int i = node.ChildNodes.Count - 1; i >= 0; i--)
        {
            if (node.ChildNodes[i] is DomElement child)
            {
                var result = Walk(child, x, y, left, top);
                if (result is not null)
                    return result;
            }
        }

        // No child hit, this node is the target
        return node;
    }
}
