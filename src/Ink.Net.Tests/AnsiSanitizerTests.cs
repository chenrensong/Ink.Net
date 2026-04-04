using Ink.Net.Ansi;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Tests for <see cref="AnsiSanitizer"/>.</summary>
public class AnsiSanitizerTests
{
    [Fact]
    public void PlainTextPassesThrough()
    {
        Assert.Equal("Hello", AnsiSanitizer.Sanitize("Hello"));
    }

    [Fact]
    public void SgrSequencesArePreserved()
    {
        string input = "\x1B[31mRed\x1B[0m";
        Assert.Equal(input, AnsiSanitizer.Sanitize(input));
    }

    [Fact]
    public void CursorMovementIsStripped()
    {
        // \x1B[2A = cursor up 2 lines
        string input = "Hello\x1B[2AWorld";
        Assert.Equal("HelloWorld", AnsiSanitizer.Sanitize(input));
    }

    [Fact]
    public void OscSequencesArePreserved()
    {
        string input = "\x1B]8;;https://example.com\x07Link\x1B]8;;\x07";
        Assert.Equal(input, AnsiSanitizer.Sanitize(input));
    }

    [Fact]
    public void MixedSequencesFilterCorrectly()
    {
        // SGR + cursor move + text
        string input = "\x1B[31m\x1B[2AHello\x1B[0m";
        string expected = "\x1B[31mHello\x1B[0m";
        Assert.Equal(expected, AnsiSanitizer.Sanitize(input));
    }
}
