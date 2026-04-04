// Tests ported from background.tsx
// Covers: Box background color inheritance, space fills, various color formats
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Rendering;
using Ink.Net.Styles;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Background color tests aligned with JS background.tsx test suite.</summary>
public class BackgroundTests
{
    private static readonly RenderToStringOptions Opts100 = new() { Columns = 100 };

    // ANSI escape codes for background colors
    private const string BgRed = "\u001B[41m";
    private const string BgGreen = "\u001B[42m";
    private const string BgYellow = "\u001B[43m";
    private const string BgBlue = "\u001B[44m";
    private const string BgMagenta = "\u001B[45m";
    private const string BgCyan = "\u001B[46m";
    private const string BgHexRed = "\u001B[48;2;255;0;0m"; // #FF0000
    private const string BgAnsi256Nine = "\u001B[48;5;9m";   // ansi256(9)
    private const string BgReset = "\u001B[49m";

    // ── Text inherits parent Box background ──────────────────────────────

    [Fact]
    public void TextInheritsParentBoxBackgroundColor()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BackgroundColor = "green", AlignSelf = AlignSelfMode.FlexStart }, new[]
            {
                b.Text("Hello World"),
            })
        }, Opts100);

        Assert.Equal($"{BgGreen}Hello World{BgReset}", output);
    }

    [Fact]
    public void TextWithoutParentBoxBackgroundHasNoInheritance()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { AlignSelf = AlignSelfMode.FlexStart }, new[]
            {
                b.Text("Hello World"),
            })
        }, Opts100);

        Assert.Equal("Hello World", output);
    }

    [Fact]
    public void MultipleTextElementsInheritSameBackground()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BackgroundColor = "yellow", AlignSelf = AlignSelfMode.FlexStart }, new[]
            {
                b.Text("Hello "),
                b.Text("World"),
            })
        }, Opts100);

        Assert.Equal($"{BgYellow}Hello World{BgReset}", output);
    }

    // ── Box background with different color formats ──────────────────────

    [Fact]
    public void BoxBackgroundWithStandardColor()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BackgroundColor = "red", AlignSelf = AlignSelfMode.FlexStart }, new[]
            {
                b.Text("Hello"),
            })
        }, Opts100);

        Assert.Equal($"{BgRed}Hello{BgReset}", output);
    }

    [Fact]
    public void BoxBackgroundWithHexColor()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BackgroundColor = "#FF0000", AlignSelf = AlignSelfMode.FlexStart }, new[]
            {
                b.Text("Hello"),
            })
        }, Opts100);

        Assert.Equal($"{BgHexRed}Hello{BgReset}", output);
    }

    [Fact]
    public void BoxBackgroundWithRgbColor()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BackgroundColor = "rgb(255, 0, 0)", AlignSelf = AlignSelfMode.FlexStart }, new[]
            {
                b.Text("Hello"),
            })
        }, Opts100);

        Assert.Equal($"{BgHexRed}Hello{BgReset}", output);
    }

    [Fact]
    public void BoxBackgroundWithAnsi256Color()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BackgroundColor = "ansi256(9)", AlignSelf = AlignSelfMode.FlexStart }, new[]
            {
                b.Text("Hello"),
            })
        }, Opts100);

        Assert.Equal($"{BgAnsi256Nine}Hello{BgReset}", output);
    }

    // ── Box background space fill tests ──────────────────────────────────

    [Fact]
    public void BoxBackgroundFillsEntireAreaWithStandardColor()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BackgroundColor = "red", Width = 10, Height = 3, AlignSelf = AlignSelfMode.FlexStart }, new[]
            {
                b.Text("Hello"),
            })
        }, Opts100);

        Assert.Contains(BgRed, output);
        Assert.Contains(BgReset, output);
        Assert.Contains("Hello", output);
        // Should contain a full background fill line
        Assert.Contains($"{BgRed}          {BgReset}", output);
    }

    [Fact]
    public void BoxBackgroundFillsWithHexColor()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BackgroundColor = "#FF0000", Width = 10, Height = 3, AlignSelf = AlignSelfMode.FlexStart }, new[]
            {
                b.Text("Hello"),
            })
        }, Opts100);

        Assert.Contains("Hello", output);
        Assert.Contains(BgHexRed, output);
        Assert.Contains(BgReset, output);
    }

    [Fact]
    public void BoxBackgroundWithBorderFillsContentArea()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle
            {
                BackgroundColor = "cyan",
                BorderStyle = "round",
                Width = 10,
                Height = 5,
                AlignSelf = AlignSelfMode.FlexStart,
            }, new[]
            {
                b.Text("Hi"),
            })
        }, Opts100);

        Assert.Contains("Hi", output);
        Assert.Contains(BgCyan, output);
        Assert.Contains(BgReset, output);
        Assert.Contains("╭", output);
        Assert.Contains("╮", output);
    }

    [Fact]
    public void BoxBackgroundWithPaddingFillsEntirePaddedArea()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle
            {
                BackgroundColor = "magenta",
                Padding = 1,
                Width = 10,
                Height = 5,
                AlignSelf = AlignSelfMode.FlexStart,
            }, new[]
            {
                b.Text("Hi"),
            })
        }, Opts100);

        Assert.Contains("Hi", output);
        Assert.Contains(BgMagenta, output);
        Assert.Contains(BgReset, output);
    }

    [Fact]
    public void BoxBackgroundWithCenterAlignmentFillsEntireArea()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle
            {
                BackgroundColor = "blue",
                Width = 10,
                Height = 3,
                JustifyContent = JustifyContentMode.Center,
                AlignSelf = AlignSelfMode.FlexStart,
            }, new[]
            {
                b.Text("Hi"),
            })
        }, Opts100);

        Assert.Contains("Hi", output);
        Assert.Contains(BgBlue, output);
        Assert.Contains(BgReset, output);
    }

    [Fact]
    public void BoxBackgroundWithColumnLayoutFillsEntireArea()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle
            {
                BackgroundColor = "green",
                FlexDirection = FlexDirectionMode.Column,
                Width = 10,
                Height = 5,
                AlignSelf = AlignSelfMode.FlexStart,
            }, new[]
            {
                b.Text("Line 1"),
                b.Text("Line 2"),
            })
        }, Opts100);

        Assert.Contains("Line 1", output);
        Assert.Contains("Line 2", output);
        Assert.Contains(BgGreen, output);
        Assert.Contains(BgReset, output);
    }

    [Fact]
    public void BoxBackgroundFillsFullWidthOnEveryLineWhenTextWraps()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BackgroundColor = "red", Width = 10, AlignSelf = AlignSelfMode.FlexStart }, new[]
            {
                b.Text("Hello World!!"),
            })
        }, Opts100);

        // Both lines should be padded to full 10-char width
        Assert.Equal($"{BgRed}Hello     {BgReset}\n{BgRed}World!!   {BgReset}", output);
    }
}
