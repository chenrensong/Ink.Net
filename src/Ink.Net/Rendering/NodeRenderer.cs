// -----------------------------------------------------------------------
// <copyright file="NodeRenderer.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) render-node-to-output.ts
// </copyright>
// -----------------------------------------------------------------------

using System.Text;
using Facebook.Yoga;
using Ink.Net.Dom;
using Ink.Net.Styles;
using Ink.Net.Text;
using static Facebook.Yoga.YGNodeLayoutAPI;
using static Facebook.Yoga.YGNodeStyleAPI;
using YogaNode = Facebook.Yoga.Node;

namespace Ink.Net.Rendering;

/// <summary>
/// Renders the DOM tree into an <see cref="Output"/> buffer.
/// <para>1:1 port of Ink JS <c>render-node-to-output.ts</c>.</para>
/// </summary>
public static class NodeRenderer
{
    // ─── applyPaddingToText ──────────────────────────────────────────

    /// <summary>
    /// If parent container is <c>Box</c>, text nodes will be treated as separate nodes.
    /// To ensure text nodes are aligned correctly, take X and Y of the first text node
    /// and use it as offset for the rest.
    /// <para>Corresponds to JS <c>applyPaddingToText(node, text)</c>.</para>
    /// </summary>
    private static string ApplyPaddingToText(DomElement node, string text)
    {
        YogaNode? yogaNode = node.ChildNodes.Count > 0 ? node.ChildNodes[0].YogaNode : null;

        if (yogaNode is not null)
        {
            int offsetX = (int)YGNodeLayoutGetLeft(yogaNode);
            int offsetY = (int)YGNodeLayoutGetTop(yogaNode);
            text = new string('\n', offsetY) + IndentString(text, offsetX);
        }

        return text;
    }

    /// <summary>
    /// Indent each line by the given number of spaces.
    /// <para>Port of npm <c>indent-string</c>.</para>
    /// </summary>
    private static string IndentString(string text, int count)
    {
        if (count <= 0) return text;
        string indent = new(' ', count);
        var lines = text.Split('\n');
        var sb = new StringBuilder();
        for (int i = 0; i < lines.Length; i++)
        {
            if (i > 0) sb.Append('\n');
            if (lines[i].Length > 0)
                sb.Append(indent).Append(lines[i]);
        }
        return sb.ToString();
    }

    // ─── Screen reader output ────────────────────────────────────────

    /// <summary>
    /// Render node to a screen-reader-friendly text string.
    /// <para>Corresponds to JS <c>renderNodeToScreenReaderOutput(node, options)</c>.</para>
    /// </summary>
    public static string RenderToScreenReaderOutput(
        DomElement node,
        string? parentRole = null,
        bool skipStaticElements = false)
    {
        if (skipStaticElements && node.InternalStatic)
            return "";

        var yogaNode = node.YogaNode;
        if (yogaNode is not null && YGNodeStyleGetDisplay(yogaNode) == YGDisplay.None)
            return "";

        string output = "";

        if (node.NodeType == InkNodeType.Text)
        {
            output = TextSquasher.Squash(node);
        }
        else if (node.NodeType is InkNodeType.Box or InkNodeType.Root)
        {
            string separator = node.Style.FlexDirection is FlexDirectionMode.Row or FlexDirectionMode.RowReverse
                ? " "
                : "\n";

            var childNodes = node.Style.FlexDirection is FlexDirectionMode.RowReverse or FlexDirectionMode.ColumnReverse
                ? node.ChildNodes.AsEnumerable().Reverse()
                : node.ChildNodes;

            var parts = new List<string>();
            foreach (var childNode in childNodes)
            {
                if (childNode is DomElement childElem)
                {
                    string? role = node.InternalAccessibility?.Role.ToString().ToLowerInvariant();
                    if (role == "none") role = null;
                    string childOutput = RenderToScreenReaderOutput(childElem, role, skipStaticElements);
                    if (!string.IsNullOrEmpty(childOutput))
                        parts.Add(childOutput);
                }
            }
            output = string.Join(separator, parts);
        }

        // Apply accessibility decorations
        if (node.InternalAccessibility is { } access)
        {
            if (access.State is { } state)
            {
                var stateNames = string.Join(", ", state.GetActiveStateNames());
                if (!string.IsNullOrEmpty(stateNames))
                    output = $"({stateNames}) {output}";
            }

            string? role = access.Role != AccessibilityRole.None
                ? access.Role.ToString().ToLowerInvariant()
                : null;

            if (role is not null && role != parentRole)
                output = $"{role}: {output}";
        }

        return output;
    }

    // ─── Main render to output ───────────────────────────────────────

    /// <summary>
    /// After nodes are laid out, render each to the output object,
    /// which later gets rendered to terminal.
    /// <para>Corresponds to JS <c>renderNodeToOutput(node, output, options)</c>.</para>
    /// </summary>
    public static void Render(
        DomElement node,
        Output output,
        int offsetX = 0,
        int offsetY = 0,
        OutputTransformer[]? transformers = null,
        bool skipStaticElements = false)
    {
        transformers ??= [];

        if (skipStaticElements && node.InternalStatic)
            return;

        var yogaNode = node.YogaNode;

        if (yogaNode is null) return;

        if (YGNodeStyleGetDisplay(yogaNode) == YGDisplay.None)
            return;

        // Left and top positions in Yoga are relative to their parent node
        int x = offsetX + (int)YGNodeLayoutGetLeft(yogaNode);
        int y = offsetY + (int)YGNodeLayoutGetTop(yogaNode);

        // Transformers are functions that transform final text output of each component
        OutputTransformer[] newTransformers = transformers;

        if (node.InternalTransform is not null)
        {
            newTransformers = [node.InternalTransform, .. transformers];
        }

        // ── ink-text ─────────────────────────────────────────────────
        if (node.NodeType == InkNodeType.Text)
        {
            string text = TextSquasher.Squash(node);

            if (text.Length > 0)
            {
                int currentWidth = StringWidthHelper.GetWidestLine(text);
                float maxWidth = LayoutHelper.GetMaxWidth(yogaNode);

                if (currentWidth > maxWidth)
                {
                    var textWrap = node.Style.TextWrap ?? TextWrapMode.Wrap;
                    text = TextWrapper.Wrap(text, (int)maxWidth, textWrap);
                }

                text = ApplyPaddingToText(node, text);
                output.Write(x, y, text, newTransformers);
            }

            return;
        }

        // ── ink-box ──────────────────────────────────────────────────
        bool clipped = false;

        if (node.NodeType == InkNodeType.Box)
        {
            BackgroundRenderer.Render(x, y, node, output);
            BorderRenderer.Render(x, y, node, output);

            bool clipH = node.Style.OverflowX == OverflowMode.Hidden || node.Style.Overflow == OverflowMode.Hidden;
            bool clipV = node.Style.OverflowY == OverflowMode.Hidden || node.Style.Overflow == OverflowMode.Hidden;

            if (clipH || clipV)
            {
                int? x1 = clipH ? x + (int)YGNodeLayoutGetBorder(yogaNode, YGEdge.Left) : null;
                int? x2 = clipH ? x + (int)YGNodeLayoutGetWidth(yogaNode) - (int)YGNodeLayoutGetBorder(yogaNode, YGEdge.Right) : null;
                int? y1 = clipV ? y + (int)YGNodeLayoutGetBorder(yogaNode, YGEdge.Top) : null;
                int? y2 = clipV ? y + (int)YGNodeLayoutGetHeight(yogaNode) - (int)YGNodeLayoutGetBorder(yogaNode, YGEdge.Bottom) : null;

                output.Clip(new OutputClip { X1 = x1, X2 = x2, Y1 = y1, Y2 = y2 });
                clipped = true;
            }
        }

        // ── Recurse children ─────────────────────────────────────────
        if (node.NodeType is InkNodeType.Root or InkNodeType.Box)
        {
            foreach (var childNode in node.ChildNodes)
            {
                if (childNode is DomElement childElem)
                {
                    Render(childElem, output, x, y, newTransformers, skipStaticElements);
                }
            }

            if (clipped)
            {
                output.Unclip();
            }
        }
    }
}
