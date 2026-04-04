// Tests ported from cursor.tsx
// Covers: CursorManager — position setting, events, hide
using Ink.Net.Terminal;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Cursor management tests aligned with JS cursor.tsx test suite.</summary>
public class CursorManagerTests
{
    [Fact]
    public void InitialPositionIsNull()
    {
        var cm = new CursorManager();
        Assert.Null(cm.Position);
    }

    [Fact]
    public void SetCursorPositionUpdatesPosition()
    {
        var cm = new CursorManager();
        cm.SetCursorPosition(5, 0);

        Assert.NotNull(cm.Position);
        Assert.Equal(5, cm.Position!.Value.X);
        Assert.Equal(0, cm.Position!.Value.Y);
    }

    [Fact]
    public void SetCursorPositionWithStruct()
    {
        var cm = new CursorManager();
        cm.SetCursorPosition(new CursorPosition(3, 2));

        Assert.Equal(new CursorPosition(3, 2), cm.Position);
    }

    [Fact]
    public void HideCursorClearsPosition()
    {
        var cm = new CursorManager();
        cm.SetCursorPosition(5, 0);
        cm.HideCursor();

        Assert.Null(cm.Position);
    }

    [Fact]
    public void CursorPositionChangedEventFires()
    {
        var cm = new CursorManager();
        CursorPosition? received = null;
        cm.CursorPositionChanged += pos => received = pos;

        cm.SetCursorPosition(2, 0);

        Assert.NotNull(received);
        Assert.Equal(new CursorPosition(2, 0), received);
    }

    [Fact]
    public void CursorPositionChangedEventFiresOnHide()
    {
        var cm = new CursorManager();
        cm.SetCursorPosition(5, 0);

        bool fired = false;
        cm.CursorPositionChanged += _ => fired = true;

        cm.HideCursor();

        Assert.True(fired);
    }

    [Fact]
    public void EventDoesNotFireWhenPositionUnchanged()
    {
        var cm = new CursorManager();
        cm.SetCursorPosition(5, 0);

        int fireCount = 0;
        cm.CursorPositionChanged += _ => fireCount++;

        cm.SetCursorPosition(5, 0); // Same position

        Assert.Equal(0, fireCount);
    }

    [Fact]
    public void CursorFollowsTextInput()
    {
        var cm = new CursorManager();

        // Simulate typing: "> " + "a" -> cursor at x=3
        cm.SetCursorPosition(2, 0); // After "> "
        Assert.Equal(new CursorPosition(2, 0), cm.Position);

        cm.SetCursorPosition(3, 0); // After "> a"
        Assert.Equal(new CursorPosition(3, 0), cm.Position);
    }
}
