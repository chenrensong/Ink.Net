// Tests ported from borders.tsx
// Comprehensive border rendering tests
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Border tests aligned with JS borders.tsx test suite.</summary>
public class BorderTests
{
    private static readonly RenderToStringOptions Opts100 = new() { Columns = 100 };

    // ── Single border chars (round style) ──────────────────────────────
    private const string TL = "╭"; // topLeft
    private const string TR = "╮"; // topRight
    private const string BL = "╰"; // bottomLeft
    private const string BR = "╯"; // bottomRight
    private const string H = "─";  // horizontal (top/bottom)
    private const string V = "│";  // vertical (left/right)

    // single style chars
    private const string STL = "┌";
    private const string STR = "┐";
    private const string SBL = "└";
    private const string SBR = "┘";
    private const string SH = "─";
    private const string SV = "│";

    // Helper to build boxen-like output
    private static string Boxen(string content, int? width = null, string style = "round")
    {
        var tl = style == "round" ? TL : STL;
        var tr = style == "round" ? TR : STR;
        var bl = style == "round" ? BL : SBL;
        var br = style == "round" ? BR : SBR;
        var h = H;
        var v = style == "round" ? V : SV;

        var lines = content.Split('\n');
        int contentWidth = width.HasValue ? width.Value - 2 : 0;
        if (!width.HasValue)
        {
            foreach (var line in lines)
                if (line.Length > contentWidth)
                    contentWidth = line.Length;
        }

        var sb = new System.Text.StringBuilder();
        sb.Append(tl);
        sb.Append(string.Concat(Enumerable.Repeat(h, contentWidth)));
        sb.Append(tr);
        sb.Append('\n');

        foreach (var line in lines)
        {
            sb.Append(v);
            sb.Append(line);
            sb.Append(new string(' ', contentWidth - line.Length));
            sb.Append(v);
            sb.Append('\n');
        }

        sb.Append(bl);
        sb.Append(string.Concat(Enumerable.Repeat(h, contentWidth)));
        sb.Append(br);

        return sb.ToString();
    }

    // ── Fit-content box ─────────────────────────────────────────────────

    [Fact]
    public void FitContentBox()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BorderStyle = "round", AlignSelf = AlignSelfMode.FlexStart }, new[]
            {
                b.Text("Hello World"),
            })
        }, Opts100);

        Assert.Equal(Boxen("Hello World"), output);
    }

    [Fact]
    public void FixedWidthBox()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BorderStyle = "round", Width = 20 }, new[]
            {
                b.Text("Hello World"),
            })
        }, Opts100);

        Assert.Equal(Boxen("Hello World".PadRight(18), width: 20), output);
    }

    [Fact]
    public void FixedWidthAndHeightBox()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BorderStyle = "round", Width = 20, Height = 20 }, new[]
            {
                b.Text("Hello World"),
            })
        }, Opts100);

        Assert.Equal(Boxen("Hello World".PadRight(18) + string.Concat(Enumerable.Repeat("\n", 17)), width: 20), output);
    }

    [Fact]
    public void BoxWithPadding()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BorderStyle = "round", Padding = 1, AlignSelf = AlignSelfMode.FlexStart }, new[]
            {
                b.Text("Hello World"),
            })
        }, Opts100);

        Assert.Equal(Boxen("\n Hello World \n"), output);
    }

    [Fact]
    public void BoxWithHorizontalAlignment()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BorderStyle = "round", Width = 20, JustifyContent = JustifyContentMode.Center }, new[]
            {
                b.Text("Hello World"),
            })
        }, Opts100);

        Assert.Equal(Boxen("   Hello World    ", width: 20), output);
    }

    [Fact]
    public void BoxWithWrapping()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BorderStyle = "round", Width = 10 }, new[]
            {
                b.Text("Hello World"),
            })
        }, Opts100);

        Assert.Equal(Boxen("Hello   \nWorld", width: 10), output);
    }

    // ── Hide individual borders ─────────────────────────────────────────

    [Fact]
    public void HideTopBorder()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, AlignItems = AlignItemsMode.FlexStart }, new[]
            {
                b.Text("Above"),
                b.Box(new InkStyle { BorderStyle = "round", BorderTop = false }, new[]
                {
                    b.Text("Content"),
                }),
                b.Text("Below"),
            })
        }, Opts100);

        Assert.Equal(string.Join("\n", new[]
        {
            "Above",
            $"{V}Content{V}",
            $"{BL}{H}{H}{H}{H}{H}{H}{H}{BR}",
            "Below",
        }), output);
    }

    [Fact]
    public void HideBottomBorder()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, AlignItems = AlignItemsMode.FlexStart }, new[]
            {
                b.Text("Above"),
                b.Box(new InkStyle { BorderStyle = "round", BorderBottom = false }, new[]
                {
                    b.Text("Content"),
                }),
                b.Text("Below"),
            })
        }, Opts100);

        Assert.Equal(string.Join("\n", new[]
        {
            "Above",
            $"{TL}{H}{H}{H}{H}{H}{H}{H}{TR}",
            $"{V}Content{V}",
            "Below",
        }), output);
    }

    [Fact]
    public void HideTopAndBottomBorders()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, AlignItems = AlignItemsMode.FlexStart }, new[]
            {
                b.Text("Above"),
                b.Box(new InkStyle { BorderStyle = "round", BorderTop = false, BorderBottom = false }, new[]
                {
                    b.Text("Content"),
                }),
                b.Text("Below"),
            })
        }, Opts100);

        Assert.Equal(string.Join("\n", new[]
        {
            "Above",
            $"{V}Content{V}",
            "Below",
        }), output);
    }

    [Fact]
    public void HideLeftBorder()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, AlignItems = AlignItemsMode.FlexStart }, new[]
            {
                b.Text("Above"),
                b.Box(new InkStyle { BorderStyle = "round", BorderLeft = false }, new[]
                {
                    b.Text("Content"),
                }),
                b.Text("Below"),
            })
        }, Opts100);

        Assert.Equal(string.Join("\n", new[]
        {
            "Above",
            $"{H}{H}{H}{H}{H}{H}{H}{TR}",
            $"Content{V}",
            $"{H}{H}{H}{H}{H}{H}{H}{BR}",
            "Below",
        }), output);
    }

    [Fact]
    public void HideRightBorder()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, AlignItems = AlignItemsMode.FlexStart }, new[]
            {
                b.Text("Above"),
                b.Box(new InkStyle { BorderStyle = "round", BorderRight = false }, new[]
                {
                    b.Text("Content"),
                }),
                b.Text("Below"),
            })
        }, Opts100);

        Assert.Equal(string.Join("\n", new[]
        {
            "Above",
            $"{TL}{H}{H}{H}{H}{H}{H}{H}",
            $"{V}Content",
            $"{BL}{H}{H}{H}{H}{H}{H}{H}",
            "Below",
        }), output);
    }

    [Fact]
    public void HideLeftAndRightBorder()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, AlignItems = AlignItemsMode.FlexStart }, new[]
            {
                b.Text("Above"),
                b.Box(new InkStyle { BorderStyle = "round", BorderLeft = false, BorderRight = false }, new[]
                {
                    b.Text("Content"),
                }),
                b.Text("Below"),
            })
        }, Opts100);

        Assert.Equal(string.Join("\n", new[]
        {
            "Above",
            $"{H}{H}{H}{H}{H}{H}{H}",
            "Content",
            $"{H}{H}{H}{H}{H}{H}{H}",
            "Below",
        }), output);
    }

    [Fact]
    public void HideAllBorders()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, AlignItems = AlignItemsMode.FlexStart }, new[]
            {
                b.Text("Above"),
                b.Box(new InkStyle
                {
                    BorderStyle = "round",
                    BorderTop = false,
                    BorderBottom = false,
                    BorderLeft = false,
                    BorderRight = false,
                }, new[]
                {
                    b.Text("Content"),
                }),
                b.Text("Below"),
            })
        }, Opts100);

        Assert.Equal(string.Join("\n", new[] { "Above", "Content", "Below" }), output);
    }

    // ── Single style borders ───────────────────────────────────────────

    [Fact]
    public void SingleBorderStyle()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BorderStyle = "single", AlignSelf = AlignSelfMode.FlexStart }, new[]
            {
                b.Text("Hello"),
            })
        }, Opts100);

        Assert.Equal($"{STL}{SH}{SH}{SH}{SH}{SH}{STR}\n{SV}Hello{SV}\n{SBL}{SH}{SH}{SH}{SH}{SH}{SBR}", output);
    }
}
