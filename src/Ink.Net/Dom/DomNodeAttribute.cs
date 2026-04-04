// -----------------------------------------------------------------------
// <copyright file="DomNodeAttribute.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) dom.ts — DOMNodeAttribute
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Dom;

/// <summary>
/// DOM 节点属性值，对应 Ink JS 中的 <c>DOMNodeAttribute = boolean | string | number</c>。
/// <para>使用联合值结构体，避免装箱。NativeAOT 兼容。</para>
/// </summary>
public readonly struct DomNodeAttribute : IEquatable<DomNodeAttribute>
{
    /// <summary>属性值的类型标识。</summary>
    public enum ValueKind : byte
    {
        /// <summary>布尔值。</summary>
        Bool = 0,

        /// <summary>字符串值。</summary>
        String = 1,

        /// <summary>数值 (双精度浮点)。</summary>
        Number = 2,
    }

    private readonly double _number;
    private readonly string? _string;
    private readonly bool _bool;

    /// <summary>获取属性值的类型。</summary>
    public ValueKind Kind { get; }

    /// <summary>获取布尔值。仅当 <see cref="Kind"/> 为 <see cref="ValueKind.Bool"/> 时有意义。</summary>
    public bool BoolValue => _bool;

    /// <summary>获取字符串值。仅当 <see cref="Kind"/> 为 <see cref="ValueKind.String"/> 时有意义。</summary>
    public string? StringValue => _string;

    /// <summary>获取数值。仅当 <see cref="Kind"/> 为 <see cref="ValueKind.Number"/> 时有意义。</summary>
    public double NumberValue => _number;

    private DomNodeAttribute(bool value)
    {
        Kind = ValueKind.Bool;
        _bool = value;
        _string = null;
        _number = 0;
    }

    private DomNodeAttribute(string value)
    {
        Kind = ValueKind.String;
        _string = value;
        _bool = false;
        _number = 0;
    }

    private DomNodeAttribute(double value)
    {
        Kind = ValueKind.Number;
        _number = value;
        _bool = false;
        _string = null;
    }

    /// <summary>从布尔值隐式转换。</summary>
    public static implicit operator DomNodeAttribute(bool value) => new(value);

    /// <summary>从字符串隐式转换。</summary>
    public static implicit operator DomNodeAttribute(string value) => new(value);

    /// <summary>从整数隐式转换。</summary>
    public static implicit operator DomNodeAttribute(int value) => new(value);

    /// <summary>从双精度浮点隐式转换。</summary>
    public static implicit operator DomNodeAttribute(double value) => new(value);

    /// <inheritdoc/>
    public bool Equals(DomNodeAttribute other) =>
        Kind == other.Kind &&
        Kind switch
        {
            ValueKind.Bool => _bool == other._bool,
            ValueKind.String => _string == other._string,
            ValueKind.Number => _number == other._number, // ReSharper disable once CompareOfFloatsByEqualityOperator
            _ => false,
        };

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is DomNodeAttribute d && Equals(d);

    /// <inheritdoc/>
    public override int GetHashCode() => Kind switch
    {
        ValueKind.Bool => HashCode.Combine(Kind, _bool),
        ValueKind.String => HashCode.Combine(Kind, _string),
        ValueKind.Number => HashCode.Combine(Kind, _number),
        _ => 0,
    };

    /// <inheritdoc/>
    public override string ToString() => Kind switch
    {
        ValueKind.Bool => _bool.ToString(),
        ValueKind.String => _string ?? string.Empty,
        ValueKind.Number => _number.ToString("G"),
        _ => string.Empty,
    };

    public static bool operator ==(DomNodeAttribute left, DomNodeAttribute right) => left.Equals(right);
    public static bool operator !=(DomNodeAttribute left, DomNodeAttribute right) => !left.Equals(right);
}
