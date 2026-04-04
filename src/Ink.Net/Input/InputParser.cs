// 1:1 port of input-parser.ts
// Parses raw terminal input into discrete input events (key sequences and paste events).

namespace Ink.Net.Input;

/// <summary>
/// Represents a single input event: either a key sequence string or a paste event.
/// </summary>
public readonly struct InputEvent
{
    /// <summary>True when this event represents pasted text.</summary>
    public bool IsPaste { get; }

    /// <summary>
    /// For key events: the raw escape sequence or character.
    /// For paste events: the pasted text content.
    /// </summary>
    public string Value { get; }

    private InputEvent(string value, bool isPaste)
    {
        Value = value;
        IsPaste = isPaste;
    }

    public static InputEvent Key(string sequence) => new(sequence, false);
    public static InputEvent Paste(string text) => new(text, true);

    public override string ToString() => IsPaste ? $"Paste({Value})" : $"Key({Value})";
}

/// <summary>
/// Stateful input parser. Accumulates pending bytes across <see cref="Push"/> calls
/// and splits them into discrete <see cref="InputEvent"/>s.
/// </summary>
public sealed class InputParser
{
    private const char Escape = '\u001B';
    private const string PasteStart = "\u001B[200~";
    private const string PasteEnd = "\u001B[201~";

    private string _pending = "";

    /// <summary>
    /// Feed a chunk of raw terminal input and return the events that could be
    /// fully parsed.  Any incomplete trailing escape sequence is buffered until
    /// the next call.
    /// </summary>
    public List<InputEvent> Push(string chunk)
    {
        var parsed = ParseKeypresses(_pending + chunk);
        _pending = parsed.Pending;
        return parsed.Events;
    }

    /// <summary>
    /// Returns <c>true</c> when there is a pending incomplete escape sequence
    /// that is NOT part of a bracketed paste.
    /// </summary>
    public bool HasPendingEscape()
    {
        return _pending.StartsWith(Escape)
            && !_pending.StartsWith(PasteStart)
            && _pending != "\u001B[200";
    }

    /// <summary>
    /// If the pending buffer starts with an escape, flush it as a single event
    /// and clear the buffer.  Returns <c>null</c> when there is nothing to flush.
    /// </summary>
    public string? FlushPendingEscape()
    {
        if (!_pending.StartsWith(Escape))
            return null;

        var result = _pending;
        _pending = "";
        return result;
    }

    /// <summary>Clear any buffered pending input.</summary>
    public void Reset()
    {
        _pending = "";
    }

    // ── internal parsing helpers ──────────────────────────────────────

    private sealed class ParsedInput
    {
        public List<InputEvent> Events { get; } = new();
        public string Pending { get; set; } = "";
    }

    /// <summary>Result of attempting to parse a single escape/control sequence.</summary>
    private readonly struct ParsedSequence
    {
        public static readonly ParsedSequence PendingResult = new() { IsPending = true };

        /// <summary>True when the input ends mid-sequence (need more data).</summary>
        public bool IsPending { get; init; }

        /// <summary>
        /// True when a valid sequence was recognised.  
        /// False + not pending ⇒ the bytes do not form a known sequence.
        /// </summary>
        public bool IsValid { get; init; }

        public string Sequence { get; init; }
        public int NextIndex { get; init; }
    }

    // ── byte classification (same ranges as the JS version) ──────────

    private static bool IsCsiParameterByte(int b) => b >= 0x30 && b <= 0x3F;
    private static bool IsCsiIntermediateByte(int b) => b >= 0x20 && b <= 0x2F;
    private static bool IsCsiFinalByte(int b) => b >= 0x40 && b <= 0x7E;

    // ── sequence parsers ─────────────────────────────────────────────

    private static ParsedSequence ParseCsiSequence(string input, int startIndex, int prefixLength)
    {
        int csiPayloadStart = startIndex + prefixLength + 1;
        int index = csiPayloadStart;

        for (; index < input.Length; index++)
        {
            int b = char.ConvertToUtf32(input, index);
            // Handle surrogate pairs
            if (char.IsHighSurrogate(input[index]))
            {
                return ParsedSequence.PendingResult;
            }

            if (IsCsiParameterByte(b) || IsCsiIntermediateByte(b))
                continue;

            // Preserve legacy terminal function-key sequences like ESC[[A and ESC[[5~.
            if (b == 0x5B && index == csiPayloadStart)
                continue;

            if (IsCsiFinalByte(b))
            {
                return new ParsedSequence
                {
                    IsValid = true,
                    Sequence = input[startIndex..(index + 1)],
                    NextIndex = index + 1,
                };
            }

            // Not a recognised byte – invalid sequence.
            return default;
        }

        return ParsedSequence.PendingResult;
    }

    private static ParsedSequence ParseSs3Sequence(string input, int startIndex, int prefixLength)
    {
        int nextIndex = startIndex + prefixLength + 2;
        if (nextIndex > input.Length)
            return ParsedSequence.PendingResult;

        int finalByte = input[nextIndex - 1];
        if (!IsCsiFinalByte(finalByte))
            return default;

        return new ParsedSequence
        {
            IsValid = true,
            Sequence = input[startIndex..nextIndex],
            NextIndex = nextIndex,
        };
    }

    private static ParsedSequence ParseControlSequence(string input, int startIndex, int prefixLength)
    {
        int typeIndex = startIndex + prefixLength;
        if (typeIndex >= input.Length)
            return ParsedSequence.PendingResult;

        char sequenceType = input[typeIndex];

        if (sequenceType == '[')
            return ParseCsiSequence(input, startIndex, prefixLength);

        if (sequenceType == 'O')
            return ParseSs3Sequence(input, startIndex, prefixLength);

        return default;
    }

    private static (string Sequence, int NextIndex) ParseEscapedCodePoint(string input, int escapeIndex)
    {
        int nextCodePoint = char.ConvertToUtf32(input, escapeIndex + 1);
        int nextCodePointLength = nextCodePoint > 0xFFFF ? 2 : 1;
        int nextIndex = escapeIndex + 1 + nextCodePointLength;

        return (input[escapeIndex..nextIndex], nextIndex);
    }

    /// <summary>
    /// Parse a complete escape sequence starting at <paramref name="escapeIndex"/>.
    /// Returns (sequence, nextIndex) or ("pending", -1) if more data is needed.
    /// </summary>
    private static (string? Sequence, int NextIndex, bool IsPending) ParseEscapeSequence(
        string input, int escapeIndex)
    {
        if (escapeIndex == input.Length - 1)
            return (null, -1, true);

        char next = input[escapeIndex + 1];

        if (next == Escape)
        {
            if (escapeIndex + 2 >= input.Length)
                return (null, -1, true);

            var doubleEsc = ParseControlSequence(input, escapeIndex, 2);
            if (doubleEsc.IsPending)
                return (null, -1, true);

            if (doubleEsc.IsValid)
                return (doubleEsc.Sequence, doubleEsc.NextIndex, false);

            // Two bare escapes
            return (input[escapeIndex..(escapeIndex + 2)], escapeIndex + 2, false);
        }

        var ctrl = ParseControlSequence(input, escapeIndex, 1);
        if (ctrl.IsPending)
            return (null, -1, true);

        if (ctrl.IsValid)
            return (ctrl.Sequence, ctrl.NextIndex, false);

        var (seq, ni) = ParseEscapedCodePoint(input, escapeIndex);
        return (seq, ni, false);
    }

    /// <summary>
    /// Split a chunk of non-escape text so that backspace bytes (0x7F and 0x08) become
    /// individual events.  Other control characters like \r and \t are NOT split because
    /// they can legitimately appear inside pasted text.
    /// </summary>
    private static void SplitBackspaceBytes(string text, List<InputEvent> events)
    {
        int segmentStart = 0;

        for (int i = 0; i < text.Length; i++)
        {
            char ch = text[i];
            if (ch == '\u007F' || ch == '\u0008')
            {
                if (i > segmentStart)
                    events.Add(InputEvent.Key(text[segmentStart..i]));

                events.Add(InputEvent.Key(ch.ToString()));
                segmentStart = i + 1;
            }
        }

        if (segmentStart < text.Length)
            events.Add(InputEvent.Key(text[segmentStart..]));
    }

    private static ParsedInput ParseKeypresses(string input)
    {
        var result = new ParsedInput();
        int index = 0;

        while (index < input.Length)
        {
            int escapeIndex = input.IndexOf(Escape, index);

            if (escapeIndex == -1)
            {
                SplitBackspaceBytes(input[index..], result.Events);
                result.Pending = "";
                return result;
            }

            if (escapeIndex > index)
                SplitBackspaceBytes(input[index..escapeIndex], result.Events);

            var (sequence, nextIndex, isPending) = ParseEscapeSequence(input, escapeIndex);

            if (isPending)
            {
                result.Pending = input[escapeIndex..];
                return result;
            }

            if (sequence == PasteStart)
            {
                int afterStart = nextIndex;
                int endIndex = input.IndexOf(PasteEnd, afterStart, StringComparison.Ordinal);
                if (endIndex == -1)
                {
                    result.Pending = input[escapeIndex..];
                    return result;
                }

                result.Events.Add(InputEvent.Paste(input[afterStart..endIndex]));
                index = endIndex + PasteEnd.Length;
                continue;
            }

            result.Events.Add(InputEvent.Key(sequence!));
            index = nextIndex;
        }

        result.Pending = "";
        return result;
    }
}
