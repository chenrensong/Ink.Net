// -----------------------------------------------------------------------
// <copyright file="TreeBuilder.cs" company="Ink.Net">
//   Imperative DOM tree builder API — replaces React reconciler.
//   Provides Box() / Text() / Spacer() / Build() methods.
// </copyright>
// -----------------------------------------------------------------------

using Facebook.Yoga;
using Ink.Net.Dom;
using Ink.Net.Styles;
using static Facebook.Yoga.YGNodeAPI;
using static Facebook.Yoga.YGNodeStyleAPI;
using static Facebook.Yoga.YGNodeLayoutAPI;

namespace Ink.Net.Builder;

/// <summary>
/// A child element that can be added to a Box or Root.
/// Produced by <see cref="TreeBuilder.Box"/>, <see cref="TreeBuilder.Text"/>, etc.
/// </summary>
public sealed class TreeNode
{
    internal InkNode Inner { get; }

    internal TreeNode(InkNode inner)
    {
        Inner = inner;
    }
}

/// <summary>
/// Imperative DOM tree builder — the "builder-api" replacement for React reconciler.
/// <para>
/// Usage:
/// <code>
/// var tree = new TreeBuilder();
/// var root = tree.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, children: new[]
/// {
///     tree.Text("Hello, "),
///     tree.Text("World!", new InkStyle { /* color */ }),
///     tree.Box(new InkStyle { FlexGrow = 1 }, children: new[]
///     {
///         tree.Text("Nested content"),
///     }),
/// });
///
/// string output = tree.RenderToString(root, columns: 80);
/// </code>
/// </para>
/// </summary>
public sealed class TreeBuilder
{
    // ─── Box ─────────────────────────────────────────────────────────

    /// <summary>
    /// Create a Box node (ink-box) with optional style and children.
    /// <para>Corresponds to React <c>&lt;Box&gt;</c> component.</para>
    /// <para>
    /// Default styles (matching JS Box component):
    /// <c>flexDirection: row, flexShrink: 1, flexGrow: 0, flexWrap: nowrap</c>.
    /// User-provided style overrides these defaults.
    /// </para>
    /// </summary>
    public TreeNode Box(InkStyle? style = null, TreeNode[]? children = null, OutputTransformer? transform = null)
    {
        var node = DomTree.CreateNode(InkNodeType.Box);

        // Apply Box defaults matching JS: flexDirection='row', flexShrink=1, flexGrow=0, flexWrap='nowrap'
        if (node.YogaNode is not null)
        {
            YGNodeStyleSetFlexDirection(node.YogaNode, YGFlexDirection.Row);
            YGNodeStyleSetFlexShrink(node.YogaNode, 1);
            YGNodeStyleSetFlexGrow(node.YogaNode, 0);
            YGNodeStyleSetFlexWrap(node.YogaNode, YGWrap.NoWrap);
        }

        if (style is not null)
        {
            DomTree.SetStyle(node, style);
            if (node.YogaNode is not null)
                StyleApplier.Apply(node.YogaNode, style);
        }

        if (transform is not null)
            node.InternalTransform = transform;

        if (children is not null)
        {
            foreach (var child in children)
            {
                DomTree.AppendChildNode(node, child.Inner);
            }
        }

        return new TreeNode(node);
    }

    /// <summary>
    /// Create a Text node (ink-text) with text content and optional style.
    /// <para>Corresponds to React <c>&lt;Text&gt;</c> component.</para>
    /// </summary>
    public TreeNode Text(string content, InkStyle? style = null, OutputTransformer? transform = null)
    {
        var textElem = DomTree.CreateNode(InkNodeType.Text);

        if (style is not null)
        {
            DomTree.SetStyle(textElem, style);
            if (textElem.YogaNode is not null)
                StyleApplier.Apply(textElem.YogaNode, style);
        }

        if (transform is not null)
            textElem.InternalTransform = transform;

        // Create a text literal node inside the Text element
        var literal = DomTree.CreateTextNode(content);
        DomTree.AppendChildNode(textElem, literal);

        return new TreeNode(textElem);
    }

    /// <summary>
    /// Create a Spacer node (ink-box with flexGrow=1).
    /// <para>Corresponds to React <c>&lt;Spacer /&gt;</c> component.</para>
    /// </summary>
    public TreeNode Spacer()
    {
        return Box(new InkStyle { FlexGrow = 1 });
    }

    /// <summary>
    /// Create a Newline node — an empty box that takes up vertical space.
    /// <para>
    /// Corresponds to React <c>&lt;Newline /&gt;</c> component.
    /// In JS Ink, <c>&lt;Newline /&gt;</c> renders <c>&lt;ink-text&gt;\n&lt;/ink-text&gt;</c> inside a column-direction parent.
    /// We model this as an empty box with explicit height, which produces the correct blank-line effect
    /// without the text-measurement issues of a literal "\n" node.
    /// </para>
    /// </summary>
    public TreeNode Newline(int count = 1)
    {
        return Box(new InkStyle { Height = count });
    }

    // ─── Build root ──────────────────────────────────────────────────

    /// <summary>
    /// Build a complete DOM tree with an ink-root wrapper.
    /// Calculates Yoga layout with the specified dimensions.
    /// </summary>
    /// <param name="children">Root-level children.</param>
    /// <param name="columns">Terminal width in columns. Default 80.</param>
    /// <param name="rows">Terminal height in rows. Default 24. Pass <c>null</c> for auto-height (content-determined).</param>
    /// <returns>The root <see cref="DomElement"/> ready for rendering.</returns>
    public DomElement Build(TreeNode[] children, int columns = 80, int? rows = 24)
    {
        var root = DomTree.CreateNode(InkNodeType.Root);

        foreach (var child in children)
        {
            DomTree.AppendChildNode(root, child.Inner);
        }

        // Calculate layout (pass NaN for height when rows is null → auto-size)
        float layoutHeight = rows.HasValue ? rows.Value : float.NaN;
        YGNodeCalculateLayout(root.YogaNode!, columns, layoutHeight, YGDirection.LTR);

        return root;
    }

    /// <summary>
    /// Build a root from a single child.
    /// </summary>
    public DomElement Build(TreeNode child, int columns = 80, int? rows = 24)
    {
        return Build([child], columns, rows);
    }
}
