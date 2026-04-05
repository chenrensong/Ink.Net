// -----------------------------------------------------------------------
// ConsoleRawMode.cs — Cross-platform raw-mode / VT-input console setup
//
// JS Ink calls process.stdin.setRawMode(true) via Node.js to put the TTY
// in raw mode so every keypress (incl. arrow keys) is delivered immediately
// as VT escape sequences.  C# needs to do the same thing manually:
//
// Windows  — SetConsoleMode (kernel32.dll)
//   Disable ENABLE_LINE_INPUT             → no line-buffering
//   Disable ENABLE_ECHO_INPUT             → Ink owns all display
//   Enable  ENABLE_VIRTUAL_TERMINAL_INPUT → arrow keys → ESC[A/B/C/D
//   Keep    ENABLE_PROCESSED_INPUT        → Ctrl+C still fires CancelKeyPress
//
// Linux / macOS — tcsetattr (libc)
//   Clear ICANON   → disable canonical (line-buffered) mode
//   Clear ECHO     → no local echo
//   Keep  ISIG     → Ctrl+C still delivers SIGINT
//
// Note: ICANON flag value differs per platform:
//   Linux  c_lflag bit: 0x0002
//   macOS  c_lflag bit: 0x0100
// -----------------------------------------------------------------------

using System.Runtime.InteropServices;

namespace Ink.Net.Terminal;

/// <summary>
/// Sets the console to raw / VT-input mode so that arrow keys and other
/// special keys are delivered as VT escape sequences rather than being
/// consumed by the line-editor.
/// <para>
/// Call <see cref="Enter"/> to activate and <see cref="Restore"/> (or
/// <see cref="Dispose"/>) to return to the original mode.
/// </para>
/// <para>Safe to call when stdin is redirected / non-TTY — silently ignored.</para>
/// </summary>
public sealed class ConsoleRawMode : IDisposable
{
    // ═══════════════════════════════════════════════════════════════════
    // Windows — kernel32.dll
    // ═══════════════════════════════════════════════════════════════════

    private const int  Win_STD_INPUT_HANDLE              = -10;
    private const uint Win_ENABLE_LINE_INPUT             = 0x0002;
    private const uint Win_ENABLE_ECHO_INPUT             = 0x0004;
    private const uint Win_ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200;
    // ENABLE_PROCESSED_INPUT (0x0001) intentionally kept so Ctrl+C → CancelKeyPress

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    // ═══════════════════════════════════════════════════════════════════
    // Unix — libc (Linux + macOS)
    //
    // termios struct layout (c_lflag is at offset 12 on both platforms):
    //   Linux x86_64 : c_iflag(4)+c_oflag(4)+c_cflag(4)+c_lflag(4)+c_line(1)+c_cc[32]+pad(3)+speeds(8) = 60 bytes
    //   macOS arm64  : c_iflag(4)+c_oflag(4)+c_cflag(4)+c_lflag(4)+c_cc[20]+speeds(8)                  = 44 bytes
    // We use Size=64 to safely cover both; only c_lflag (offset 12) is accessed.
    // ═══════════════════════════════════════════════════════════════════

    [StructLayout(LayoutKind.Explicit, Size = 64)]
    private struct Termios
    {
        [FieldOffset(0)]  public uint c_iflag;
        [FieldOffset(4)]  public uint c_oflag;
        [FieldOffset(8)]  public uint c_cflag;
        [FieldOffset(12)] public uint c_lflag;  // ← only field we modify
        // remaining bytes (c_line, c_cc[], speeds) are not accessed
    }

    // ICANON flag differs between Linux and macOS:
    private static readonly uint Unix_ICANON =
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 0x0100u : 0x0002u;
    private const uint Unix_ECHO   = 0x0008; // same on both Linux and macOS
    // ISIG (Ctrl+C → SIGINT) intentionally kept

    private const int Unix_STDIN_FD = 0;
    private const int Unix_TCSANOW  = 0;

    [DllImport("libc", EntryPoint = "tcgetattr", SetLastError = true)]
    private static extern int tcgetattr(int fd, out Termios termios);

    [DllImport("libc", EntryPoint = "tcsetattr", SetLastError = true)]
    private static extern int tcsetattr(int fd, int optionalActions, ref Termios termios);

    // ═══════════════════════════════════════════════════════════════════
    // Shared state
    // ═══════════════════════════════════════════════════════════════════

    private uint    _savedWinMode;
    private Termios _savedUnixTermios;
    private bool    _active;
    private bool    _disposed;

    // ═══════════════════════════════════════════════════════════════════
    // Public API
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Activate raw VT-input mode.  On Windows: disables line-buffering,
    /// enables VT sequences.  On Linux/macOS: disables canonical mode and echo.
    /// </summary>
    public void Enter()
    {
        if (_active) return;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            EnterWindows();
        else
            EnterUnix();
    }

    /// <summary>
    /// Restore the console input mode saved by <see cref="Enter"/>.
    /// Safe to call even if <see cref="Enter"/> was never called or failed.
    /// </summary>
    public void Restore()
    {
        if (!_active) return;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            RestoreWindows();
        else
            RestoreUnix();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Restore();
    }

    // ═══════════════════════════════════════════════════════════════════
    // Windows implementation
    // ═══════════════════════════════════════════════════════════════════

    private void EnterWindows()
    {
        try
        {
            var handle = GetStdHandle(Win_STD_INPUT_HANDLE);
            if (!GetConsoleMode(handle, out uint mode)) return;

            _savedWinMode = mode;
            uint newMode = (mode & ~(Win_ENABLE_LINE_INPUT | Win_ENABLE_ECHO_INPUT))
                           | Win_ENABLE_VIRTUAL_TERMINAL_INPUT;

            if (SetConsoleMode(handle, newMode))
                _active = true;
        }
        catch { /* not a real console (redirected/test) — ignore */ }
    }

    private void RestoreWindows()
    {
        try
        {
            var handle = GetStdHandle(Win_STD_INPUT_HANDLE);
            SetConsoleMode(handle, _savedWinMode);
        }
        catch { }
        finally { _active = false; }
    }

    // ═══════════════════════════════════════════════════════════════════
    // Unix implementation  (Linux + macOS)
    // ═══════════════════════════════════════════════════════════════════

    private void EnterUnix()
    {
        try
        {
            if (tcgetattr(Unix_STDIN_FD, out _savedUnixTermios) != 0) return;

            var raw = _savedUnixTermios;
            // Disable canonical mode (no line-buffering) and local echo.
            // ISIG is kept so Ctrl+C still delivers SIGINT.
            raw.c_lflag &= ~(Unix_ICANON | Unix_ECHO);

            if (tcsetattr(Unix_STDIN_FD, Unix_TCSANOW, ref raw) == 0)
                _active = true;
        }
        catch { /* not a TTY (redirected/test) — ignore */ }
    }

    private void RestoreUnix()
    {
        try
        {
            tcsetattr(Unix_STDIN_FD, Unix_TCSANOW, ref _savedUnixTermios);
        }
        catch { }
        finally { _active = false; }
    }
}
