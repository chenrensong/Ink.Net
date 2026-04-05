// -----------------------------------------------------------------------
// <copyright file="AnsiWrapper.cs" company="Ink.Net">
//   ANSI-aware text wrapping — wraps text at specified column width
//   while preserving ANSI escape sequences.
// </copyright>
// -----------------------------------------------------------------------

using System.Text;
using Ink.Net.Ansi;

namespace Ink.Net.Text;

/// <summary>
/// ANSI-aware text wrapping. Wraps text at specified width while preserving ANSI sequences.
/// </summary>
public static class AnsiWrapper
{
    /// <summary>
    /// Wrap text at the specified column width, preserving ANSI escape sequences.
    /// </summary>
    /// <param name="input">Text possibly containing ANSI sequences.</param>
    /// <param name="columns">Maximum line width in columns.</param>
    /// <param name="hard">If true, break words that exceed line width.</param>
    /// <param name="wordWrap">If true, try to break at word boundaries.</param>
    /// <param name="trim">If true, trim leading whitespace on wrapped lines.</param>
    public static string Wrap(string input, int columns, bool hard = true, bool wordWrap = true, bool trim = true)
    {
        if (string.IsNullOrEmpty(input) || columns <= 0) return input;

        var lines = input.Split('\n');
        var result = new StringBuilder();

        for (int lineIdx = 0; lineIdx < lines.Length; lineIdx++)
        {
            if (lineIdx > 0) result.Append('\n');
            string line = lines[lineIdx];

            if (StringWidthHelper.GetStringWidth(StripAnsi(line)) <= columns)
            {
                result.Append(line);
                continue;
            }

            // Need to wrap this line
            WrapLine(line, columns, hard, wordWrap, trim, result);
        }

        return result.ToString();
    }

    private static void WrapLine(string line, int columns, bool hard, bool wordWrap, bool trim, StringBuilder result)
    {
        var tokens = AnsiTokenizer.Tokenize(line);
        int currentWidth = 0;
        string activeStyle = "";
        bool firstWrap = true;

        foreach (var token in tokens)
        {
            if (token.Type != AnsiTokenType.Text)
            {
                result.Append(token.Value);
                if (token.Type == AnsiTokenType.Csi && token.FinalCharacter == "m")
                    activeStyle = token.Value; // Track SGR
                continue;
            }

            foreach (char c in token.Value)
            {
                int charWidth = StringWidthHelper.GetStringWidth(c.ToString());

                if (currentWidth + charWidth > columns)
                {
                    // Wrap
                    if (!firstWrap || currentWidth > 0)
                    {
                        if (activeStyle.Length > 0) result.Append("\x1b[0m");
                        result.Append('\n');
                        if (activeStyle.Length > 0) result.Append(activeStyle);
                    }
                    currentWidth = 0;
                    firstWrap = false;

                    if (trim && c == ' ') continue; // Skip leading space
                }

                result.Append(c);
                currentWidth += charWidth;
            }
        }
    }

    private static string StripAnsi(string text)
    {
        if (!text.Contains('\x1b')) return text;
        var sb = new StringBuilder(text.Length);
        var tokens = AnsiTokenizer.Tokenize(text);
        foreach (var token in tokens)
        {
            if (token.Type == AnsiTokenType.Text)
                sb.Append(token.Value);
        }
        return sb.ToString();
    }
}
