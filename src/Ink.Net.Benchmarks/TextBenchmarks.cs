// Text processing benchmarks — measures string width, wrapping, and ANSI handling.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Ink.Net.Styles;
using Ink.Net.Text;

namespace Ink.Net.Benchmarks;

/// <summary>
/// Text processing benchmarks — measures string width, wrapping, and ANSI handling.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class TextBenchmarks
{
    private const string PlainText = "Hello World, this is a benchmark string for testing purposes.";
    private const string AnsiText = "\x1B[31mHello\x1B[0m \x1B[32mWorld\x1B[0m, this is \x1B[1ma test\x1B[0m.";
    private const string CjkText = "你好世界，这是一个用于基准测试的字符串。";
    private const string EmojiText = "🍔🍟🌭🍕🌮🌯🥙🥗🥘🍲";
    private const string LongText = "Cupcake ipsum dolor sit amet candy candy. Sesame snaps cookie I love tootsie roll apple pie bonbon wafer. Caramels sesame snaps icing cotton candy I love cookie sweet roll. I love bonbon sweet.";

    [Benchmark(Description = "StringWidth: plain text")]
    public int PlainTextWidth() => StringWidthHelper.GetStringWidth(PlainText);

    [Benchmark(Description = "StringWidth: ANSI text")]
    public int AnsiTextWidth() => StringWidthHelper.GetStringWidth(AnsiText);

    [Benchmark(Description = "StringWidth: CJK text")]
    public int CjkTextWidth() => StringWidthHelper.GetStringWidth(CjkText);

    [Benchmark(Description = "StringWidth: Emoji text")]
    public int EmojiTextWidth() => StringWidthHelper.GetStringWidth(EmojiText);

    [Benchmark(Description = "TextWrapper.Wrap: long text to 40 cols")]
    public string WrapLongText() => TextWrapper.Wrap(LongText, 40, TextWrapMode.Wrap);

    [Benchmark(Description = "TextWrapper.Wrap: truncate end")]
    public string TruncateEnd() => TextWrapper.Wrap(LongText, 40, TextWrapMode.TruncateEnd);

    [Benchmark(Description = "TextWrapper.Wrap: truncate middle")]
    public string TruncateMiddle() => TextWrapper.Wrap(LongText, 40, TextWrapMode.TruncateMiddle);

    [Benchmark(Description = "WidestLine: multiline text")]
    public int WidestLineMultiline() => StringWidthHelper.GetWidestLine("Hello World\n你好世界\n🍔🍟🌭🍕🌮");
}
