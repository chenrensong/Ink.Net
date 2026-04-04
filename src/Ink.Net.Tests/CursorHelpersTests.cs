using Ink.Net.Terminal;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Tests for <see cref="CursorHelpers"/>.</summary>
public class CursorHelpersTests
{
    [Fact]
    public void CursorUpGeneratesCorrectEscape()
    {
        Assert.Equal("\u001B[3A", CursorHelpers.CursorUp(3));
        Assert.Equal("", CursorHelpers.CursorUp(0));
    }

    [Fact]
    public void CursorDownGeneratesCorrectEscape()
    {
        Assert.Equal("\u001B[2B", CursorHelpers.CursorDown(2));
    }

    [Fact]
    public void CursorToGeneratesCorrectEscape()
    {
        // Column 0 → \x1B[1G (1-based)
        Assert.Equal("\u001B[1G", CursorHelpers.CursorTo(0));
        Assert.Equal("\u001B[6G", CursorHelpers.CursorTo(5));
    }

    [Fact]
    public void EraseLinesGeneratesCorrectSequence()
    {
        string erase = CursorHelpers.EraseLines(3);
        Assert.Contains("\u001B[2K", erase); // Erase line
        Assert.Contains("\u001B[1A", erase); // Move up
    }

    [Fact]
    public void BuildCursorSuffixWithNullReturnsEmpty()
    {
        Assert.Equal("", CursorHelpers.BuildCursorSuffix(10, null));
    }

    [Fact]
    public void BuildCursorSuffixWithPosition()
    {
        string suffix = CursorHelpers.BuildCursorSuffix(10, new CursorPosition(5, 3));
        Assert.Contains(CursorHelpers.ShowCursorEscape, suffix);
    }

    [Fact]
    public void CursorPositionChangedDetectsDifference()
    {
        Assert.True(CursorHelpers.CursorPositionChanged(
            new CursorPosition(0, 0), new CursorPosition(1, 0)));
        Assert.False(CursorHelpers.CursorPositionChanged(
            new CursorPosition(0, 0), new CursorPosition(0, 0)));
        Assert.False(CursorHelpers.CursorPositionChanged(null, null));
        Assert.True(CursorHelpers.CursorPositionChanged(null, new CursorPosition(0, 0)));
    }
}
