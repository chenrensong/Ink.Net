using Ink.Net.Rendering;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Tests for <see cref="Output"/>.</summary>
public class OutputTests
{
    [Fact]
    public void EmptyOutputProducesSpaces()
    {
        var output = new Output(5, 2);
        var (str, height) = output.Get();
        Assert.Equal(2, height);
        // Both lines should be empty (trimmed)
        var lines = str.Split('\n');
        Assert.Equal(2, lines.Length);
        Assert.All(lines, line => Assert.Equal("", line));
    }

    [Fact]
    public void WriteAtPosition()
    {
        var output = new Output(10, 3);
        output.Write(0, 0, "Hello");
        output.Write(0, 1, "World");
        var (str, _) = output.Get();
        var lines = str.Split('\n');
        Assert.Equal("Hello", lines[0]);
        Assert.Equal("World", lines[1]);
    }

    [Fact]
    public void WriteWithOffset()
    {
        var output = new Output(10, 1);
        output.Write(3, 0, "Hi");
        var (str, _) = output.Get();
        Assert.StartsWith("   Hi", str.Split('\n')[0]);
    }

    [Fact]
    public void ClipExcludesContent()
    {
        var output = new Output(10, 3);
        output.Clip(new OutputClip { X1 = 0, X2 = 5, Y1 = 0, Y2 = 2 });
        output.Write(0, 0, "ABCDEFGHIJ");
        output.Unclip();
        var (str, _) = output.Get();
        // Only first 5 chars should appear
        Assert.Equal("ABCDE", str.Split('\n')[0]);
    }

    [Fact]
    public void MultiLineWrite()
    {
        var output = new Output(10, 3);
        output.Write(0, 0, "Line1\nLine2\nLine3");
        var (str, _) = output.Get();
        var lines = str.Split('\n');
        Assert.Equal("Line1", lines[0]);
        Assert.Equal("Line2", lines[1]);
        Assert.Equal("Line3", lines[2]);
    }

    [Fact]
    public void SliceAnsiPreservesAnsiCodes()
    {
        // Slice colored text
        string colored = "\x1B[31mHello\x1B[0m";
        string sliced = Output.SliceAnsi(colored, 0, 3);
        Assert.Contains("Hel", sliced);
    }
}
