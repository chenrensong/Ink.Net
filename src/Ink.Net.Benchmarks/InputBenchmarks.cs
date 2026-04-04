// Input parsing benchmarks — measures keypress parsing and input handler throughput.
// Supplements JS benchmarks by covering the new input subsystem.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Ink.Net.Input;

namespace Ink.Net.Benchmarks;

/// <summary>
/// Input parsing benchmarks — measures keypress parsing, input parser, and handler dispatch.
/// <para>
/// No direct JS benchmark equivalent; covers the input subsystem added in Ink.Net.
/// </para>
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class InputBenchmarks
{
    // ─── Raw key sequences ─────────────────────────────────────────
    private const string PlainChar = "a";
    private const string ArrowUp = "\u001B[A";
    private const string ArrowDown = "\u001B[B";
    private const string ShiftTab = "\u001B[Z";
    private const string CtrlC = "\x03";
    private const string EscapeKey = "\u001B";
    private const string FunctionF5 = "\u001B[15~";
    private const string KittyKeyA = "\u001B[97u"; // Kitty protocol 'a'
    private const string KittyCtrlShiftA = "\u001B[97;6u"; // Kitty Ctrl+Shift+'a'

    // Bracketed paste
    private const string PasteStart = "\u001B[200~";
    private const string PasteEnd = "\u001B[201~";
    private const string PastedText = "Hello, World!";
    private static readonly string BracketedPaste = $"{PasteStart}{PastedText}{PasteEnd}";

    // Mixed input (multiple keys in one chunk)
    private static readonly string MixedInput = "abc" + ArrowUp + ArrowDown + "xyz\r" + ShiftTab;

    // ─── KeypressParser ────────────────────────────────────────────

    [Benchmark(Description = "KeypressParser: plain char")]
    public ParsedKey ParsePlainChar() => KeypressParser.Parse(PlainChar);

    [Benchmark(Description = "KeypressParser: arrow key")]
    public ParsedKey ParseArrowKey() => KeypressParser.Parse(ArrowUp);

    [Benchmark(Description = "KeypressParser: function key")]
    public ParsedKey ParseFunctionKey() => KeypressParser.Parse(FunctionF5);

    [Benchmark(Description = "KeypressParser: kitty protocol")]
    public ParsedKey ParseKittyKey() => KeypressParser.Parse(KittyKeyA);

    [Benchmark(Description = "KeypressParser: kitty with modifiers")]
    public ParsedKey ParseKittyModifiers() => KeypressParser.Parse(KittyCtrlShiftA);

    // ─── InputParser ───────────────────────────────────────────────

    [Benchmark(Description = "InputParser: single char")]
    public List<InputEvent> ParseSingleChar()
    {
        var parser = new InputParser();
        return parser.Push(PlainChar);
    }

    [Benchmark(Description = "InputParser: bracketed paste")]
    public List<InputEvent> ParseBracketedPaste()
    {
        var parser = new InputParser();
        return parser.Push(BracketedPaste);
    }

    [Benchmark(Description = "InputParser: mixed input chunk")]
    public List<InputEvent> ParseMixedInput()
    {
        var parser = new InputParser();
        return parser.Push(MixedInput);
    }

    // ─── InputHandler dispatch ─────────────────────────────────────

    private InputHandler? _handler;
    private int _callbackCount;

    [GlobalSetup]
    public void Setup()
    {
        _handler = new InputHandler { ExitOnCtrlC = false };
        _handler.Register((input, key) => _callbackCount++);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _handler?.Dispose();
    }

    [Benchmark(Description = "InputHandler: dispatch plain char")]
    public void DispatchPlainChar()
    {
        _handler!.HandleData(PlainChar);
    }

    [Benchmark(Description = "InputHandler: dispatch arrow key")]
    public void DispatchArrowKey()
    {
        _handler!.HandleData(ArrowUp);
    }

    [Benchmark(Description = "InputHandler: dispatch mixed chunk")]
    public void DispatchMixedChunk()
    {
        _handler!.HandleData(MixedInput);
    }

    [Benchmark(Description = "InputHandler: 1000 dispatches")]
    public void ThousandDispatches()
    {
        for (int i = 0; i < 1_000; i++)
        {
            _handler!.HandleData(PlainChar);
        }
    }
}
