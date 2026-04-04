// Tests ported from text.tsx
// Covers: empty text, ANSI sanitization, text content rendering
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Text rendering tests aligned with JS text.tsx test suite.</summary>
public class TextTests
{
    private static readonly RenderToStringOptions Opts100 = new() { Columns = 100 };

    // Helper: render text inside a Box (matching JS renderText helper)
    private static string RenderText(string text) =>
        InkApp.RenderToString(b => new[]
        {
            b.Box(children: new[] { b.Text(text) })
        }, Opts100);

    // ── Empty / null-equivalent text ────────────────────────────────────

    [Fact]
    public void TextWithEmptyContent()
    {
        // Corresponds to: <Text /> and <Text>{null}</Text>
        var output = InkApp.RenderToString(b => b.Text(""), Opts100);
        Assert.Equal("", output);
    }

    [Fact]
    public void TextWithContentConstructor()
    {
        // See https://github.com/vadimdemedes/ink/issues/743
        var output = InkApp.RenderToString(b => b.Text("constructor"), Opts100);
        Assert.Equal("constructor", output);
    }

    // ── ANSI cursor movement stripping ─────────────────────────────────

    [Fact]
    public void StripAnsiCursorMovementSequencesFromText()
    {
        // \x1b[1A = cursor up, \x1b[2K = clear line, \x1b[1B = cursor down
        // \x1b[32m = green (SGR, preserved), \x1b[0m = reset (SGR, preserved)
        var input = "\u001B[1A\u001B[2KStarting client ... \u001B[32mdone\u001B[0m\u001B[1B";

        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(children: new[] { b.Text(input) })
        }, Opts100);

        Assert.DoesNotContain("\u001B[1A", output);
        Assert.DoesNotContain("\u001B[2K", output);
        Assert.DoesNotContain("\u001B[1B", output);
        Assert.Equal("Starting client ... done", StripAnsi(output));
    }

    [Fact]
    public void StripAnsiCursorPositionAndEraseSequences()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(children: new[] { b.Text("Hello\u001B[5;10HWorld\u001B[2J!") })
        }, Opts100);

        Assert.DoesNotContain("\u001B[5;10H", output);
        Assert.DoesNotContain("\u001B[2J", output);
        Assert.Equal("HelloWorld!", StripAnsi(output));
    }

    [Fact]
    public void PreserveSgrColorSequencesInText()
    {
        var output = InkApp.RenderToString(b => new[]
        {
            b.Box(children: new[] { b.Text("\u001B[32mgreen\u001B[0m normal") })
        }, Opts100);

        Assert.Contains("\u001B[", output);
        Assert.Equal("green normal", StripAnsi(output));
    }

    [Fact]
    public void PreserveOscHyperlinkSequencesInText()
    {
        var output = RenderText("\u001B]8;;https://example.com\u0007link\u001B]8;;\u0007");

        Assert.Contains("\u001B]8;;", output);
        Assert.Equal("link", StripAnsi(output));
    }

    [Fact]
    public void PreserveOscHyperlinkSequencesWithStTerminator()
    {
        var output = RenderText("\u001B]8;;https://example.com\u001B\\link\u001B]8;;\u001B\\");

        Assert.Contains("\u001B]8;;", output);
        Assert.Contains("\u001B\\", output);
        Assert.Equal("link", StripAnsi(output));
    }

    [Fact]
    public void PreserveC1OscSequencesInText()
    {
        var input = "\u009D8;;https://example.com\u0007link\u009D8;;\u0007";
        var output = RenderText(input);

        Assert.Contains("\u009D8;;https://example.com", output);
        Assert.Contains("\u009D8;;\u0007", output);
        Assert.Equal(input, output);
    }

    [Fact]
    public void PreserveC1OscHyperlinkSequencesWithStTerminator()
    {
        var input = "\u009D8;;https://example.com\u001B\\link\u009D8;;\u001B\\";
        var output = RenderText(input);

        Assert.Contains("\u009D8;;https://example.com", output);
        Assert.Contains("\u001B\\", output);
        Assert.Equal(input, output);
    }

    [Fact]
    public void PreserveSgrSequencesWithColonParameters()
    {
        var output = RenderText("A\u001B[38:2::255:100:0mcolor\u001B[0mB");

        Assert.Contains("\u001B[38:2::255:100:0m", output);
        Assert.Equal("AcolorB", StripAnsi(output));
    }

    [Fact]
    public void StripCompleteNonSgrCsiSequencesWithoutLeakingParameters()
    {
        var input = "A\u001B[>4;2mB\u001B[2 qC";
        var output = RenderText(input);

        Assert.DoesNotContain("4;2m", output);
        Assert.DoesNotContain(" q", output);
        Assert.Equal("ABC", StripAnsi(output));
    }

    [Fact]
    public void StripCompleteC1NonSgrCsiSequencesWithoutLeakingParameters()
    {
        var output = RenderText("A\u009B>4;2mB\u009B2 qC");

        Assert.DoesNotContain("4;2m", output);
        Assert.DoesNotContain(" q", output);
        Assert.Equal("ABC", StripAnsi(output));
    }

    [Fact]
    public void StripCompleteEscControlSequencesWithIntermediates()
    {
        var output = RenderText("A\u001B#8B\u001BcC");

        Assert.DoesNotContain("\u001B#8", output);
        Assert.DoesNotContain("\u001Bc", output);
        Assert.Equal("ABC", StripAnsi(output));
    }

    [Fact]
    public void StripTmuxDcsPassthroughWrappersWithoutLeakingPayload()
    {
        var wrappedHyperlinkStart = "\u001BPtmux;\u001B\u001B]8;;https://example.com\u0007\u001B\\";
        var wrappedHyperlinkEnd = "\u001BPtmux;\u001B\u001B]8;;\u0007\u001B\\";
        var output = RenderText($"{wrappedHyperlinkStart}link{wrappedHyperlinkEnd}");

        Assert.DoesNotContain("tmux;", output);
        Assert.DoesNotContain("\u001BP", output);
        Assert.DoesNotContain("\u001B\\", output);
        Assert.Equal("link", StripAnsi(output));
    }

    [Fact]
    public void StripTmuxDcsPassthroughWrappersWithStTerminatedOscPayload()
    {
        var wrappedHyperlinkStart = "\u001BPtmux;\u001B\u001B]8;;https://example.com\u001B\u001B\\\u001B\\";
        var wrappedHyperlinkEnd = "\u001BPtmux;\u001B\u001B]8;;\u001B\u001B\\\u001B\\";
        var output = RenderText($"{wrappedHyperlinkStart}link{wrappedHyperlinkEnd}");

        Assert.DoesNotContain("tmux;", output);
        Assert.DoesNotContain("\u001B\\", output);
        Assert.Equal("link", StripAnsi(output));
    }

    [Fact]
    public void StripC1DcsControlStringsAsCompleteUnits()
    {
        var output = RenderText("A\u0090payload\u001B\\B\u0090payload\u009CC");

        Assert.DoesNotContain("payload", output);
        Assert.Equal("ABC", StripAnsi(output));
    }

    [Fact]
    public void StripPmAndApcControlStringsAsCompleteUnits()
    {
        var output = RenderText("A\u001B^pm-payload\u001B\\B\u001B_apc-payload\u001B\\C");

        Assert.DoesNotContain("pm-payload", output);
        Assert.DoesNotContain("apc-payload", output);
        Assert.Equal("ABC", StripAnsi(output));
    }

    [Fact]
    public void StripC1PmAndApcControlStringsAsCompleteUnits()
    {
        var output = RenderText("A\u009Epm-payload\u009CB\u009Fapc-payload\u009CC");

        Assert.DoesNotContain("pm-payload", output);
        Assert.DoesNotContain("apc-payload", output);
        Assert.Equal("ABC", StripAnsi(output));
    }

    [Fact]
    public void StripEscSosControlStringsAsCompleteUnits()
    {
        var output = RenderText("A\u001BXpayload\u001B\\B");

        Assert.DoesNotContain("payload", output);
        Assert.Equal("AB", StripAnsi(output));
    }

    [Fact]
    public void StripC1SosControlStringsAsCompleteUnits()
    {
        var output = RenderText("A\u0098payload\u001B\\B\u0098payload\u009CC");

        Assert.DoesNotContain("payload", output);
        Assert.Equal("ABC", StripAnsi(output));
    }

    [Fact]
    public void StripMalformedSosControlStringsToAvoidPayloadLeaks()
    {
        var output = RenderText("A\u001BXpayload\u0007B\u0098payload");

        Assert.DoesNotContain("payload", output);
        Assert.Equal("A", StripAnsi(output));
    }

    [Fact]
    public void PreserveSgrSequencesAroundStrippedSosControlStrings()
    {
        var output = RenderText("A\u001B[32mgreen\u001B[0m\u001BXpayload\u001B\\B");

        Assert.Contains("\u001B[", output);
        Assert.DoesNotContain("payload", output);
        Assert.Equal("AgreenB", StripAnsi(output));
    }

    [Fact]
    public void StripTmuxDcsPassthroughContainingBelUntilFinalStTerminator()
    {
        var input = "A\u001BPtmux;\u001B\u001B]0;title\u0007\u001B\\B";
        var output = RenderText(input);

        Assert.DoesNotContain("tmux;", output);
        Assert.DoesNotContain("title", output);
        Assert.Equal("AB", StripAnsi(output));
    }

    [Fact]
    public void StripIncompleteDcsPassthroughSequencesToAvoidPayloadLeaks()
    {
        var incompleteSequence = "\u001BPtmux;\u001B";
        var output = RenderText($"{incompleteSequence}link");

        Assert.DoesNotContain("tmux;", output);
        Assert.Equal("", StripAnsi(output));
    }

    [Fact]
    public void StripIncompleteC1DcsControlStringsToAvoidPayloadLeaks()
    {
        var output = RenderText("A\u0090payload");

        Assert.DoesNotContain("payload", output);
        Assert.Equal("A", StripAnsi(output));
    }

    [Fact]
    public void StripIncompleteOscControlStringsToAvoidPayloadLeaks()
    {
        var output = RenderText("A\u001B]8;;https://example.comlink");

        Assert.DoesNotContain("https://example.com", output);
        Assert.Equal("A", StripAnsi(output));
    }

    [Fact]
    public void StripIncompleteC1OscControlStringsToAvoidPayloadLeaks()
    {
        var output = RenderText("A\u009D8;;https://example.comlink");

        Assert.DoesNotContain("https://example.com", output);
        Assert.Equal("A", StripAnsi(output));
    }

    [Fact]
    public void StripIncompleteEscControlSequencesWithIntermediatesToAvoidPayloadLeaks()
    {
        var output = RenderText("A\u001B#");

        Assert.DoesNotContain("\u001B#", output);
        Assert.Equal("A", StripAnsi(output));
    }

    [Fact]
    public void StripMalformedEscControlSequencesWithIntermediatesAndNonFinalBytes()
    {
        var output = RenderText("A\u001B#\u0007payload");

        Assert.DoesNotContain("payload", output);
        Assert.Equal("A", StripAnsi(output));
    }

    [Fact]
    public void StripStandaloneSt9cBytesFromTextOutput()
    {
        var output = RenderText("A\u009CB");

        // Use ordinal comparison to avoid .NET culture-sensitive matching issues with C1 chars
        Assert.True(output.IndexOf('\u009C') < 0, "Output should not contain U+009C (ST)");
        Assert.Equal("AB", StripAnsi(output));
    }

    [Fact]
    public void StripStandaloneC1ControlCharactersFromTextOutput()
    {
        var output = RenderText("A\u0085B\u008EC");

        // Use ordinal comparison to avoid .NET culture-sensitive matching issues with C1 chars
        Assert.True(output.IndexOf('\u0085') < 0, "Output should not contain U+0085 (NEL)");
        Assert.True(output.IndexOf('\u008E') < 0, "Output should not contain U+008E (SS2)");
        Assert.Equal("ABC", StripAnsi(output));
    }

    // ── Helper ──────────────────────────────────────────────────────────

    /// <summary>
    /// Strip all ANSI escape sequences from a string.
    /// Handles CSI, OSC, DCS, PM, APC, SOS, ESC sequences, and C1 control chars.
    /// </summary>
    private static string StripAnsi(string input)
    {
        // Use the tokenizer for accurate stripping — only keep Text tokens
        var tokens = Ink.Net.Ansi.AnsiTokenizer.Tokenize(input);
        var sb = new System.Text.StringBuilder(input.Length);
        foreach (var token in tokens)
        {
            if (token.Type == Ink.Net.Ansi.AnsiTokenType.Text)
                sb.Append(token.Value);
        }
        return sb.ToString();
    }
}
