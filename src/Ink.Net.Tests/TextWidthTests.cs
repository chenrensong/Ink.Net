// Tests ported from text-width.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;
using Ink.Net.Text;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Text width layout tests aligned with JS test/text-width.tsx.</summary>
public class TextWidthTests
{
    private static readonly RenderToStringOptions Opts100 = new() { Columns = 100 };

    [Fact]
    public void WideCharactersDoNotAddExtraSpaceInsideFixedWidthBox()
    {
        // test('wide characters do not add extra space inside fixed-width Box')
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[]
            {
                b.Box(children: new[]
                {
                    b.Box(new InkStyle { Width = 2 }, new[] { b.Text("🍔") }),
                    b.Text("|"),
                }),
                b.Box(children: new[]
                {
                    b.Box(new InkStyle { Width = 2 }, new[] { b.Text("⏳") }),
                    b.Text("|"),
                }),
            })
        }, Opts100);

        var lines = output.Split('\n');
        Assert.Equal(2, lines.Length);
        Assert.Equal("🍔|", lines[0]);
        Assert.Equal("⏳|", lines[1]);
    }

    [Fact]
    public void CjkCharactersOccupyCorrectWidthInFixedWidthBox()
    {
        // test('CJK characters occupy correct width in fixed-width Box')
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(children: new[]
            {
                b.Box(new InkStyle { Width = 4 }, new[] { b.Text("你好") }),
                b.Text("|"),
            })
        }, Opts100);

        Assert.Equal("你好|", output);
    }

    [Fact]
    public void MixedAsciiAndWideCharactersAlignCorrectly()
    {
        // test('mixed ASCII and wide characters align correctly')
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[]
            {
                b.Box(children: new[]
                {
                    b.Box(new InkStyle { Width = 6 }, new[] { b.Text("ab🍔cd") }),
                    b.Text("|"),
                }),
                b.Box(children: new[]
                {
                    b.Box(new InkStyle { Width = 6 }, new[] { b.Text("abcdef") }),
                    b.Text("|"),
                }),
            })
        }, Opts100);

        var lines = output.Split('\n');
        Assert.Equal(2, lines.Length);
        Assert.Equal("ab🍔cd|", lines[0]);
        Assert.Equal("abcdef|", lines[1]);
    }

    [Fact]
    public void AnsiStyledTextDoesNotAffectLayoutWidth()
    {
        // test('ANSI styled text does not affect layout width')
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(children: new[]
            {
                b.Box(new InkStyle { Width = 5 }, new[]
                {
                    b.Text("\u001b[31mhello\u001b[0m"),
                }),
                b.Text("|"),
            })
        }, Opts100);

        // Strip ANSI to verify layout correctness
        var stripped = System.Text.RegularExpressions.Regex.Replace(output, @"\u001b\[[0-9;]*m", "");
        Assert.Equal("hello|", stripped);
    }

    [Fact]
    public void EmptyTextDoesNotAffectSiblingLayout()
    {
        // test('empty Text does not affect sibling layout')
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(children: new[]
            {
                b.Text(""),
                b.Text("hello"),
            })
        }, Opts100);

        Assert.Equal("hello", output);
    }
}
