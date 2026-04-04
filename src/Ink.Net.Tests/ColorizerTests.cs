using Ink.Net.Rendering;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Tests for <see cref="Colorizer"/>.</summary>
public class ColorizerTests
{
    [Fact]
    public void NullColorReturnsOriginal()
    {
        Assert.Equal("text", Colorizer.Colorize("text", null, ColorType.Foreground));
    }

    [Fact]
    public void EmptyColorReturnsOriginal()
    {
        Assert.Equal("text", Colorizer.Colorize("text", "", ColorType.Foreground));
    }

    [Fact]
    public void NamedColorForeground()
    {
        string result = Colorizer.Colorize("hi", "red", ColorType.Foreground);
        Assert.StartsWith("\x1B[31m", result);
        Assert.EndsWith("\x1B[39m", result);
        Assert.Contains("hi", result);
    }

    [Fact]
    public void NamedColorBackground()
    {
        string result = Colorizer.Colorize("bg", "blue", ColorType.Background);
        Assert.StartsWith("\x1B[44m", result);
        Assert.EndsWith("\x1B[49m", result);
    }

    [Fact]
    public void HexColorForeground()
    {
        string result = Colorizer.Colorize("hex", "#ff0000", ColorType.Foreground);
        Assert.StartsWith("\x1B[38;2;255;0;0m", result);
        Assert.EndsWith("\x1B[39m", result);
    }

    [Fact]
    public void ShortHexColor()
    {
        string result = Colorizer.Colorize("s", "#f00", ColorType.Foreground);
        Assert.StartsWith("\x1B[38;2;255;0;0m", result);
    }

    [Fact]
    public void RgbColor()
    {
        string result = Colorizer.Colorize("rgb", "rgb(0,255,0)", ColorType.Background);
        Assert.StartsWith("\x1B[48;2;0;255;0m", result);
        Assert.EndsWith("\x1B[49m", result);
    }

    [Fact]
    public void Ansi256Color()
    {
        string result = Colorizer.Colorize("256", "ansi256(196)", ColorType.Foreground);
        Assert.StartsWith("\x1B[38;5;196m", result);
        Assert.EndsWith("\x1B[39m", result);
    }

    [Fact]
    public void DimAppliesCorrectCodes()
    {
        string result = Colorizer.Dim("dim");
        Assert.Equal("\x1B[2mdim\x1B[22m", result);
    }
}
