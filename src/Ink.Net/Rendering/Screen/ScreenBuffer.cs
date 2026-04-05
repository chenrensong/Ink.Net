// -----------------------------------------------------------------------
// <copyright file="ScreenBuffer.cs" company="Ink.Net">
//   Port from Ink (JS) screen.ts — Packed cell-based screen buffer
// </copyright>
// -----------------------------------------------------------------------

using System.Runtime.CompilerServices;

namespace Ink.Net.Rendering.Screen;

/// <summary>
/// A view of a single cell in the screen buffer.
/// </summary>
public struct Cell
{
    public string Char;
    public int StyleId;
    public CellWidth Width;
    public string? Hyperlink;
}

/// <summary>
/// Rectangle for damage tracking.
/// </summary>
public readonly record struct ScreenRect(int X, int Y, int Width, int Height);

/// <summary>
/// Packed cell-based screen buffer. Each cell is stored as 2 Int32s in a contiguous array
/// to minimize GC pressure.
/// <para>
/// Layout: word0 = charId, word1 = styleId[31:17] | hyperlinkId[16:2] | width[1:0]
/// </para>
/// </summary>
public sealed class ScreenBuffer
{
    private const int StyleShift = 17;
    private const int HyperlinkShift = 2;
    private const int HyperlinkMask = 0x7FFF; // 15 bits
    private const int WidthMask = 3; // 2 bits

    public int Width { get; }
    public int Height { get; }

    /// <summary>Packed cell data — 2 Int32s per cell.</summary>
    internal int[] Cells { get; }

    /// <summary>Shared character pool.</summary>
    public CharPool CharPool { get; }

    /// <summary>Shared hyperlink pool.</summary>
    public HyperlinkPool HyperlinkPool { get; }

    /// <summary>Empty style ID.</summary>
    public int EmptyStyleId { get; }

    /// <summary>Bounding box of cells written this frame (damage region).</summary>
    public ScreenRect? Damage { get; set; }

    /// <summary>Per-cell noSelect bitmap. 1 = exclude from selection.</summary>
    public byte[] NoSelect { get; }

    /// <summary>Per-row soft-wrap continuation marker.</summary>
    public int[] SoftWrap { get; }

    public ScreenBuffer(int width, int height, CharPool charPool, HyperlinkPool hyperlinkPool, int emptyStyleId)
    {
        Width = width;
        Height = height;
        CharPool = charPool;
        HyperlinkPool = hyperlinkPool;
        EmptyStyleId = emptyStyleId;
        Cells = new int[width * height * 2];
        NoSelect = new byte[width * height];
        SoftWrap = new int[height];
    }

    /// <summary>Reset all cells to empty.</summary>
    public void Reset()
    {
        Array.Clear(Cells);
        Array.Clear(NoSelect);
        Array.Clear(SoftWrap);
        Damage = null;
    }

    /// <summary>Set a cell at (x, y).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetCell(int x, int y, int charId, int styleId, CellWidth width, int hyperlinkId = 0)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return;
        int ci = (y * Width + x) << 1;
        Cells[ci] = charId;
        Cells[ci + 1] = (styleId << StyleShift) | (hyperlinkId << HyperlinkShift) | (int)width;
        TrackDamage(x, y);
    }

    /// <summary>Get a cell at (x, y).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Cell GetCell(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return new Cell { Char = " ", StyleId = EmptyStyleId, Width = CellWidth.Narrow };

        int ci = (y * Width + x) << 1;
        int word0 = Cells[ci];
        int word1 = Cells[ci + 1];

        return new Cell
        {
            Char = CharPool.Get(word0),
            StyleId = word1 >> StyleShift,
            Width = (CellWidth)(word1 & WidthMask),
            Hyperlink = HyperlinkPool.Get((word1 >> HyperlinkShift) & HyperlinkMask),
        };
    }

    /// <summary>Check if a cell is empty (never written).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsEmpty(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return true;
        int ci = (y * Width + x) << 1;
        return Cells[ci] == 0 && Cells[ci + 1] == 0;
    }

    /// <summary>Mark a cell as no-select.</summary>
    public void MarkNoSelect(int x, int y, int width, int height)
    {
        int maxX = Math.Min(x + width, Width);
        int maxY = Math.Min(y + height, Height);
        for (int row = Math.Max(0, y); row < maxY; row++)
        {
            int rowStart = row * Width;
            for (int col = Math.Max(0, x); col < maxX; col++)
                NoSelect[rowStart + col] = 1;
        }
    }

    /// <summary>Clear a rectangular region to empty cells.</summary>
    public void ClearRegion(int x, int y, int width, int height)
    {
        int maxX = Math.Min(x + width, Width);
        int maxY = Math.Min(y + height, Height);
        for (int row = Math.Max(0, y); row < maxY; row++)
        {
            int rowCI = (row * Width + Math.Max(0, x)) << 1;
            int count = (maxX - Math.Max(0, x)) << 1;
            Array.Clear(Cells, rowCI, count);
        }
    }

    /// <summary>
    /// Copy a rectangular region from another screen buffer (blit).
    /// </summary>
    public void BlitFrom(ScreenBuffer source, int srcX, int srcY, int dstX, int dstY, int width, int height)
    {
        for (int row = 0; row < height; row++)
        {
            int sy = srcY + row;
            int dy = dstY + row;
            if (sy < 0 || sy >= source.Height || dy < 0 || dy >= Height) continue;

            int sx = Math.Max(0, srcX);
            int dx = Math.Max(0, dstX);
            int copyWidth = Math.Min(width, Math.Min(source.Width - sx, Width - dx));
            if (copyWidth <= 0) continue;

            int srcCI = (sy * source.Width + sx) << 1;
            int dstCI = (dy * Width + dx) << 1;
            Array.Copy(source.Cells, srcCI, Cells, dstCI, copyWidth << 1);

            // Copy noSelect
            Array.Copy(source.NoSelect, sy * source.Width + sx, NoSelect, dy * Width + dx, copyWidth);
        }
    }

    /// <summary>Shift rows up by n (for scrolling).</summary>
    public void ShiftRowsUp(int count)
    {
        if (count <= 0 || count >= Height) { Reset(); return; }

        int cellsToCopy = (Height - count) * Width * 2;
        Array.Copy(Cells, count * Width * 2, Cells, 0, cellsToCopy);
        Array.Clear(Cells, cellsToCopy, count * Width * 2);

        int noSelToCopy = (Height - count) * Width;
        Array.Copy(NoSelect, count * Width, NoSelect, 0, noSelToCopy);
        Array.Clear(NoSelect, noSelToCopy, count * Width);

        Array.Copy(SoftWrap, count, SoftWrap, 0, Height - count);
        Array.Clear(SoftWrap, Height - count, count);
    }

    private void TrackDamage(int x, int y)
    {
        if (Damage is null)
        {
            Damage = new ScreenRect(x, y, 1, 1);
        }
        else
        {
            var d = Damage.Value;
            int minX = Math.Min(d.X, x);
            int minY = Math.Min(d.Y, y);
            int maxX = Math.Max(d.X + d.Width, x + 1);
            int maxY = Math.Max(d.Y + d.Height, y + 1);
            Damage = new ScreenRect(minX, minY, maxX - minX, maxY - minY);
        }
    }

    /// <summary>
    /// Diff this screen against a previous screen and invoke a callback for each changed cell.
    /// Returns true if callback returned true (early exit).
    /// </summary>
    public bool Diff(ScreenBuffer prev, Action<int, int, Cell?, Cell?> callback)
    {
        int maxH = Math.Max(prev.Height, Height);
        int maxW = Math.Max(prev.Width, Width);

        for (int y = 0; y < maxH; y++)
        {
            for (int x = 0; x < maxW; x++)
            {
                bool prevIn = y < prev.Height && x < prev.Width;
                bool nextIn = y < Height && x < Width;

                if (prevIn && nextIn)
                {
                    int prevCI = (y * prev.Width + x) << 1;
                    int nextCI = (y * Width + x) << 1;
                    if (prev.Cells[prevCI] == Cells[nextCI] && prev.Cells[prevCI + 1] == Cells[nextCI + 1])
                        continue;
                    callback(x, y, prev.GetCell(x, y), GetCell(x, y));
                }
                else if (prevIn)
                {
                    callback(x, y, prev.GetCell(x, y), null);
                }
                else if (nextIn)
                {
                    if (IsEmpty(x, y)) continue;
                    callback(x, y, null, GetCell(x, y));
                }
            }
        }
        return false;
    }
}
