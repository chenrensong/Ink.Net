using Ink.Net.Ansi;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Tests for <see cref="AnsiTokenizer"/>.</summary>
public class AnsiTokenizerTests
{
    [Fact]
    public void PlainTextReturnsOneTextToken()
    {
        var tokens = AnsiTokenizer.Tokenize("Hello World");
        Assert.Single(tokens);
        Assert.Equal(AnsiTokenType.Text, tokens[0].Type);
        Assert.Equal("Hello World", tokens[0].Value);
    }

    [Fact]
    public void CsiSequenceIsParsed()
    {
        // SGR sequence: \x1B[31m = red foreground
        var tokens = AnsiTokenizer.Tokenize("\x1B[31mHello\x1B[0m");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(AnsiTokenType.Csi, tokens[0].Type);
        Assert.Equal("31", tokens[0].ParameterString);
        Assert.Equal("m", tokens[0].FinalCharacter);
        Assert.Equal(AnsiTokenType.Text, tokens[1].Type);
        Assert.Equal("Hello", tokens[1].Value);
        Assert.Equal(AnsiTokenType.Csi, tokens[2].Type);
        Assert.Equal("0", tokens[2].ParameterString);
    }

    [Fact]
    public void OscSequenceIsParsed()
    {
        // OSC hyperlink: ESC ] 8 ; ; url BEL text ESC ] 8 ; ; BEL
        string link = "\x1B]8;;https://example.com\x07Link\x1B]8;;\x07";
        var tokens = AnsiTokenizer.Tokenize(link);
        Assert.True(tokens.Count >= 3);
        Assert.Equal(AnsiTokenType.Osc, tokens[0].Type);
        Assert.Equal(AnsiTokenType.Text, tokens[1].Type);
        Assert.Equal("Link", tokens[1].Value);
    }

    [Fact]
    public void HasAnsiControlCharactersDetectsEscape()
    {
        Assert.True(AnsiTokenizer.HasAnsiControlCharacters("\x1B[31m"));
        Assert.False(AnsiTokenizer.HasAnsiControlCharacters("plain"));
    }

    [Fact]
    public void MalformedSequenceMarkedAsInvalid()
    {
        // Lone ESC at end
        var tokens = AnsiTokenizer.Tokenize("abc\x1B");
        Assert.True(tokens.Count >= 1);
        Assert.Contains(tokens, t => t.Type == AnsiTokenType.Invalid);
    }

    [Fact]
    public void EscSequenceIsParsed()
    {
        // ESC sequence: ESC 7 (Save cursor)
        var tokens = AnsiTokenizer.Tokenize("\x1B" + "7");
        Assert.Single(tokens);
        Assert.Equal(AnsiTokenType.Esc, tokens[0].Type);
        Assert.Equal("7", tokens[0].FinalCharacter);
    }
}
