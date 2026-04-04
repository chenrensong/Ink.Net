// -----------------------------------------------------------------------
// <copyright file="Output.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) output.ts
//   "Virtual" output class — positions and saves output of each node.
// </copyright>
// -----------------------------------------------------------------------

using System.Text;
using Ink.Net.Ansi;
using Ink.Net.Dom;
using Ink.Net.Text;

namespace Ink.Net.Rendering;

/// <summary>
/// Clip region for output operations.
/// <para>Corresponds to JS <c>Clip</c> type.</para>
/// </summary>
public sealed class OutputClip
{
    public int? X1 { get; set; }
    public int? X2 { get; set; }
    public int? Y1 { get; set; }
    public int? Y2 { get; set; }
}

/// <summary>
/// A single styled character in the output buffer.
/// Replaces JS <c>StyledChar</c> from <c>@alcalzone/ansi-tokenize</c>.
/// </summary>
internal struct StyledChar
{
    /// <summary>The character value (may be empty for multi-width trailing cells).</summary>
    public string Value;
    /// <summary>ANSI style prefix (e.g. color codes) to apply before this character.</summary>
    public string StylePrefix;
    /// <summary>ANSI style suffix (e.g. reset codes) to apply after this character.</summary>
    public string StyleSuffix;

    public static StyledChar Space => new() { Value = " ", StylePrefix = "", StyleSuffix = "" };
}

/// <summary>
/// "Virtual" output class.
/// <para>
/// Handles the positioning and saving of the output of each node in the tree.
/// Also responsible for applying transformations to each character of the output.
/// </para>
/// <para>Used to generate the final output of all nodes before writing it to actual output stream (e.g. stdout).</para>
/// <para>1:1 port of Ink JS <c>output.ts</c>.</para>
/// </summary>
public sealed class Output
{
    public int Width { get; }
    public int Height { get; }

    private readonly List<Operation> _operations = new();

    // ─── Caches ──────────────────────────────────────────────────────
    private readonly Dictionary<string, int> _widthCache = new();
    private readonly Dictionary<string, int> _blockWidthCache = new();

    public Output(int width, int height)
    {
        Width = width;
        Height = height;
    }

    // ─── Public API (mirrors JS) ─────────────────────────────────────

    /// <summary>
    /// Write text at position (x, y) with optional transformers.
    /// <para>Corresponds to JS <c>output.write(x, y, text, { transformers })</c>.</para>
    /// </summary>
    public void Write(int x, int y, string text, OutputTransformer[]? transformers = null)
    {
        if (string.IsNullOrEmpty(text))
            return;

        _operations.Add(new WriteOperation(x, y, text, transformers ?? []));
    }

    /// <summary>
    /// Push a clip region.
    /// <para>Corresponds to JS <c>output.clip(clip)</c>.</para>
    /// </summary>
    public void Clip(OutputClip clip)
    {
        _operations.Add(new ClipOperation(clip));
    }

    /// <summary>
    /// Pop the most recent clip region.
    /// <para>Corresponds to JS <c>output.unclip()</c>.</para>
    /// </summary>
    public void Unclip()
    {
        _operations.Add(new UnclipOperation());
    }

    /// <summary>
    /// Compose all operations into the final output string.
    /// <para>Corresponds to JS <c>output.get()</c>.</para>
    /// </summary>
    public (string Output, int Height) Get()
    {
        // Initialize output grid with spaces
        var grid = new StyledChar[Height][];
        for (int y = 0; y < Height; y++)
        {
            grid[y] = new StyledChar[Width];
            for (int x = 0; x < Width; x++)
            {
                grid[y][x] = StyledChar.Space;
            }
        }

        var clips = new List<OutputClip>();

        foreach (var operation in _operations)
        {
            switch (operation)
            {
                case ClipOperation clipOp:
                    clips.Add(clipOp.Clip);
                    break;

                case UnclipOperation:
                    if (clips.Count > 0)
                        clips.RemoveAt(clips.Count - 1);
                    break;

                case WriteOperation writeOp:
                    ProcessWrite(grid, writeOp, clips);
                    break;
            }
        }

        // Generate final string
        var sb = new StringBuilder();
        for (int y = 0; y < grid.Length; y++)
        {
            if (y > 0) sb.Append('\n');

            var lineSb = new StringBuilder();
            var row = grid[y];

            for (int x = 0; x < row.Length; x++)
            {
                ref var ch = ref row[x];
                if (ch.StylePrefix?.Length > 0)
                    lineSb.Append(ch.StylePrefix);
                lineSb.Append(ch.Value);
                if (ch.StyleSuffix?.Length > 0)
                    lineSb.Append(ch.StyleSuffix);
            }

            // trimEnd equivalent
            sb.Append(lineSb.ToString().TrimEnd());
        }

        return (sb.ToString(), grid.Length);
    }

    // ─── Process a single write operation ────────────────────────────

    private void ProcessWrite(StyledChar[][] grid, WriteOperation op, List<OutputClip> clips)
    {
        int x = op.X;
        int y = op.Y;
        string text = op.Text;
        var transformers = op.Transformers;

        string[] lines = text.Split('\n');

        // Apply clip
        var clip = clips.Count > 0 ? clips[^1] : null;

        if (clip is not null)
        {
            bool clipH = clip.X1.HasValue && clip.X2.HasValue;
            bool clipV = clip.Y1.HasValue && clip.Y2.HasValue;

            // Skip if entirely outside clip
            if (clipH)
            {
                int width = CachedGetWidestLine(text);
                if (x + width < clip.X1!.Value || x > clip.X2!.Value)
                    return;
            }

            if (clipV)
            {
                int height = lines.Length;
                if (y + height < clip.Y1!.Value || y > clip.Y2!.Value)
                    return;
            }

            if (clipH)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    int from = x < clip.X1!.Value ? clip.X1.Value - x : 0;
                    int lineWidth = CachedGetStringWidth(lines[i]);
                    int to = x + lineWidth > clip.X2!.Value ? clip.X2.Value - x : lineWidth;
                    lines[i] = SliceAnsi(lines[i], from, to);
                }

                if (x < clip.X1!.Value)
                    x = clip.X1.Value;
            }

            if (clipV)
            {
                int from = y < clip.Y1!.Value ? clip.Y1.Value - y : 0;
                int height = lines.Length;
                int to = y + height > clip.Y2!.Value ? clip.Y2.Value - y : height;

                lines = lines[from..to];

                if (y < clip.Y1!.Value)
                    y = clip.Y1.Value;
            }
        }

        int offsetY = 0;

        for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            int currentY = y + offsetY;
            if (currentY < 0 || currentY >= grid.Length)
            {
                offsetY++;
                continue;
            }

            var currentRow = grid[currentY];
            string line = lines[lineIndex];

            // Apply transformers
            foreach (var transformer in transformers)
            {
                line = transformer(line, lineIndex);
            }

            // Parse styled characters from the line
            var styledChars = ParseStyledChars(line);
            int offsetX = x;

            foreach (var sc in styledChars)
            {
                if (offsetX < 0)
                {
                    offsetX++;
                    continue;
                }

                if (offsetX >= currentRow.Length)
                    break;

                currentRow[offsetX] = sc;

                int charWidth = Math.Max(1, StringWidthHelper.GetStringWidth(sc.Value));

                // For multi-column characters, clear following cells
                if (charWidth > 1)
                {
                    for (int i = 1; i < charWidth && offsetX + i < currentRow.Length; i++)
                    {
                        currentRow[offsetX + i] = new StyledChar
                        {
                            Value = "",
                            StylePrefix = sc.StylePrefix,
                            StyleSuffix = sc.StyleSuffix,
                        };
                    }
                }

                offsetX += charWidth;
            }

            offsetY++;
        }
    }

    // ─── ANSI-aware string slicing (simplified slice-ansi) ───────────

    /// <summary>
    /// Slice a string by visible character positions, preserving ANSI codes.
    /// Simplified port of the <c>slice-ansi</c> npm package.
    /// </summary>
    public static string SliceAnsi(string text, int from, int to)
    {
        if (from >= to)
            return "";

        var tokens = AnsiTokenizer.Tokenize(text);
        var result = new StringBuilder();
        int visibleIndex = 0;
        var activeStyles = new StringBuilder();

        foreach (var token in tokens)
        {
            if (token.Type == AnsiTokenType.Text)
            {
                foreach (var rune in token.Value.EnumerateRunes())
                {
                    string ch = rune.ToString();
                    int w = StringWidthHelper.GetStringWidth(ch);

                    if (visibleIndex >= from && visibleIndex < to)
                    {
                        if (activeStyles.Length > 0)
                        {
                            result.Append(activeStyles);
                            activeStyles.Clear();
                        }

                        result.Append(ch);
                    }

                    visibleIndex += Math.Max(1, w);

                    if (visibleIndex >= to)
                        break;
                }
            }
            else if (token.Type is AnsiTokenType.Csi or AnsiTokenType.Osc)
            {
                // Preserve style sequences
                if (visibleIndex >= from && visibleIndex < to)
                    result.Append(token.Value);
                else
                    activeStyles.Append(token.Value);
            }

            if (visibleIndex >= to)
                break;
        }

        return result.ToString();
    }

    // ─── Parse line into styled chars ────────────────────────────────

    private static List<StyledChar> ParseStyledChars(string line)
    {
        var result = new List<StyledChar>();
        var tokens = AnsiTokenizer.Tokenize(line);
        var pendingStyles = new StringBuilder();

        foreach (var token in tokens)
        {
            if (token.Type == AnsiTokenType.Text)
            {
                string prefix = pendingStyles.Length > 0 ? pendingStyles.ToString() : "";
                if (pendingStyles.Length > 0)
                    pendingStyles.Clear();

                foreach (var rune in token.Value.EnumerateRunes())
                {
                    string ch = rune.ToString();
                    result.Add(new StyledChar
                    {
                        Value = ch,
                        StylePrefix = prefix,
                        StyleSuffix = "",
                    });
                    prefix = ""; // Only apply to first char
                }
            }
            else if (token.Type is AnsiTokenType.Csi or AnsiTokenType.Osc)
            {
                // Check if it's a reset — add as suffix to the last char
                if (result.Count > 0 && IsResetSequence(token.Value))
                {
                    ref var last = ref System.Runtime.InteropServices.CollectionsMarshal.AsSpan(result)[^1];
                    last.StyleSuffix = (last.StyleSuffix ?? "") + token.Value;
                }
                else
                {
                    pendingStyles.Append(token.Value);
                }
            }
        }

        return result;
    }

    private static bool IsResetSequence(string seq)
    {
        // \x1B[0m or \x1B[39m or \x1B[49m etc. (common resets)
        return seq.EndsWith("m") && (
            seq.Contains("[0m") ||
            seq.Contains("[39m") ||
            seq.Contains("[49m") ||
            seq.Contains("[22m") ||
            seq.Contains("[24m") ||
            seq.Contains("[27m") ||
            seq.Contains("[28m") ||
            seq.Contains("[29m"));
    }

    // ─── Cached helpers ──────────────────────────────────────────────

    private int CachedGetStringWidth(string text)
    {
        if (_widthCache.TryGetValue(text, out var cached))
            return cached;

        int width = StringWidthHelper.GetStringWidth(text);
        _widthCache[text] = width;
        return width;
    }

    private int CachedGetWidestLine(string text)
    {
        if (_blockWidthCache.TryGetValue(text, out var cached))
            return cached;

        int width = 0;
        foreach (var line in text.Split('\n'))
        {
            width = Math.Max(width, CachedGetStringWidth(line));
        }

        _blockWidthCache[text] = width;
        return width;
    }

    // ─── Operation types ─────────────────────────────────────────────

    private abstract class Operation { }

    private sealed class WriteOperation(int x, int y, string text, OutputTransformer[] transformers) : Operation
    {
        public int X { get; } = x;
        public int Y { get; } = y;
        public string Text { get; } = text;
        public OutputTransformer[] Transformers { get; } = transformers;
    }

    private sealed class ClipOperation(OutputClip clip) : Operation
    {
        public OutputClip Clip { get; } = clip;
    }

    private sealed class UnclipOperation : Operation { }
}
