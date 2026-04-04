// -----------------------------------------------------------------------
// <copyright file="StyleApplier.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) styles.ts — apply*Styles functions
// </copyright>
// -----------------------------------------------------------------------

using Facebook.Yoga;
using static Facebook.Yoga.YGNodeStyleAPI;
using YogaNode = Facebook.Yoga.Node;

namespace Ink.Net.Styles;

/// <summary>
/// 将 <see cref="InkStyle"/> 属性 1:1 映射到 Yoga.Net 的 <see cref="YogaNode"/> 布局属性上。
/// <para>
/// 对应 Ink JS <c>styles.ts</c> 中的 <c>applyPositionStyles</c>、<c>applyMarginStyles</c> 等函数。
/// </para>
/// </summary>
public static class StyleApplier
{
    /// <summary>
    /// 将样式增量应用到 Yoga 节点上。
    /// <para>
    /// 对应 JS <c>styles(node, style, currentStyle)</c>。
    /// <paramref name="style"/> 包含变更的属性（diff 结果），
    /// <paramref name="currentStyle"/> 是合并后的完整样式（用于 Border 计算）。
    /// </para>
    /// </summary>
    /// <param name="node">目标 Yoga 节点。</param>
    /// <param name="style">变更的样式属性 (diff)。</param>
    /// <param name="currentStyle">当前完整样式 (可选，默认与 style 相同)。</param>
    public static void Apply(YogaNode node, InkStyle style, InkStyle? currentStyle = null)
    {
        currentStyle ??= style;

        ApplyPositionStyles(node, style);
        ApplyMarginStyles(node, style);
        ApplyPaddingStyles(node, style);
        ApplyFlexStyles(node, style);
        ApplyDimensionStyles(node, style);
        ApplyDisplayStyles(node, style);
        ApplyBorderStyles(node, style, currentStyle);
        ApplyGapStyles(node, style);
    }

    // ═════════════════════════════════════════════════════════════════════
    //  Position (对应 JS applyPositionStyles)
    // ═════════════════════════════════════════════════════════════════════

    private static void ApplyPositionStyles(YogaNode node, InkStyle style)
    {
        if (style.Position.HasValue)
        {
            // 对应 JS: node.setPositionType(positionType)
            var positionType = style.Position.Value switch
            {
                PositionMode.Absolute => YGPositionType.Absolute,
                PositionMode.Static => YGPositionType.Static,
                _ => YGPositionType.Relative,
            };
            YGNodeStyleSetPositionType(node, positionType);
        }

        // 对应 JS: for (const [property, edge] of positionEdges) { ... }
        ApplyPositionEdge(node, style.Top, YGEdge.Top);
        ApplyPositionEdge(node, style.Right, YGEdge.Right);
        ApplyPositionEdge(node, style.Bottom, YGEdge.Bottom);
        ApplyPositionEdge(node, style.Left, YGEdge.Left);
    }

    private static void ApplyPositionEdge(YogaNode node, DimensionValue? value, YGEdge edge)
    {
        if (!value.HasValue) return;

        var v = value.Value;
        if (v.IsPercent)
        {
            // 对应 JS: node.setPositionPercent(edge, Number.parseFloat(value))
            YGNodeStyleSetPositionPercent(node, edge, v.Value);
        }
        else
        {
            // 对应 JS: node.setPosition(edge, value)
            YGNodeStyleSetPosition(node, edge, v.Value);
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    //  Margin (对应 JS applyMarginStyles)
    // ═════════════════════════════════════════════════════════════════════

    private static void ApplyMarginStyles(YogaNode node, InkStyle style)
    {
        // 对应 JS: node.setMargin(Yoga.EDGE_ALL, style.margin ?? 0)
        if (style.Margin.HasValue)
            YGNodeStyleSetMargin(node, YGEdge.All, style.Margin ?? 0);

        // 对应 JS: node.setMargin(Yoga.EDGE_HORIZONTAL, style.marginX ?? 0)
        if (style.MarginX.HasValue)
            YGNodeStyleSetMargin(node, YGEdge.Horizontal, style.MarginX ?? 0);

        // 对应 JS: node.setMargin(Yoga.EDGE_VERTICAL, style.marginY ?? 0)
        if (style.MarginY.HasValue)
            YGNodeStyleSetMargin(node, YGEdge.Vertical, style.MarginY ?? 0);

        // 对应 JS: node.setMargin(Yoga.EDGE_START, style.marginLeft || 0)
        if (style.MarginLeft.HasValue)
            YGNodeStyleSetMargin(node, YGEdge.Start, style.MarginLeft ?? 0);

        // 对应 JS: node.setMargin(Yoga.EDGE_END, style.marginRight || 0)
        if (style.MarginRight.HasValue)
            YGNodeStyleSetMargin(node, YGEdge.End, style.MarginRight ?? 0);

        // 对应 JS: node.setMargin(Yoga.EDGE_TOP, style.marginTop || 0)
        if (style.MarginTop.HasValue)
            YGNodeStyleSetMargin(node, YGEdge.Top, style.MarginTop ?? 0);

        // 对应 JS: node.setMargin(Yoga.EDGE_BOTTOM, style.marginBottom || 0)
        if (style.MarginBottom.HasValue)
            YGNodeStyleSetMargin(node, YGEdge.Bottom, style.MarginBottom ?? 0);
    }

    // ═════════════════════════════════════════════════════════════════════
    //  Padding (对应 JS applyPaddingStyles)
    // ═════════════════════════════════════════════════════════════════════

    private static void ApplyPaddingStyles(YogaNode node, InkStyle style)
    {
        if (style.Padding.HasValue)
            YGNodeStyleSetPadding(node, YGEdge.All, style.Padding ?? 0);

        if (style.PaddingX.HasValue)
            YGNodeStyleSetPadding(node, YGEdge.Horizontal, style.PaddingX ?? 0);

        if (style.PaddingY.HasValue)
            YGNodeStyleSetPadding(node, YGEdge.Vertical, style.PaddingY ?? 0);

        if (style.PaddingLeft.HasValue)
            YGNodeStyleSetPadding(node, YGEdge.Left, style.PaddingLeft ?? 0);

        if (style.PaddingRight.HasValue)
            YGNodeStyleSetPadding(node, YGEdge.Right, style.PaddingRight ?? 0);

        if (style.PaddingTop.HasValue)
            YGNodeStyleSetPadding(node, YGEdge.Top, style.PaddingTop ?? 0);

        if (style.PaddingBottom.HasValue)
            YGNodeStyleSetPadding(node, YGEdge.Bottom, style.PaddingBottom ?? 0);
    }

    // ═════════════════════════════════════════════════════════════════════
    //  Flex (对应 JS applyFlexStyles)
    // ═════════════════════════════════════════════════════════════════════

    private static void ApplyFlexStyles(YogaNode node, InkStyle style)
    {
        // ── flexGrow ──
        if (style.FlexGrow.HasValue)
            YGNodeStyleSetFlexGrow(node, style.FlexGrow ?? 0);

        // ── flexShrink ──
        if (style.FlexShrink.HasValue)
            YGNodeStyleSetFlexShrink(node, style.FlexShrink ?? 1);

        // ── flexWrap ──
        if (style.FlexWrap.HasValue)
        {
            var wrap = style.FlexWrap.Value switch
            {
                FlexWrapMode.Wrap => YGWrap.Wrap,
                FlexWrapMode.WrapReverse => YGWrap.WrapReverse,
                _ => YGWrap.NoWrap,
            };
            YGNodeStyleSetFlexWrap(node, wrap);
        }

        // ── flexDirection ──
        if (style.FlexDirection.HasValue)
        {
            var dir = style.FlexDirection.Value switch
            {
                FlexDirectionMode.Row => YGFlexDirection.Row,
                FlexDirectionMode.RowReverse => YGFlexDirection.RowReverse,
                FlexDirectionMode.ColumnReverse => YGFlexDirection.ColumnReverse,
                _ => YGFlexDirection.Column,
            };
            YGNodeStyleSetFlexDirection(node, dir);
        }

        // ── flexBasis ──
        if (style.FlexBasis.HasValue)
        {
            var basis = style.FlexBasis.Value;
            if (basis.IsPercent)
            {
                // 对应 JS: node.setFlexBasisPercent(Number.parseInt(style.flexBasis, 10))
                YGNodeStyleSetFlexBasisPercent(node, basis.Value);
            }
            else if (basis.IsAuto)
            {
                // 对应 JS: node.setFlexBasis(Number.NaN)
                YGNodeStyleSetFlexBasisAuto(node);
            }
            else
            {
                // 对应 JS: node.setFlexBasis(style.flexBasis)
                YGNodeStyleSetFlexBasis(node, basis.Value);
            }
        }

        // ── alignItems ──
        if (style.AlignItems.HasValue)
        {
            var align = style.AlignItems.Value switch
            {
                AlignItemsMode.FlexStart => YGAlign.FlexStart,
                AlignItemsMode.Center => YGAlign.Center,
                AlignItemsMode.FlexEnd => YGAlign.FlexEnd,
                AlignItemsMode.Baseline => YGAlign.Baseline,
                _ => YGAlign.Stretch, // 默认 stretch
            };
            YGNodeStyleSetAlignItems(node, align);
        }

        // ── alignSelf ──
        if (style.AlignSelf.HasValue)
        {
            var align = style.AlignSelf.Value switch
            {
                AlignSelfMode.FlexStart => YGAlign.FlexStart,
                AlignSelfMode.Center => YGAlign.Center,
                AlignSelfMode.FlexEnd => YGAlign.FlexEnd,
                AlignSelfMode.Stretch => YGAlign.Stretch,
                AlignSelfMode.Baseline => YGAlign.Baseline,
                _ => YGAlign.Auto, // 默认 auto
            };
            YGNodeStyleSetAlignSelf(node, align);
        }

        // ── alignContent ──
        if (style.AlignContent.HasValue)
        {
            var align = style.AlignContent.Value switch
            {
                AlignContentMode.Center => YGAlign.Center,
                AlignContentMode.FlexEnd => YGAlign.FlexEnd,
                AlignContentMode.SpaceBetween => YGAlign.SpaceBetween,
                AlignContentMode.SpaceAround => YGAlign.SpaceAround,
                AlignContentMode.SpaceEvenly => YGAlign.SpaceEvenly,
                AlignContentMode.Stretch => YGAlign.Stretch,
                _ => YGAlign.FlexStart, // 默认 flex-start
            };
            YGNodeStyleSetAlignContent(node, align);
        }

        // ── justifyContent ──
        if (style.JustifyContent.HasValue)
        {
            var justify = style.JustifyContent.Value switch
            {
                JustifyContentMode.Center => YGJustify.Center,
                JustifyContentMode.FlexEnd => YGJustify.FlexEnd,
                JustifyContentMode.SpaceBetween => YGJustify.SpaceBetween,
                JustifyContentMode.SpaceAround => YGJustify.SpaceAround,
                JustifyContentMode.SpaceEvenly => YGJustify.SpaceEvenly,
                _ => YGJustify.FlexStart, // 默认 flex-start
            };
            YGNodeStyleSetJustifyContent(node, justify);
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    //  Dimension (对应 JS applyDimensionStyles)
    // ═════════════════════════════════════════════════════════════════════

    private static void ApplyDimensionStyles(YogaNode node, InkStyle style)
    {
        // ── width ──
        if (style.Width.HasValue)
        {
            var w = style.Width.Value;
            if (w.IsPercent) YGNodeStyleSetWidthPercent(node, w.Value);
            else if (w.IsAuto) YGNodeStyleSetWidthAuto(node);
            else YGNodeStyleSetWidth(node, w.Value);
        }

        // ── height ──
        if (style.Height.HasValue)
        {
            var h = style.Height.Value;
            if (h.IsPercent) YGNodeStyleSetHeightPercent(node, h.Value);
            else if (h.IsAuto) YGNodeStyleSetHeightAuto(node);
            else YGNodeStyleSetHeight(node, h.Value);
        }

        // ── minWidth ──
        if (style.MinWidth.HasValue)
        {
            var v = style.MinWidth.Value;
            if (v.IsPercent) YGNodeStyleSetMinWidthPercent(node, v.Value);
            else YGNodeStyleSetMinWidth(node, v.Value);
        }

        // ── minHeight ──
        if (style.MinHeight.HasValue)
        {
            var v = style.MinHeight.Value;
            if (v.IsPercent) YGNodeStyleSetMinHeightPercent(node, v.Value);
            else YGNodeStyleSetMinHeight(node, v.Value);
        }

        // ── maxWidth ──
        if (style.MaxWidth.HasValue)
        {
            var v = style.MaxWidth.Value;
            if (v.IsPercent) YGNodeStyleSetMaxWidthPercent(node, v.Value);
            else YGNodeStyleSetMaxWidth(node, v.Value);
        }

        // ── maxHeight ──
        if (style.MaxHeight.HasValue)
        {
            var v = style.MaxHeight.Value;
            if (v.IsPercent) YGNodeStyleSetMaxHeightPercent(node, v.Value);
            else YGNodeStyleSetMaxHeight(node, v.Value);
        }

        // ── aspectRatio ──
        if (style.AspectRatio.HasValue)
        {
            // 对应 JS: node.setAspectRatio(style.aspectRatio)
            YGNodeStyleSetAspectRatio(node, style.AspectRatio.Value);
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    //  Display (对应 JS applyDisplayStyles)
    // ═════════════════════════════════════════════════════════════════════

    private static void ApplyDisplayStyles(YogaNode node, InkStyle style)
    {
        if (style.Display.HasValue)
        {
            // 对应 JS: node.setDisplay(style.display === 'flex' ? DISPLAY_FLEX : DISPLAY_NONE)
            YGNodeStyleSetDisplay(node,
                style.Display.Value == DisplayMode.Flex ? YGDisplay.Flex : YGDisplay.None);
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    //  Border (对应 JS applyBorderStyles)
    //  注意：仅 borderStyle/borderTop/borderBottom/borderLeft/borderRight
    //        会影响 Yoga 布局（边框宽度为 0 或 1）。
    //        颜色、dimColor 等属性仅在渲染层使用。
    // ═════════════════════════════════════════════════════════════════════

    private static void ApplyBorderStyles(YogaNode node, InkStyle style, InkStyle currentStyle)
    {
        // 对应 JS: const hasBorderChanges = 'borderStyle' in style || ...
        bool hasBorderChanges =
            style.BorderStyle is not null ||
            style.BorderTop.HasValue ||
            style.BorderBottom.HasValue ||
            style.BorderLeft.HasValue ||
            style.BorderRight.HasValue;

        if (!hasBorderChanges)
            return;

        // 对应 JS: const borderWidth = currentStyle.borderStyle ? 1 : 0;
        float borderWidth = currentStyle.BorderStyle is not null ? 1f : 0f;

        // 对应 JS: node.setBorder(Yoga.EDGE_TOP, currentStyle.borderTop === false ? 0 : borderWidth)
        YGNodeStyleSetBorder(node, YGEdge.Top,
            currentStyle.BorderTop == false ? 0 : borderWidth);
        YGNodeStyleSetBorder(node, YGEdge.Bottom,
            currentStyle.BorderBottom == false ? 0 : borderWidth);
        YGNodeStyleSetBorder(node, YGEdge.Left,
            currentStyle.BorderLeft == false ? 0 : borderWidth);
        YGNodeStyleSetBorder(node, YGEdge.Right,
            currentStyle.BorderRight == false ? 0 : borderWidth);
    }

    // ═════════════════════════════════════════════════════════════════════
    //  Gap (对应 JS applyGapStyles)
    // ═════════════════════════════════════════════════════════════════════

    private static void ApplyGapStyles(YogaNode node, InkStyle style)
    {
        if (style.Gap.HasValue)
            YGNodeStyleSetGap(node, YGGutter.All, style.Gap ?? 0);

        if (style.ColumnGap.HasValue)
            YGNodeStyleSetGap(node, YGGutter.Column, style.ColumnGap ?? 0);

        if (style.RowGap.HasValue)
            YGNodeStyleSetGap(node, YGGutter.Row, style.RowGap ?? 0);
    }
}
