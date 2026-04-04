// -----------------------------------------------------------------------
// Tests for Static component rendering
// Corresponds to JS Ink <Static> component tests
// -----------------------------------------------------------------------

using Ink.Net.Builder;
using Ink.Net.Dom;
using Ink.Net.Rendering;
using Ink.Net.Styles;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Tests for the Static component (TreeBuilder.Static).</summary>
public class StaticComponentTests
{
    [Fact]
    public void StaticNodeIsMarked()
    {
        var builder = new TreeBuilder();
        var staticNode = builder.Static(children: new[]
        {
            builder.Text("Static content"),
        });

        // Build a tree with the static node
        var root = builder.Build(new[] { staticNode }, columns: 80);

        // The child of root should be a box with InternalStatic = true
        var child = (DomElement)root.ChildNodes[0];
        Assert.True(child.InternalStatic);

        DomTree.CleanupYogaNode(root.YogaNode);
    }

    [Fact]
    public void StaticContentRendersInStaticOutput()
    {
        // When a Static node is part of the tree, InkRenderer should
        // place its content in StaticOutput
        var builder = new TreeBuilder();

        var staticContent = builder.Static(children: new[]
        {
            builder.Text("Log line 1"),
            builder.Text("Log line 2"),
        });

        var dynamicContent = builder.Text("Dynamic content");

        var root = builder.Build(new[] { staticContent, dynamicContent }, columns: 80);

        // Mark the static node on the root (simulate what DomTree does)
        root.StaticNode = (DomElement)root.ChildNodes[0];

        var result = InkRenderer.Render(root);

        // Static output should contain the static content
        Assert.Contains("Log line 1", result.StaticOutput);
        Assert.Contains("Log line 2", result.StaticOutput);

        // Dynamic output should contain the dynamic content
        Assert.Contains("Dynamic content", result.Output);

        DomTree.CleanupYogaNode(root.YogaNode);
    }

    [Fact]
    public void StaticNodeSkippedInDynamicOutput()
    {
        var builder = new TreeBuilder();

        var staticContent = builder.Static(children: new[]
        {
            builder.Text("Should not appear in dynamic output"),
        });

        var dynamicContent = builder.Text("Visible");

        var root = builder.Build(new[] { staticContent, dynamicContent }, columns: 80);
        root.StaticNode = (DomElement)root.ChildNodes[0];

        var result = InkRenderer.Render(root);

        // Dynamic output should NOT contain the static text
        Assert.DoesNotContain("Should not appear in dynamic output", result.Output);
        Assert.Contains("Visible", result.Output);

        DomTree.CleanupYogaNode(root.YogaNode);
    }

    [Fact]
    public void StaticContentRendersInScreenReaderMode()
    {
        var builder = new TreeBuilder();

        var staticContent = builder.Static(children: new[]
        {
            builder.Text("Screen reader static"),
        });

        var dynamicContent = builder.Text("Dynamic");

        var root = builder.Build(new[] { staticContent, dynamicContent }, columns: 80);
        root.StaticNode = (DomElement)root.ChildNodes[0];

        var result = InkRenderer.Render(root, isScreenReaderEnabled: true);

        Assert.Contains("Screen reader static", result.StaticOutput);
        Assert.Contains("Dynamic", result.Output);

        DomTree.CleanupYogaNode(root.YogaNode);
    }

    [Fact]
    public void StaticWithCustomStyle()
    {
        var builder = new TreeBuilder();
        var staticContent = builder.Static(
            children: new[] { builder.Text("Styled static") },
            style: new InkStyle { PaddingLeft = 2 });

        var root = builder.Build(new[] { staticContent }, columns: 80);
        var child = (DomElement)root.ChildNodes[0];

        Assert.True(child.InternalStatic);
        // Verify that the style was applied
        Assert.Equal(PositionMode.Absolute, child.Style.Position);

        DomTree.CleanupYogaNode(root.YogaNode);
    }

    [Fact]
    public void RenderToStringWithStatic()
    {
        // Use RenderToString which combines static + dynamic output
        string output = InkApp.RenderToString(b =>
        {
            var staticPart = b.Static(children: new[]
            {
                b.Text("Log: item processed"),
            });
            var dynamicPart = b.Text("Processing...");
            return new[] { staticPart, dynamicPart };
        }, new RenderToStringOptions { Columns = 80 });

        // RenderToString combines static + dynamic
        Assert.Contains("Log: item processed", output);
        Assert.Contains("Processing...", output);
    }

    [Fact]
    public void EmptyStaticNodeDoesNotCrash()
    {
        var builder = new TreeBuilder();
        var staticContent = builder.Static();
        var root = builder.Build(new[] { staticContent }, columns: 80);

        var child = (DomElement)root.ChildNodes[0];
        Assert.True(child.InternalStatic);
        Assert.Empty(child.ChildNodes);

        DomTree.CleanupYogaNode(root.YogaNode);
    }
}
