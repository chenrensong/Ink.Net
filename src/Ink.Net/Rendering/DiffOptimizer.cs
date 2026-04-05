// -----------------------------------------------------------------------
// <copyright file="DiffOptimizer.cs" company="Ink.Net">
//   Port from Ink (JS) optimizer.ts — Diff patch optimization
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Rendering;

/// <summary>
/// Type of a diff patch.
/// </summary>
public enum DiffPatchType : byte
{
    Stdout,
    CursorMove,
    CursorTo,
    StyleStr,
    Hyperlink,
    CursorShow,
    CursorHide,
    Clear,
}

/// <summary>
/// A single diff patch operation.
/// </summary>
public sealed class DiffPatch
{
    public DiffPatchType Type { get; init; }
    public string Content { get; set; } = "";
    public int X { get; set; }
    public int Y { get; set; }
    public int Count { get; set; }
    public string? Uri { get; set; }

    public static DiffPatch StdoutPatch(string content) => new() { Type = DiffPatchType.Stdout, Content = content };
    public static DiffPatch CursorMovePatch(int x, int y) => new() { Type = DiffPatchType.CursorMove, X = x, Y = y };
    public static DiffPatch CursorToPatch(int x, int y) => new() { Type = DiffPatchType.CursorTo, X = x, Y = y };
    public static DiffPatch StylePatch(string str) => new() { Type = DiffPatchType.StyleStr, Content = str };
    public static DiffPatch HyperlinkPatch(string? uri) => new() { Type = DiffPatchType.Hyperlink, Uri = uri };
    public static DiffPatch ShowCursor() => new() { Type = DiffPatchType.CursorShow };
    public static DiffPatch HideCursor() => new() { Type = DiffPatchType.CursorHide };
    public static DiffPatch ClearPatch(int count) => new() { Type = DiffPatchType.Clear, Count = count };
}

/// <summary>
/// Optimizes a list of diff patches by merging, deduplicating, and cancelling redundant operations.
/// Port of JS optimizer.ts.
/// </summary>
public static class DiffOptimizer
{
    /// <summary>
    /// Optimize a diff by applying all optimization rules in a single pass.
    /// </summary>
    public static List<DiffPatch> Optimize(List<DiffPatch> diff)
    {
        if (diff.Count <= 1) return diff;

        var result = new List<DiffPatch>(diff.Count);

        foreach (var patch in diff)
        {
            // Skip no-ops
            if (patch.Type == DiffPatchType.Stdout && patch.Content.Length == 0) continue;
            if (patch.Type == DiffPatchType.CursorMove && patch.X == 0 && patch.Y == 0) continue;
            if (patch.Type == DiffPatchType.Clear && patch.Count == 0) continue;

            // Try to merge with previous patch
            if (result.Count > 0)
            {
                var last = result[^1];

                // Merge consecutive cursorMove
                if (patch.Type == DiffPatchType.CursorMove && last.Type == DiffPatchType.CursorMove)
                {
                    last.X += patch.X;
                    last.Y += patch.Y;
                    continue;
                }

                // Collapse consecutive cursorTo (only last matters)
                if (patch.Type == DiffPatchType.CursorTo && last.Type == DiffPatchType.CursorTo)
                {
                    result[^1] = patch;
                    continue;
                }

                // Concat adjacent style patches
                if (patch.Type == DiffPatchType.StyleStr && last.Type == DiffPatchType.StyleStr)
                {
                    last.Content += patch.Content;
                    continue;
                }

                // Dedupe hyperlinks
                if (patch.Type == DiffPatchType.Hyperlink && last.Type == DiffPatchType.Hyperlink && patch.Uri == last.Uri)
                    continue;

                // Cancel cursor hide/show pairs
                if ((patch.Type == DiffPatchType.CursorShow && last.Type == DiffPatchType.CursorHide) ||
                    (patch.Type == DiffPatchType.CursorHide && last.Type == DiffPatchType.CursorShow))
                {
                    result.RemoveAt(result.Count - 1);
                    continue;
                }
            }

            result.Add(patch);
        }

        return result;
    }
}
