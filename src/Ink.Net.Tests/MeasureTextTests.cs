// Tests ported from measure-text.tsx
using Ink.Net.Text;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Text measurement tests aligned with JS test/measure-text.tsx.</summary>
public class MeasureTextTests
{
    [Fact]
    public void MeasureSingleWord()
    {
        var result = TextMeasurer.Measure("constructor");
        Assert.Equal(11, result.Width);
        Assert.Equal(1, result.Height);
    }

    [Fact]
    public void MeasureEmptyString()
    {
        var result = TextMeasurer.Measure("");
        Assert.Equal(0, result.Width);
        Assert.Equal(0, result.Height);
    }

    [Fact]
    public void MeasureMultilineText()
    {
        var result = TextMeasurer.Measure("hello\nworld");
        Assert.Equal(5, result.Width);
        Assert.Equal(2, result.Height);
    }

    [Fact]
    public void MeasureMultilineTextWithVaryingLineLengths()
    {
        var result = TextMeasurer.Measure("a\nfoo\nhi");
        Assert.Equal(3, result.Width);
        Assert.Equal(3, result.Height);
    }

    [Fact]
    public void MeasureTextWithTrailingNewline()
    {
        var result = TextMeasurer.Measure("hello\n");
        Assert.Equal(5, result.Width);
        Assert.Equal(2, result.Height);
    }

    [Fact]
    public void MeasureTextWithOnlyNewlines()
    {
        var result = TextMeasurer.Measure("\n\n");
        Assert.Equal(0, result.Width);
        Assert.Equal(3, result.Height);
    }

    [Fact]
    public void ReturnsCachedResultOnRepeatedCalls()
    {
        // Clear cache first to ensure deterministic behavior
        TextMeasurer.ClearCache();
        var first = TextMeasurer.Measure("cached-test");
        Assert.Equal(11, first.Width);
        Assert.Equal(1, first.Height);
        var second = TextMeasurer.Measure("cached-test");
        Assert.Equal(first, second);
    }

    [Fact]
    public void MeasureTextWithAnsiEscapeSequences()
    {
        var result = TextMeasurer.Measure("\u001B[31mred\u001B[0m");
        Assert.Equal(3, result.Width);
        Assert.Equal(1, result.Height);
    }

    [Fact]
    public void MeasureTextWith256ColorAnsi()
    {
        var result = TextMeasurer.Measure("\u001B[38;5;196mred\u001B[0m");
        Assert.Equal(3, result.Width);
        Assert.Equal(1, result.Height);
    }

    [Fact]
    public void MeasureTextWithWideCharacters()
    {
        var result = TextMeasurer.Measure("你好");
        Assert.Equal(4, result.Width);
        Assert.Equal(1, result.Height);
    }

    [Fact]
    public void MeasureTextWithEmoji()
    {
        var result = TextMeasurer.Measure("🍔");
        Assert.Equal(2, result.Width);
        Assert.Equal(1, result.Height);
    }

    [Fact]
    public void MeasureMultilineWithWideCharacters()
    {
        var result = TextMeasurer.Measure("🍔🍟\nabc");
        Assert.Equal(4, result.Width);
        Assert.Equal(2, result.Height);
    }
}
