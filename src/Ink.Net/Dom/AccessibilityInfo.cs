// -----------------------------------------------------------------------
// <copyright file="AccessibilityInfo.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) dom.ts — internal_accessibility
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Dom;

/// <summary>
/// 可访问性角色，对应 Ink JS 中 <c>internal_accessibility.role</c> 的枚举值。
/// </summary>
public enum AccessibilityRole : byte
{
    None = 0,
    Button,
    Checkbox,
    Combobox,
    List,
    Listbox,
    Listitem,
    Menu,
    Menuitem,
    Option,
    Progressbar,
    Radio,
    Radiogroup,
    Tab,
    Tablist,
    Table,
    Textbox,
    Timer,
    Toolbar,
}

/// <summary>
/// 可访问性状态，对应 Ink JS 中 <c>internal_accessibility.state</c>。
/// </summary>
public sealed class AccessibilityState
{
    public bool Busy { get; set; }
    public bool Checked { get; set; }
    public bool Disabled { get; set; }
    public bool Expanded { get; set; }
    public bool Multiline { get; set; }
    public bool Multiselectable { get; set; }
    public bool Readonly { get; set; }
    public bool Required { get; set; }
    public bool Selected { get; set; }

    /// <summary>
    /// 获取所有为 <c>true</c> 的状态名称列表。
    /// </summary>
    public IEnumerable<string> GetActiveStateNames()
    {
        if (Busy) yield return "busy";
        if (Checked) yield return "checked";
        if (Disabled) yield return "disabled";
        if (Expanded) yield return "expanded";
        if (Multiline) yield return "multiline";
        if (Multiselectable) yield return "multiselectable";
        if (Readonly) yield return "readonly";
        if (Required) yield return "required";
        if (Selected) yield return "selected";
    }
}

/// <summary>
/// 可访问性信息容器，对应 Ink JS 中的 <c>internal_accessibility</c> 对象。
/// </summary>
public sealed class AccessibilityInfo
{
    /// <summary>ARIA 角色。</summary>
    public AccessibilityRole Role { get; set; }

    /// <summary>ARIA 标签 (用于屏幕阅读器覆盖)。对应 JS <c>aria-label</c> prop。</summary>
    public string? Label { get; set; }

    /// <summary>
    /// Whether the element is hidden from screen readers.
    /// <para>对应 JS <c>aria-hidden</c> prop。</para>
    /// </summary>
    public bool Hidden { get; set; }

    /// <summary>ARIA 状态。</summary>
    public AccessibilityState State { get; set; } = new();
}
