// Ported from benchmark/static/static.tsx (1:1 file mapping)
//
// The JS benchmark renders incrementally with <Static> items accumulating over time.
// In C# we simulate this by rendering trees of increasing size.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;

namespace Ink.Net.Benchmarks;

/// <summary>
/// Static (accumulated items) benchmark — 1:1 port of JS <c>benchmark/static/static.tsx</c>.
/// <para>
/// JS renders 1000 items accumulating via &lt;Static&gt;.
/// C# simulates this by rendering trees with an increasing number of items.
/// </para>
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class StaticBenchmarks
{
    [Params(10, 100, 500)]
    public int ItemCount { get; set; }

    /// <summary>
    /// Render a tree with N accumulated items (simulates Static output).
    /// </summary>
    [Benchmark(Description = "Static: render N accumulated items")]
    public string RenderAccumulatedItems()
    {
        return InkApp.RenderToString(b =>
        {
            var items = new List<TreeNode>();

            // Accumulated "static" items
            for (int i = 0; i < ItemCount; i++)
            {
                items.Add(b.Box(new InkStyle { Padding = 1, FlexDirection = FlexDirectionMode.Column }, new[]
                {
                    b.Text($"Item #{i}"),
                    b.Text("Item content"),
                }));
            }

            // Dynamic footer (like the bottom section in JS benchmark)
            items.Add(b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, Padding = 1 }, new[]
            {
                b.Text("Hello World"),
                b.Text($"Rendered: {ItemCount}"),
                b.Box(new InkStyle { MarginTop = 1, Width = 60 }, new[]
                {
                    b.Text(
                        "Cupcake ipsum dolor sit amet candy candy. Sesame snaps cookie I love " +
                        "tootsie roll apple pie bonbon wafer."),
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
            }));

            return new[]
            {
                b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, items.ToArray())
            };
        });
    }
}
