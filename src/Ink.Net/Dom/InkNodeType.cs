// -----------------------------------------------------------------------
// <copyright file="InkNodeType.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) dom.ts — ElementNames | TextName
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Dom;

/// <summary>
/// 节点类型枚举，对应 Ink JS 中的 <c>ElementNames | TextName</c>。
/// <para>
/// 映射关系：
/// <list type="bullet">
///   <item><c>Root</c>        → <c>'ink-root'</c></item>
///   <item><c>Box</c>         → <c>'ink-box'</c></item>
///   <item><c>Text</c>        → <c>'ink-text'</c></item>
///   <item><c>VirtualText</c> → <c>'ink-virtual-text'</c></item>
///   <item><c>TextLiteral</c> → <c>'#text'</c></item>
/// </list>
/// </para>
/// </summary>
public enum InkNodeType : byte
{
    /// <summary>根容器节点 (ink-root)。管理全屏/非全屏模式。</summary>
    Root = 0,

    /// <summary>Flex 布局容器节点 (ink-box)。</summary>
    Box = 1,

    /// <summary>文本块节点 (ink-text)。拥有独立的 Yoga 布局节点和 MeasureFunc。</summary>
    Text = 2,

    /// <summary>
    /// 虚拟文本节点 (ink-virtual-text)。
    /// 不创建独立 Yoga 节点，防止嵌套文本被 Yoga 误解为独立 flex children。
    /// </summary>
    VirtualText = 3,

    /// <summary>文本字面量节点 (#text)。不参与 Yoga 布局。</summary>
    TextLiteral = 4,

    /// <summary>超链接节点 (ink-link)。不创建 Yoga 节点，类似 VirtualText。</summary>
    Link = 5,

    /// <summary>预渲染 ANSI 内容节点 (ink-raw-ansi)。拥有 Yoga 节点和自定义 MeasureFunc。</summary>
    RawAnsi = 6,

    /// <summary>进度条节点 (ink-progress)。不创建 Yoga 节点。</summary>
    Progress = 7,
}
