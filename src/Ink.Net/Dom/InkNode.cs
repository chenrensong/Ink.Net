// -----------------------------------------------------------------------
// <copyright file="InkNode.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) dom.ts — InkNode base type
// </copyright>
// -----------------------------------------------------------------------

using Ink.Net.Styles;
using YogaNode = Facebook.Yoga.Node;

namespace Ink.Net.Dom;

/// <summary>
/// Ink DOM 节点基类。
/// <para>对应 Ink JS <c>dom.ts</c> 中的 <c>InkNode</c> 类型：</para>
/// <code>
/// type InkNode = {
///     parentNode: DOMElement | undefined;
///     yogaNode?: YogaNode;
///     internal_static?: boolean;
///     style: Styles;
/// };
/// </code>
/// <para>所有 DOM 节点（元素节点和文本字面量节点）均继承此类。</para>
/// </summary>
public abstract class InkNode
{
    /// <summary>获取节点类型。</summary>
    public InkNodeType NodeType { get; }

    /// <summary>
    /// 获取或设置父元素节点。
    /// <para>对应 JS <c>parentNode: DOMElement | undefined</c>。</para>
    /// </summary>
    public DomElement? ParentNode { get; internal set; }

    /// <summary>
    /// 获取关联的 Yoga 布局节点。
    /// <para>
    /// <see cref="InkNodeType.VirtualText"/> 和 <see cref="InkNodeType.TextLiteral"/>
    /// 节点不创建 Yoga 节点（返回 <c>null</c>）。
    /// </para>
    /// </summary>
    public YogaNode? YogaNode { get; protected set; }

    /// <summary>
    /// 标记节点是否为 <c>&lt;Static&gt;</c> 组件的子节点。
    /// <para>对应 JS <c>internal_static</c>。</para>
    /// </summary>
    public bool InternalStatic { get; set; }

    /// <summary>
    /// 获取或设置节点的样式属性。
    /// <para>对应 JS <c>style: Styles</c>。渲染代码假设 style 始终为非 null 对象。</para>
    /// </summary>
    public InkStyle Style { get; set; } = new();

    /// <summary>初始化节点基类。</summary>
    /// <param name="nodeType">节点类型。</param>
    protected InkNode(InkNodeType nodeType)
    {
        NodeType = nodeType;
    }

    /// <summary>
    /// 检查节点是否为文本类型节点（ink-text 或 ink-virtual-text）。
    /// </summary>
    public bool IsTextNode =>
        NodeType is InkNodeType.Text or InkNodeType.VirtualText;

    /// <summary>
    /// 检查节点是否为容器类型节点（ink-root 或 ink-box）。
    /// </summary>
    public bool IsContainerNode =>
        NodeType is InkNodeType.Root or InkNodeType.Box;
}
