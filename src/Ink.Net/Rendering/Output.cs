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
/// <para>
/// Each character stores the complete ANSI "style state" that should be active
/// for it. During output composition, transitions are emitted only when the
/// style state changes between adjacent characters (matching JS
/// <c>styledCharsToString</c> behavior).
/// </para>
/// </summary>
internal struct StyledChar
{
    /// <summary>The character value (may be empty for multi-width trailing cells).</summary>
    public string Value;

    /// <summary>
    /// Complete set of active ANSI opening codes for this character (concatenated).
    /// E.g. <c>"\x1B[41m"</c> for red background.
    /// Empty string means no style.
    /// </summary>
    public string Style;

    public static StyledChar Space => new() { Value = " ", Style = "" };
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

        // Generate final string — emit ANSI codes only on style transitions
        // (mirrors JS styledCharsToString behavior)
        var sb = new StringBuilder();
        for (int y = 0; y < grid.Length; y++)
        {
            if (y > 0) sb.Append('\n');

            var lineSb = new StringBuilder();
            var row = grid[y];
            string activeStyle = "";

            for (int x = 0; x < row.Length; x++)
            {
                ref var ch = ref row[x];
                string curStyle = ch.Style ?? "";

                if (curStyle != activeStyle)
                {
                    // Style changed — emit close for old, open for new
                    if (activeStyle.Length > 0)
                        lineSb.Append(ComputeCloseCodes(activeStyle));
                    if (curStyle.Length > 0)
                        lineSb.Append(curStyle);
                    activeStyle = curStyle;
                }

                lineSb.Append(ch.Value);
            }

            // Close any remaining open style at end of line
            if (activeStyle.Length > 0)
                lineSb.Append(ComputeCloseCodes(activeStyle));

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
                            Style = sc.Style,
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

    /// <summary>
    /// Parse a line into StyledChars, propagating the ANSI style state to every character.
    /// <para>
    /// Opening ANSI codes accumulate in the active style. Reset codes clear them.
    /// Each character receives the full active style, so even when cells are overwritten
    /// in the grid, neighbouring cells retain their own style information.
    /// </para>
    /// </summary>
    private static List<StyledChar> ParseStyledChars(string line)
    {
        var result = new List<StyledChar>();
        var tokens = AnsiTokenizer.Tokenize(line);
        var activeStyle = new StringBuilder();

        foreach (var token in tokens)
        {
            if (token.Type == AnsiTokenType.Text)
            {
                string style = activeStyle.ToString();
                foreach (var rune in token.Value.EnumerateRunes())
                {
                    result.Add(new StyledChar
                    {
                        Value = rune.ToString(),
                        Style = style,
                    });
                }
            }
            else if (token.Type is AnsiTokenType.Csi or AnsiTokenType.Osc)
            {
                if (IsResetSequence(token.Value))
                {
                    activeStyle.Clear();
                }
                else
                {
                    activeStyle.Append(token.Value);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Determine whether an ANSI sequence is a "reset" (closing) code.
    /// </summary>
    private static bool IsResetSequence(string seq)
    {
        // CSI SGR resets: \x1B[0m, \x1B[39m, \x1B[49m, \x1B[22m, etc.
        // Use Ordinal comparisons throughout to avoid ICU collation issues with C1 control chars
        if (seq.EndsWith('m') && (
            seq.Contains("[0m", StringComparison.Ordinal) ||
            seq.Contains("[39m", StringComparison.Ordinal) ||
            seq.Contains("[49m", StringComparison.Ordinal) ||
            seq.Contains("[22m", StringComparison.Ordinal) ||
            seq.Contains("[23m", StringComparison.Ordinal) ||
            seq.Contains("[24m", StringComparison.Ordinal) ||
            seq.Contains("[25m", StringComparison.Ordinal) ||
            seq.Contains("[27m", StringComparison.Ordinal) ||
            seq.Contains("[28m", StringComparison.Ordinal) ||
            seq.Contains("[29m", StringComparison.Ordinal)))
        {
            return true;
        }

        // OSC hyperlink close: ESC]8;;BEL or ESC]8;;ST or C1_OSC 8;;BEL etc.
        // A closing hyperlink has ]8;; followed immediately by the terminator (no URL).
        if (IsOscHyperlinkClose(seq))
            return true;

        return false;
    }

    /// <summary>
    /// Check if an OSC sequence is a hyperlink close (empty URL).
    /// Opening: <c>ESC]8;;https://example.com BEL</c>
    /// Closing: <c>ESC]8;; BEL</c> (no URL)
    /// </summary>
    private static bool IsOscHyperlinkClose(string seq)
    {
        // ESC ] 8;; BEL  → length ~7
        // ESC ] 8;; ESC\ → length ~8
        // C1_OSC 8;; BEL → length ~5
        if (seq.Length > 20) return false; // Too long to be a close
        int idx = seq.IndexOf("8;;", StringComparison.Ordinal);
        if (idx < 0) return false;
        // After "8;;" there should only be the terminator (BEL, ST, ESC\)
        int afterParams = idx + 3;
        if (afterParams >= seq.Length) return false;
        string rest = seq[afterParams..];
        // Use ordinal comparison to avoid ICU collation issues with control chars
        return string.Equals(rest, "\u0007", StringComparison.Ordinal)
            || string.Equals(rest, "\u001B\\", StringComparison.Ordinal)
            || string.Equals(rest, "\u009C", StringComparison.Ordinal)
            || rest.Length == 0;
    }

    /// <summary>
    /// Compute the ANSI close codes for a given style (set of opening codes).
    /// </summary>
    private static string ComputeCloseCodes(string style)
    {
        if (string.IsNullOrEmpty(style)) return "";

        var tokens = AnsiTokenizer.Tokenize(style).ToList();
        var closeSb = new StringBuilder();
        var emittedResets = new HashSet<string>();

        // Iterate in reverse order to emit close codes in correct reverse order
        for (int i = tokens.Count - 1; i >= 0; i--)
        {
            var token = tokens[i];
            if (token.Type == AnsiTokenType.Csi && token.FinalCharacter == "m")
            {
                string closeCode = GetSgrCloseCode(token.ParameterString);
                if (closeCode.Length > 0 && emittedResets.Add(closeCode))
                    closeSb.Append(closeCode);
            }
            else if (token.Type == AnsiTokenType.Osc)
            {
                // For OSC hyperlink, generate the close sequence
                // CRITICAL: Use StringComparison.Ordinal for all comparisons
                // because .NET's culture-sensitive comparison (ICU) can incorrectly
                // equate C1 control characters (e.g. BEL 0x07 ≈ ST 0x9C).
                if (token.Value.Contains("8;", StringComparison.Ordinal))
                {
                    // Determine the original introducer and terminator style
                    string introducer = token.Value.StartsWith("\u009D", StringComparison.Ordinal)
                        ? "\u009D" : "\u001B]";

                    // Match the terminator from the opening sequence
                    string terminator;
                    if (token.Value.EndsWith("\u001B\\", StringComparison.Ordinal))
                        terminator = "\u001B\\";
                    else if (token.Value.EndsWith("\u009C", StringComparison.Ordinal))
                        terminator = "\u009C";
                    else
                        terminator = "\u0007"; // BEL (default)

                    closeSb.Append($"{introducer}8;;{terminator}");
                }
            }
        }

        return closeSb.ToString();
    }

    /// <summary>
    /// Get the SGR close code for a given SGR parameter string.
    /// </summary>
    private static string GetSgrCloseCode(string paramString)
    {
        if (string.IsNullOrEmpty(paramString)) return "\x1B[0m";

        string firstParam = paramString.Split(';', ':')[0];
        if (!int.TryParse(firstParam, out int code)) return "\x1B[0m";

        return code switch
        {
            >= 30 and <= 37 or 38 or >= 90 and <= 97 => "\x1B[39m",       // foreground
            >= 40 and <= 47 or 48 or >= 100 and <= 107 => "\x1B[49m",     // background
            1 or 2 => "\x1B[22m",                                           // bold / dim
            3 => "\x1B[23m",                                                 // italic
            4 => "\x1B[24m",                                                 // underline
            5 or 6 => "\x1B[25m",                                           // blink
            7 => "\x1B[27m",                                                 // inverse
            8 => "\x1B[28m",                                                 // hidden
            9 => "\x1B[29m",                                                 // strikethrough
            _ => "",                                                          // unknown — don't emit
        };
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
