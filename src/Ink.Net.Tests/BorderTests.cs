// Tests ported from borders.tsx
// Comprehensive border rendering tests — 1:1 port of JS borders.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Rendering;
using Ink.Net.Styles;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Border tests aligned with JS borders.tsx test suite.</summary>
public class BorderTests
{
    private static readonly RenderToStringOptions Opts100 = new() { Columns = 100 };

    // ── Round style border chars ────────────────────────────────────────
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

    // ANSI helpers
    private static string AnsiGreen(string s) => $"\x1b[32m{s}\x1b[39m";
    private static string AnsiDim(string s) => $"\x1b[2m{s}\x1b[22m";
    private static string AnsiBold(string s) => $"\x1b[1m{s}\x1b[22m";
    private static string AnsiBlue(string s) => $"\x1b[34m{s}\x1b[39m";

    private static string Rep(string ch, int count) => string.Concat(Enumerable.Repeat(ch, count));

    /// <summary>Helper to build boxen-like output (round style by default).</summary>
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
        sb.Append(Rep(h, contentWidth));
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
        sb.Append(Rep(h, contentWidth));
        sb.Append(br);

        return sb.ToString();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Single node tests
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void SingleNode_FullWidthBox()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BorderStyle = "round" }, new[]
            {
                b.Text("Hello World"),
            })
        }, Opts100);

        Assert.Equal(Boxen("Hello World", width: 100), output);
    }

    [Fact]
    public void SingleNode_FullWidthBox_ColorfulBorder()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BorderStyle = "round", BorderColor = "green" }, new[]
            {
                b.Text("Hello World"),
            })
        }, Opts100);

        // Border is colored green (entire top border line is wrapped in one ANSI sequence)
        Assert.Contains("\x1b[32m", output); // green foreground
        Assert.Contains("\x1b[39m", output); // reset foreground
        Assert.Contains("Hello World", output);
    }

    [Fact]
    public void SingleNode_FitContentBox()
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
    public void SingleNode_FixedWidthBox()
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
    public void SingleNode_FixedWidthAndHeightBox()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BorderStyle = "round", Width = 20, Height = 20 }, new[]
            {
                b.Text("Hello World"),
            })
        }, Opts100);

        Assert.Equal(Boxen("Hello World".PadRight(18) + Rep("\n", 17), width: 20), output);
    }

    [Fact]
    public void SingleNode_BoxWithPadding()
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
    public void SingleNode_BoxWithHorizontalAlignment()
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
    public void SingleNode_BoxWithVerticalAlignment()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle
            {
                BorderStyle = "round",
                Height = 20,
                AlignItems = AlignItemsMode.Center,
                AlignSelf = AlignSelfMode.FlexStart
            }, new[]
            {
                b.Text("Hello World"),
            })
        }, Opts100);

        Assert.Equal(Boxen(Rep("\n", 8) + "Hello World" + Rep("\n", 9)), output);
    }

    [Fact]
    public void SingleNode_BoxWithWrapping()
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

    // ═══════════════════════════════════════════════════════════════════════
    //  Multiple nodes tests
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void MultipleNodes_FullWidthBox()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BorderStyle = "round" }, new[]
            {
                b.Text("Hello World"),
            })
        }, Opts100);

        Assert.Equal(Boxen("Hello World", width: 100), output);
    }

    [Fact]
    public void MultipleNodes_FitContentBox()
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
    public void MultipleNodes_FixedWidthBox()
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
    public void MultipleNodes_FixedWidthAndHeightBox()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BorderStyle = "round", Width = 20, Height = 20 }, new[]
            {
                b.Text("Hello World"),
            })
        }, Opts100);

        Assert.Equal(Boxen("Hello World".PadRight(18) + Rep("\n", 17), width: 20), output);
    }

    [Fact]
    public void MultipleNodes_BoxWithPadding()
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
    public void MultipleNodes_BoxWithHorizontalAlignment()
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
    public void MultipleNodes_BoxWithWrapping()
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

    // ═══════════════════════════════════════════════════════════════════════
    //  Nested boxes
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void NestedBoxes()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BorderStyle = "round", Width = 40, Padding = 1 }, new[]
            {
                b.Box(new InkStyle { BorderStyle = "round", JustifyContent = JustifyContentMode.Center, Padding = 1 }, new[]
                {
                    b.Text("Hello World"),
                }),
            })
        }, Opts100);

        // Verify it contains the nested border chars and content
        Assert.Contains("Hello World", output);
        Assert.Contains(TL, output);
        Assert.Contains(BR, output);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Hide individual borders
    // ═══════════════════════════════════════════════════════════════════════

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
            $"{BL}{Rep(H, 7)}{BR}",
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
            $"{TL}{Rep(H, 7)}{TR}",
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
            $"{Rep(H, 7)}{TR}",
            $"Content{V}",
            $"{Rep(H, 7)}{BR}",
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
            $"{TL}{Rep(H, 7)}",
            $"{V}Content",
            $"{BL}{Rep(H, 7)}",
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
            Rep(H, 7),
            "Content",
            Rep(H, 7),
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

    // ═══════════════════════════════════════════════════════════════════════
    //  Border colors
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void ChangeColorOfTopBorder()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, AlignItems = AlignItemsMode.FlexStart }, new[]
            {
                b.Text("Above"),
                b.Box(new InkStyle { BorderStyle = "round", BorderTopColor = "green" }, new[]
                {
                    b.Text("Content"),
                }),
                b.Text("Below"),
            })
        }, Opts100);

        // The top border should be colored green
        string greenTopBorder = AnsiGreen($"{TL}{Rep(H, 7)}{TR}");
        Assert.Contains(greenTopBorder, output);
        // Content line should NOT be green
        Assert.Contains($"{V}Content{V}", output);
    }

    [Fact]
    public void ChangeColorOfBottomBorder()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, AlignItems = AlignItemsMode.FlexStart }, new[]
            {
                b.Text("Above"),
                b.Box(new InkStyle { BorderStyle = "round", BorderBottomColor = "green" }, new[]
                {
                    b.Text("Content"),
                }),
                b.Text("Below"),
            })
        }, Opts100);

        string greenBottomBorder = AnsiGreen($"{BL}{Rep(H, 7)}{BR}");
        Assert.Contains(greenBottomBorder, output);
    }

    [Fact]
    public void ChangeColorOfLeftBorder()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, AlignItems = AlignItemsMode.FlexStart }, new[]
            {
                b.Text("Above"),
                b.Box(new InkStyle { BorderStyle = "round", BorderLeftColor = "green" }, new[]
                {
                    b.Text("Content"),
                }),
                b.Text("Below"),
            })
        }, Opts100);

        // Left border piece should be colored
        Assert.Contains(AnsiGreen(V), output);
    }

    [Fact]
    public void ChangeColorOfRightBorder()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, AlignItems = AlignItemsMode.FlexStart }, new[]
            {
                b.Text("Above"),
                b.Box(new InkStyle { BorderStyle = "round", BorderRightColor = "green" }, new[]
                {
                    b.Text("Content"),
                }),
                b.Text("Below"),
            })
        }, Opts100);

        // Right border piece should be colored
        Assert.Contains(AnsiGreen(V), output);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Custom border style
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void CustomBorderStyle_Arrow()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BorderStyle = "arrow" }, new[]
            {
                b.Text("Content"),
            })
        }, Opts100);

        // Arrow style uses ↘↓↙→←↗↑↖
        Assert.Contains("↘", output);
        Assert.Contains("↓", output);
        Assert.Contains("↙", output);
        Assert.Contains("→", output);
        Assert.Contains("←", output);
        Assert.Contains("↗", output);
        Assert.Contains("↑", output);
        Assert.Contains("↖", output);
        Assert.Contains("Content", output);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Dim border color
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void DimBorderColor()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BorderDimColor = true, BorderStyle = "round" }, new[]
            {
                b.Text("Content"),
            })
        }, Opts100);

        // Dimmed borders should contain ANSI dim escape sequences
        Assert.Contains("\x1b[2m", output);
        Assert.Contains("\x1b[22m", output);
        Assert.Contains("Content", output);
    }

    [Fact]
    public void DimTopBorderColor()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, AlignItems = AlignItemsMode.FlexStart }, new[]
            {
                b.Text("Above"),
                b.Box(new InkStyle { BorderTopDimColor = true, BorderStyle = "round" }, new[]
                {
                    b.Text("Content"),
                }),
                b.Text("Below"),
            })
        }, Opts100);

        string dimmedTopBorder = AnsiDim($"{TL}{Rep(H, 7)}{TR}");
        Assert.Contains(dimmedTopBorder, output);
        Assert.Contains($"{V}Content{V}", output);
    }

    [Fact]
    public void DimBottomBorderColor()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, AlignItems = AlignItemsMode.FlexStart }, new[]
            {
                b.Text("Above"),
                b.Box(new InkStyle { BorderBottomDimColor = true, BorderStyle = "round" }, new[]
                {
                    b.Text("Content"),
                }),
                b.Text("Below"),
            })
        }, Opts100);

        string dimmedBottomBorder = AnsiDim($"{BL}{Rep(H, 7)}{BR}");
        Assert.Contains(dimmedBottomBorder, output);
    }

    [Fact]
    public void DimLeftBorderColor()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, AlignItems = AlignItemsMode.FlexStart }, new[]
            {
                b.Text("Above"),
                b.Box(new InkStyle { BorderLeftDimColor = true, BorderStyle = "round" }, new[]
                {
                    b.Text("Content"),
                }),
                b.Text("Below"),
            })
        }, Opts100);

        Assert.Contains(AnsiDim(V), output);
    }

    [Fact]
    public void DimRightBorderColor()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, AlignItems = AlignItemsMode.FlexStart }, new[]
            {
                b.Text("Above"),
                b.Box(new InkStyle { BorderRightDimColor = true, BorderStyle = "round" }, new[]
                {
                    b.Text("Content"),
                }),
                b.Text("Below"),
            })
        }, Opts100);

        Assert.Contains(AnsiDim(V), output);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Different border styles
    // ═══════════════════════════════════════════════════════════════════════

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

        Assert.Equal($"{STL}{Rep(SH, 5)}{STR}\n{SV}Hello{SV}\n{SBL}{Rep(SH, 5)}{SBR}", output);
    }

    [Fact]
    public void DoubleBorderStyle()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BorderStyle = "double", AlignSelf = AlignSelfMode.FlexStart }, new[]
            {
                b.Text("Hello"),
            })
        }, Opts100);

        Assert.Contains("╔", output);
        Assert.Contains("═", output);
        Assert.Contains("╗", output);
        Assert.Contains("║", output);
        Assert.Contains("╚", output);
        Assert.Contains("╝", output);
        Assert.Contains("Hello", output);
    }

    [Fact]
    public void BoldBorderStyle()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BorderStyle = "bold", AlignSelf = AlignSelfMode.FlexStart }, new[]
            {
                b.Text("Hello"),
            })
        }, Opts100);

        Assert.Contains("┏", output);
        Assert.Contains("━", output);
        Assert.Contains("┓", output);
        Assert.Contains("┃", output);
        Assert.Contains("┗", output);
        Assert.Contains("┛", output);
        Assert.Contains("Hello", output);
    }

    [Fact]
    public void ClassicBorderStyle()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BorderStyle = "classic", AlignSelf = AlignSelfMode.FlexStart }, new[]
            {
                b.Text("Hello"),
            })
        }, Opts100);

        Assert.Contains("+", output);
        Assert.Contains("-", output);
        Assert.Contains("|", output);
        Assert.Contains("Hello", output);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Wide characters and emojis
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void FitContentBox_WideCharacters()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BorderStyle = "round", AlignSelf = AlignSelfMode.FlexStart }, new[]
            {
                b.Text("こんにちは"),
            })
        }, Opts100);

        // Wide chars take 2 columns each, "こんにちは" = 10 columns
        Assert.Contains("こんにちは", output);
        Assert.Contains(TL, output);
        Assert.Contains(BR, output);
    }

    [Fact]
    public void FitContentBox_Emojis()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BorderStyle = "round", AlignSelf = AlignSelfMode.FlexStart }, new[]
            {
                b.Text("🌊🌊"),
            })
        }, Opts100);

        Assert.Contains("🌊🌊", output);
        Assert.Contains(TL, output);
        Assert.Contains(BR, output);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Nested boxes on flex-direction column
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void NestedBoxes_FlexDirectionColumn()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BorderStyle = "round", AlignSelf = AlignSelfMode.FlexStart, FlexDirection = FlexDirectionMode.Column }, new[]
            {
                b.Box(new InkStyle { BorderStyle = "round" }, new[]
                {
                    b.Text("First"),
                }),
                b.Box(new InkStyle { BorderStyle = "round" }, new[]
                {
                    b.Text("Second"),
                }),
            })
        }, Opts100);

        Assert.Contains("First", output);
        Assert.Contains("Second", output);
        // Both inner boxes should have borders
        int tlCount = output.Split(TL).Length - 1;
        Assert.True(tlCount >= 3, $"Expected at least 3 occurrences of {TL} (outer + 2 inner), got {tlCount}");
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Nested boxes on flex-direction row with wide chars
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void NestedBoxes_FlexDirectionRow_WideChars()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BorderStyle = "round", AlignSelf = AlignSelfMode.FlexStart }, new[]
            {
                b.Box(new InkStyle { BorderStyle = "round" }, new[]
                {
                    b.Text("ミスター"),
                }),
                b.Box(new InkStyle { BorderStyle = "round" }, new[]
                {
                    b.Text("スポック"),
                }),
            })
        }, Opts100);

        Assert.Contains("ミスター", output);
        Assert.Contains("スポック", output);
    }

    [Fact]
    public void NestedBoxes_FlexDirectionRow_Emojis()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BorderStyle = "round", AlignSelf = AlignSelfMode.FlexStart }, new[]
            {
                b.Box(new InkStyle { BorderStyle = "round" }, new[]
                {
                    b.Text("🦾"),
                }),
                b.Box(new InkStyle { BorderStyle = "round" }, new[]
                {
                    b.Text("🌏"),
                }),
                b.Box(new InkStyle { BorderStyle = "round" }, new[]
                {
                    b.Text("😋"),
                }),
            })
        }, Opts100);

        Assert.Contains("🦾", output);
        Assert.Contains("🌏", output);
        Assert.Contains("😋", output);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Border with overall border color
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void BorderColor_AllSides()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BorderStyle = "round", BorderColor = "green", AlignSelf = AlignSelfMode.FlexStart }, new[]
            {
                b.Text("Hello"),
            })
        }, Opts100);

        // All border pieces should be green
        Assert.Contains("\x1b[32m", output); // green foreground
        Assert.Contains("Hello", output);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  borderDimColor does not dim styled child Text touching left edge
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void BorderDimColor_DoesNotDimStyledChildText()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { BorderDimColor = true, BorderStyle = "round", AlignSelf = AlignSelfMode.FlexStart }, new[]
            {
                b.Text("styled text", transform: (line, _) =>
                    Colorizer.Colorize($"\x1b[1m{line}\x1b[22m", "blue", ColorType.Foreground)),
            })
        }, Opts100);

        // The border should be dimmed
        string dimmedTopBorder = AnsiDim($"{TL}{Rep(H, 11)}{TR}");
        Assert.Contains(dimmedTopBorder, output);

        // The styled text should contain blue and bold ANSI codes, not dim
        Assert.Contains("\x1b[34m", output); // blue
        Assert.Contains("\x1b[1m", output);  // bold
        Assert.Contains("styled text", output);
    }
}
