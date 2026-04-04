// -----------------------------------------------------------------------
// <copyright file="TerminalUtils.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) utils.ts
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Terminal;

/// <summary>
/// Terminal utility functions.
/// <para>1:1 port of Ink JS <c>utils.ts</c>.</para>
/// </summary>
public static class TerminalUtils
{
    /// <summary>
    /// Get the effective terminal dimensions.
    /// <para>
    /// Falls back to standard defaults (80×24) when dimensions cannot be determined.
    /// Corresponds to JS <c>getWindowSize(stdout)</c>.
    /// </para>
    /// </summary>
    public static (int Columns, int Rows) GetWindowSize()
    {
        try
        {
            int columns = Console.WindowWidth;
            int rows = Console.WindowHeight;

            if (columns > 0 && rows > 0)
                return (columns, rows);
        }
        catch
        {
            // Console may not be available (e.g. in tests or piped mode)
        }

        return (80, 24);
    }
}
