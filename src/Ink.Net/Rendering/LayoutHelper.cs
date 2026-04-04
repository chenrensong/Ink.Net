// -----------------------------------------------------------------------
// <copyright file="LayoutHelper.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) get-max-width.ts
// </copyright>
// -----------------------------------------------------------------------

using static Facebook.Yoga.YGNodeLayoutAPI;
using YogaNode = Facebook.Yoga.Node;

namespace Ink.Net.Rendering;

/// <summary>
/// Yoga layout helper utilities.
/// <para>1:1 port of Ink JS <c>get-max-width.ts</c>.</para>
/// </summary>
public static class LayoutHelper
{
    /// <summary>
    /// Get the maximum available width for content inside a Yoga node,
    /// subtracting left/right padding and border.
    /// <para>Corresponds to JS <c>getMaxWidth(yogaNode)</c>.</para>
    /// </summary>
    public static float GetMaxWidth(YogaNode yogaNode)
    {
        return YGNodeLayoutGetWidth(yogaNode)
            - YGNodeLayoutGetPadding(yogaNode, Facebook.Yoga.YGEdge.Left)
            - YGNodeLayoutGetPadding(yogaNode, Facebook.Yoga.YGEdge.Right)
            - YGNodeLayoutGetBorder(yogaNode, Facebook.Yoga.YGEdge.Left)
            - YGNodeLayoutGetBorder(yogaNode, Facebook.Yoga.YGEdge.Right);
    }
}
