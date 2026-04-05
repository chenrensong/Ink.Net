// -----------------------------------------------------------------------
// <copyright file="LogUpdate.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) log-update.ts
//   Incremental terminal refresh: erase + rewrite.
// </copyright>
// -----------------------------------------------------------------------

using System.Text;

namespace Ink.Net.Terminal;

/// <summary>
/// Options for creating a <see cref="LogUpdate"/> instance.
/// </summary>
public sealed class LogUpdateOptions
{
    /// <summary>Whether to show the cursor. Default false (cursor will be hidden).</summary>
    public bool ShowCursor { get; init; }

    /// <summary>Whether to use incremental rendering (diff-based). Default false (standard erase-all).</summary>
    public bool Incremental { get; init; }

    /// <summary>Whether to use synchronized output (BSU/ESU). Default false.</summary>
    public bool Synchronize { get; init; }
}

/// <summary>
/// Terminal log update manager — erases and rewrites output efficiently.
/// <para>1:1 port of Ink JS <c>log-update.ts</c>.</para>
/// <para>
/// Supports both "standard" (erase-all) and "incremental" (line-diff) modes.
/// </para>
/// </summary>
public sealed class LogUpdate
{
    private readonly TextWriter _stream;
    private readonly bool _showCursor;
    private readonly bool _incremental;
    private readonly bool _synchronize;

    // Standard mode state
    private int _previousLineCount;
    private string _previousOutput = "";
    private bool _hasHiddenCursor;
    private CursorPosition? _cursorPosition;
    private bool _cursorDirty;
    private CursorPosition? _previousCursorPosition;
    private bool _cursorWasShown;

    // Incremental mode state (extra)
    private string[] _previousLines = [];

    private LogUpdate(TextWriter stream, bool showCursor, bool incremental, bool synchronize)
    {
        _stream = stream;
        _showCursor = showCursor;
        _incremental = incremental;
        _synchronize = synchronize;
    }

    /// <summary>
    /// Create a new LogUpdate instance.
    /// <para>Corresponds to JS <c>logUpdate.create(stream, opts)</c>.</para>
    /// </summary>
    public static LogUpdate Create(TextWriter stream, LogUpdateOptions? options = null)
    {
        options ??= new LogUpdateOptions();
        return new LogUpdate(stream, options.ShowCursor, options.Incremental, options.Synchronize);
    }

    // ─── Helpers ─────────────────────────────────────────────────────

    private static int VisibleLineCount(string[] lines, string str)
        => str.EndsWith('\n') ? lines.Length - 1 : lines.Length;

    private CursorPosition? GetActiveCursor()
        => _cursorDirty ? _cursorPosition : null;

    private bool HasChanges(string str, CursorPosition? activeCursor)
    {
        bool cursorChanged = CursorHelpers.CursorPositionChanged(activeCursor, _previousCursorPosition);
        return str != _previousOutput || cursorChanged;
    }

    // ─── Public API ──────────────────────────────────────────────────

    /// <summary>
    /// Render output to the stream. Returns true if something was written.
    /// <para>Corresponds to JS <c>render(str)</c>.</para>
    /// </summary>
    public bool Render(string str)
    {
        return _incremental ? RenderIncremental(str) : RenderStandard(str);
    }

    /// <summary>
    /// Clear the current output from the terminal.
    /// <para>Corresponds to JS <c>render.clear()</c>.</para>
    /// </summary>
    public void Clear()
    {
        string prefix = CursorHelpers.BuildReturnToBottomPrefix(
            _cursorWasShown,
            _incremental ? _previousLines.Length : _previousLineCount,
            _previousCursorPosition);

        _stream.Write(prefix + CursorHelpers.EraseLines(
            _incremental ? _previousLines.Length : _previousLineCount));

        _previousOutput = "";
        _previousLineCount = 0;
        _previousLines = [];
        _previousCursorPosition = null;
        _cursorWasShown = false;
    }

    /// <summary>
    /// Signal that rendering is done — restore cursor if needed.
    /// <para>Corresponds to JS <c>render.done()</c>.</para>
    /// </summary>
    public void Done()
    {
        _previousOutput = "";
        _previousLineCount = 0;
        _previousLines = [];
        _previousCursorPosition = null;
        _cursorWasShown = false;

        if (!_showCursor)
        {
            _stream.Write(CursorHelpers.ShowCursorEscape);
            _hasHiddenCursor = false;
        }
    }

    /// <summary>
    /// Reset internal state without writing to stream.
    /// <para>Corresponds to JS <c>render.reset()</c>.</para>
    /// </summary>
    public void Reset()
    {
        _previousOutput = "";
        _previousLineCount = 0;
        _previousLines = [];
        _previousCursorPosition = null;
        _cursorWasShown = false;
    }

    /// <summary>
    /// Synchronize cursor state after external writes.
    /// <para>Corresponds to JS <c>render.sync(str)</c>.</para>
    /// </summary>
    public void Sync(string str)
    {
        var activeCursor = _cursorDirty ? _cursorPosition : null;
        _cursorDirty = false;

        var lines = str.Split('\n');
        _previousOutput = str;
        _previousLineCount = lines.Length;
        _previousLines = lines;

        if (activeCursor is null && _cursorWasShown)
        {
            _stream.Write(CursorHelpers.HideCursorEscape);
        }

        if (activeCursor is not null)
        {
            _stream.Write(CursorHelpers.BuildCursorSuffix(
                VisibleLineCount(lines, str), activeCursor));
        }

        _previousCursorPosition = activeCursor;
        _cursorWasShown = activeCursor is not null;
    }

    /// <summary>
    /// Set the cursor position for the next render.
    /// <para>Corresponds to JS <c>render.setCursorPosition(position)</c>.</para>
    /// </summary>
    public void SetCursorPosition(CursorPosition? position)
    {
        _cursorPosition = position;
        _cursorDirty = true;
    }

    /// <summary>
    /// Whether the cursor position has been updated since last render.
    /// <para>Corresponds to JS <c>render.isCursorDirty()</c>.</para>
    /// </summary>
    public bool IsCursorDirty() => _cursorDirty;

    /// <summary>
    /// Whether the next render will actually write something.
    /// <para>Corresponds to JS <c>render.willRender(str)</c>.</para>
    /// </summary>
    public bool WillRender(string str) => HasChanges(str, GetActiveCursor());

    // ─── Standard rendering ──────────────────────────────────────────

    private bool RenderStandard(string str)
    {
        if (!_showCursor && !_hasHiddenCursor)
        {
            _stream.Write(CursorHelpers.HideCursorEscape);
            _hasHiddenCursor = true;
        }

        var activeCursor = GetActiveCursor();
        _cursorDirty = false;
        bool cursorChanged = CursorHelpers.CursorPositionChanged(activeCursor, _previousCursorPosition);

        if (!HasChanges(str, activeCursor))
            return false;

        var lines = str.Split('\n');
        int visibleCount = VisibleLineCount(lines, str);
        string cursorSuffix = CursorHelpers.BuildCursorSuffix(visibleCount, activeCursor);

        if (str == _previousOutput && cursorChanged)
        {
            if (_synchronize) _stream.Write(SynchronizedWrite.Bsu);
            _stream.Write(CursorHelpers.BuildCursorOnlySequence(new CursorOnlyInput
            {
                CursorWasShown = _cursorWasShown,
                PreviousLineCount = _previousLineCount,
                PreviousCursorPosition = _previousCursorPosition,
                VisibleLineCount = visibleCount,
                CurrentCursorPosition = activeCursor,
            }));
            if (_synchronize) _stream.Write(SynchronizedWrite.Esu);
        }
        else
        {
            _previousOutput = str;
            string returnPrefix = CursorHelpers.BuildReturnToBottomPrefix(
                _cursorWasShown, _previousLineCount, _previousCursorPosition);
            if (_synchronize) _stream.Write(SynchronizedWrite.Bsu);
            _stream.Write(
                returnPrefix +
                CursorHelpers.EraseLines(_previousLineCount) +
                str +
                cursorSuffix);
            if (_synchronize) _stream.Write(SynchronizedWrite.Esu);
            _previousLineCount = lines.Length;
        }

        _previousCursorPosition = activeCursor;
        _cursorWasShown = activeCursor is not null;
        return true;
    }

    // ─── Incremental rendering ───────────────────────────────────────

    private bool RenderIncremental(string str)
    {
        if (!_showCursor && !_hasHiddenCursor)
        {
            _stream.Write(CursorHelpers.HideCursorEscape);
            _hasHiddenCursor = true;
        }

        var activeCursor = GetActiveCursor();
        _cursorDirty = false;
        bool cursorChanged = CursorHelpers.CursorPositionChanged(activeCursor, _previousCursorPosition);

        if (!HasChanges(str, activeCursor))
            return false;

        var nextLines = str.Split('\n');
        int visibleCount = VisibleLineCount(nextLines, str);
        int previousVisible = VisibleLineCount(_previousLines, _previousOutput);

        if (str == _previousOutput && cursorChanged)
        {
            if (_synchronize) _stream.Write(SynchronizedWrite.Bsu);
            _stream.Write(CursorHelpers.BuildCursorOnlySequence(new CursorOnlyInput
            {
                CursorWasShown = _cursorWasShown,
                PreviousLineCount = _previousLines.Length,
                PreviousCursorPosition = _previousCursorPosition,
                VisibleLineCount = visibleCount,
                CurrentCursorPosition = activeCursor,
            }));
            if (_synchronize) _stream.Write(SynchronizedWrite.Esu);

            _previousCursorPosition = activeCursor;
            _cursorWasShown = activeCursor is not null;
            return true;
        }

        string returnPrefix = CursorHelpers.BuildReturnToBottomPrefix(
            _cursorWasShown, _previousLines.Length, _previousCursorPosition);

        if (str == "\n" || _previousOutput.Length == 0)
        {
            string cursorSuffix = CursorHelpers.BuildCursorSuffix(visibleCount, activeCursor);
            if (_synchronize) _stream.Write(SynchronizedWrite.Bsu);
            _stream.Write(
                returnPrefix +
                CursorHelpers.EraseLines(_previousLines.Length) +
                str +
                cursorSuffix);
            if (_synchronize) _stream.Write(SynchronizedWrite.Esu);

            _cursorWasShown = activeCursor is not null;
            _previousCursorPosition = activeCursor;
            _previousOutput = str;
            _previousLines = nextLines;
            return true;
        }

        bool hasTrailingNewline = str.EndsWith('\n');
        var buffer = new StringBuilder();
        buffer.Append(returnPrefix);

        // Clear extra lines if current content is shorter
        if (visibleCount < previousVisible)
        {
            bool previousHadTrailingNewline = _previousOutput.EndsWith('\n');
            int extraSlot = previousHadTrailingNewline ? 1 : 0;
            buffer.Append(CursorHelpers.EraseLines(previousVisible - visibleCount + extraSlot));
            buffer.Append(CursorHelpers.CursorUp(visibleCount));
        }
        else
        {
            buffer.Append(CursorHelpers.CursorUp(_previousLines.Length - 1));
        }

        for (int i = 0; i < visibleCount; i++)
        {
            bool isLastLine = i == visibleCount - 1;

            if (i < _previousLines.Length && nextLines[i] == _previousLines[i])
            {
                if (!isLastLine || hasTrailingNewline)
                    buffer.Append("\u001B[E"); // cursorNextLine
                continue;
            }

            buffer.Append(CursorHelpers.CursorTo(0));
            buffer.Append(nextLines[i]);
            buffer.Append("\u001B[K"); // eraseEndLine
            if (!isLastLine || hasTrailingNewline)
                buffer.Append('\n');
        }

        buffer.Append(CursorHelpers.BuildCursorSuffix(visibleCount, activeCursor));
        if (_synchronize) _stream.Write(SynchronizedWrite.Bsu);
        _stream.Write(buffer.ToString());
        if (_synchronize) _stream.Write(SynchronizedWrite.Esu);

        _cursorWasShown = activeCursor is not null;
        _previousCursorPosition = activeCursor;
        _previousOutput = str;
        _previousLines = nextLines;
        return true;
    }
}
