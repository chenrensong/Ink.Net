// -----------------------------------------------------------------------
// <copyright file="TextNode.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) dom.ts — TextNode type
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Dom;

/// <summary>
/// 文本字面量节点，对应 Ink JS <c>dom.ts</c> 中的 <c>TextNode</c> 类型。
/// <code>
/// export type TextNode = {
///     nodeName: TextName;
///     nodeValue: string;
/// } &amp; InkNode;
/// </code>
/// <para>文本字面量节点不创建 Yoga 布局节点，由父级 <c>ink-text</c> 节点统一测量。</para>
/// </summary>
public sealed class TextNode : InkNode
{
    /// <summary>
    /// 获取或设置文本内容。对应 JS <c>nodeValue</c>。
    /// </summary>
    public string NodeValue { get; internal set; }

    /// <summary>
    /// 创建一个新的文本字面量节点。
    /// </summary>
    /// <param name="text">初始文本内容。</param>
    internal TextNode(string text) : base(InkNodeType.TextLiteral)
    {
        NodeValue = text;
    }
}
