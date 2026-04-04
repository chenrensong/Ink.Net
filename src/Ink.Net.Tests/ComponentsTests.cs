// Tests ported from components.tsx
// Covers: text, text wrapping, truncation, transforms, spacer, newline
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Component tests aligned with JS components.tsx test suite.</summary>
public class ComponentsTests
{
    private static readonly RenderToStringOptions Opts100 = new() { Columns = 100 };

    // ── Basic text ──────────────────────────────────────────────────

    [Fact]
    public void TextRendersContent()
    {
        string output = InkApp.RenderToString(b => b.Text("Hello World"), Opts100);
        Assert.Equal("Hello World", output);
    }

    [Fact]
    public void TextWithVariable()
    {
        string output = InkApp.RenderToString(b => b.Text($"Count: {1}"), Opts100);
        Assert.Equal("Count: 1", output);
    }

    [Fact]
    public void MultipleTextNodes()
    {
        // Two text nodes squashed into one (inside a Box acting as ink-text parent)
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Row }, new[]
            {
                b.Text("Hello"),
                b.Text(" World"),
            })
        }, Opts100);

        Assert.Equal("Hello World", output);
    }

    [Fact]
    public void NumberRendersAsText()
    {
        string output = InkApp.RenderToString(b => b.Text("1"), Opts100);
        Assert.Equal("1", output);
    }

    [Fact]
    public void EmptyTextNodeRendersEmpty()
    {
        string output = InkApp.RenderToString(b => b.Text(""), Opts100);
        Assert.Equal("", output);
    }

    [Fact]
    public void IgnoreEmptyTextNodeInColumn()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[]
            {
                b.Box(children: new[] { b.Text("Hello World") }),
                b.Text(""),
            })
        }, Opts100);

        Assert.Equal("Hello World", output);
    }

    // ── Text wrapping ───────────────────────────────────────────────

    [Fact]
    public void WrapText()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Width = 7 }, new[]
            {
                b.Text("Hello World", new InkStyle { TextWrap = TextWrapMode.Wrap }),
            })
        }, Opts100);

        Assert.Equal("Hello\nWorld", output);
    }

    [Fact]
    public void DontWrapTextIfEnoughSpace()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Width = 20 }, new[]
            {
                b.Text("Hello World", new InkStyle { TextWrap = TextWrapMode.Wrap }),
            })
        }, Opts100);

        Assert.Equal("Hello World", output);
    }

    [Fact]
    public void TruncateTextEnd()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Width = 7 }, new[]
            {
                b.Text("Hello World", new InkStyle { TextWrap = TextWrapMode.TruncateEnd }),
            })
        }, Opts100);

        Assert.Equal("Hello …", output);
    }

    [Fact]
    public void TruncateTextMiddle()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Width = 7 }, new[]
            {
                b.Text("Hello World", new InkStyle { TextWrap = TextWrapMode.TruncateMiddle }),
            })
        }, Opts100);

        Assert.Equal("Hel…rld", output);
    }

    [Fact]
    public void TruncateTextStart()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Width = 7 }, new[]
            {
                b.Text("Hello World", new InkStyle { TextWrap = TextWrapMode.TruncateStart }),
            })
        }, Opts100);

        Assert.Equal("… World", output);
    }

    // ── Transform ───────────────────────────────────────────────────

    [Fact]
    public void TransformChildren()
    {
        // Outer transform: [index: text], Inner transform: {index: text}
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(
                transform: (text, index) => $"[{index}: {text}]",
                children: new[]
                {
                    b.Text("test", transform: (text, index) => $"{{{index}: {text}}}"),
                })
        }, Opts100);

        Assert.Equal("[0: {0: test}]", output);
    }

    [Fact]
    public void TransformWithMultipleLines()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(
                transform: (text, index) => $"[{index}: {text}]",
                children: new[]
                {
                    b.Text("hello world\ngoodbye world"),
                })
        }, Opts100);

        Assert.Equal("[0: hello world]\n[1: goodbye world]", output);
    }

    // ── Spacer ──────────────────────────────────────────────────────

    [Fact]
    public void HorizontalSpacer()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { Width = 20 }, new[]
            {
                b.Text("Left"),
                b.Spacer(),
                b.Text("Right"),
            })
        }, Opts100);

        Assert.Equal("Left           Right", output);
    }

    [Fact]
    public void VerticalSpacer()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, Height = 6 }, new[]
            {
                b.Text("Top"),
                b.Spacer(),
                b.Text("Bottom"),
            })
        }, Opts100);

        Assert.Equal("Top\n\n\n\n\nBottom", output);
    }

    // ── Newline ─────────────────────────────────────────────────────

    [Fact]
    public void NewlineInText()
    {
        // Simulate <Text>Hello<Newline />World</Text> using box with column layout
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[]
            {
                b.Text("Hello"),
                b.Text("World"),
            })
        }, Opts100);

        Assert.Equal("Hello\nWorld", output);
    }

    [Fact]
    public void MultipleNewlines()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[]
            {
                b.Text("Hello"),
                b.Newline(),
                b.Text("World"),
            })
        }, Opts100);

        Assert.Equal("Hello\n\nWorld", output);
    }
}
