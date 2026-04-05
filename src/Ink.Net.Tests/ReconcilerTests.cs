// -----------------------------------------------------------------------
// ReconcilerTests.cs — Tree update scenarios aligned with ink/test/reconciler.tsx
// Uses InkApp + debug stdout to compare frames (JS createStdout last write).
// -----------------------------------------------------------------------

using System.Text;
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Rendering;
using Ink.Net.Styles;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Captures the most recent full frame written in debug mode (no trailing newline).</summary>
public sealed class FrameCaptureWriter : TextWriter
{
    public override Encoding Encoding => Encoding.UTF8;

    public string LastFrame { get; private set; } = "";

    public override void Write(string? value)
    {
        if (string.IsNullOrEmpty(value)) return;
        var v = value.EndsWith('\n') ? value[..^1] : value;
        // Terminal height pads with blank lines; reconciler tests compare logical content only.
        LastFrame = v.TrimEnd('\n', '\r');
    }

    public override void Write(char value) => Write(value.ToString());
}

public class ReconcilerTests
{
    private const int Cols = 100;

    [Fact]
    public void UpdateChild_MatchesReferenceTree()
    {
        bool update = false;

        TreeNode[] BuildActual(TreeBuilder b) => new[] { b.Text(update ? "B" : "A") };
        TreeNode[] BuildExpected(TreeBuilder b) => new[] { b.Text(update ? "B" : "A") };

        var wa = new FrameCaptureWriter();
        var wb = new FrameCaptureWriter();

        var actual = InkApp.Render(BuildActual, new RenderOptions { Stdout = wa, Debug = true, Columns = Cols });
        var expected = InkApp.Render(BuildExpected, new RenderOptions { Stdout = wb, Debug = true, Columns = Cols });

        Assert.Equal(wb.LastFrame, wa.LastFrame);

        update = true;
        actual.Rerender(BuildActual);
        expected.Rerender(BuildExpected);

        Assert.Equal(wb.LastFrame, wa.LastFrame);

        actual.Unmount();
        expected.Unmount();
    }

    [Fact]
    public void UpdateTextNode_MatchesReferenceTree()
    {
        bool update = false;

        // Row so "Hello " + letter matches a single <Text>Hello {letter}</Text> reference frame.
        TreeNode[] BuildActual(TreeBuilder b) => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Row }, new[]
            {
                b.Text("Hello "),
                b.Text(update ? "B" : "A"),
            }),
        };

        TreeNode[] BuildExpected(TreeBuilder b) => new[]
        {
            b.Text(update ? "Hello B" : "Hello A"),
        };

        var wa = new FrameCaptureWriter();
        var wb = new FrameCaptureWriter();

        var actual = InkApp.Render(BuildActual, new RenderOptions { Stdout = wa, Debug = true, Columns = Cols });
        var expected = InkApp.Render(BuildExpected, new RenderOptions { Stdout = wb, Debug = true, Columns = Cols });

        Assert.Equal(wb.LastFrame, wa.LastFrame);

        update = true;
        actual.Rerender(BuildActual);
        expected.Rerender(BuildExpected);

        Assert.Equal(wb.LastFrame, wa.LastFrame);

        actual.Unmount();
        expected.Unmount();
    }

    [Fact]
    public void RemoveStyleProp_MarginLeft_RemovesLeadingSpace()
    {
        bool withStyle = true;

        TreeNode[] Build(TreeBuilder b) => new[]
        {
            b.Box(
                withStyle ? new InkStyle { MarginLeft = 1 } : null,
                new[] { b.Text("X") }),
        };

        var w = new FrameCaptureWriter();
        var inst = InkApp.Render(Build, new RenderOptions { Stdout = w, Debug = true, Columns = Cols });

        Assert.Equal(" X", w.LastFrame);

        withStyle = false;
        inst.Rerender(Build);

        Assert.Equal("X", w.LastFrame);

        inst.Unmount();
    }

    [Fact]
    public void AppendChild_MatchesReferenceTree()
    {
        bool append = false;

        TreeNode[] BuildActual(TreeBuilder b)
        {
            var kids = new List<TreeNode> { b.Text("A") };
            if (append) kids.Add(b.Text("B"));
            return new[]
            {
                b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, kids.ToArray()),
            };
        }

        TreeNode[] BuildExpected(TreeBuilder b) => BuildActual(b);

        var wa = new FrameCaptureWriter();
        var wb = new FrameCaptureWriter();

        var actual = InkApp.Render(BuildActual, new RenderOptions { Stdout = wa, Debug = true, Columns = Cols });
        var expected = InkApp.Render(BuildExpected, new RenderOptions { Stdout = wb, Debug = true, Columns = Cols });

        Assert.Equal(wb.LastFrame, wa.LastFrame);

        append = true;
        actual.Rerender(BuildActual);
        expected.Rerender(BuildExpected);

        Assert.Equal(wb.LastFrame, wa.LastFrame);

        actual.Unmount();
        expected.Unmount();
    }

    [Fact]
    public void InsertChildBetween_MatchesReferenceTree()
    {
        bool insert = false;

        TreeNode[] BuildActual(TreeBuilder b)
        {
            if (insert)
            {
                return new[]
                {
                    b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[]
                    {
                        b.Text("A"),
                        b.Text("B"),
                        b.Text("C"),
                    }),
                };
            }

            return new[]
            {
                b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[]
                {
                    b.Text("A"),
                    b.Text("C"),
                }),
            };
        }

        TreeNode[] BuildExpected(TreeBuilder b) => BuildActual(b);

        var wa = new FrameCaptureWriter();
        var wb = new FrameCaptureWriter();

        var actual = InkApp.Render(BuildActual, new RenderOptions { Stdout = wa, Debug = true, Columns = Cols });
        var expected = InkApp.Render(BuildExpected, new RenderOptions { Stdout = wb, Debug = true, Columns = Cols });

        Assert.Equal(wb.LastFrame, wa.LastFrame);

        insert = true;
        actual.Rerender(BuildActual);
        expected.Rerender(BuildExpected);

        Assert.Equal(wb.LastFrame, wa.LastFrame);

        actual.Unmount();
        expected.Unmount();
    }

    [Fact]
    public void RemoveChild_MatchesReferenceTree()
    {
        bool remove = false;

        TreeNode[] BuildActual(TreeBuilder b)
        {
            var kids = new List<TreeNode> { b.Text("A") };
            if (!remove) kids.Add(b.Text("B"));
            return new[]
            {
                b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, kids.ToArray()),
            };
        }

        TreeNode[] BuildExpected(TreeBuilder b) => BuildActual(b);

        var wa = new FrameCaptureWriter();
        var wb = new FrameCaptureWriter();

        var actual = InkApp.Render(BuildActual, new RenderOptions { Stdout = wa, Debug = true, Columns = Cols });
        var expected = InkApp.Render(BuildExpected, new RenderOptions { Stdout = wb, Debug = true, Columns = Cols });

        Assert.Equal(wb.LastFrame, wa.LastFrame);

        remove = true;
        actual.Rerender(BuildActual);
        expected.Rerender(BuildExpected);

        Assert.Equal(wb.LastFrame, wa.LastFrame);

        actual.Unmount();
        expected.Unmount();
    }

    [Fact]
    public void ReorderChildren_MatchesReferenceTree()
    {
        bool reorder = false;

        TreeNode[] BuildActual(TreeBuilder b)
        {
            if (reorder)
            {
                return new[]
                {
                    b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[]
                    {
                        b.Text("B"),
                        b.Text("A"),
                    }),
                };
            }

            return new[]
            {
                b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[]
                {
                    b.Text("A"),
                    b.Text("B"),
                }),
            };
        }

        TreeNode[] BuildExpected(TreeBuilder b) => BuildActual(b);

        var wa = new FrameCaptureWriter();
        var wb = new FrameCaptureWriter();

        var actual = InkApp.Render(BuildActual, new RenderOptions { Stdout = wa, Debug = true, Columns = Cols });
        var expected = InkApp.Render(BuildExpected, new RenderOptions { Stdout = wb, Debug = true, Columns = Cols });

        Assert.Equal(wb.LastFrame, wa.LastFrame);

        reorder = true;
        actual.Rerender(BuildActual);
        expected.Rerender(BuildExpected);

        Assert.Equal(wb.LastFrame, wa.LastFrame);

        actual.Unmount();
        expected.Unmount();
    }

    [Fact]
    public void ReplaceChildNodeWithText_GreenThenPlain()
    {
        bool replace = false;

        TreeNode[] Build(TreeBuilder b) => new[]
        {
            b.Text(replace
                ? "x"
                : Colorizer.Colorize("test", "green", ColorType.Foreground)),
        };

        var w = new FrameCaptureWriter();
        var inst = InkApp.Render(Build, new RenderOptions { Stdout = w, Debug = true, Columns = Cols });

        Assert.Equal(Colorizer.Colorize("test", "green", ColorType.Foreground), w.LastFrame);

        replace = true;
        inst.Rerender(Build);

        Assert.Equal("x", w.LastFrame);

        inst.Unmount();
    }

    /// <summary>Semantic port of reconciler "support suspense": Loading → delayed resolved text.</summary>
    [Fact]
    public async Task SupportSuspense_AsyncSemanticPort()
    {
        var loaded = false;

        TreeNode[] Build(TreeBuilder b) => new[]
        {
            b.Text(loaded ? "Hello World" : "Loading"),
        };

        var w = new FrameCaptureWriter();
        var inst = InkApp.Render(Build, new RenderOptions { Stdout = w, Debug = true, Columns = Cols });

        Assert.Equal("Loading", w.LastFrame);

        await Task.Delay(150, TestContext.Current.CancellationToken);
        loaded = true;
        inst.Rerender(Build);

        Assert.Equal("Hello World", w.LastFrame);

        inst.Unmount();
    }

    /// <summary>Semantic port of concurrent suspense: external resolve then rerender.</summary>
    [Fact]
    public async Task SupportSuspenseConcurrent_ExternalResolveSemanticPort()
    {
        var tcs = new TaskCompletionSource();
        string? data = null;

        TreeNode[] Build(TreeBuilder b) => new[]
        {
            b.Text(data is null ? "Loading" : data),
        };

        var w = new FrameCaptureWriter();
        var inst = InkApp.Render(Build, new RenderOptions { Stdout = w, Debug = true, Columns = Cols });

        Assert.Equal("Loading", w.LastFrame);

        data = "Hello Concurrent World";
        tcs.SetResult();
        await tcs.Task;
        inst.Rerender(Build);

        Assert.Equal("Hello Concurrent World", w.LastFrame);

        inst.Unmount();
    }
}
