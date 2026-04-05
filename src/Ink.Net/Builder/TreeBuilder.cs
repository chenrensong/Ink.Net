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
///     tree.Text("World!", new InkStyle { Color = "cyan" }),
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
    /// <param name="style">Style properties for the box.</param>
    /// <param name="children">Child nodes.</param>
    /// <param name="transform">Optional output transformer.</param>
    /// <param name="ariaLabel">Optional ARIA label for screen readers.</param>
    /// <param name="ariaRole">Optional ARIA role.</param>
    /// <param name="ariaHidden">If true, hidden from screen readers.</param>
    /// <param name="ariaState">Optional ARIA state.</param>
    public TreeNode Box(
        InkStyle? style = null,
        TreeNode[]? children = null,
        OutputTransformer? transform = null,
        string? ariaLabel = null,
        AccessibilityRole? ariaRole = null,
        bool ariaHidden = false,
        AccessibilityState? ariaState = null)
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

        // Apply accessibility attributes
        ApplyAccessibility(node, ariaLabel, ariaRole, ariaHidden, ariaState);

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
    /// <param name="content">Text content.</param>
    /// <param name="style">Style properties.</param>
    /// <param name="transform">Optional output transformer.</param>
    /// <param name="ariaLabel">Optional ARIA label for screen readers.</param>
    /// <param name="ariaRole">Optional ARIA role.</param>
    /// <param name="ariaHidden">If true, hidden from screen readers.</param>
    /// <param name="ariaState">Optional ARIA state.</param>
    public TreeNode Text(
        string content,
        InkStyle? style = null,
        OutputTransformer? transform = null,
        string? ariaLabel = null,
        AccessibilityRole? ariaRole = null,
        bool ariaHidden = false,
        AccessibilityState? ariaState = null)
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

        // Apply accessibility attributes
        ApplyAccessibility(textElem, ariaLabel, ariaRole, ariaHidden, ariaState);

        // Create a text literal node inside the Text element
        var literal = DomTree.CreateTextNode(content);
        DomTree.AppendChildNode(textElem, literal);

        return new TreeNode(textElem);
    }

    // ─── Transform ──────────────────────────────────────────────────

    /// <summary>
    /// Create a Transform node that applies a string transformation to its children's output.
    /// <para>Corresponds to React <c>&lt;Transform transform={fn}&gt;children&lt;/Transform&gt;</c> component.</para>
    /// <para>
    /// The Transform component is an <c>ink-text</c> node with <c>flexGrow=0, flexShrink=1, flexDirection=row</c>
    /// and the <c>internal_transform</c> function set.
    /// </para>
    /// </summary>
    /// <param name="transform">
    /// Transformation function applied to each line of the children's rendered output.
    /// Receives <c>(text, index)</c> where <c>index</c> is the line index.
    /// </param>
    /// <param name="children">Child nodes whose output will be transformed.</param>
    public TreeNode Transform(OutputTransformer transform, TreeNode[]? children = null)
    {
        // Use Box type (not Text) because Text nodes have a Yoga measure function
        // and cannot have children. The InternalTransform on a Box is applied during
        // rendering to all text output of its children, same as JS <Transform>.
        var node = DomTree.CreateNode(InkNodeType.Box);

        // Apply Transform defaults matching JS: flexGrow=0, flexShrink=1, flexDirection=row
        if (node.YogaNode is not null)
        {
            YGNodeStyleSetFlexGrow(node.YogaNode, 0);
            YGNodeStyleSetFlexShrink(node.YogaNode, 1);
            YGNodeStyleSetFlexDirection(node.YogaNode, YGFlexDirection.Row);
        }

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

    // ─── Spacer ─────────────────────────────────────────────────────

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

    // ─── Static ─────────────────────────────────────────────────────

    /// <summary>
    /// Create a Static node — content rendered permanently above dynamic content.
    /// <para>
    /// Corresponds to React <c>&lt;Static items={items}&gt;{render}&lt;/Static&gt;</c> component.
    /// In JS Ink, Static renders an <c>ink-box</c> with <c>internal_static=true</c>,
    /// <c>position=absolute</c>, <c>flexDirection=column</c>.
    /// Items are rendered only once and not cleared on re-render.
    /// </para>
    /// </summary>
    /// <param name="children">Child nodes to render as static content.</param>
    /// <param name="style">Optional additional styles (merged with static defaults).</param>
    public TreeNode Static(TreeNode[]? children = null, InkStyle? style = null)
    {
        var effectiveStyle = new InkStyle
        {
            Position = PositionMode.Absolute,
            FlexDirection = FlexDirectionMode.Column,
        };

        // Merge user style if provided
        if (style is not null)
        {
            if (style.FlexDirection.HasValue)
                effectiveStyle.FlexDirection = style.FlexDirection;
            if (style.Padding.HasValue) effectiveStyle.Padding = style.Padding;
            if (style.PaddingTop.HasValue) effectiveStyle.PaddingTop = style.PaddingTop;
            if (style.PaddingBottom.HasValue) effectiveStyle.PaddingBottom = style.PaddingBottom;
            if (style.PaddingLeft.HasValue) effectiveStyle.PaddingLeft = style.PaddingLeft;
            if (style.PaddingRight.HasValue) effectiveStyle.PaddingRight = style.PaddingRight;
            if (style.Margin.HasValue) effectiveStyle.Margin = style.Margin;
            if (style.Gap.HasValue) effectiveStyle.Gap = style.Gap;
        }

        var node = DomTree.CreateNode(InkNodeType.Box);
        node.InternalStatic = true;

        if (node.YogaNode is not null)
        {
            YGNodeStyleSetFlexDirection(node.YogaNode, YGFlexDirection.Column);
            YGNodeStyleSetPositionType(node.YogaNode, YGPositionType.Absolute);
        }

        DomTree.SetStyle(node, effectiveStyle);
        if (node.YogaNode is not null)
            StyleApplier.Apply(node.YogaNode, effectiveStyle);

        if (children is not null)
        {
            foreach (var child in children)
            {
                DomTree.AppendChildNode(node, child.Inner);
            }
        }

        return new TreeNode(node);
    }

    // ─── Link ─────────────────────────────────────────────────────────

    /// <summary>
    /// Create a Link node for hyperlinks (OSC 8).
    /// </summary>
    public TreeNode Link(string url, TreeNode[]? children = null)
    {
        var node = DomTree.CreateNode(InkNodeType.Link);
        DomTree.SetAttribute(node, "href", url);
        if (children is not null)
        {
            foreach (var child in children)
                DomTree.AppendChildNode(node, child.Inner);
        }
        return new TreeNode(node);
    }

    // ─── RawAnsi ────────────────────────────────────────────────────

    /// <summary>
    /// Create a RawAnsi node for pre-rendered ANSI content.
    /// </summary>
    public TreeNode RawAnsi(string content, int width, int height)
    {
        var node = DomTree.CreateNode(InkNodeType.RawAnsi);
        DomTree.SetAttribute(node, "rawWidth", width);
        DomTree.SetAttribute(node, "rawHeight", height);
        DomTree.SetAttribute(node, "rawContent", content);
        return new TreeNode(node);
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

            // Auto-detect Static node and set StaticNode on root (same as JS reconciler)
            if (child.Inner is DomElement elem && elem.InternalStatic)
            {
                root.StaticNode = elem;
            }
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

    // ─── Private helpers ─────────────────────────────────────────────

    /// <summary>
    /// Apply accessibility info to a node.
    /// <para>Corresponds to JS aria-label, aria-role, aria-hidden, aria-state props.</para>
    /// </summary>
    private static void ApplyAccessibility(
        DomElement node,
        string? ariaLabel,
        AccessibilityRole? ariaRole,
        bool ariaHidden,
        AccessibilityState? ariaState)
    {
        if (ariaLabel is null && ariaRole is null && !ariaHidden && ariaState is null)
            return;

        node.InternalAccessibility ??= new AccessibilityInfo();

        if (ariaRole.HasValue)
            node.InternalAccessibility.Role = ariaRole.Value;

        if (ariaState is not null)
            node.InternalAccessibility.State = ariaState;

        if (ariaHidden)
        {
            node.InternalAccessibility.Hidden = true;
        }

        // Store aria-label for screen reader output
        if (ariaLabel is not null)
        {
            node.InternalAccessibility.Label = ariaLabel;
        }
    }
}
