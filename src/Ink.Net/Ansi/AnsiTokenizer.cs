// -----------------------------------------------------------------------
// <copyright file="AnsiTokenizer.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) ansi-tokenizer.ts
// </copyright>
// -----------------------------------------------------------------------

using System.Runtime.CompilerServices;

namespace Ink.Net.Ansi;

// ═══════════════════════════════════════════════════════════════════════
//  Token types — mirror JS AnsiToken union
// ═══════════════════════════════════════════════════════════════════════

/// <summary>Token type tag. Corresponds to JS <c>AnsiToken.type</c>.</summary>
public enum AnsiTokenType : byte
{
    Text,
    Csi,
    Esc,
    Osc,
    Dcs,
    Pm,
    Apc,
    Sos,
    St,
    C1,
    Invalid,
}

/// <summary>
/// A single ANSI token produced by <see cref="AnsiTokenizer"/>.
/// <para>1:1 corresponds to JS <c>AnsiToken</c> union type.</para>
/// </summary>
public readonly struct AnsiToken
{
    /// <summary>Token type.</summary>
    public AnsiTokenType Type { get; }

    /// <summary>Raw text value of the token.</summary>
    public string Value { get; }

    /// <summary>CSI / ESC parameter string. Only meaningful for <see cref="AnsiTokenType.Csi"/>.</summary>
    public string ParameterString { get; }

    /// <summary>CSI / ESC intermediate string. Only meaningful for <see cref="AnsiTokenType.Csi"/> and <see cref="AnsiTokenType.Esc"/>.</summary>
    public string IntermediateString { get; }

    /// <summary>CSI / ESC final character. Only meaningful for <see cref="AnsiTokenType.Csi"/> and <see cref="AnsiTokenType.Esc"/>.</summary>
    public string FinalCharacter { get; }

    public AnsiToken(AnsiTokenType type, string value,
        string parameterString = "", string intermediateString = "", string finalCharacter = "")
    {
        Type = type;
        Value = value;
        ParameterString = parameterString;
        IntermediateString = intermediateString;
        FinalCharacter = finalCharacter;
    }
}

/// <summary>
/// ANSI escape sequence tokenizer.
/// <para>1:1 port of Ink JS <c>ansi-tokenizer.ts</c>.</para>
/// <para>
/// Handles CSI, ESC, OSC, DCS, PM, APC, SOS, ST, and C1 control sequences
/// per ECMA-48 specification.
/// </para>
/// </summary>
public static class AnsiTokenizer
{
    // ─── C0 / C1 constants ───────────────────────────────────────────
    private const char BellCharacter = '\u0007';
    private const char EscapeCharacter = '\u001B';
    private const char StringTerminatorCharacter = '\u009C';
    private const char CsiCharacter = '\u009B';
    private const char OscCharacter = '\u009D';
    private const char DcsCharacter = '\u0090';
    private const char PmCharacter = '\u009E';
    private const char ApcCharacter = '\u009F';
    private const char SosCharacter = '\u0098';

    // ─── Character class predicates ──────────────────────────────────

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsCsiParameterCharacter(char c) => c >= 0x30 && c <= 0x3F;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsCsiIntermediateCharacter(char c) => c >= 0x20 && c <= 0x2F;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsCsiFinalCharacter(char c) => c >= 0x40 && c <= 0x7E;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsEscapeIntermediateCharacter(char c) => c >= 0x20 && c <= 0x2F;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsEscapeFinalCharacter(char c) => c >= 0x30 && c <= 0x7E;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsC1ControlCharacter(char c) => c >= 0x80 && c <= 0x9F;

    // ─── CSI sequence reader ─────────────────────────────────────────

    /// <summary>
    /// Read a CSI sequence starting at <paramref name="fromIndex"/>.
    /// Returns null if the sequence is malformed.
    /// </summary>
    private static (int EndIndex, string ParameterString, string IntermediateString, string FinalCharacter)?
        ReadCsiSequence(string text, int fromIndex)
    {
        int index = fromIndex;

        // Read parameter bytes (0x30-0x3F)
        while (index < text.Length && IsCsiParameterCharacter(text[index]))
            index++;

        string parameterString = text[fromIndex..index];
        int intermediateStartIndex = index;

        // Read intermediate bytes (0x20-0x2F)
        while (index < text.Length && IsCsiIntermediateCharacter(text[index]))
            index++;

        string intermediateString = text[intermediateStartIndex..index];

        // Read final byte (0x40-0x7E)
        if (index >= text.Length || !IsCsiFinalCharacter(text[index]))
            return null;

        string finalCharacter = text[index].ToString();
        return (index + 1, parameterString, intermediateString, finalCharacter);
    }

    // ─── Control string terminator finder ────────────────────────────

    private static int? FindControlStringTerminatorIndex(string text, int fromIndex, bool allowBellTerminator)
    {
        for (int index = fromIndex; index < text.Length; index++)
        {
            char c = text[index];

            if (allowBellTerminator && c == BellCharacter)
                return index + 1;

            if (c == StringTerminatorCharacter)
                return index + 1;

            if (c == EscapeCharacter)
            {
                if (index + 1 < text.Length)
                {
                    char following = text[index + 1];

                    // Tmux escapes ESC bytes in payload as ESC ESC.
                    if (following == EscapeCharacter)
                    {
                        index++;
                        continue;
                    }

                    if (following == '\\')
                        return index + 2;
                }
            }
        }

        return null;
    }

    // ─── ESC sequence reader ─────────────────────────────────────────

    private static (int EndIndex, string IntermediateString, string FinalCharacter)?
        ReadEscapeSequence(string text, int fromIndex)
    {
        int index = fromIndex;

        while (index < text.Length && IsEscapeIntermediateCharacter(text[index]))
            index++;

        string intermediateString = text[fromIndex..index];

        if (index >= text.Length || !IsEscapeFinalCharacter(text[index]))
            return null;

        string finalCharacter = text[index].ToString();
        return (index + 1, intermediateString, finalCharacter);
    }

    // ─── Control string type mapping ─────────────────────────────────

    private static (AnsiTokenType Type, bool AllowBellTerminator)?
        GetControlStringFromEscapeIntroducer(char c)
    {
        return c switch
        {
            ']' => (AnsiTokenType.Osc, true),
            'P' => (AnsiTokenType.Dcs, false),
            '^' => (AnsiTokenType.Pm, false),
            '_' => (AnsiTokenType.Apc, false),
            'X' => (AnsiTokenType.Sos, false),
            _ => null,
        };
    }

    private static (AnsiTokenType Type, bool AllowBellTerminator)?
        GetControlStringFromC1Introducer(char c)
    {
        return c switch
        {
            OscCharacter => (AnsiTokenType.Osc, true),
            DcsCharacter => (AnsiTokenType.Dcs, false),
            PmCharacter => (AnsiTokenType.Pm, false),
            ApcCharacter => (AnsiTokenType.Apc, false),
            SosCharacter => (AnsiTokenType.Sos, false),
            _ => null,
        };
    }

    // ─── Public API ──────────────────────────────────────────────────

    /// <summary>
    /// Quick check whether the text contains any ANSI control characters.
    /// <para>Corresponds to JS <c>hasAnsiControlCharacters(text)</c>.</para>
    /// </summary>
    public static bool HasAnsiControlCharacters(string text)
    {
        if (text.Contains(EscapeCharacter))
            return true;

        foreach (char c in text)
        {
            if (IsC1ControlCharacter(c))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Tokenize a string into ANSI tokens.
    /// <para>1:1 corresponds to JS <c>tokenizeAnsi(text)</c>.</para>
    /// </summary>
    public static List<AnsiToken> Tokenize(string text)
    {
        if (!HasAnsiControlCharacters(text))
            return [new AnsiToken(AnsiTokenType.Text, text)];

        var tokens = new List<AnsiToken>();
        int textStartIndex = 0;

        for (int index = 0; index < text.Length;)
        {
            char c = text[index];

            // ── ESC-introduced sequences ─────────────────────────────
            if (c == EscapeCharacter)
            {
                if (index + 1 >= text.Length)
                    return MalformedFromIndex(tokens, text, textStartIndex, index);

                char following = text[index + 1];

                // ESC [ ... → CSI sequence
                if (following == '[')
                {
                    var csi = ReadCsiSequence(text, index + 2);
                    if (csi is null)
                        return MalformedFromIndex(tokens, text, textStartIndex, index);

                    if (index > textStartIndex)
                        tokens.Add(new AnsiToken(AnsiTokenType.Text, text[textStartIndex..index]));

                    tokens.Add(new AnsiToken(
                        AnsiTokenType.Csi,
                        text[index..csi.Value.EndIndex],
                        csi.Value.ParameterString,
                        csi.Value.IntermediateString,
                        csi.Value.FinalCharacter));

                    index = csi.Value.EndIndex;
                    textStartIndex = index;
                    continue;
                }

                // ESC ] / P / ^ / _ / X → control string
                var escCtrl = GetControlStringFromEscapeIntroducer(following);
                if (escCtrl is not null)
                {
                    var termIdx = FindControlStringTerminatorIndex(text, index + 2, escCtrl.Value.AllowBellTerminator);
                    if (termIdx is null)
                        return MalformedFromIndex(tokens, text, textStartIndex, index);

                    if (index > textStartIndex)
                        tokens.Add(new AnsiToken(AnsiTokenType.Text, text[textStartIndex..index]));

                    tokens.Add(new AnsiToken(escCtrl.Value.Type, text[index..termIdx.Value]));
                    index = termIdx.Value;
                    textStartIndex = index;
                    continue;
                }

                // ESC <intermediate>* <final> → ESC sequence
                var esc = ReadEscapeSequence(text, index + 1);
                if (esc is null)
                {
                    // Incomplete escape sequences with intermediates are malformed.
                    if (IsEscapeIntermediateCharacter(following))
                        return MalformedFromIndex(tokens, text, textStartIndex, index);

                    // Ignore lone ESC and continue.
                    if (index > textStartIndex)
                        tokens.Add(new AnsiToken(AnsiTokenType.Text, text[textStartIndex..index]));

                    index++;
                    textStartIndex = index;
                    continue;
                }

                if (index > textStartIndex)
                    tokens.Add(new AnsiToken(AnsiTokenType.Text, text[textStartIndex..index]));

                tokens.Add(new AnsiToken(
                    AnsiTokenType.Esc,
                    text[index..esc.Value.EndIndex],
                    intermediateString: esc.Value.IntermediateString,
                    finalCharacter: esc.Value.FinalCharacter));

                index = esc.Value.EndIndex;
                textStartIndex = index;
                continue;
            }

            // ── C1 CSI (0x9B) ────────────────────────────────────────
            if (c == CsiCharacter)
            {
                var csi = ReadCsiSequence(text, index + 1);
                if (csi is null)
                    return MalformedFromIndex(tokens, text, textStartIndex, index);

                if (index > textStartIndex)
                    tokens.Add(new AnsiToken(AnsiTokenType.Text, text[textStartIndex..index]));

                tokens.Add(new AnsiToken(
                    AnsiTokenType.Csi,
                    text[index..csi.Value.EndIndex],
                    csi.Value.ParameterString,
                    csi.Value.IntermediateString,
                    csi.Value.FinalCharacter));

                index = csi.Value.EndIndex;
                textStartIndex = index;
                continue;
            }

            // ── C1 control string introducers ────────────────────────
            var c1Ctrl = GetControlStringFromC1Introducer(c);
            if (c1Ctrl is not null)
            {
                var termIdx = FindControlStringTerminatorIndex(text, index + 1, c1Ctrl.Value.AllowBellTerminator);
                if (termIdx is null)
                    return MalformedFromIndex(tokens, text, textStartIndex, index);

                if (index > textStartIndex)
                    tokens.Add(new AnsiToken(AnsiTokenType.Text, text[textStartIndex..index]));

                tokens.Add(new AnsiToken(c1Ctrl.Value.Type, text[index..termIdx.Value]));
                index = termIdx.Value;
                textStartIndex = index;
                continue;
            }

            // ── ST (0x9C) ────────────────────────────────────────────
            if (c == StringTerminatorCharacter)
            {
                if (index > textStartIndex)
                    tokens.Add(new AnsiToken(AnsiTokenType.Text, text[textStartIndex..index]));

                tokens.Add(new AnsiToken(AnsiTokenType.St, c.ToString()));
                index++;
                textStartIndex = index;
                continue;
            }

            // ── Other C1 controls ────────────────────────────────────
            if (IsC1ControlCharacter(c))
            {
                if (index > textStartIndex)
                    tokens.Add(new AnsiToken(AnsiTokenType.Text, text[textStartIndex..index]));

                tokens.Add(new AnsiToken(AnsiTokenType.C1, c.ToString()));
                index++;
                textStartIndex = index;
                continue;
            }

            index++;
        }

        if (textStartIndex < text.Length)
            tokens.Add(new AnsiToken(AnsiTokenType.Text, text[textStartIndex..]));

        return tokens;
    }

    // ─── Malformed helper ────────────────────────────────────────────

    private static List<AnsiToken> MalformedFromIndex(
        List<AnsiToken> tokens, string text, int textStartIndex, int fromIndex)
    {
        if (fromIndex > textStartIndex)
            tokens.Add(new AnsiToken(AnsiTokenType.Text, text[textStartIndex..fromIndex]));

        // Treat the remainder as invalid so callers can drop it as one unsafe unit.
        tokens.Add(new AnsiToken(AnsiTokenType.Invalid, text[fromIndex..]));
        return tokens;
    }
}
