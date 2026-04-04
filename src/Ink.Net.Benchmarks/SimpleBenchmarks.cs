// Ported from benchmark/simple/simple.tsx (1:1 file mapping)
//
// The JS benchmark renders a complex tree 100,000 times via rerender().
// In C# we measure the same tree structure using BenchmarkDotNet.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;

namespace Ink.Net.Benchmarks;

/// <summary>
/// Simple render benchmark — 1:1 port of JS <c>benchmark/simple/simple.tsx</c>.
/// <para>
/// JS does <c>rerender(&lt;App /&gt;)</c> 100,000 times.
/// C# measures <c>RenderToString</c> throughput for the same tree shape.
/// </para>
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class SimpleBenchmarks
{
    /// <summary>
    /// Build the tree from benchmark/simple/simple.tsx:
    /// Box(column, padding=1) → Text("Hello World")
    ///                         → Box(marginTop=1, w=60) → Text(long lorem)
    ///                         → Box(marginTop=1, column) → Text("Colors:")
    ///                                                     → Box(column, paddingLeft=1) → Text("-Red"), Text("-Blue"), Text("-Green")
    /// </summary>
    private static TreeNode[] BuildSimpleTree(TreeBuilder b) => new[]
    {
        b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, Padding = 1 }, new[]
        {
            b.Text("Hello World"),
            b.Box(new InkStyle { MarginTop = 1, Width = 60 }, new[]
            {
                b.Text(
                    "Cupcake ipsum dolor sit amet candy candy. Sesame snaps cookie I love " +
                    "tootsie roll apple pie bonbon wafer. Caramels sesame snaps icing " +
                    "cotton candy I love cookie sweet roll. I love bonbon sweet."),
            }),
            b.Box(new InkStyle { MarginTop = 1, FlexDirection = FlexDirectionMode.Column }, new[]
            {
                b.Text("Colors:"),
                b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, PaddingLeft = 1 }, new[]
                {
                    b.Text("- Red"),
                    b.Text("- Blue"),
                    b.Text("- Green"),
                }),
            }),
        })
    };

    [Benchmark(Description = "Simple: single render")]
    public string SingleRender()
    {
        return InkApp.RenderToString(BuildSimpleTree);
    }

    [Benchmark(Description = "Simple: 1,000 rerenders")]
    public string ThousandRerenders()
    {
        string result = "";
        for (int i = 0; i < 1_000; i++)
        {
            result = InkApp.RenderToString(BuildSimpleTree);
        }
        return result;
    }

    [Benchmark(Description = "Simple: 10,000 rerenders")]
    public string TenThousandRerenders()
    {
        string result = "";
        for (int i = 0; i < 10_000; i++)
        {
            result = InkApp.RenderToString(BuildSimpleTree);
        }
        return result;
    }
}
