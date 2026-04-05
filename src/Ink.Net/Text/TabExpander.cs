// -----------------------------------------------------------------------
// <copyright file="TabExpander.cs" company="Ink.Net">
//   Port from Ink (JS) tabstops.ts — Tab character expansion
// </copyright>
// -----------------------------------------------------------------------

using System.Text;
using Ink.Net.Termio;

namespace Ink.Net.Text;

/// <summary>
/// Tab expansion using 8-column intervals (POSIX default).
/// </summary>
public static class TabExpander
{
    private const int DefaultTabInterval = 8;

    /// <summary>Expand tab characters based on column position.</summary>
    public static string ExpandTabs(string text, int interval = DefaultTabInterval)
    {
        if (!text.Contains('\t')) return text;

        var tokenizer = new Tokenizer();
        var tokens = tokenizer.Feed(text);
        tokens.AddRange(tokenizer.Flush());

        var sb = new StringBuilder(text.Length);
        int column = 0;

        foreach (var token in tokens)
        {
            if (token.Type == TokenType.Sequence)
            {
                sb.Append(token.Value);
            }
            else
            {
                foreach (char c in token.Value)
                {
                    if (c == '\t')
                    {
                        int spaces = interval - (column % interval);
                        sb.Append(' ', spaces);
                        column += spaces;
                    }
                    else if (c == '\n')
                    {
                        sb.Append(c);
                        column = 0;
                    }
                    else
                    {
                        sb.Append(c);
                        column += StringWidthHelper.GetStringWidth(c.ToString());
                    }
                }
            }
        }

        return sb.ToString();
    }
}
