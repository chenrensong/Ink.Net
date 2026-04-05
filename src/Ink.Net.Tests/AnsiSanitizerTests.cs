// Tests ported from sanitize-ansi.ts (29 tests)
using System.Text;
using Ink.Net.Ansi;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Tests for <see cref="AnsiSanitizer"/>. Aligned with JS sanitize-ansi.ts.</summary>
public class AnsiSanitizerTests
{
    // ─── Helper ──────────────────────────────────────────────────────────

    /// <summary>
    /// Equivalent to JS strip-ansi: removes all ANSI escape sequences, keeping only text.
    /// </summary>
    private static string StripAnsi(string text)
    {
        if (!text.Contains('\x1b') && !AnsiTokenizer.HasAnsiControlCharacters(text))
            return text;

        var sb = new StringBuilder(text.Length);
        foreach (var token in AnsiTokenizer.Tokenize(text))
        {
            if (token.Type == AnsiTokenType.Text)
                sb.Append(token.Value);
        }
        return sb.ToString();
    }

    // ═══════════════════════════════════════════════════════════════════
    // Plain text & SGR
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void PreservePlainText()
    {
        Assert.Equal("hello", AnsiSanitizer.Sanitize("hello"));
    }

    [Fact]
    public void PreserveSgrSequences()
    {
        string output = AnsiSanitizer.Sanitize("A\u001B[38:2::255:100:0mcolor\u001B[0mB");

        Assert.Contains("\u001B[38:2::255:100:0m", output);
        Assert.Equal("AcolorB", StripAnsi(output));
    }

    // ═══════════════════════════════════════════════════════════════════
    // OSC hyperlinks
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void PreserveOscHyperlinks()
    {
        string output = AnsiSanitizer.Sanitize(
            "\u001B]8;;https://example.com\u001B\\link\u001B]8;;\u001B\\");

        Assert.Contains("\u001B]8;;https://example.com", output);
        Assert.Equal("link", StripAnsi(output));
    }

    [Fact]
    public void PreserveOscHyperlinks_TerminatedByC1ST()
    {
        string output = AnsiSanitizer.Sanitize(
            "\u001B]8;;https://example.com\u009Clink\u001B]8;;\u009C");

        Assert.Contains("\u001B]8;;https://example.com\u009C", output);
        Assert.Equal("link", StripAnsi(output));
    }

    [Fact]
    public void PreserveC1OscHyperlinks_TerminatedByC1ST()
    {
        string input = "\u009D8;;https://example.com\u009Clink\u009D8;;\u009C";
        string output = AnsiSanitizer.Sanitize(input);

        Assert.Contains("\u009D8;;https://example.com\u009C", output);
        Assert.Equal(input, output);
    }

    [Fact]
    public void PreserveC1OscHyperlinks_TerminatedByEscST()
    {
        string input = "\u009D8;;https://example.com\u001B\\link\u009D8;;\u001B\\";
        string output = AnsiSanitizer.Sanitize(input);

        Assert.Contains("\u009D8;;https://example.com\u001B\\", output);
        Assert.Equal(input, output);
    }

    [Fact]
    public void PreserveC1OscHyperlinks_TerminatedByBEL()
    {
        string input = "\u009D8;;https://example.com\u0007link\u009D8;;\u0007";
        string output = AnsiSanitizer.Sanitize(input);

        Assert.Contains("\u009D8;;https://example.com\u0007", output);
        Assert.Equal(input, output);
    }

    // ═══════════════════════════════════════════════════════════════════
    // CSI stripping / preservation
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void StripNonSgrCsiSequencesAsCompleteUnits()
    {
        string output = AnsiSanitizer.Sanitize("A\u001B[>4;2mB\u001B[2 qC");

        Assert.DoesNotContain("4;2m", output);
        Assert.DoesNotContain(" q", output);
        Assert.Equal("ABC", StripAnsi(output));
    }

    [Fact]
    public void StripC1NonSgrCsiSequencesAsCompleteUnits()
    {
        string output = AnsiSanitizer.Sanitize("A\u009B>4;2mB\u009B2 qC");

        Assert.DoesNotContain("4;2m", output);
        Assert.DoesNotContain(" q", output);
        Assert.Equal("ABC", StripAnsi(output));
    }

    [Fact]
    public void PreserveC1SgrCsiSequences()
    {
        string output = AnsiSanitizer.Sanitize("A\u009B31mgreen\u009B0mB");

        Assert.Contains("\u009B31m", output);
        Assert.Equal("AgreenB", StripAnsi(output));
    }

    [Fact]
    public void StripPrivateParameterMSequencesThatAreNotSgr()
    {
        string output = AnsiSanitizer.Sanitize("A\u001B[>4;2mB");

        Assert.DoesNotContain("\u001B[>4;2m", output);
        Assert.Equal("AB", StripAnsi(output));
    }

    // ═══════════════════════════════════════════════════════════════════
    // DCS sequences
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void StripTmuxDcsPassthroughWrappersWithEscapedStPayloadTerminators()
    {
        string wrappedHyperlinkStart =
            "\u001BPtmux;\u001B\u001B]8;;https://example.com\u001B\u001B\\\u001B\\";
        string wrappedHyperlinkEnd =
            "\u001BPtmux;\u001B\u001B]8;;\u001B\u001B\\\u001B\\";
        string output = AnsiSanitizer.Sanitize(
            $"{wrappedHyperlinkStart}link{wrappedHyperlinkEnd}");

        Assert.DoesNotContain("tmux;", output);
        Assert.DoesNotContain("\u001BP", output);
        Assert.Equal("link", StripAnsi(output));
    }

    [Fact]
    public void StripIncompleteDcsPassthroughSequencesToAvoidPayloadLeaks()
    {
        string output = AnsiSanitizer.Sanitize("A\u001BPtmux;\u001Blink");

        Assert.DoesNotContain("tmux;", output);
        Assert.Equal("A", StripAnsi(output));
    }

    [Fact]
    public void StripDcsControlStringsWithBelInPayloadUntilStTerminator()
    {
        string output = AnsiSanitizer.Sanitize("A\u001BPpayload\u0007still-payload\u001B\\B");

        Assert.DoesNotContain("payload", output);
        Assert.DoesNotContain("still-payload", output);
        Assert.Equal("AB", StripAnsi(output));
    }

    // ═══════════════════════════════════════════════════════════════════
    // SOS sequences
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void StripEscSosControlStringsAsCompleteUnits()
    {
        string output = AnsiSanitizer.Sanitize("A\u001BXpayload\u001B\\B");

        Assert.DoesNotContain("payload", output);
        Assert.Equal("AB", StripAnsi(output));
    }

    [Fact]
    public void StripEscSosControlStringsWithC1StTerminator()
    {
        string output = AnsiSanitizer.Sanitize("A\u001BXpayload\u009CB");

        Assert.DoesNotContain("payload", output);
        Assert.Equal("AB", StripAnsi(output));
    }

    [Fact]
    public void StripC1SosControlStringsAsCompleteUnitsWithC1St()
    {
        string output = AnsiSanitizer.Sanitize("A\u0098payload\u009CB");

        Assert.DoesNotContain("payload", output);
        Assert.Equal("AB", StripAnsi(output));
    }

    [Fact]
    public void StripC1SosControlStringsAsCompleteUnitsWithEscSt()
    {
        string output = AnsiSanitizer.Sanitize("A\u0098payload\u001B\\B");

        Assert.DoesNotContain("payload", output);
        Assert.Equal("AB", StripAnsi(output));
    }

    [Fact]
    public void StripEscSosWithBelTerminatorAsMalformedControlString()
    {
        string output = AnsiSanitizer.Sanitize("A\u001BXpayload\u0007B");

        Assert.DoesNotContain("payload", output);
        Assert.Equal("A", StripAnsi(output));
    }

    [Fact]
    public void StripC1SosWithBelTerminatorAsMalformedControlString()
    {
        string output = AnsiSanitizer.Sanitize("A\u0098payload\u0007B");

        Assert.DoesNotContain("payload", output);
        Assert.Equal("A", StripAnsi(output));
    }

    [Fact]
    public void StripIncompleteEscSosControlStringsToAvoidPayloadLeaks()
    {
        string output = AnsiSanitizer.Sanitize("A\u001BXpayload");

        Assert.DoesNotContain("payload", output);
        Assert.Equal("A", StripAnsi(output));
    }

    [Fact]
    public void StripIncompleteC1SosControlStringsToAvoidPayloadLeaks()
    {
        string output = AnsiSanitizer.Sanitize("A\u0098payload");

        Assert.DoesNotContain("payload", output);
        Assert.Equal("A", StripAnsi(output));
    }

    [Fact]
    public void StripSosWithEscapedEscInPayloadUntilFinalStTerminator()
    {
        string output = AnsiSanitizer.Sanitize("A\u001BXfoo\u001B\u001B\\bar\u001B\\B");

        Assert.DoesNotContain("foo", output);
        Assert.DoesNotContain("bar", output);
        Assert.Equal("AB", StripAnsi(output));
    }

    [Fact]
    public void PreserveSgrAroundStrippedSosControlStrings()
    {
        string output = AnsiSanitizer.Sanitize("A\u001B[31mR\u001B[0m\u001BXpayload\u001B\\B");

        Assert.Contains("\u001B[31m", output);
        Assert.Contains("\u001B[0m", output);
        Assert.DoesNotContain("payload", output);
        Assert.Equal("ARB", StripAnsi(output));
    }

    // ═══════════════════════════════════════════════════════════════════
    // Edge cases
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void StripEscStSequences()
    {
        string output = AnsiSanitizer.Sanitize("A\u001B\\B");

        Assert.DoesNotContain("\u001B\\", output);
        Assert.Equal("AB", StripAnsi(output));
    }

    [Fact]
    public void StripMalformedEscControlSequencesWithIntermediatesAndNonFinalBytes()
    {
        string output = AnsiSanitizer.Sanitize("A\u001B#\u0007payload");

        Assert.DoesNotContain("payload", output);
        Assert.Equal("A", StripAnsi(output));
    }

    [Fact]
    public void StripIncompleteCsiAfterPreservingPriorSgrContent()
    {
        string output = AnsiSanitizer.Sanitize("A\u001B[31mB\u001B[");

        Assert.Contains("\u001B[31m", output);
        Assert.Equal("AB", StripAnsi(output));
    }

    [Fact]
    public void StripStandaloneStBytes()
    {
        string output = AnsiSanitizer.Sanitize("A\u009CB");

        // Use ordinal comparison — C1 chars confuse culture-sensitive Contains
        Assert.False(output.Contains("\u009C", StringComparison.Ordinal));
        Assert.Equal("AB", StripAnsi(output));
    }

    [Fact]
    public void StripStandaloneC1ControlCharacters()
    {
        string output = AnsiSanitizer.Sanitize("A\u0085B\u008EC");

        Assert.False(output.Contains("\u0085", StringComparison.Ordinal));
        Assert.False(output.Contains("\u008E", StringComparison.Ordinal));
        Assert.Equal("ABC", StripAnsi(output));
    }
}
