// Tests ported from log-update.tsx
using Ink.Net.Terminal;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>LogUpdate tests aligned with JS test/log-update.tsx.</summary>
public class LogUpdateTests
{
    private static StringWriter CreateStream() => new();

    // ── Standard rendering ─────────────────────────────────────────

    [Fact]
    public void StandardRendering_RendersAndUpdatesOutput()
    {
        var stream = CreateStream();
        var render = LogUpdate.Create(stream, new LogUpdateOptions { ShowCursor = true });

        render.Render("Hello\n");
        var first = stream.ToString();
        Assert.Contains("Hello", first);

        render.Render("World\n");
        var all = stream.ToString();
        Assert.Contains("World", all);
    }

    [Fact]
    public void StandardRendering_SkipsIdenticalOutput()
    {
        var stream = CreateStream();
        var render = LogUpdate.Create(stream, new LogUpdateOptions { ShowCursor = true });

        render.Render("Hello\n");
        var afterFirst = stream.ToString();

        render.Render("Hello\n");
        var afterSecond = stream.ToString();

        // Should not write again for identical output
        Assert.Equal(afterFirst, afterSecond);
    }

    // ── Incremental rendering ──────────────────────────────────────

    [Fact]
    public void IncrementalRendering_RendersAndUpdatesOutput()
    {
        var stream = CreateStream();
        var render = LogUpdate.Create(stream, new LogUpdateOptions { ShowCursor = true, Incremental = true });

        render.Render("Hello\n");
        Assert.Contains("Hello", stream.ToString());

        render.Render("World\n");
        Assert.Contains("World", stream.ToString());
    }

    [Fact]
    public void IncrementalRendering_SkipsIdenticalOutput()
    {
        var stream = CreateStream();
        var render = LogUpdate.Create(stream, new LogUpdateOptions { ShowCursor = true, Incremental = true });

        render.Render("Hello\n");
        var afterFirst = stream.ToString();

        render.Render("Hello\n");
        var afterSecond = stream.ToString();

        Assert.Equal(afterFirst, afterSecond);
    }

    [Fact]
    public void IncrementalRendering_SurgicalUpdates()
    {
        var stream = CreateStream();
        var render = LogUpdate.Create(stream, new LogUpdateOptions { ShowCursor = true, Incremental = true });

        render.Render("Line 1\nLine 2\nLine 3\n");
        render.Render("Line 1\nUpdated\nLine 3\n");

        var output = stream.ToString();
        Assert.Contains("Updated", output);
        Assert.Contains("\u001B[E", output); // cursorNextLine (skips unchanged lines)
    }

    [Fact]
    public void IncrementalRendering_ClearsExtraLinesWhenOutputShrinks()
    {
        var stream = CreateStream();
        var render = LogUpdate.Create(stream, new LogUpdateOptions { ShowCursor = true, Incremental = true });

        render.Render("Line 1\nLine 2\nLine 3\n");
        render.Render("Line 1\n");

        var output = stream.ToString();
        // Should contain erase lines sequence
        Assert.Contains(CursorHelpers.EraseLines(2), output);
    }

    [Fact]
    public void IncrementalRendering_WhenOutputGrows()
    {
        var stream = CreateStream();
        var render = LogUpdate.Create(stream, new LogUpdateOptions { ShowCursor = true, Incremental = true });

        render.Render("Line 1\n");
        render.Render("Line 1\nLine 2\nLine 3\n");

        var output = stream.ToString();
        Assert.Contains("Line 2", output);
        Assert.Contains("Line 3", output);
    }

    // ── Clear and Done ─────────────────────────────────────────────

    [Fact]
    public void IncrementalRendering_ClearResetsState()
    {
        var stream = CreateStream();
        var render = LogUpdate.Create(stream, new LogUpdateOptions { ShowCursor = true, Incremental = true });

        render.Render("Line 1\nLine 2\nLine 3\n");
        render.Clear();

        // After clear, next render should be fresh
        var streamAfterClear = new StringWriter();
        var render2 = LogUpdate.Create(streamAfterClear, new LogUpdateOptions { ShowCursor = true, Incremental = true });
        render2.Render("Line 1\n");
        Assert.Contains("Line 1", streamAfterClear.ToString());
    }

    [Fact]
    public void IncrementalRendering_DoneResetsBeforeNextRender()
    {
        var stream = CreateStream();
        var render = LogUpdate.Create(stream, new LogUpdateOptions { ShowCursor = true, Incremental = true });

        render.Render("Line 1\nLine 2\nLine 3\n");
        render.Done();

        // After done, internal state is reset
        render.Render("Line 1\n");
        Assert.Contains("Line 1", stream.ToString());
    }

    // ── Cursor positioning ────────────────────────────────────────

    [Fact]
    public void StandardRendering_PositionsCursorAfterOutput()
    {
        var stream = CreateStream();
        var render = LogUpdate.Create(stream, new LogUpdateOptions { ShowCursor = true });

        render.SetCursorPosition(new CursorPosition(5, 1));
        render.Render("Line 1\nLine 2\nLine 3\n");

        var output = stream.ToString();
        Assert.Contains("Line 3", output);
        // Cursor should be positioned: cursorUp(2) + cursorTo(5) + showCursor
        Assert.Contains(CursorHelpers.CursorUp(2), output);
        Assert.Contains(CursorHelpers.CursorTo(5), output);
        Assert.Contains(CursorHelpers.ShowCursorEscape, output);
    }

    [Fact]
    public void StandardRendering_HidesCursorBeforeEraseWhenPreviouslyShown()
    {
        var stream = CreateStream();
        var render = LogUpdate.Create(stream, new LogUpdateOptions { ShowCursor = true });

        render.SetCursorPosition(new CursorPosition(0, 0));
        render.Render("Hello\n");
        render.SetCursorPosition(new CursorPosition(0, 0));
        render.Render("World\n");

        var output = stream.ToString();
        // Should contain hide cursor before the second render
        Assert.Contains(CursorHelpers.HideCursorEscape, output);
        // Should contain show cursor
        Assert.Contains(CursorHelpers.ShowCursorEscape, output);
    }

    [Fact]
    public void StandardRendering_NoCursorPositioningWhenUndefined()
    {
        var stream = CreateStream();
        var render = LogUpdate.Create(stream, new LogUpdateOptions { ShowCursor = true });

        render.Render("Hello\n");

        var output = stream.ToString();
        Assert.DoesNotContain(CursorHelpers.ShowCursorEscape, output);
    }

    [Fact]
    public void StandardRendering_ClearingCursorStopsPositioning()
    {
        var stream = CreateStream();
        var render = LogUpdate.Create(stream, new LogUpdateOptions { ShowCursor = true });

        render.SetCursorPosition(new CursorPosition(0, 0));
        render.Render("Hello\n");

        render.SetCursorPosition(null);
        var before = stream.ToString();
        render.Render("World\n");
        var after = stream.ToString();

        // The portion after the first render should not contain show cursor
        var secondPart = after.Substring(before.Length);
        Assert.DoesNotContain(CursorHelpers.ShowCursorEscape, secondPart);
    }

    [Fact]
    public void IncrementalRendering_PositionsCursorAfterSurgicalUpdates()
    {
        var stream = CreateStream();
        var render = LogUpdate.Create(stream, new LogUpdateOptions { ShowCursor = true, Incremental = true });

        render.SetCursorPosition(new CursorPosition(5, 1));
        render.Render("Line 1\nLine 2\nLine 3\n");

        var output = stream.ToString();
        Assert.EndsWith(
            CursorHelpers.CursorUp(2) + CursorHelpers.CursorTo(5) + CursorHelpers.ShowCursorEscape,
            output);
    }

    [Fact]
    public void StandardRendering_RepositionsCursorWhenOnlyPositionChanges()
    {
        var stream = CreateStream();
        var render = LogUpdate.Create(stream, new LogUpdateOptions { ShowCursor = true });

        render.SetCursorPosition(new CursorPosition(2, 0));
        render.Render("Hello\n");
        var afterFirst = stream.ToString();

        // Same output, different cursor position
        render.SetCursorPosition(new CursorPosition(3, 0));
        render.Render("Hello\n");
        var afterSecond = stream.ToString();

        // Should have written something new
        Assert.NotEqual(afterFirst, afterSecond);
        Assert.Contains(CursorHelpers.ShowCursorEscape, afterSecond);
    }

    // ── Sync ───────────────────────────────────────────────────────

    [Fact]
    public void StandardRendering_SyncWithoutCursorDoesNotWrite()
    {
        var stream = CreateStream();
        var render = LogUpdate.Create(stream, new LogUpdateOptions { ShowCursor = true });

        render.Sync("Line 1\nLine 2\nLine 3\n");

        // Should not write to stream (no cursor was set)
        Assert.Equal("", stream.ToString());
    }

    [Fact]
    public void StandardRendering_SyncResetsCursorState()
    {
        var stream = CreateStream();
        var render = LogUpdate.Create(stream, new LogUpdateOptions { ShowCursor = true });

        render.SetCursorPosition(new CursorPosition(5, 0));
        render.Render("Line 1\nLine 2\nLine 3\n");

        render.Sync("Fresh output\n");
        var beforeUpdate = stream.ToString();

        render.Render("Updated output\n");
        var afterUpdate = stream.ToString();
        var updatePart = afterUpdate.Substring(beforeUpdate.Length);

        // After sync, should NOT include hideCursor + cursorDown
        Assert.DoesNotContain(CursorHelpers.HideCursorEscape, updatePart);
    }

    [Fact]
    public void SyncWritesCursorSuffixWhenDirty()
    {
        var stream = CreateStream();
        var render = LogUpdate.Create(stream, new LogUpdateOptions { ShowCursor = true });

        render.SetCursorPosition(new CursorPosition(5, 1));
        render.Sync("Line 1\nLine 2\nLine 3\n");

        var output = stream.ToString();
        // 3 visible lines, cursor at y=1 → cursorUp(3-1)=cursorUp(2)
        Assert.Contains(CursorHelpers.CursorUp(2), output);
        Assert.Contains(CursorHelpers.CursorTo(5), output);
        Assert.Contains(CursorHelpers.ShowCursorEscape, output);
    }

    [Fact]
    public void SyncHidesCursorWhenPreviousRenderShowedCursor()
    {
        var stream = CreateStream();
        var render = LogUpdate.Create(stream, new LogUpdateOptions { ShowCursor = true });

        render.SetCursorPosition(new CursorPosition(5, 1));
        render.Render("Line 1\nLine 2\nLine 3\n");
        var afterFirst = stream.ToString();

        render.Sync("Fresh output\n");
        var afterSync = stream.ToString();
        var syncPart = afterSync.Substring(afterFirst.Length);

        Assert.Equal(CursorHelpers.HideCursorEscape, syncPart);
    }

    // ── No trailing newline ─────────────────────────────────────────

    [Fact]
    public void IncrementalRendering_NoTrailingNewline_Update()
    {
        var stream = CreateStream();
        var render = LogUpdate.Create(stream, new LogUpdateOptions { ShowCursor = true, Incremental = true });

        render.Render("A\nB");
        render.Render("A\nC");

        var output = stream.ToString();
        Assert.Contains("C", output);
        Assert.Contains("\u001B[E", output); // cursorNextLine for skipping A
    }

    [Fact]
    public void IncrementalRendering_NoTrailingNewline_Shrink()
    {
        var stream = CreateStream();
        var render = LogUpdate.Create(stream, new LogUpdateOptions { ShowCursor = true, Incremental = true });

        render.Render("A\nB");
        render.Render("A");

        var output = stream.ToString();
        Assert.Contains(CursorHelpers.EraseLines(1), output);
    }

    [Fact]
    public void IncrementalRendering_NoTrailingNewline_Grow()
    {
        var stream = CreateStream();
        var render = LogUpdate.Create(stream, new LogUpdateOptions { ShowCursor = true, Incremental = true });

        render.Render("A");
        render.Render("A\nB\nC");

        var output = stream.ToString();
        Assert.Contains("B", output);
        Assert.Contains("C", output);
    }

    [Fact]
    public void IncrementalRendering_IdenticalContentIsSkipped()
    {
        var stream = CreateStream();
        var render = LogUpdate.Create(stream, new LogUpdateOptions { ShowCursor = true, Incremental = true });

        render.Render("A\nB");
        var after1 = stream.ToString();
        render.Render("A\nB");
        var after2 = stream.ToString();

        Assert.Equal(after1, after2);
    }

    [Fact]
    public void IncrementalRendering_RenderToEmptyString()
    {
        var stream = CreateStream();
        var render = LogUpdate.Create(stream, new LogUpdateOptions { ShowCursor = true, Incremental = true });

        render.Render("Line 1\nLine 2\nLine 3\n");
        render.Render("\n");

        var output = stream.ToString();
        // Should erase all lines
        Assert.Contains(CursorHelpers.EraseLines(4), output);
    }
}
