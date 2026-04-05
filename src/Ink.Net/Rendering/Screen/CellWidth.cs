// -----------------------------------------------------------------------
// <copyright file="CellWidth.cs" company="Ink.Net">
//   Port from Ink (JS) screen.ts — Cell width classification
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Rendering.Screen;

/// <summary>
/// Cell width classification for handling double-wide characters (CJK, emoji).
/// </summary>
public enum CellWidth : byte
{
    /// <summary>Normal width character, cell width 1.</summary>
    Narrow = 0,
    /// <summary>Wide character, cell width 2. This cell contains the actual character.</summary>
    Wide = 1,
    /// <summary>Spacer occupying the second visual column of a wide character.</summary>
    SpacerTail = 2,
    /// <summary>Spacer at end of soft-wrapped line where a wide char continues on next line.</summary>
    SpacerHead = 3,
}
