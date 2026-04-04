// Transform benchmarks — measures output transform processing.
// Supplements JS benchmarks by covering the Transform component.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;

namespace Ink.Net.Benchmarks;

/// <summary>
/// Transform benchmarks — measures output transformation during rendering.
/// <para>
/// No direct JS benchmark equivalent; covers the Transform feature added in Ink.Net.
/// </para>
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class TransformBenchmarks
{
    private static readonly RenderToStringOptions Opts100 = new() { Columns = 100 };

    [Benchmark(Description = "Transform: simple text transform")]
    public string SimpleTransform()
    {
        return InkApp.RenderToString(b => b.Text("hello world",
            transform: (text, idx) => text.ToUpperInvariant()), Opts100);
    }

    [Benchmark(Description = "Transform: nested Box + Text transform")]
    public string NestedTransform()
    {
        return InkApp.RenderToString(b => new[]
        {
            b.Box(children: new[]
            {
                b.Text("Hello", transform: (s, _) => $"[{s}]"),
                b.Text(" World", transform: (s, _) => $"<{s}>"),
            })
        }, Opts100);
    }

    [Benchmark(Description = "Transform: explicit Transform() method")]
    public string ExplicitTransform()
    {
        return InkApp.RenderToString(b => new[]
        {
            b.Transform(
                transform: (text, index) => $"[{index}: {text}]",
                children: new[]
                {
                    b.Text("test"),
                })
        }, Opts100);
    }

    [Benchmark(Description = "Transform: multiline transform")]
    public string MultilineTransform()
    {
        return InkApp.RenderToString(b => new[]
        {
            b.Transform(
                transform: (text, index) => $"{index + 1}| {text}",
                children: new[]
                {
                    b.Text("line one\nline two\nline three\nline four\nline five"),
                })
        }, Opts100);
    }

    [Benchmark(Description = "Transform: 100 transforms")]
    public string HundredTransforms()
    {
        return InkApp.RenderToString(b =>
        {
            var items = new TreeNode[100];
            for (int i = 0; i < 100; i++)
            {
                int idx = i;
                items[i] = b.Text($"item-{i}", transform: (s, _) => $"[{idx}:{s}]");
            }

            return new[]
            {
                b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, items)
            };
        }, Opts100);
    }
}
