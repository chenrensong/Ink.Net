// -----------------------------------------------------------------------
// <copyright file="DomElement.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) dom.ts — DOMElement type
// </copyright>
// -----------------------------------------------------------------------

using Ink.Net.Styles;
using YogaNode = Facebook.Yoga.Node;

namespace Ink.Net.Dom;

/// <summary>
/// DOM 元素节点，对应 Ink JS <c>dom.ts</c> 中的 <c>DOMElement</c> 类型。
/// <para>
/// 涵盖以下元素类型：
/// <list type="bullet">
///   <item><see cref="InkNodeType.Root"/>       — <c>ink-root</c></item>
///   <item><see cref="InkNodeType.Box"/>        — <c>ink-box</c></item>
///   <item><see cref="InkNodeType.Text"/>       — <c>ink-text</c></item>
///   <item><see cref="InkNodeType.VirtualText"/> — <c>ink-virtual-text</c></item>
/// </list>
/// </para>
/// </summary>
public sealed class DomElement : InkNode
{
    /// <summary>
    /// 创建一个新的 DOM 元素节点。
    /// <para>
    /// 除 <see cref="InkNodeType.VirtualText"/> 外，所有节点类型都会创建关联的 Yoga 布局节点。
    /// </para>
    /// </summary>
    /// <param name="nodeType">元素类型（不能为 <see cref="InkNodeType.TextLiteral"/>）。</param>
    internal DomElement(InkNodeType nodeType) : base(nodeType)
    {
        // ink-virtual-text 不创建 Yoga 节点
        // 对应 JS: yogaNode: nodeName === 'ink-virtual-text' ? undefined : Yoga.Node.create()
        if (nodeType != InkNodeType.VirtualText)
        {
            YogaNode = Facebook.Yoga.YGNodeAPI.YGNodeNew();
        }
    }

    // ─── Attributes ──────────────────────────────────────────────────────

    /// <summary>
    /// 节点属性字典。对应 JS <c>attributes: Record&lt;string, DOMNodeAttribute&gt;</c>。
    /// </summary>
    public Dictionary<string, DomNodeAttribute> Attributes { get; } = new();

    // ─── Children ────────────────────────────────────────────────────────

    /// <summary>
    /// 子节点列表。对应 JS <c>childNodes: DOMNode[]</c>。
    /// </summary>
    public List<InkNode> ChildNodes { get; } = new();

    // ─── Transform ───────────────────────────────────────────────────────

    /// <summary>
    /// 输出变换函数。对应 JS <c>internal_transform?: OutputTransformer</c>。
    /// <para>用于在渲染时对文本内容进行颜色等转换。</para>
    /// </summary>
    public OutputTransformer? InternalTransform { get; set; }

    // ─── Accessibility ───────────────────────────────────────────────────

    /// <summary>
    /// 可访问性信息。对应 JS <c>internal_accessibility</c>。
    /// </summary>
    public AccessibilityInfo? InternalAccessibility { get; set; }

    // ─── Static / Root 特有属性 ──────────────────────────────────────────

    /// <summary>
    /// 标记 <c>&lt;Static&gt;</c> 组件内容是否有变更。
    /// <para>仅在 Root 节点上有意义。对应 JS <c>isStaticDirty</c>。</para>
    /// </summary>
    public bool IsStaticDirty { get; set; }

    /// <summary>
    /// 对 <c>&lt;Static&gt;</c> 节点的引用，避免全树遍历。
    /// <para>仅在 Root 节点上有意义。对应 JS <c>staticNode</c>。</para>
    /// </summary>
    public DomElement? StaticNode { get; set; }

    // ─── 回调事件 ────────────────────────────────────────────────────────

    /// <summary>
    /// 布局计算完成后的回调。对应 JS <c>onComputeLayout</c>。
    /// </summary>
    public event Action? ComputeLayout;

    /// <summary>
    /// 渲染请求回调（节流后触发）。对应 JS <c>onRender</c>。
    /// </summary>
    public event Action? RenderRequested;

    /// <summary>
    /// 立即渲染请求回调（用于 Static 子节点）。对应 JS <c>onImmediateRender</c>。
    /// </summary>
    public event Action? ImmediateRenderRequested;

    /// <summary>触发 <see cref="ComputeLayout"/> 事件。</summary>
    internal void OnComputeLayout() => ComputeLayout?.Invoke();

    /// <summary>触发 <see cref="RenderRequested"/> 事件。</summary>
    internal void OnRenderRequested() => RenderRequested?.Invoke();

    /// <summary>触发 <see cref="ImmediateRenderRequested"/> 事件。</summary>
    internal void OnImmediateRenderRequested() => ImmediateRenderRequested?.Invoke();

    // ─── Layout Listeners (Root 节点) ────────────────────────────────────

    private HashSet<Action>? _layoutListeners;

    /// <summary>
    /// 添加布局监听器。仅 Root 节点有效。
    /// <para>对应 JS <c>addLayoutListener</c>。</para>
    /// </summary>
    /// <returns>取消订阅的 <see cref="IDisposable"/>。</returns>
    internal IDisposable AddLayoutListener(Action listener)
    {
        if (NodeType != InkNodeType.Root)
            return EmptyDisposable.Instance;

        _layoutListeners ??= new HashSet<Action>();
        _layoutListeners.Add(listener);

        return new LayoutListenerDisposable(this, listener);
    }

    /// <summary>
    /// 触发所有布局监听器。仅 Root 节点有效。
    /// <para>对应 JS <c>emitLayoutListeners</c>。</para>
    /// </summary>
    internal void EmitLayoutListeners()
    {
        if (NodeType != InkNodeType.Root || _layoutListeners is null)
            return;

        foreach (var listener in _layoutListeners)
        {
            listener();
        }
    }

    private void RemoveLayoutListener(Action listener)
    {
        _layoutListeners?.Remove(listener);
    }

    // ─── 内部辅助 ────────────────────────────────────────────────────────

    private sealed class LayoutListenerDisposable : IDisposable
    {
        private DomElement? _element;
        private Action? _listener;

        public LayoutListenerDisposable(DomElement element, Action listener)
        {
            _element = element;
            _listener = listener;
        }

        public void Dispose()
        {
            _element?.RemoveLayoutListener(_listener!);
            _element = null;
            _listener = null;
        }
    }

    private sealed class EmptyDisposable : IDisposable
    {
        public static readonly EmptyDisposable Instance = new();
        public void Dispose() { }
    }
}
