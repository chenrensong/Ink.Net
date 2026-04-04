// Ink.Net Benchmarks — ported from JS Ink benchmark/simple/simple.tsx
//
// The JS benchmark renders a complex tree 100,000 times.
// Our C# equivalent benchmarks the core rendering pipeline components.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Rendering;
using Ink.Net.Styles;
using Ink.Net.Text;

namespace Ink.Net.Benchmarks;

/// <summary>
/// Core rendering benchmarks — ported from JS <c>benchmark/simple/simple.tsx</c>.
/// <para>
/// The JS benchmark rerenders a complex UI tree 100,000 times.
/// Our benchmarks measure the individual pipeline stages and end-to-end rendering.
/// </para>
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class RenderBenchmarks
{
    // ─── Simple text rendering ───────────────────────────────────────

    [Benchmark(Description = "RenderToString: simple text")]
    public string SimpleText()
    {
        return InkApp.RenderToString(b => b.Text("Hello World"));
    }

    // ─── Complex tree (mirrors JS benchmark/simple/simple.tsx) ──────

    [Benchmark(Description = "RenderToString: complex tree (1 iteration)")]
    public string ComplexTree()
    {
        return InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, Padding = 1 }, new[]
            {
                b.Text("Hello World"),
                b.Box(new InkStyle { MarginTop = 1, Width = 60 }, new[]
                {
                    b.Text("Cupcake ipsum dolor sit amet candy candy. Sesame snaps cookie I love tootsie roll apple pie bonbon wafer."),
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
        });
    }

    // ─── Rerender benchmark (mirrors JS 100k rerenders) ─────────────

    [Benchmark(Description = "RenderToString: 1000 rerenders")]
    public string ThousandRerenders()
    {
        string result = "";
        for (int i = 0; i < 1000; i++)
        {
            result = InkApp.RenderToString(b => new[]
            {
                b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, Padding = 1 }, new[]
                {
                    b.Text("Hello World"),
                    b.Box(new InkStyle { MarginTop = 1, Width = 60 }, new[]
                    {
                        b.Text("Cupcake ipsum dolor sit amet candy."),
                    }),
                    b.Box(new InkStyle { MarginTop = 1, FlexDirection = FlexDirectionMode.Column }, new[]
                    {
                        b.Text("- Red"),
                        b.Text("- Blue"),
                        b.Text("- Green"),
                    }),
                })
            });
        }
        return result;
    }

    // ─── Layout-only benchmarks ─────────────────────────────────────

    [Benchmark(Description = "RenderToString: table layout (5 rows)")]
    public string TableLayout()
    {
        return InkApp.RenderToString(b =>
        {
            var rows = new TreeNode[6]; // 1 header + 5 data
            rows[0] = b.Box(children: new[]
            {
                b.Box(new InkStyle { Width = DimensionValue.Percent(10) }, new[] { b.Text("ID") }),
                b.Box(new InkStyle { Width = DimensionValue.Percent(50) }, new[] { b.Text("Name") }),
                b.Box(new InkStyle { Width = DimensionValue.Percent(40) }, new[] { b.Text("Email") }),
            });
            for (int i = 0; i < 5; i++)
            {
                rows[i + 1] = b.Box(children: new[]
                {
                    b.Box(new InkStyle { Width = DimensionValue.Percent(10) }, new[] { b.Text(i.ToString()) }),
                    b.Box(new InkStyle { Width = DimensionValue.Percent(50) }, new[] { b.Text($"User_{i}") }),
                    b.Box(new InkStyle { Width = DimensionValue.Percent(40) }, new[] { b.Text($"user{i}@example.com") }),
                });
            }
            return new[] { b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, Width = 80 }, rows) };
        });
    }

    // ─── Border rendering ───────────────────────────────────────────

    [Benchmark(Description = "RenderToString: bordered boxes")]
    public string BorderedBoxes()
    {
        return InkApp.RenderToString(b => new[]
        {
            b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[]
            {
                b.Box(new InkStyle { BorderStyle = "single" }, new[] { b.Text("single") }),
                b.Box(new InkStyle { BorderStyle = "double" }, new[] { b.Text("double") }),
                b.Box(new InkStyle { BorderStyle = "round" }, new[] { b.Text("round") }),
                b.Box(new InkStyle { BorderStyle = "bold" }, new[] { b.Text("bold") }),
            })
        });
    }
}

/// <summary>
/// Text processing benchmarks — measures string width, wrapping, and ANSI handling.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class TextBenchmarks
{
    private const string PlainText = "Hello World, this is a benchmark string for testing purposes.";
    private const string AnsiText = "\x1B[31mHello\x1B[0m \x1B[32mWorld\x1B[0m, this is \x1B[1ma test\x1B[0m.";
    private const string LongText = "Cupcake ipsum dolor sit amet candy candy. Sesame snaps cookie I love tootsie roll apple pie bonbon wafer. Caramels sesame snaps icing cotton candy I love cookie sweet roll. I love bonbon sweet.";

    [Benchmark(Description = "StringWidth: plain text")]
    public int PlainTextWidth() => StringWidthHelper.GetStringWidth(PlainText);

    [Benchmark(Description = "StringWidth: ANSI text")]
    public int AnsiTextWidth() => StringWidthHelper.GetStringWidth(AnsiText);

    [Benchmark(Description = "TextWrapper.Wrap: long text to 40 cols")]
    public string WrapLongText() => TextWrapper.Wrap(LongText, 40, TextWrapMode.Wrap);

    [Benchmark(Description = "TextWrapper.Wrap: truncate end")]
    public string TruncateEnd() => TextWrapper.Wrap(LongText, 40, TextWrapMode.TruncateEnd);

    [Benchmark(Description = "TextWrapper.Wrap: truncate middle")]
    public string TruncateMiddle() => TextWrapper.Wrap(LongText, 40, TextWrapMode.TruncateMiddle);
}

/// <summary>
/// Output buffer benchmarks — measures grid composition and ANSI style handling.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class OutputBenchmarks
{
    [Benchmark(Description = "Output: write + get (80x24)")]
    public string WriteAndGet80x24()
    {
        var output = new Output(80, 24);
        output.Write(0, 0, "Hello World");
        output.Write(0, 1, "Second line with some content");
        output.Write(0, 2, "Third line");
        var (result, _) = output.Get();
        return result;
    }

    [Benchmark(Description = "Output: write + get with ANSI (80x24)")]
    public string WriteAndGetAnsi80x24()
    {
        var output = new Output(80, 24);
        output.Write(0, 0, "\x1B[31mHello\x1B[0m \x1B[32mWorld\x1B[0m");
        output.Write(0, 1, "\x1B[1mBold text\x1B[22m");
        var (result, _) = output.Get();
        return result;
    }

    [Benchmark(Description = "Output: write with clip (80x10)")]
    public string WriteWithClip()
    {
        var output = new Output(80, 10);
        output.Clip(new OutputClip { X1 = 5, X2 = 20, Y1 = 0, Y2 = 5 });
        output.Write(0, 0, "This is a long line that should be clipped");
        output.Write(0, 1, "Another long line");
        output.Unclip();
        var (result, _) = output.Get();
        return result;
    }
}
