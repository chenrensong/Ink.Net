// -----------------------------------------------------------------------
// <copyright file="StderrProvider.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) StderrContext.ts + useStderr.ts
//   Provides access to stderr stream and a write method that preserves Ink output.
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Terminal;

/// <summary>
/// Provides access to the stderr stream and a write method that preserves Ink output.
/// <para>Corresponds to JS <c>useStderr()</c> / <c>StderrContext</c>.</para>
/// </summary>
public sealed class StderrProvider
{
    private readonly TextWriter _writer;
    private readonly Action<string>? _writeCallback;

    internal StderrProvider(TextWriter writer, Action<string>? writeCallback = null)
    {
        _writer = writer;
        _writeCallback = writeCallback;
    }

    /// <summary>
    /// The stderr stream (TextWriter).
    /// <para>Corresponds to JS <c>stderr</c> property.</para>
    /// </summary>
    public TextWriter Writer => _writer;

    /// <summary>
    /// Write any string to stderr while preserving Ink's output.
    /// <para>
    /// Corresponds to JS <c>write(data)</c> from <c>useStderr()</c>.
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
