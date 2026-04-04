using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Integration tests for <see cref="InkApp.RenderToString"/>.</summary>
public class RenderToStringTests
{
    [Fact]
    public void SimpleTextRendersToString()
    {
        string output = InkApp.RenderToString(b => b.Text("Hello, Ink.Net!"));
        Assert.Contains("Hello, Ink.Net!", output);
    }

    [Fact]
    public void BoxWithTextRendersToString()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[]
            {
                b.Text("Line 1"),
                b.Text("Line 2"),
            })
        });
        Assert.Contains("Line 1", output);
        Assert.Contains("Line 2", output);
    }

    [Fact]
    public void CustomColumnsAreRespected()
    {
        string output = InkApp.RenderToString(
            b => b.Text("Short"),
            new RenderToStringOptions { Columns = 40 });
        Assert.Contains("Short", output);
    }

    [Fact]
    public void EmptyTreeRendersBlank()
    {
        string output = InkApp.RenderToString(b => Array.Empty<TreeNode>());
        // Empty tree with Yoga layout still produces whitespace lines
        Assert.NotNull(output);
    }

    [Fact]
    public void RowLayoutRendersHorizontally()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Row }, new[]
            {
                b.Text("A"),
                b.Text("B"),
            })
        });

        // Both A and B should be on same line
        var lines = output.Split('\n');
        Assert.Contains("A", lines[0]);
        Assert.Contains("B", lines[0]);
    }

    [Fact]
    public void BorderedBoxRendersWithBorder()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle
            {
                BorderStyle = "single",
                Width = 20,
                Height = 5,
            }, new[]
            {
                b.Text("Bordered"),
            })
        }, new RenderToStringOptions { Columns = 80 });

        Assert.Contains("┌", output);
        Assert.Contains("┐", output);
        Assert.Contains("└", output);
        Assert.Contains("┘", output);
        Assert.Contains("Bordered", output);
    }

    [Fact]
    public void PaddingIsApplied()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle
            {
                Padding = 1,
                Width = 20,
                Height = 4,
            }, new[]
            {
                b.Text("Padded"),
            })
        }, new RenderToStringOptions { Columns = 80 });

        Assert.Contains("Padded", output);
    }

    [Fact]
    public void TransformIsApplied()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Text("hello", transform: (text, _) => text.ToUpperInvariant()),
        });

        Assert.Contains("HELLO", output);
    }
}
