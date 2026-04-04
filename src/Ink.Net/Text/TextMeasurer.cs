// -----------------------------------------------------------------------
// <copyright file="TextMeasurer.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) measure-text.ts
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Concurrent;

namespace Ink.Net.Text;

/// <summary>
/// 文本尺寸测量器，对应 Ink JS <c>measure-text.ts</c>。
/// <para>使用线程安全的缓存避免重复计算。</para>
/// </summary>
public static class TextMeasurer
{
    /// <summary>测量结果。</summary>
    public readonly struct TextDimension : IEquatable<TextDimension>
    {
        /// <summary>文本的显示宽度（最宽行的终端列数）。</summary>
        public int Width { get; }

        /// <summary>文本的行数。</summary>
        public int Height { get; }

        /// <summary>创建尺寸结果。</summary>
        public TextDimension(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <inheritdoc/>
        public bool Equals(TextDimension other) => Width == other.Width && Height == other.Height;
        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is TextDimension d && Equals(d);
        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Width, Height);

        public static bool operator ==(TextDimension left, TextDimension right) => left.Equals(right);
        public static bool operator !=(TextDimension left, TextDimension right) => !left.Equals(right);
    }

    // 缓存 - 对应 JS: const cache = new Map<string, Output>()
    private static readonly ConcurrentDictionary<string, TextDimension> s_cache = new();

    /// <summary>
    /// 测量文本的终端显示尺寸（宽度和行数）。
    /// <para>对应 JS <c>measureText(text)</c>。</para>
    /// <para>内部使用 <see cref="StringWidthHelper"/> 正确处理 CJK 双宽字符。</para>
    /// </summary>
    /// <param name="text">待测量的文本。</param>
    /// <returns>文本的 (宽度, 高度) 尺寸。</returns>
    public static TextDimension Measure(string text)
    {
        if (string.IsNullOrEmpty(text))
            return new TextDimension(0, 0);

        if (s_cache.TryGetValue(text, out var cached))
            return cached;

        // 对应 JS: const width = widestLine(text);
        int width = StringWidthHelper.GetWidestLine(text);

        // 对应 JS: const height = text.split('\n').length;
        int height = CountLines(text);

        var dimensions = new TextDimension(width, height);
        s_cache.TryAdd(text, dimensions);

        return dimensions;
    }

    /// <summary>
    /// 清除测量缓存。
    /// </summary>
    public static void ClearCache()
    {
        s_cache.Clear();
    }

    /// <summary>
    /// 快速计算文本行数。
    /// </summary>
    private static int CountLines(string text)
    {
        if (text.Length == 0)
            return 0;

        int count = 1;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
                count++;
        }

        return count;
    }
}
