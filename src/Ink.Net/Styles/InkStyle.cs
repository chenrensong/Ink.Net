// -----------------------------------------------------------------------
// <copyright file="InkStyle.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) styles.ts — Styles type
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Styles;

/// <summary>文本换行模式。对应 JS <c>Styles.textWrap</c>。</summary>
public enum TextWrapMode : byte
{
    /// <summary>自动换行 (默认)。</summary>
    Wrap = 0,
    /// <summary>截断末尾。</summary>
    End,
    /// <summary>截断中间。</summary>
    Middle,
    /// <summary>截断末尾 (别名)。</summary>
    TruncateEnd,
    /// <summary>截断 (别名)。</summary>
    Truncate,
    /// <summary>截断中间 (别名)。</summary>
    TruncateMiddle,
    /// <summary>截断开头。</summary>
    TruncateStart,
}

/// <summary>定位模式。</summary>
public enum PositionMode : byte
{
    Relative = 0,
    Absolute,
    Static,
}

/// <summary>Flex 方向。</summary>
public enum FlexDirectionMode : byte
{
    Row = 0,
    Column,
    RowReverse,
    ColumnReverse,
}

/// <summary>Flex 换行模式。</summary>
public enum FlexWrapMode : byte
{
    NoWrap = 0,
    Wrap,
    WrapReverse,
}

/// <summary>对齐模式 (alignItems / alignContent)。</summary>
public enum AlignItemsMode : byte
{
    Stretch = 0,
    FlexStart,
    Center,
    FlexEnd,
    Baseline,
}

/// <summary>自身对齐模式 (alignSelf)。</summary>
public enum AlignSelfMode : byte
{
    Auto = 0,
    FlexStart,
    Center,
    FlexEnd,
    Stretch,
    Baseline,
}

/// <summary>内容对齐模式 (alignContent)。</summary>
public enum AlignContentMode : byte
{
    FlexStart = 0,
    Center,
    FlexEnd,
    Stretch,
    SpaceBetween,
    SpaceAround,
    SpaceEvenly,
}

/// <summary>主轴对齐模式 (justifyContent)。</summary>
public enum JustifyContentMode : byte
{
    FlexStart = 0,
    Center,
    FlexEnd,
    SpaceBetween,
    SpaceAround,
    SpaceEvenly,
}

/// <summary>显示模式。</summary>
public enum DisplayMode : byte
{
    Flex = 0,
    None,
}

/// <summary>溢出处理模式。</summary>
public enum OverflowMode : byte
{
    Visible = 0,
    Hidden,
}

/// <summary>
/// 尺寸值（可以是绝对值、百分比值或 auto）。
/// 对应 Ink JS 中 <c>width/height</c> 等属性的 <c>number | string</c> 类型。
/// </summary>
public readonly struct DimensionValue : IEquatable<DimensionValue>
{
    /// <summary>值的单位类型。</summary>
    public enum DimensionUnit : byte
    {
        /// <summary>绝对值 (points)。</summary>
        Points = 0,
        /// <summary>百分比值。</summary>
        Percent,
        /// <summary>自动尺寸。</summary>
        Auto,
    }

    private readonly float _value;

    /// <summary>获取数值。</summary>
    public float Value => _value;

    /// <summary>获取单位类型。</summary>
    public DimensionUnit Unit { get; }

    /// <summary>是否为绝对值。</summary>
    public bool IsPoints => Unit == DimensionUnit.Points;

    /// <summary>是否为百分比值。</summary>
    public bool IsPercent => Unit == DimensionUnit.Percent;

    /// <summary>是否为 auto。</summary>
    public bool IsAuto => Unit == DimensionUnit.Auto;

    private DimensionValue(float value, DimensionUnit unit)
    {
        _value = value;
        Unit = unit;
    }

    /// <summary>创建绝对值。</summary>
    public static DimensionValue Points(float value) => new(value, DimensionUnit.Points);

    /// <summary>创建百分比值。</summary>
    public static DimensionValue Percent(float value) => new(value, DimensionUnit.Percent);

    /// <summary>自动尺寸值。</summary>
    public static readonly DimensionValue AutoValue = new(0, DimensionUnit.Auto);

    /// <summary>从浮点数隐式转换为绝对值。</summary>
    public static implicit operator DimensionValue(float value) => Points(value);

    /// <summary>从整数隐式转换为绝对值。</summary>
    public static implicit operator DimensionValue(int value) => Points(value);

    /// <summary>
    /// 从字符串解析尺寸值。支持 "50%" (百分比) 和 "100" (绝对值)。
    /// 对应 JS 中 <c>Number.parseInt(style.width, 10)</c>。
    /// </summary>
    public static DimensionValue Parse(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Points(0);

        // JS: Number.parseInt(value, 10) / Number.parseFloat(value)
        // 百分比字符串示例: "50%"，Ink JS 直接 parseInt 取数值部分
        if (float.TryParse(value.AsSpan().TrimEnd('%'), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var num))
        {
            return value.Contains('%') ? Percent(num) : Points(num);
        }

        return Points(0);
    }

    /// <inheritdoc/>
    public bool Equals(DimensionValue other) => Unit == other.Unit && _value == other._value;
    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is DimensionValue d && Equals(d);
    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(_value, Unit);

    public static bool operator ==(DimensionValue left, DimensionValue right) => left.Equals(right);
    public static bool operator !=(DimensionValue left, DimensionValue right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString() => Unit switch
    {
        DimensionUnit.Points => _value.ToString("G"),
        DimensionUnit.Percent => $"{_value}%",
        DimensionUnit.Auto => "auto",
        _ => _value.ToString("G"),
    };
}

/// <summary>
/// Ink 节点样式属性集合。
/// <para>1:1 对应 Ink JS <c>styles.ts</c> 中的 <c>Styles</c> 类型。</para>
/// <para>所有属性均为可空类型，以区分 "未设置" 和 "设置为默认值"，用于增量样式更新 (diff)。</para>
/// </summary>
public sealed class InkStyle
{
    // ─── Text ────────────────────────────────────────────────────────────

    /// <summary>文本换行模式。默认 <see cref="TextWrapMode.Wrap"/>。</summary>
    public TextWrapMode? TextWrap { get; set; }

    // ─── Position ────────────────────────────────────────────────────────

    /// <summary>定位模式。</summary>
    public PositionMode? Position { get; set; }

    /// <summary>顶部偏移量 (绝对值或百分比)。</summary>
    public DimensionValue? Top { get; set; }

    /// <summary>右侧偏移量 (绝对值或百分比)。</summary>
    public DimensionValue? Right { get; set; }

    /// <summary>底部偏移量 (绝对值或百分比)。</summary>
    public DimensionValue? Bottom { get; set; }

    /// <summary>左侧偏移量 (绝对值或百分比)。</summary>
    public DimensionValue? Left { get; set; }

    // ─── Gap ─────────────────────────────────────────────────────────────

    /// <summary>列间距。</summary>
    public float? ColumnGap { get; set; }

    /// <summary>行间距。</summary>
    public float? RowGap { get; set; }

    /// <summary>列和行间距的简写。</summary>
    public float? Gap { get; set; }

    // ─── Margin ──────────────────────────────────────────────────────────

    /// <summary>所有方向的外边距。</summary>
    public float? Margin { get; set; }

    /// <summary>水平外边距 (MarginLeft + MarginRight)。</summary>
    public float? MarginX { get; set; }

    /// <summary>垂直外边距 (MarginTop + MarginBottom)。</summary>
    public float? MarginY { get; set; }

    /// <summary>顶部外边距。</summary>
    public float? MarginTop { get; set; }

    /// <summary>底部外边距。</summary>
    public float? MarginBottom { get; set; }

    /// <summary>左侧外边距。</summary>
    public float? MarginLeft { get; set; }

    /// <summary>右侧外边距。</summary>
    public float? MarginRight { get; set; }

    // ─── Padding ─────────────────────────────────────────────────────────

    /// <summary>所有方向的内边距。</summary>
    public float? Padding { get; set; }

    /// <summary>水平内边距 (PaddingLeft + PaddingRight)。</summary>
    public float? PaddingX { get; set; }

    /// <summary>垂直内边距 (PaddingTop + PaddingBottom)。</summary>
    public float? PaddingY { get; set; }

    /// <summary>顶部内边距。</summary>
    public float? PaddingTop { get; set; }

    /// <summary>底部内边距。</summary>
    public float? PaddingBottom { get; set; }

    /// <summary>左侧内边距。</summary>
    public float? PaddingLeft { get; set; }

    /// <summary>右侧内边距。</summary>
    public float? PaddingRight { get; set; }

    // ─── Flex ────────────────────────────────────────────────────────────

    /// <summary>Flex 增长因子。</summary>
    public float? FlexGrow { get; set; }

    /// <summary>Flex 收缩因子。</summary>
    public float? FlexShrink { get; set; }

    /// <summary>Flex 方向。</summary>
    public FlexDirectionMode? FlexDirection { get; set; }

    /// <summary>Flex 基准尺寸。</summary>
    public DimensionValue? FlexBasis { get; set; }

    /// <summary>Flex 换行。</summary>
    public FlexWrapMode? FlexWrap { get; set; }

    /// <summary>交叉轴子项对齐方式。</summary>
    public AlignItemsMode? AlignItems { get; set; }

    /// <summary>自身在交叉轴上的对齐方式。</summary>
    public AlignSelfMode? AlignSelf { get; set; }

    /// <summary>多行时交叉轴的对齐方式。</summary>
    public AlignContentMode? AlignContent { get; set; }

    /// <summary>主轴对齐方式。</summary>
    public JustifyContentMode? JustifyContent { get; set; }

    // ─── Dimension ───────────────────────────────────────────────────────

    /// <summary>宽度 (绝对值或百分比)。</summary>
    public DimensionValue? Width { get; set; }

    /// <summary>高度 (绝对值或百分比)。</summary>
    public DimensionValue? Height { get; set; }

    /// <summary>最小宽度。</summary>
    public DimensionValue? MinWidth { get; set; }

    /// <summary>最小高度。</summary>
    public DimensionValue? MinHeight { get; set; }

    /// <summary>最大宽度。</summary>
    public DimensionValue? MaxWidth { get; set; }

    /// <summary>最大高度。</summary>
    public DimensionValue? MaxHeight { get; set; }

    /// <summary>宽高比。</summary>
    public float? AspectRatio { get; set; }

    // ─── Display ─────────────────────────────────────────────────────────

    /// <summary>显示模式。设为 <see cref="DisplayMode.None"/> 以隐藏元素。</summary>
    public DisplayMode? Display { get; set; }

    // ─── Border (Ink 特有，部分映射到 Yoga) ─────────────────────────────

    /// <summary>边框样式名称 (如 "single", "double", "round" 等，或自定义 BoxStyle)。</summary>
    public string? BorderStyle { get; set; }

    /// <summary>是否显示顶部边框。默认 true。</summary>
    public bool? BorderTop { get; set; }

    /// <summary>是否显示底部边框。默认 true。</summary>
    public bool? BorderBottom { get; set; }

    /// <summary>是否显示左侧边框。默认 true。</summary>
    public bool? BorderLeft { get; set; }

    /// <summary>是否显示右侧边框。默认 true。</summary>
    public bool? BorderRight { get; set; }

    /// <summary>边框颜色 (所有方向)。</summary>
    public string? BorderColor { get; set; }

    /// <summary>顶部边框颜色。</summary>
    public string? BorderTopColor { get; set; }

    /// <summary>底部边框颜色。</summary>
    public string? BorderBottomColor { get; set; }

    /// <summary>左侧边框颜色。</summary>
    public string? BorderLeftColor { get; set; }

    /// <summary>右侧边框颜色。</summary>
    public string? BorderRightColor { get; set; }

    /// <summary>是否使边框颜色变暗 (所有方向)。</summary>
    public bool? BorderDimColor { get; set; }

    /// <summary>是否使顶部边框颜色变暗。</summary>
    public bool? BorderTopDimColor { get; set; }

    /// <summary>是否使底部边框颜色变暗。</summary>
    public bool? BorderBottomDimColor { get; set; }

    /// <summary>是否使左侧边框颜色变暗。</summary>
    public bool? BorderLeftDimColor { get; set; }

    /// <summary>是否使右侧边框颜色变暗。</summary>
    public bool? BorderRightDimColor { get; set; }

    // ─── Overflow (Ink 特有，渲染层处理) ─────────────────────────────────

    /// <summary>溢出处理 (所有方向)。</summary>
    public OverflowMode? Overflow { get; set; }

    /// <summary>水平溢出处理。</summary>
    public OverflowMode? OverflowX { get; set; }

    /// <summary>垂直溢出处理。</summary>
    public OverflowMode? OverflowY { get; set; }

    // ─── Background ──────────────────────────────────────────────────────

    /// <summary>背景颜色。</summary>
    public string? BackgroundColor { get; set; }
}
