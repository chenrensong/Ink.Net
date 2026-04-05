// -----------------------------------------------------------------------
// <copyright file="Tokenizer.cs" company="Ink.Net">
//   Port from Ink (JS) termio/tokenize.ts — Escape sequence tokenizer
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Termio;

/// <summary>Token from the tokenizer.</summary>
public readonly record struct Token(TokenType Type, string Value);

/// <summary>Token type.</summary>
public enum TokenType : byte { Text, Sequence }

/// <summary>
/// Streaming escape sequence tokenizer. Splits terminal input into text and escape sequence tokens.
/// </summary>
public sealed class Tokenizer
{
    private enum State { Ground, Escape, EscapeIntermediate, Csi, Ss3, Osc, Dcs }

    private State _state = State.Ground;
    private string _buffer = "";

    /// <summary>Feed input and get resulting tokens.</summary>
    public List<Token> Feed(string input)
    {
        var tokens = new List<Token>();
        var data = _buffer + input;
        int i = 0, textStart = 0, seqStart = 0;
        _buffer = "";
        _state = _buffer.Length > 0 ? _state : State.Ground;

        while (i < data.Length)
        {
            int code = data[i];
            switch (_state)
            {
                case State.Ground:
                    if (code == 0x1B)
                    {
                        if (i > textStart) tokens.Add(new Token(TokenType.Text, data[textStart..i]));
                        seqStart = i;
                        _state = State.Escape;
                        i++;
                    }
                    else i++;
                    break;

                case State.Escape:
                    if (code == 0x5B) { _state = State.Csi; i++; } // [
                    else if (code == 0x5D) { _state = State.Osc; i++; } // ]
                    else if (code == 0x50 || code == 0x5F) { _state = State.Dcs; i++; } // P or _
                    else if (code == 0x4F) { _state = State.Ss3; i++; } // O
                    else if (code >= 0x20 && code <= 0x2F) { _state = State.EscapeIntermediate; i++; }
                    else if (code >= 0x30 && code <= 0x7E)
                    {
                        i++;
                        tokens.Add(new Token(TokenType.Sequence, data[seqStart..i]));
                        _state = State.Ground; textStart = i;
                    }
                    else { _state = State.Ground; textStart = seqStart; }
                    break;

                case State.EscapeIntermediate:
                    if (code >= 0x20 && code <= 0x2F) i++;
                    else if (code >= 0x30 && code <= 0x7E)
                    {
                        i++;
                        tokens.Add(new Token(TokenType.Sequence, data[seqStart..i]));
                        _state = State.Ground; textStart = i;
                    }
                    else { _state = State.Ground; textStart = seqStart; }
                    break;

                case State.Csi:
                    if (code >= 0x40 && code <= 0x7E)
                    {
                        i++;
                        tokens.Add(new Token(TokenType.Sequence, data[seqStart..i]));
                        _state = State.Ground; textStart = i;
                    }
                    else if ((code >= 0x30 && code <= 0x3F) || (code >= 0x20 && code <= 0x2F)) i++;
                    else { _state = State.Ground; textStart = seqStart; }
                    break;

                case State.Ss3:
                    if (code >= 0x40 && code <= 0x7E)
                    {
                        i++;
                        tokens.Add(new Token(TokenType.Sequence, data[seqStart..i]));
                        _state = State.Ground; textStart = i;
                    }
                    else { _state = State.Ground; textStart = seqStart; }
                    break;

                case State.Osc:
                    if (code == 0x07) // BEL
                    {
                        i++;
                        tokens.Add(new Token(TokenType.Sequence, data[seqStart..i]));
                        _state = State.Ground; textStart = i;
                    }
                    else if (code == 0x1B && i + 1 < data.Length && data[i + 1] == '\\')
                    {
                        i += 2;
                        tokens.Add(new Token(TokenType.Sequence, data[seqStart..i]));
                        _state = State.Ground; textStart = i;
                    }
                    else i++;
                    break;

                case State.Dcs:
                    if (code == 0x07)
                    {
                        i++;
                        tokens.Add(new Token(TokenType.Sequence, data[seqStart..i]));
                        _state = State.Ground; textStart = i;
                    }
                    else if (code == 0x1B && i + 1 < data.Length && data[i + 1] == '\\')
                    {
                        i += 2;
                        tokens.Add(new Token(TokenType.Sequence, data[seqStart..i]));
                        _state = State.Ground; textStart = i;
                    }
                    else i++;
                    break;
            }
        }

        if (_state == State.Ground)
        {
            if (textStart < data.Length) tokens.Add(new Token(TokenType.Text, data[textStart..]));
        }
        else
        {
            _buffer = data[seqStart..];
        }

        return tokens;
    }

    /// <summary>Flush any buffered incomplete sequence.</summary>
    public List<Token> Flush()
    {
        if (_buffer.Length == 0) return new List<Token>();
        var result = new List<Token> { new Token(TokenType.Sequence, _buffer) };
        _buffer = "";
        _state = State.Ground;
        return result;
    }

    /// <summary>Reset tokenizer state.</summary>
    public void Reset() { _state = State.Ground; _buffer = ""; }
}
