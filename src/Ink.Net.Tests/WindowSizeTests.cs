// Tests ported from terminal-resize.tsx
// Covers: WindowSizeMonitor — dimensions, resize events, cleanup
using Ink.Net.Terminal;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Window size monitoring tests aligned with JS terminal-resize.tsx test suite.</summary>
public class WindowSizeTests
{
    [Fact]
    public void ReturnsCurrentTerminalDimensions()
    {
        var monitor = new WindowSizeMonitor();
        var size = monitor.Size;

        // Should have positive dimensions (fallback at minimum)
        Assert.True(size.Columns > 0);
        Assert.True(size.Rows > 0);

        monitor.Dispose();
    }

    [Fact]
    public void SetSizeUpdatesDimensions()
    {
        var monitor = new WindowSizeMonitor();

        monitor.SetSize(120, 40);

        Assert.Equal(120, monitor.Size.Columns);
        Assert.Equal(40, monitor.Size.Rows);

        monitor.Dispose();
    }

    [Fact]
    public void ResizedEventFiresOnSizeChange()
    {
        var monitor = new WindowSizeMonitor();
        WindowSize? received = null;
        monitor.Resized += size => received = size;

        monitor.SetSize(60, 20);

        Assert.NotNull(received);
        Assert.Equal(60, received.Value.Columns);
        Assert.Equal(20, received.Value.Rows);

        monitor.Dispose();
    }

    [Fact]
    public void ResizedEventDoesNotFireWhenSizeUnchanged()
    {
        var monitor = new WindowSizeMonitor();
        monitor.SetSize(80, 24); // Set initial

        int fireCount = 0;
        monitor.Resized += _ => fireCount++;

        monitor.SetSize(80, 24); // Same size

        Assert.Equal(0, fireCount);

        monitor.Dispose();
    }

    [Fact]
    public void DisposeStopsMonitoring()
    {
        var monitor = new WindowSizeMonitor();
        monitor.Start();
        monitor.Dispose();

        // Should not throw after dispose
        monitor.SetSize(100, 50); // Silently ignored or just updates local
    }

    [Fact]
    public void WindowSizeEquality()
    {
        var a = new WindowSize(80, 24);
        var b = new WindowSize(80, 24);
        var c = new WindowSize(100, 30);

        Assert.Equal(a, b);
        Assert.NotEqual(a, c);
        Assert.True(a == b);
        Assert.True(a != c);
    }
}
