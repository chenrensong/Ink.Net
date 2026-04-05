// -----------------------------------------------------------------------
// <copyright file="Geometry.cs" company="Ink.Net">
//   Port from Ink (JS) layout/geometry.ts — Point, Size, Rectangle types
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Rendering;

/// <summary>A 2D point.</summary>
public readonly record struct Point(int X, int Y);

/// <summary>A 2D size.</summary>
public readonly record struct Size(int Width, int Height);

/// <summary>A rectangle defined by position and size.</summary>
public readonly record struct Rectangle(int X, int Y, int Width, int Height)
{
    /// <summary>Right edge (exclusive).</summary>
    public int Right => X + Width;

    /// <summary>Bottom edge (exclusive).</summary>
    public int Bottom => Y + Height;

    /// <summary>Check if this rectangle contains a point.</summary>
    public bool Contains(int x, int y) => x >= X && x < Right && y >= Y && y < Bottom;

    /// <summary>Check if this rectangle intersects another.</summary>
    public bool Intersects(Rectangle other) =>
        X < other.Right && Right > other.X && Y < other.Bottom && Bottom > other.Y;

    /// <summary>Compute the union of two rectangles.</summary>
    public static Rectangle Union(Rectangle a, Rectangle b)
    {
        int x = Math.Min(a.X, b.X);
        int y = Math.Min(a.Y, b.Y);
        int right = Math.Max(a.Right, b.Right);
        int bottom = Math.Max(a.Bottom, b.Bottom);
        return new Rectangle(x, y, right - x, bottom - y);
    }

    /// <summary>Compute the intersection of two rectangles (may be empty).</summary>
    public static Rectangle Intersect(Rectangle a, Rectangle b)
    {
        int x = Math.Max(a.X, b.X);
        int y = Math.Max(a.Y, b.Y);
        int right = Math.Min(a.Right, b.Right);
        int bottom = Math.Min(a.Bottom, b.Bottom);
        if (right <= x || bottom <= y) return new Rectangle(0, 0, 0, 0);
        return new Rectangle(x, y, right - x, bottom - y);
    }

    /// <summary>Clamp a value between min and max.</summary>
    public static int Clamp(int value, int min, int max) => Math.Max(min, Math.Min(max, value));
}
