// -----------------------------------------------------------------------
// <copyright file="MeasureElement.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) measure-element.ts
// </copyright>
// -----------------------------------------------------------------------

using Ink.Net.Dom;
using static Facebook.Yoga.YGNodeLayoutAPI;

namespace Ink.Net.Rendering;

/// <summary>
/// Measure a DOM element's computed dimensions from its Yoga layout.
/// <para>1:1 port of Ink JS <c>measure-element.ts</c>.</para>
/// </summary>
public static class MeasureElement
{
    /// <summary>
    /// Measured dimensions output.
    /// </summary>
    public readonly struct ElementDimensions
    {
        public float Width { get; }
        public float Height { get; }

        public ElementDimensions(float width, float height)
        {
            Width = width;
            Height = height;
        }
    }

    /// <summary>
    /// Measure the width and height of a DOM element.
    /// <para>Corresponds to JS <c>measureElement(node)</c>.</para>
    /// </summary>
    public static ElementDimensions Measure(DomElement node)
    {
        var yogaNode = node.YogaNode;
        if (yogaNode is null)
            return new ElementDimensions(0, 0);
        return new ElementDimensions(
            YGNodeLayoutGetWidth(yogaNode),
            YGNodeLayoutGetHeight(yogaNode));
    }
}
