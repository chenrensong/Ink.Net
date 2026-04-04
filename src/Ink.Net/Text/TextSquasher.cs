// -----------------------------------------------------------------------
// <copyright file="TextSquasher.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) squash-text-nodes.ts
// </copyright>
// -----------------------------------------------------------------------

using Ink.Net.Ansi;
using Ink.Net.Dom;
using System.Text;

namespace Ink.Net.Text;

/// <summary>
/// 文本节点合并器。将多个子文本节点合并为单个字符串，便于统一测量和输出。
/// <para>
/// 对应 Ink JS <c>squash-text-nodes.ts</c>：
/// <code>
/// // &lt;Text&gt;hello{' '}world&lt;/Text&gt;
/// // 实际是 3 个文本节点，squash 后合并为一个字符串 "hello world"
/// </code>
/// </para>
/// </summary>
public static class TextSquasher
{
    /// <summary>
    /// 将元素节点下的所有文本子节点合并为单个字符串。
    /// <para>对应 JS <c>squashTextNodes(node)</c>。</para>
    /// </summary>
    /// <param name="node">目标元素节点（通常是 <c>ink-text</c>）。</param>
    /// <returns>合并后的文本。</returns>
    public static string Squash(DomElement node)
    {
        var sb = new StringBuilder();
        SquashCore(node, sb);
        var result = sb.ToString();

        // 对应 JS: return sanitizeAnsi(text);
        // 使用完整的 ANSI 清理器替代简化版本
        return AnsiSanitizer.Sanitize(result);
    }

    private static void SquashCore(DomElement node, StringBuilder sb)
    {
        for (int index = 0; index < node.ChildNodes.Count; index++)
        {
            var childNode = node.ChildNodes[index];
            if (childNode is null)
                continue;

            string nodeText;

            if (childNode is TextNode textNode)
            {
                // 对应 JS: if (childNode.nodeName === '#text') { nodeText = childNode.nodeValue; }
                nodeText = textNode.NodeValue;
            }
            else if (childNode is DomElement element &&
                     (element.NodeType == InkNodeType.Text || element.NodeType == InkNodeType.VirtualText))
            {
                // 对应 JS: nodeText = squashTextNodes(childNode);
                var childSb = new StringBuilder();
                SquashCore(element, childSb);
                nodeText = childSb.ToString();

                // 对应 JS: if (nodeText.length > 0 && typeof childNode.internal_transform === 'function')
                if (nodeText.Length > 0 && element.InternalTransform is not null)
                {
                    nodeText = element.InternalTransform(nodeText, index);
                }
            }
            else
            {
                continue;
            }

            sb.Append(nodeText);
        }
    }

    // SanitizeAnsi 已迁移到 Ink.Net.Ansi.AnsiSanitizer.Sanitize()
}
