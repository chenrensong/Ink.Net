// Tests ported from ansi-tokenizer.ts (26 tests)
using Ink.Net.Ansi;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Tests for <see cref="AnsiTokenizer"/>. Aligned 1:1 with JS ansi-tokenizer.ts.</summary>
public class AnsiTokenizerTests
{
    // ─── Helper ──────────────────────────────────────────────────────

    private static AnsiTokenType[] Types(List<AnsiToken> tokens)
        => tokens.Select(t => t.Type).ToArray();

    // ═══════════════════════════════════════════════════════════════════
    // Plain text
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void TokenizePlainText()
    {
        var tokens = AnsiTokenizer.Tokenize("hello");
        Assert.Single(tokens);
        Assert.Equal(AnsiTokenType.Text, tokens[0].Type);
        Assert.Equal("hello", tokens[0].Value);
    }

    // ═══════════════════════════════════════════════════════════════════
    // CSI sequences
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void TokenizeEscCsiSgrSequence()
    {
        var tokens = AnsiTokenizer.Tokenize("A\u001B[31mB");

        Assert.Equal(
            new[] { AnsiTokenType.Text, AnsiTokenType.Csi, AnsiTokenType.Text },
            Types(tokens));

        Assert.Equal("A", tokens[0].Value);
        Assert.Equal("B", tokens[2].Value);

        Assert.Equal(AnsiTokenType.Csi, tokens[1].Type);
        Assert.Equal("\u001B[31m", tokens[1].Value);
        Assert.Equal("31", tokens[1].ParameterString);
        Assert.Equal("", tokens[1].IntermediateString);
        Assert.Equal("m", tokens[1].FinalCharacter);
    }

    [Fact]
    public void TokenizeC1CsiSequence()
    {
        var tokens = AnsiTokenizer.Tokenize("A\u009B2 qB");
        var csi = tokens[1];

        Assert.Equal(AnsiTokenType.Csi, csi.Type);
        Assert.Equal("\u009B2 q", csi.Value);
        Assert.Equal("2", csi.ParameterString);
        Assert.Equal(" ", csi.IntermediateString);
        Assert.Equal("q", csi.FinalCharacter);
    }

    [Fact]
    public void TokenizeC1SgrCsiSequence()
    {
        var tokens = AnsiTokenizer.Tokenize("A\u009B31mB");

        Assert.Equal(
            new[] { AnsiTokenType.Text, AnsiTokenType.Csi, AnsiTokenType.Text },
            Types(tokens));

        var csi = tokens[1];
        Assert.Equal(AnsiTokenType.Csi, csi.Type);
        Assert.Equal("\u009B31m", csi.Value);
        Assert.Equal("31", csi.ParameterString);
        Assert.Equal("", csi.IntermediateString);
        Assert.Equal("m", csi.FinalCharacter);
    }

    [Fact]
    public void TokenizeIncompleteCsiAsInvalidAndStop()
    {
        var tokens = AnsiTokenizer.Tokenize("A\u001B[");

        Assert.Equal(2, tokens.Count);
        Assert.Equal(AnsiTokenType.Text, tokens[0].Type);
        Assert.Equal("A", tokens[0].Value);
        Assert.Equal(AnsiTokenType.Invalid, tokens[1].Type);
        Assert.Equal("\u001B[", tokens[1].Value);
    }

    [Fact]
    public void TokenizeIncompleteC1CsiAsInvalidAndStop()
    {
        var tokens = AnsiTokenizer.Tokenize("A\u009B31");

        Assert.Equal(2, tokens.Count);
        Assert.Equal(AnsiTokenType.Text, tokens[0].Type);
        Assert.Equal("A", tokens[0].Value);
        Assert.Equal(AnsiTokenType.Invalid, tokens[1].Type);
        Assert.Equal("\u009B31", tokens[1].Value);
    }

    // ═══════════════════════════════════════════════════════════════════
    // OSC sequences
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void TokenizeOscControlStringWithStTerminator()
    {
        var tokens = AnsiTokenizer.Tokenize("A\u001B]8;;https://example.com\u001B\\B");

        Assert.Equal(
            new[] { AnsiTokenType.Text, AnsiTokenType.Osc, AnsiTokenType.Text },
            Types(tokens));

        Assert.Equal("\u001B]8;;https://example.com\u001B\\", tokens[1].Value);
    }

    [Fact]
    public void TokenizeC1OscWithC1StTerminator()
    {
        var tokens = AnsiTokenizer.Tokenize("A\u009D8;;https://example.com\u009CB");

        Assert.Equal(
            new[] { AnsiTokenType.Text, AnsiTokenType.Osc, AnsiTokenType.Text },
            Types(tokens));

        Assert.Equal("\u009D8;;https://example.com\u009C", tokens[1].Value);
    }

    [Fact]
    public void TokenizeC1OscWithEscStTerminator()
    {
        var tokens = AnsiTokenizer.Tokenize("A\u009D8;;https://example.com\u001B\\B");

        Assert.Equal(
            new[] { AnsiTokenType.Text, AnsiTokenType.Osc, AnsiTokenType.Text },
            Types(tokens));

        Assert.Equal("\u009D8;;https://example.com\u001B\\", tokens[1].Value);
    }

    [Fact]
    public void TokenizeC1OscControlStringWithBelTerminator()
    {
        var tokens = AnsiTokenizer.Tokenize("A\u009D8;;https://example.com\u0007B");

        Assert.Equal(
            new[] { AnsiTokenType.Text, AnsiTokenType.Osc, AnsiTokenType.Text },
            Types(tokens));

        Assert.Equal("\u009D8;;https://example.com\u0007", tokens[1].Value);
    }

    [Fact]
    public void TokenizeIncompleteC1OscAsInvalidAndStop()
    {
        var tokens = AnsiTokenizer.Tokenize("A\u009D8;;https://example.com");

        Assert.Equal(2, tokens.Count);
        Assert.Equal(AnsiTokenType.Text, tokens[0].Type);
        Assert.Equal("A", tokens[0].Value);
        Assert.Equal(AnsiTokenType.Invalid, tokens[1].Type);
        Assert.Equal("\u009D8;;https://example.com", tokens[1].Value);
    }

    // ═══════════════════════════════════════════════════════════════════
    // DCS sequences
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void TokenizeTmuxDcsPassthroughAsOneControlStringToken()
    {
        var tokens = AnsiTokenizer.Tokenize(
            "A\u001BPtmux;\u001B\u001B]8;;https://example.com\u001B\u001B\\\u001B\\B");

        Assert.Equal(
            new[] { AnsiTokenType.Text, AnsiTokenType.Dcs, AnsiTokenType.Text },
            Types(tokens));

        var dcs = tokens[1];
        Assert.Equal(AnsiTokenType.Dcs, dcs.Type);
        Assert.StartsWith("\u001BPtmux;", dcs.Value);
        Assert.EndsWith("\u001B\\", dcs.Value);
    }

    [Fact]
    public void TokenizeDcsWithBelInPayloadUntilStTerminator()
    {
        var tokens = AnsiTokenizer.Tokenize("A\u001BPpayload\u0007still-payload\u001B\\B");

        Assert.Equal(
            new[] { AnsiTokenType.Text, AnsiTokenType.Dcs, AnsiTokenType.Text },
            Types(tokens));

        var dcs = tokens[1];
        Assert.Equal(AnsiTokenType.Dcs, dcs.Type);
        Assert.Contains("\u0007", dcs.Value);
        Assert.EndsWith("\u001B\\", dcs.Value);
    }

    // ═══════════════════════════════════════════════════════════════════
    // SOS sequences
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void TokenizeEscSosControlStringWithStTerminator()
    {
        var tokens = AnsiTokenizer.Tokenize("A\u001BXpayload\u001B\\B");

        Assert.Equal(
            new[] { AnsiTokenType.Text, AnsiTokenType.Sos, AnsiTokenType.Text },
            Types(tokens));

        Assert.Equal("\u001BXpayload\u001B\\", tokens[1].Value);
    }

    [Fact]
    public void TokenizeEscSosControlStringWithC1StTerminator()
    {
        var tokens = AnsiTokenizer.Tokenize("A\u001BXpayload\u009CB");

        Assert.Equal(
            new[] { AnsiTokenType.Text, AnsiTokenType.Sos, AnsiTokenType.Text },
            Types(tokens));

        Assert.Equal("\u001BXpayload\u009C", tokens[1].Value);
    }

    [Fact]
    public void TokenizeC1SosControlStringWithC1StTerminator()
    {
        var tokens = AnsiTokenizer.Tokenize("A\u0098payload\u009CB");

        Assert.Equal(
            new[] { AnsiTokenType.Text, AnsiTokenType.Sos, AnsiTokenType.Text },
            Types(tokens));

        Assert.Equal("\u0098payload\u009C", tokens[1].Value);
    }

    [Fact]
    public void TokenizeC1SosControlStringWithEscStTerminator()
    {
        var tokens = AnsiTokenizer.Tokenize("A\u0098payload\u001B\\B");

        Assert.Equal(
            new[] { AnsiTokenType.Text, AnsiTokenType.Sos, AnsiTokenType.Text },
            Types(tokens));

        Assert.Equal("\u0098payload\u001B\\", tokens[1].Value);
    }

    [Fact]
    public void TokenizeEscSosWithBelTerminatorAsInvalidAndStop()
    {
        var tokens = AnsiTokenizer.Tokenize("A\u001BXpayload\u0007B");

        Assert.Equal(2, tokens.Count);
        Assert.Equal(AnsiTokenType.Text, tokens[0].Type);
        Assert.Equal("A", tokens[0].Value);
        Assert.Equal(AnsiTokenType.Invalid, tokens[1].Type);
        Assert.Equal("\u001BXpayload\u0007B", tokens[1].Value);
    }

    [Fact]
    public void TokenizeC1SosWithBelTerminatorAsInvalidAndStop()
    {
        var tokens = AnsiTokenizer.Tokenize("A\u0098payload\u0007B");

        Assert.Equal(2, tokens.Count);
        Assert.Equal(AnsiTokenType.Text, tokens[0].Type);
        Assert.Equal("A", tokens[0].Value);
        Assert.Equal(AnsiTokenType.Invalid, tokens[1].Type);
        Assert.Equal("\u0098payload\u0007B", tokens[1].Value);
    }

    [Fact]
    public void TokenizeIncompleteC1SosAsInvalidAndStop()
    {
        var tokens = AnsiTokenizer.Tokenize("A\u0098payload");

        Assert.Equal(2, tokens.Count);
        Assert.Equal(AnsiTokenType.Text, tokens[0].Type);
        Assert.Equal("A", tokens[0].Value);
        Assert.Equal(AnsiTokenType.Invalid, tokens[1].Type);
        Assert.Equal("\u0098payload", tokens[1].Value);
    }

    [Fact]
    public void TokenizeIncompleteEscSosAsInvalidAndStop()
    {
        var tokens = AnsiTokenizer.Tokenize("A\u001BXpayload");

        Assert.Equal(2, tokens.Count);
        Assert.Equal(AnsiTokenType.Text, tokens[0].Type);
        Assert.Equal("A", tokens[0].Value);
        Assert.Equal(AnsiTokenType.Invalid, tokens[1].Type);
        Assert.Equal("\u001BXpayload", tokens[1].Value);
    }

    [Fact]
    public void TokenizeSosWithEscapedEscInPayloadUntilFinalStTerminator()
    {
        var tokens = AnsiTokenizer.Tokenize("A\u001BXfoo\u001B\u001B\\bar\u001B\\B");

        Assert.Equal(
            new[] { AnsiTokenType.Text, AnsiTokenType.Sos, AnsiTokenType.Text },
            Types(tokens));

        var sos = tokens[1];
        Assert.Contains("\u001B\u001B\\", sos.Value);
        Assert.EndsWith("\u001B\\", sos.Value);
    }

    // ═══════════════════════════════════════════════════════════════════
    // ESC sequences
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void TokenizeIncompleteEscIntermediateSequenceAsInvalidAndStop()
    {
        var tokens = AnsiTokenizer.Tokenize("A\u001B#");

        Assert.Equal(2, tokens.Count);
        Assert.Equal(AnsiTokenType.Text, tokens[0].Type);
        Assert.Equal("A", tokens[0].Value);
        Assert.Equal(AnsiTokenType.Invalid, tokens[1].Type);
        Assert.Equal("\u001B#", tokens[1].Value);
    }

    [Fact]
    public void IgnoreLoneEscBeforeNonFinalByte()
    {
        var tokens = AnsiTokenizer.Tokenize("A\u001B\u0007B");

        Assert.Equal(2, tokens.Count);
        Assert.Equal(AnsiTokenType.Text, tokens[0].Type);
        Assert.Equal("A", tokens[0].Value);
        Assert.Equal(AnsiTokenType.Text, tokens[1].Type);
        Assert.Equal("\u0007B", tokens[1].Value);
    }

    [Fact]
    public void TokenizeEscStSequenceAsEscToken()
    {
        var tokens = AnsiTokenizer.Tokenize("A\u001B\\B");

        Assert.Equal(
            new[] { AnsiTokenType.Text, AnsiTokenType.Esc, AnsiTokenType.Text },
            Types(tokens));

        var esc = tokens[1];
        Assert.Equal(AnsiTokenType.Esc, esc.Type);
        Assert.Equal("\u001B\\", esc.Value);
        Assert.Equal("", esc.IntermediateString);
        Assert.Equal("\\", esc.FinalCharacter);
    }

    // ═══════════════════════════════════════════════════════════════════
    // C1 control characters
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void TokenizeStandaloneC1ControlsAsC1Tokens()
    {
        var tokens = AnsiTokenizer.Tokenize("A\u0085B\u008EC");

        Assert.Equal(
            new[] { AnsiTokenType.Text, AnsiTokenType.C1, AnsiTokenType.Text, AnsiTokenType.C1, AnsiTokenType.Text },
            Types(tokens));

        Assert.Equal("\u0085", tokens[1].Value);
        Assert.Equal("\u008E", tokens[3].Value);
    }

    // ═══════════════════════════════════════════════════════════════════
    // Extra: HasAnsiControlCharacters
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void HasAnsiControlCharactersDetectsEscape()
    {
        Assert.True(AnsiTokenizer.HasAnsiControlCharacters("\x1B[31m"));
        Assert.False(AnsiTokenizer.HasAnsiControlCharacters("plain"));
    }
}
