// -----------------------------------------------------------------------
// <copyright file="AlternateScreen.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) ink.tsx alternate screen support.
//   Manages terminal alternate screen buffer (used by vim, htop, less, etc.)
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Terminal;

/// <summary>
/// Manages the terminal's alternate screen buffer.
/// <para>
/// When enabled, the application renders on a separate screen and the original
/// terminal content is restored when the application exits. This is the same
/// mechanism used by programs like vim, htop, and less.
/// </para>
/// <para>
/// 1:1 port of the alternate screen logic in Ink JS <c>ink.tsx</c>.
/// </para>
/// </summary>
public sealed class AlternateScreen : IDisposable
{
    // ANSI escape sequences for alternate screen buffer
    // ESC[?1049h — enter alternate screen buffer
    // ESC[?1049l — exit alternate screen buffer
    /// <summary>ANSI escape to enter the alternate screen buffer.</summary>
    public const string EnterAlternateScreenEscape = "\u001B[?1049h";

    /// <summary>ANSI escape to exit the alternate screen buffer.</summary>
    public const string ExitAlternateScreenEscape = "\u001B[?1049l";

    /// <summary>ANSI escape to clear the entire terminal.</summary>
    public const string ClearTerminalEscape = "\u001Bc";

    private readonly TextWriter _stream;
    private bool _isActive;
    private bool _disposed;

    /// <summary>
    /// Creates a new <see cref="AlternateScreen"/> manager.
    /// </summary>
    /// <param name="stream">The stdout stream to write escape sequences to.</param>
    public AlternateScreen(TextWriter stream)
    {
        _stream = stream;
    }

    /// <summary>
    /// Whether the alternate screen is currently active.
    /// </summary>
    public bool IsActive => _isActive;

    /// <summary>
    /// Enter the alternate screen buffer.
    /// <para>Corresponds to JS <c>ansiEscapes.enterAlternativeScreen</c>.</para>
    /// </summary>
    public void Enter()
    {
        if (_isActive || _disposed) return;

        WriteBestEffort(EnterAlternateScreenEscape);
        WriteBestEffort(CursorHelpers.HideCursorEscape);
        _isActive = true;
    }

    /// <summary>
    /// Exit the alternate screen buffer, restoring the original terminal content.
    /// <para>Corresponds to JS <c>ansiEscapes.exitAlternativeScreen</c>.</para>
    /// </summary>
    public void Exit()
    {
        if (!_isActive || _disposed) return;

        WriteBestEffort(ExitAlternateScreenEscape);
        WriteBestEffort(CursorHelpers.ShowCursorEscape);
        _isActive = false;
    }

    /// <summary>
    /// Resolve whether alternate screen should be enabled based on options and environment.
    /// <para>
    /// Alternate screen only works in interactive mode with a TTY.
    /// Corresponds to JS <c>resolveAlternateScreenOption</c>.
    /// </para>
    /// </summary>
    /// <param name="requested">Whether alternate screen was requested by the user.</param>
    /// <param name="interactive">Whether the application is running in interactive mode.</param>
    /// <returns>True if alternate screen should be enabled.</returns>
    public static bool ShouldEnable(bool requested, bool interactive)
    {
        return requested && interactive;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;

        // Must exit before setting _disposed, because Exit() checks _disposed
        if (_isActive)
        {
            WriteBestEffort(ExitAlternateScreenEscape);
            WriteBestEffort(CursorHelpers.ShowCursorEscape);
            _isActive = false;
        }

        _disposed = true;
    }

    /// <summary>
    /// Best-effort write: streams may already be destroyed during shutdown.
    /// <para>Corresponds to JS <c>writeBestEffort</c> in <c>ink.tsx</c>.</para>
    /// </summary>
    private void WriteBestEffort(string data)
    {
        try
        {
            _stream.Write(data);
        }
        catch
        {
            // Best-effort: swallow errors during shutdown
        }
    }
}
