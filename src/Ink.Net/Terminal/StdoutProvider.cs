// -----------------------------------------------------------------------
// <copyright file="StdoutProvider.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) StdoutContext.ts + useStdout.ts
//   Provides access to stdout stream and a write method that preserves Ink output.
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Terminal;

/// <summary>
/// Provides access to the stdout stream and a write method that preserves Ink output.
/// <para>Corresponds to JS <c>useStdout()</c> / <c>StdoutContext</c>.</para>
/// </summary>
public sealed class StdoutProvider
{
    private readonly TextWriter _writer;

    internal StdoutProvider(TextWriter writer)
    {
        _writer = writer;
    }

    /// <summary>
    /// The stdout stream (TextWriter).
    /// <para>Corresponds to JS <c>stdout</c> property.</para>
    /// </summary>
    public TextWriter Writer => _writer;

    /// <summary>
    /// Raised when <see cref="Write"/> is called, allowing the host (InkApplication)
    /// to clear/restore Ink output around the external write.
    /// </summary>
    internal event Action<string>? WriteRequested;

    /// <summary>
    /// Write any string to stdout while preserving Ink's output.
    /// <para>
    /// Corresponds to JS <c>write(data)</c> from <c>useStdout()</c>.
    /// In JS Ink, this clears the current output, writes the string, then re-renders.
    /// </para>
    /// </summary>
    public void Write(string data)
    {
        if (WriteRequested is not null)
        {
            WriteRequested(data);
        }
        else
        {
            _writer.Write(data);
        }
    }
}
