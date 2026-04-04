// -----------------------------------------------------------------------
// <copyright file="StringWidthHelper.cs" company="Ink.Net">
//   中文/CJK 双宽字符宽度计算。
//   对应 JS 中的 'string-width' 和 'widest-line' 包的核心功能。
// </copyright>
// -----------------------------------------------------------------------

using System.Buffers;
using System.Text;
using System.Runtime.CompilerServices;

namespace Ink.Net.Text;

/// <summary>
/// 终端字符串显示宽度计算工具。
/// <para>
/// 正确处理以下场景：
/// <list type="bullet">
///   <item>CJK 统一汉字和扩展区 — 双宽 (width 2)</item>
///   <item>全角字符 (Fullwidth Forms) — 双宽 (width 2)</item>
///   <item>ANSI 转义序列 — 零宽</item>
///   <item>组合字符 (Combining) — 零宽</item>
///   <item>控制字符 — 零宽</item>
///   <item>Emoji — 双宽 (简化处理)</item>
/// </list>
/// </para>
/// </summary>
public static class StringWidthHelper
{
    /// <summary>
    /// 计算字符串在终端中的显示宽度（列数）。
    /// <para>对应 JS <c>string-width</c> 包。</para>
    /// </summary>
    /// <param name="text">要计算宽度的字符串。</param>
    /// <returns>终端显示宽度（列数）。</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static int GetStringWidth(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        int width = 0;

        // ANSI 解析状态机:
        //   0 = 正常状态
        //   1 = 刚遇到 ESC (0x1B)
        //   2 = 在 CSI 序列中 (ESC [ ... 终结符)
        //   3 = 在 OSC 序列中 (ESC ] ... ST)
        int escapeState = 0;

        // ANSI 解析状态机:
        //   0 = 正常状态
        //   1 = 刚遇到 ESC (0x1B)
        //   2 = 在 CSI 序列中 (ESC [ ... 终结符, or C1 CSI 0x9B)
        //   3 = 在 OSC 序列中 (ESC ] ... ST, or C1 OSC 0x9D) — BEL-terminated
        //   4 = 在 DCS/PM/APC/SOS 控制串中 — ST-terminated only
        //   5 = 在控制串(4)中刚遇到 ESC，等待 '\\' 构成 ST

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            // ─── ANSI 转义序列跳过 ─────────────────────────────────────

            // State 5: inside DCS/PM/APC/SOS, just saw ESC — check for '\\' (ST)
            if (escapeState == 5)
            {
                if (c == '\\')
                    escapeState = 0; // ESC \ = ST, control string ended
                else if (c == '\x1B')
                    escapeState = 5; // doubled ESC (tmux), stay in state 5
                else
                    escapeState = 4; // not ST, back to control string body
                continue;
            }

            // State 4: inside DCS/PM/APC/SOS control string — skip until ST
            if (escapeState == 4)
            {
                if (c == '\u009C') // C1 ST terminates control string
                    escapeState = 0;
                else if (c == '\x1B')
                    escapeState = 5; // might be ESC \\ (ST)
                continue;
            }

            if (escapeState == 1) // 刚遇到 ESC
            {
                if (c == '[')
                {
                    // CSI 序列开始: ESC [ <params> <final>
                    escapeState = 2;
                }
                else if (c == ']')
                {
                    // OSC 序列开始: ESC ] ... ST
                    escapeState = 3;
                }
                else if (c == 'P' || c == 'X' || c == '^' || c == '_')
                {
                    // DCS(P), SOS(X), PM(^), APC(_) — control strings terminated by ST
                    escapeState = 4;
                }
                else if (c >= 0x40 && c <= 0x5F)
                {
                    // 其他双字符 ESC 序列 (如 ESC D, ESC M)
                    escapeState = 0;
                }
                else
                {
                    // 未知序列，恢复正常
                    escapeState = 0;
                }
                continue;
            }

            if (escapeState == 2) // CSI 序列中
            {
                // CSI 参数字节: 0x30-0x3F (数字、分号等)
                // CSI 中间字节: 0x20-0x2F
                // CSI 终结字节: 0x40-0x7E (字母等)
                if (c >= 0x40 && c <= 0x7E)
                {
                    escapeState = 0; // 序列结束
                }
                // 否则继续在 CSI 中
                continue;
            }

            if (escapeState == 3) // OSC 序列中
            {
                // OSC 以 BEL (0x07)、C1 ST (0x9C) 或 ST (ESC \) 结束
                if (c == '\x07' || c == '\u009C')
                {
                    escapeState = 0;
                }
                else if (c == '\x1B')
                {
                    // 可能是 ST (ESC \)
                    escapeState = 1;
                }
                continue;
            }

            if (c == '\x1B')
            {
                escapeState = 1;
                continue;
            }

            // ─── C1 sequence introducers (8-bit equivalents) ──────────
            // Must check BEFORE the generic C1 control skip below
            if (c == '\u009B') // C1 CSI — same as ESC [
            {
                escapeState = 2;
                continue;
            }
            if (c == '\u009D') // C1 OSC — same as ESC ]
            {
                escapeState = 3;
                continue;
            }
            if (c == '\u0090' || c == '\u0098' || c == '\u009E' || c == '\u009F')
            {
                // C1 DCS(0x90), SOS(0x98), PM(0x9E), APC(0x9F) — ST-terminated
                escapeState = 4;
                continue;
            }

            // ─── 控制字符 ──────────────────────────────────────────────
            if (c < 0x20 || (c >= 0x7F && c < 0xA0))
                continue;

            // ─── 代理对 (Surrogate Pair) 处理 ──────────────────────────
            if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
            {
                int codePoint = char.ConvertToUtf32(c, text[i + 1]);
                i++; // 跳过低代理

                width += GetCodePointWidth(codePoint);
                continue;
            }

            // ─── BMP 字符宽度 ──────────────────────────────────────────
            width += GetCharWidth(c);
        }

        return width;
    }

    /// <summary>
    /// 计算多行文本中最宽行的显示宽度。
    /// <para>对应 JS <c>widest-line</c> 包。</para>
    /// </summary>
    /// <param name="text">可能包含换行符的文本。</param>
    /// <returns>最宽行的终端显示宽度。</returns>
    public static int GetWidestLine(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        int maxWidth = 0;

        foreach (var line in text.AsSpan().EnumerateLines())
        {
            int lineWidth = GetStringWidth(line.ToString());
            if (lineWidth > maxWidth)
                maxWidth = lineWidth;
        }

        return maxWidth;
    }

    /// <summary>
    /// 获取 BMP 字符的终端显示宽度。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetCharWidth(char c)
    {
        // 组合字符 (Combining) — 零宽
        if (IsCombiningChar(c))
            return 0;

        // CJK / 全角字符 — 双宽
        if (IsWideChar(c))
            return 2;

        return 1;
    }

    /// <summary>
    /// 获取 Unicode 码点的终端显示宽度（处理 BMP 外的字符）。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetCodePointWidth(int codePoint)
    {
        // CJK 扩展区 B-I (U+20000 ~ U+3FFFF)
        if (codePoint >= 0x20000 && codePoint <= 0x3FFFF)
            return 2;

        // Emoji（简化处理：大部分 Emoji 是双宽）
        // Emoji 范围较广，这里覆盖最常见的区域
        if (codePoint >= 0x1F000 && codePoint <= 0x1FFFF)
            return 2;

        // 其他补充平面字符
        return 1;
    }

    /// <summary>
    /// 判断字符是否为 CJK 双宽字符。
    /// <para>基于 Unicode East Asian Width 属性 (W 和 F)。</para>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsWideChar(char c)
    {
        // 以下范围参考 Unicode EAW (East Asian Width) 标准
        return c switch
        {
            // Hangul Jamo (韩文字母)
            >= '\u1100' and <= '\u115F' => true,
            >= '\u2329' and <= '\u232A' => true,

            // CJK 部首、康熙部首、CJK 符号和标点
            >= '\u2E80' and <= '\u303E' => true,

            // 日文平假名、片假名、注音符号、韩文兼容字母、CJK 笔划
            >= '\u3040' and <= '\u33BF' => true,

            // CJK 统一汉字扩展 A
            >= '\u3400' and <= '\u4DBF' => true,

            // CJK 统一汉字
            >= '\u4E00' and <= '\u9FFF' => true,

            // 彝文
            >= '\uA000' and <= '\uA4CF' => true,

            // 韩文音节
            >= '\uAC00' and <= '\uD7AF' => true,

            // CJK 兼容汉字
            >= '\uF900' and <= '\uFAFF' => true,

            // 竖排形式、CJK 兼容形式、小写变体
            >= '\uFE10' and <= '\uFE6F' => true,

            // 全角 ASCII 变体 (！到～)
            >= '\uFF01' and <= '\uFF60' => true,

            // 全角符号
            >= '\uFFE0' and <= '\uFFE6' => true,

            _ => false,
        };
    }

    /// <summary>
    /// 判断字符是否为组合字符（零宽）。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsCombiningChar(char c)
    {
        // Unicode Combining Diacritical Marks 和相关范围
        return c switch
        {
            >= '\u0300' and <= '\u036F' => true, // Combining Diacritical Marks
            >= '\u0483' and <= '\u0489' => true, // Cyrillic combining
            >= '\u0591' and <= '\u05BD' => true, // Hebrew combining
            >= '\u0610' and <= '\u061A' => true, // Arabic combining
            >= '\u064B' and <= '\u065F' => true, // Arabic combining
            >= '\u0670' and <= '\u0670' => true, // Arabic
            >= '\u06D6' and <= '\u06DC' => true, // Arabic
            >= '\u0730' and <= '\u074A' => true, // Syriac
            >= '\u0E31' and <= '\u0E31' => true, // Thai
            >= '\u0E34' and <= '\u0E3A' => true, // Thai
            >= '\u0E47' and <= '\u0E4E' => true, // Thai
            >= '\u1AB0' and <= '\u1AFF' => true, // Combining Diacritical Marks Extended
            >= '\u1DC0' and <= '\u1DFF' => true, // Combining Diacritical Marks Supplement
            >= '\u20D0' and <= '\u20FF' => true, // Combining Diacritical Marks for Symbols
            >= '\uFE00' and <= '\uFE0F' => true, // Variation Selectors
            >= '\uFE20' and <= '\uFE2F' => true, // Combining Half Marks
            _ => false,
        };
    }
}
