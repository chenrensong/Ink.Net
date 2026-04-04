// Output buffer benchmarks — measures grid composition and ANSI style handling.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Ink.Net.Rendering;

namespace Ink.Net.Benchmarks;

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

    [Benchmark(Description = "Output: large grid write + get (200x50)")]
    public string LargeGridWriteAndGet()
    {
        var output = new Output(200, 50);
        for (int y = 0; y < 50; y++)
        {
            output.Write(0, y, $"Line {y}: This is a fairly long text content for benchmarking the output buffer performance with larger grids.");
        }
        var (result, _) = output.Get();
        return result;
    }
}
