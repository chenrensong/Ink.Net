// -----------------------------------------------------------------------
// <copyright file="TextWrapper.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) wrap-text.ts
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Text;
using Ink.Net.Styles;

namespace Ink.Net.Text;

/// <summary>
/// 文本换行/截断处理器，对应 Ink JS <c>wrap-text.ts</c>。
/// <para>支持以下模式：</para>
/// <list type="bullet">
///   <item><see cref="TextWrapMode.Wrap"/> — 按宽度自动换行</item>
///   <item><see cref="TextWrapMode.TruncateEnd"/> / <see cref="TextWrapMode.End"/> / <see cref="TextWrapMode.Truncate"/> — 截断末尾</item>
///   <item><see cref="TextWrapMode.TruncateMiddle"/> / <see cref="TextWrapMode.Middle"/> — 截断中间</item>
///   <item><see cref="TextWrapMode.TruncateStart"/> — 截断开头</item>
/// </list>
/// </summary>
public static class TextWrapper
{
    // 缓存 - 对应 JS: const cache: Record<string, string> = {}
    private static readonly ConcurrentDictionary<string, string> s_cache = new();

    /// <summary>
    /// 根据指定模式对文本进行换行或截断。
    /// <para>对应 JS <c>wrapText(text, maxWidth, wrapType)</c>。</para>
    /// </summary>
    /// <param name="text">原始文本。</param>
    /// <param name="maxWidth">最大宽度（终端列数）。</param>
    /// <param name="wrapType">换行/截断模式。</param>
    /// <returns>处理后的文本。</returns>
    public static string Wrap(string text, int maxWidth, TextWrapMode wrapType)
    {
        // 对应 JS: const cacheKey = text + String(maxWidth) + String(wrapType);
        var cacheKey = string.Concat(text, maxWidth.ToString(), ((int)wrapType).ToString());

        if (s_cache.TryGetValue(cacheKey, out var cached))
            return cached;

        string result = text;

        if (wrapType == TextWrapMode.Wrap)
        {
            // 对应 JS: wrappedText = wrapAnsi(text, maxWidth, { trim: false, hard: true })
            result = WrapAnsi(text, maxWidth);
        }
        else if (IsTruncateMode(wrapType))
        {
            // 对应 JS: wrappedText = cliTruncate(text, maxWidth, { position })
            var position = wrapType switch
            {
                TextWrapMode.TruncateMiddle or TextWrapMode.Middle => TruncatePosition.Middle,
                TextWrapMode.TruncateStart => TruncatePosition.Start,
                _ => TruncatePosition.End,
            };

            result = TruncateText(text, maxWidth, position);
        }

        s_cache.TryAdd(cacheKey, result);
        return result;
    }

    /// <summary>清除缓存。</summary>
    public static void ClearCache()
    {
        s_cache.Clear();
    }

    // ─── 内部实现 ────────────────────────────────────────────────────────

    private enum TruncatePosition { Start, Middle, End }

    private static bool IsTruncateMode(TextWrapMode mode) =>
        mode is TextWrapMode.TruncateEnd or TextWrapMode.End
            or TextWrapMode.Truncate
            or TextWrapMode.TruncateMiddle or TextWrapMode.Middle
            or TextWrapMode.TruncateStart;

    /// <summary>
    /// ANSI 感知文本换行（word-level wrapping with hard wrap fallback）。
    /// <para>对应 JS <c>wrap-ansi</c> 包的核心逻辑 (trim: false, hard: true)。</para>
    /// <para>
    /// 算法：优先在空格处断行（word wrap）；如果一个单词超过 maxWidth 则在字符级强制断行（hard wrap）。
    /// 断行时空格被消耗（不出现在上一行末尾或下一行开头）。
    /// </para>
    /// </summary>
    private static string WrapAnsi(string text, int maxWidth)
    {
        if (maxWidth <= 0)
            return text;

        var result = new StringBuilder(text.Length + text.Length / maxWidth);
        var lines = text.Split('\n');

        for (int lineIdx = 0; lineIdx < lines.Length; lineIdx++)
        {
            if (lineIdx > 0) result.Append('\n');

            var line = lines[lineIdx];
            if (line.Length == 0)
                continue;

            // We build the current line in a buffer and track word boundaries.
            var lineBuf = new StringBuilder();
            int lineWidth = 0;

            // Track the last space position for word-level wrapping
            int lastSpacePos = -1;       // index in lineBuf where the space character is
            int widthAfterSpace = 0;     // lineWidth right after the space (including it)

            bool inEscape = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                // ANSI escape sequences — pass through without affecting width
                if (c == '\x1B')
                {
                    inEscape = true;
                    lineBuf.Append(c);
                    continue;
                }

                if (inEscape)
                {
                    lineBuf.Append(c);
                    if (c >= 0x40 && c <= 0x7E)
                        inEscape = false;
                    continue;
                }

                int charWidth = GetVisualCharWidth(line, ref i);

                // Would adding this character overflow the current line?
                if (lineWidth + charWidth > maxWidth && lineWidth > 0)
                {
                    if (c == ' ')
                    {
                        // Space itself causes overflow — use it as the break point.
                        // Output current line content, skip the space, continue.
                        result.Append(lineBuf);
                        result.Append('\n');
                        lineBuf.Clear();
                        lineWidth = 0;
                        lastSpacePos = -1;
                        continue; // Don't append the space
                    }

                    if (lastSpacePos >= 0)
                    {
                        // Word wrap: break at the last space
                        // Output everything before the space
                        result.Append(lineBuf.ToString(0, lastSpacePos));
                        result.Append('\n');

                        // Carry over everything after the space to the new line
                        string remaining = lineBuf.ToString(lastSpacePos + 1, lineBuf.Length - lastSpacePos - 1);
                        lineBuf.Clear();
                        lineBuf.Append(remaining);
                        lineWidth = lineWidth - widthAfterSpace;
                        lastSpacePos = -1;
                    }
                    else
                    {
                        // Hard wrap: no word boundary available
                        result.Append(lineBuf);
                        result.Append('\n');
                        lineBuf.Clear();
                        lineWidth = 0;
                    }
                }

                // Track space positions for word wrapping
                if (c == ' ')
                {
                    lastSpacePos = lineBuf.Length;
                    widthAfterSpace = lineWidth + charWidth;
                }

                // Append the character (handle surrogate pairs)
                if (char.IsHighSurrogate(c) && i + 1 < line.Length && char.IsLowSurrogate(line[i + 1]))
                {
                    lineBuf.Append(c);
                    lineBuf.Append(line[i + 1]);
                    i++;
                }
                else
                {
                    lineBuf.Append(c);
                }

                lineWidth += charWidth;
            }

            // Flush remaining content
            result.Append(lineBuf);
        }

        return result.ToString();
    }

    /// <summary>
    /// 简化版的文本截断。
    /// <para>对应 JS <c>cli-truncate</c> 包。</para>
    /// </summary>
    private static string TruncateText(string text, int maxWidth, TruncatePosition position)
    {
        int textWidth = StringWidthHelper.GetStringWidth(text);
        if (textWidth <= maxWidth)
            return text;

        const string ellipsis = "…";
        const int ellipsisWidth = 1;

        if (maxWidth < ellipsisWidth)
            return ellipsis[..maxWidth];

        int available = maxWidth - ellipsisWidth;

        return position switch
        {
            TruncatePosition.End => SliceByWidth(text, 0, available) + ellipsis,
            TruncatePosition.Start => ellipsis + SliceByWidthFromEnd(text, available),
            TruncatePosition.Middle => TruncateMiddle(text, available),
            _ => text,
        };
    }

    private static string TruncateMiddle(string text, int available)
    {
        int leftWidth = available / 2;
        int rightWidth = available - leftWidth;

        return SliceByWidth(text, 0, leftWidth) + "…" + SliceByWidthFromEnd(text, rightWidth);
    }

    /// <summary>按可视宽度从头截取文本。</summary>
    private static string SliceByWidth(string text, int startWidth, int width)
    {
        var sb = new StringBuilder();
        int currentWidth = 0;
        bool inEscape = false;

        for (int i = 0; i < text.Length && currentWidth < startWidth + width; i++)
        {
            char c = text[i];

            if (c == '\x1B') { inEscape = true; sb.Append(c); continue; }
            if (inEscape) { sb.Append(c); if (c >= 0x40 && c <= 0x7E) inEscape = false; continue; }

            int charWidth = GetVisualCharWidth(text, ref i);
            if (currentWidth >= startWidth && currentWidth + charWidth <= startWidth + width)
            {
                if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                {
                    sb.Append(c);
                    sb.Append(text[i + 1]);
                    i++;
                }
                else
                {
                    sb.Append(c);
                }
            }

            currentWidth += charWidth;
        }

        return sb.ToString();
    }

    /// <summary>按可视宽度从末尾截取文本。</summary>
    private static string SliceByWidthFromEnd(string text, int width)
    {
        int totalWidth = StringWidthHelper.GetStringWidth(text);
        return SliceByWidth(text, totalWidth - width, width);
    }

    /// <summary>获取当前位置字符的可视宽度。</summary>
    private static int GetVisualCharWidth(string text, ref int _)
    {
        // 简化版本：依赖 StringWidthHelper 的单字符判断
        char c = text[_];

        if (c < 0x20 || (c >= 0x7F && c < 0xA0))
            return 0;

        if (char.IsHighSurrogate(c) && _ + 1 < text.Length && char.IsLowSurrogate(text[_ + 1]))
        {
            int cp = char.ConvertToUtf32(c, text[_ + 1]);
            return (cp >= 0x20000 && cp <= 0x3FFFF) || (cp >= 0x1F000 && cp <= 0x1FFFF) ? 2 : 1;
        }

        return StringWidthHelper.GetStringWidth(c.ToString());
    }
}
