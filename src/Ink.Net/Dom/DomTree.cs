// -----------------------------------------------------------------------
// <copyright file="DomTree.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) dom.ts — all exported functions
// </copyright>
// -----------------------------------------------------------------------

using Ink.Net.Styles;
using Ink.Net.Text;
using Facebook.Yoga;
using static Facebook.Yoga.YGNodeAPI;
using static Facebook.Yoga.YGNodeStyleAPI;

namespace Ink.Net.Dom;

/// <summary>
/// DOM 树操作的静态方法集合。
/// <para>1:1 对应 Ink JS <c>dom.ts</c> 中所有导出的函数。</para>
/// </summary>
public static class DomTree
{
    // ═════════════════════════════════════════════════════════════════════
    //  节点创建
    // ═════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 创建一个新的元素节点。
    /// <para>对应 JS <c>createNode(nodeName)</c>。</para>
    /// </summary>
    /// <param name="nodeType">节点类型 (不能为 <see cref="InkNodeType.TextLiteral"/>)。</param>
    /// <returns>创建的元素节点。</returns>
    public static DomElement CreateNode(InkNodeType nodeType)
    {
        var node = new DomElement(nodeType)
        {
            InternalAccessibility = new AccessibilityInfo(),
        };

        // 对应 JS: if (nodeName === 'ink-text') { node.yogaNode?.setMeasureFunc(...) }
        if (nodeType == InkNodeType.Text && node.YogaNode is not null)
        {
            AttachMeasureFunc(node);
        }

        // ink-raw-ansi: 使用 rawWidth/rawHeight 属性作为固定尺寸的 MeasureFunc
        if (nodeType == InkNodeType.RawAnsi && node.YogaNode is not null)
        {
            AttachRawAnsiMeasureFunc(node);
        }

        return node;
    }

    /// <summary>
    /// 创建一个新的文本字面量节点。
    /// <para>对应 JS <c>createTextNode(text)</c>。</para>
    /// </summary>
    /// <param name="text">初始文本内容。</param>
    /// <returns>创建的文本节点。</returns>
    public static TextNode CreateTextNode(string text)
    {
        // 对应 JS:
        // const node: TextNode = {
        //     nodeName: '#text', nodeValue: text,
        //     yogaNode: undefined, parentNode: undefined, style: {},
        // };
        // setTextNodeValue(node, text);
        var node = new TextNode(text);
        // 注意：在 JS 中 setTextNodeValue 会调用 markNodeAsDirty，
        // 但新创建的节点尚无父节点，所以 markDirty 实际不生效
        return node;
    }

    // ═════════════════════════════════════════════════════════════════════
    //  子节点操作
    // ═════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 将子节点追加到父元素末尾。
    /// <para>对应 JS <c>appendChildNode(node, childNode)</c>。</para>
    /// <para>
    /// 注意：JS 版本的签名标注为 <c>(DOMElement, DOMElement)</c>，但 React reconciler
    /// 实际上也会传入 <c>TextNode</c>（JS 无运行时类型检查）。
    /// 此重载同时处理 <see cref="DomElement"/> 和 <see cref="TextNode"/>。
    /// </para>
    /// </summary>
    public static void AppendChildNode(DomElement node, InkNode childNode)
    {
        // 对应 JS: if (childNode.parentNode) { removeChildNode(childNode.parentNode, childNode); }
        if (childNode.ParentNode is not null)
        {
            RemoveChildNode(childNode.ParentNode, childNode);
        }

        childNode.ParentNode = node;
        node.ChildNodes.Add(childNode);

        // 对应 JS: node.yogaNode?.insertChild(childNode.yogaNode, node.yogaNode.getChildCount())
        if (childNode.YogaNode is not null && node.YogaNode is not null)
        {
            YGNodeInsertChild(node.YogaNode, childNode.YogaNode,
                YGNodeGetChildCount(node.YogaNode));
        }

        // 对应 JS: if (node.nodeName === 'ink-text' || node.nodeName === 'ink-virtual-text')
        if (node.NodeType is InkNodeType.Text or InkNodeType.VirtualText)
        {
            MarkNodeAsDirty(node);
        }
    }

    /// <summary>
    /// 在指定子节点前插入新节点。
    /// <para>对应 JS <c>insertBeforeNode(node, newChildNode, beforeChildNode)</c>。</para>
    /// </summary>
    public static void InsertBeforeNode(DomElement node, InkNode newChildNode, InkNode beforeChildNode)
    {
        // 对应 JS: if (newChildNode.parentNode) { removeChildNode(newChildNode.parentNode, newChildNode); }
        if (newChildNode.ParentNode is not null)
        {
            RemoveChildNode(newChildNode.ParentNode, newChildNode);
        }

        newChildNode.ParentNode = node;

        int index = node.ChildNodes.IndexOf(beforeChildNode);
        if (index >= 0)
        {
            node.ChildNodes.Insert(index, newChildNode);
            if (newChildNode.YogaNode is not null && node.YogaNode is not null)
            {
                YGNodeInsertChild(node.YogaNode, newChildNode.YogaNode, (nuint)index);
            }
        }
        else
        {
            node.ChildNodes.Add(newChildNode);
            if (newChildNode.YogaNode is not null && node.YogaNode is not null)
            {
                YGNodeInsertChild(node.YogaNode, newChildNode.YogaNode,
                    YGNodeGetChildCount(node.YogaNode));
            }
        }

        // 对应 JS 同上
        if (node.NodeType is InkNodeType.Text or InkNodeType.VirtualText)
        {
            MarkNodeAsDirty(node);
        }
    }

    /// <summary>
    /// 从父元素中移除子节点。
    /// <para>对应 JS <c>removeChildNode(node, removeNode)</c>。</para>
    /// </summary>
    public static void RemoveChildNode(DomElement node, InkNode removeNode)
    {
        // 对应 JS: if (removeNode.yogaNode) { removeNode.parentNode?.yogaNode?.removeChild(...) }
        if (removeNode.YogaNode is not null && removeNode.ParentNode?.YogaNode is not null)
        {
            YGNodeRemoveChild(removeNode.ParentNode.YogaNode, removeNode.YogaNode);
        }

        removeNode.ParentNode = null;

        int index = node.ChildNodes.IndexOf(removeNode);
        if (index >= 0)
        {
            node.ChildNodes.RemoveAt(index);
        }

        if (node.NodeType is InkNodeType.Text or InkNodeType.VirtualText)
        {
            MarkNodeAsDirty(node);
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    //  属性设置
    // ═════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 设置元素节点的属性。
    /// <para>对应 JS <c>setAttribute(node, key, value)</c>。</para>
    /// </summary>
    public static void SetAttribute(DomElement node, string key, DomNodeAttribute value)
    {
        node.Attributes[key] = value;
    }

    /// <summary>
    /// 设置节点的样式对象。
    /// <para>对应 JS <c>setStyle(node, style)</c>。渲染代码假设 style 始终为非 null。</para>
    /// </summary>
    public static void SetStyle(InkNode node, InkStyle? style)
    {
        node.Style = style ?? new InkStyle();
    }

    // ═════════════════════════════════════════════════════════════════════
    //  文本节点操作
    // ═════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 设置文本节点的值并标记最近的 Yoga 节点为 dirty。
    /// <para>对应 JS <c>setTextNodeValue(node, text)</c>。</para>
    /// </summary>
    public static void SetTextNodeValue(TextNode node, string text)
    {
        node.NodeValue = text;
        MarkNodeAsDirty(node);
    }

    // ═════════════════════════════════════════════════════════════════════
    //  布局监听
    // ═════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 在根节点上添加布局监听器。
    /// <para>对应 JS <c>addLayoutListener(rootNode, listener)</c>。</para>
    /// </summary>
    /// <returns>取消监听的 <see cref="IDisposable"/>。</returns>
    public static IDisposable AddLayoutListener(DomElement rootNode, Action listener)
    {
        return rootNode.AddLayoutListener(listener);
    }

    /// <summary>
    /// 触发根节点上所有布局监听器。
    /// <para>对应 JS <c>emitLayoutListeners(rootNode)</c>。</para>
    /// </summary>
    public static void EmitLayoutListeners(DomElement rootNode)
    {
        rootNode.EmitLayoutListeners();
    }

    // ═════════════════════════════════════════════════════════════════════
    //  Yoga 清理
    // ═════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 清理 Yoga 节点资源。
    /// <para>对应 JS <c>cleanupYogaNode(node)</c>。</para>
    /// </summary>
    public static void CleanupYogaNode(Facebook.Yoga.Node? yogaNode)
    {
        if (yogaNode is null) return;

        // 对应 JS: node?.unsetMeasureFunc(); node?.freeRecursive();
        // 在 Yoga.Net 中，通过设置 null 来取消 MeasureFunc
        // YGNodeSetMeasureFunc(yogaNode, null!);  — 设置为 null 取消
        YGNodeFreeRecursive(yogaNode);
    }

    // ═════════════════════════════════════════════════════════════════════
    //  私有辅助方法
    // ═════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 沿父链找到最近的拥有 Yoga 节点的节点。
    /// <para>对应 JS <c>findClosestYogaNode(node)</c>。</para>
    /// </summary>
    private static Facebook.Yoga.Node? FindClosestYogaNode(InkNode? node)
    {
        if (node?.ParentNode is null)
            return null;

        return node.YogaNode ?? FindClosestYogaNode(node.ParentNode);
    }

    /// <summary>
    /// 标记最近的 Yoga 节点为 dirty，以触发重新测量。
    /// <para>对应 JS <c>markNodeAsDirty(node)</c>。</para>
    /// </summary>
    private static void MarkNodeAsDirty(InkNode? node)
    {
        var yogaNode = FindClosestYogaNode(node);
        if (yogaNode is not null)
        {
            YGNodeMarkDirty(yogaNode);
        }
    }

    /// <summary>
    /// 为 ink-text 节点挂载 MeasureFunc。
    /// <para>
    /// 对应 JS <c>node.yogaNode?.setMeasureFunc(measureTextNode.bind(null, node))</c>。
    /// 使用闭包捕获 <see cref="DomElement"/> 引用，实现与 JS <c>bind</c> 等价的效果。
    /// </para>
    /// </summary>
    private static void AttachMeasureFunc(DomElement element)
    {
        var yogaNode = element.YogaNode!;

        // 对应 JS: measureTextNode.bind(null, node)
        // Yoga.Net YGMeasureFunc 委托签名:
        //   YGSize (Node node, float availableWidth, MeasureMode widthMode, float availableHeight, MeasureMode heightMode)
        YGNodeSetMeasureFunc(yogaNode, (_, width, widthMode, height, heightMode) =>
            MeasureTextNode(element, width));
    }

    /// <summary>
    /// 为 ink-raw-ansi 节点挂载 MeasureFunc，读取 rawWidth/rawHeight 属性。
    /// </summary>
    private static void AttachRawAnsiMeasureFunc(DomElement element)
    {
        var yogaNode = element.YogaNode!;

        YGNodeSetMeasureFunc(yogaNode, (_, width, widthMode, height, heightMode) =>
        {
            float w = 0;
            float h = 0;

            if (element.Attributes.TryGetValue("rawWidth", out var rawW))
                w = (float)rawW.NumberValue;

            if (element.Attributes.TryGetValue("rawHeight", out var rawH))
                h = (float)rawH.NumberValue;

            return new YGSize { Width = w, Height = h };
        });
    }

    /// <summary>
    /// 测量文本节点尺寸。
    /// <para>
    /// 1:1 对应 JS <c>measureTextNode(node, width)</c>。
    /// 在 MeasureFunc 中被 Yoga 布局引擎回调。
    /// </para>
    /// </summary>
    private static YGSize MeasureTextNode(DomElement node, float width)
    {
        // 对应 JS:
        // const text = node.nodeName === '#text' ? node.nodeValue : squashTextNodes(node);
        // 此处 node 始终是 DomElement (ink-text)，使用 squash 合并子文本
        var text = TextSquasher.Squash(node);

        var dimensions = TextMeasurer.Measure(text);

        // Yoga 在父级高度为 NaN（根 auto-height）等阶段会把 availableWidth/Height 设为 NaN。
        // 与 NaN 比较恒为 false，若继续 (int)Math.Max(1, NaN) 会得到非法 maxWidth，进而拖垮布局。
        if (float.IsNaN(width) || float.IsInfinity(width) || width < 0)
        {
            return new YGSize { Width = dimensions.Width, Height = dimensions.Height };
        }

        // 对应 JS: if (dimensions.width <= width) { return dimensions; }
        if (dimensions.Width <= width)
        {
            return new YGSize { Width = dimensions.Width, Height = dimensions.Height };
        }

        // 对应 JS: if (dimensions.width >= 1 && width > 0 && width < 1) { return dimensions; }
        // 当容器收缩到 <1px 时，告诉 Yoga 文本无法缩小
        if (dimensions.Width >= 1 && width > 0 && width < 1)
        {
            return new YGSize { Width = dimensions.Width, Height = dimensions.Height };
        }

        // 对应 JS: const textWrap = node.style?.textWrap ?? 'wrap';
        var textWrap = node.Style.TextWrap ?? TextWrapMode.Wrap;

        // 对应 JS: const wrappedText = wrapText(text, width, textWrap);
        int wrapCols = width >= int.MaxValue
            ? int.MaxValue
            : (int)Math.Max(1, Math.Floor(width));
        var wrappedText = TextWrapper.Wrap(text, wrapCols, textWrap);

        var wrappedDimensions = TextMeasurer.Measure(wrappedText);
        return new YGSize { Width = wrappedDimensions.Width, Height = wrappedDimensions.Height };
    }
}
