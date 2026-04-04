// -----------------------------------------------------------------------
// <copyright file="AnsiSanitizer.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) sanitize-ansi.ts
// </copyright>
// -----------------------------------------------------------------------

using System.Text;
using System.Text.RegularExpressions;

namespace Ink.Net.Ansi;

/// <summary>
/// Strip ANSI escape sequences that would conflict with Ink's layout.
/// <para>
/// Preserved: SGR sequences (colors, bold, etc. — end with 'm') and
/// OSC sequences (hyperlinks, etc. — ESC ] or C1 OSC).
/// Stripped: cursor movement, screen clearing, and other control sequences.
/// </para>
/// <para>1:1 port of Ink JS <c>sanitize-ansi.ts</c>.</para>
/// </summary>
public static class AnsiSanitizer
{
    // Corresponds to JS: /^[\d:;]*$/
    private static bool IsSgrParameterString(string s)
    {
        foreach (char c in s)
        {
            if (!char.IsAsciiDigit(c) && c != ':' && c != ';')
                return false;
        }

        return true;
    }

    /// <summary>
    /// Sanitize ANSI sequences in the text, keeping only SGR (color/style) and OSC (hyperlink) sequences.
    /// <para>Corresponds to JS <c>sanitizeAnsi(text)</c>.</para>
    /// </summary>
    public static string Sanitize(string text)
    {
        if (!AnsiTokenizer.HasAnsiControlCharacters(text))
            return text;

        var sb = new StringBuilder(text.Length);

        foreach (var token in AnsiTokenizer.Tokenize(text))
        {
            // Preserve text and OSC tokens
            if (token.Type is AnsiTokenType.Text or AnsiTokenType.Osc)
            {
                sb.Append(token.Value);
                continue;
            }

            // Preserve CSI SGR sequences (final char 'm', no intermediates, valid params)
            if (token.Type == AnsiTokenType.Csi &&
                token.FinalCharacter == "m" &&
                token.IntermediateString == "" &&
                IsSgrParameterString(token.ParameterString))
            {
                sb.Append(token.Value);
            }

            // All other sequences are stripped
        }

        return sb.ToString();
    }
}
