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
    private readonly Action<string>? _writeCallback;

    internal StdoutProvider(TextWriter writer, Action<string>? writeCallback = null)
    {
        _writer = writer;
        _writeCallback = writeCallback;
    }

    /// <summary>
    /// The stdout stream (TextWriter).
    /// <para>Corresponds to JS <c>stdout</c> property.</para>
    /// </summary>
    public TextWriter Writer => _writer;

    /// <summary>
    /// Write any string to stdout while preserving Ink's output.
    /// <para>
    /// Corresponds to JS <c>write(data)</c> from <c>useStdout()</c>.
    /// In JS Ink, this clears the current output, writes the string, then re-renders.
    /// </para>
    /// </summary>
    public void Write(string data)
    {
        if (_writeCallback is not null)
        {
            _writeCallback(data);
        }
        else
        {
            _writer.Write(data);
        }
    }
}
