using Facebook.Yoga;
using Ink.Net.Dom;
using Ink.Net.Rendering;
using Ink.Net.Styles;
using static Facebook.Yoga.YGNodeAPI;
using static Facebook.Yoga.YGNodeStyleAPI;
using static Facebook.Yoga.YGNodeLayoutAPI;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Tests for <see cref="BorderRenderer"/> and <see cref="BackgroundRenderer"/>.</summary>
public class BorderRendererTests
{
    [Fact]
    public void SingleBorderRendersCorrectly()
    {
        var root = DomTree.CreateNode(InkNodeType.Root);
        var box = DomTree.CreateNode(InkNodeType.Box);
        box.Style = new InkStyle
        {
            BorderStyle = "single",
            Width = 10,
            Height = 4,
        };
        DomTree.AppendChildNode(root, box);

        StyleApplier.Apply(box.YogaNode!, box.Style);
        YGNodeCalculateLayout(root.YogaNode!, 80, 24, YGDirection.LTR);

        var output = new Output(10, 4);
        BorderRenderer.Render(0, 0, box, output);
        var (str, _) = output.Get();

        // Top border should contain ┌ and ┐
        Assert.Contains("┌", str);
        Assert.Contains("┐", str);
        // Bottom border should contain └ and ┘
        Assert.Contains("└", str);
        Assert.Contains("┘", str);
        // Vertical border
        Assert.Contains("│", str);
    }

    [Fact]
    public void BackgroundFillsContentArea()
    {
        var root = DomTree.CreateNode(InkNodeType.Root);
        var box = DomTree.CreateNode(InkNodeType.Box);
        box.Style = new InkStyle
        {
            BackgroundColor = "red",
            Width = 5,
            Height = 2,
        };
        DomTree.AppendChildNode(root, box);

        StyleApplier.Apply(box.YogaNode!, box.Style);
        YGNodeCalculateLayout(root.YogaNode!, 80, 24, YGDirection.LTR);

        var output = new Output(5, 2);
        BackgroundRenderer.Render(0, 0, box, output);
        var (str, _) = output.Get();

        // Should contain ANSI background color code
        Assert.Contains("\x1B[41m", str);
    }
}
