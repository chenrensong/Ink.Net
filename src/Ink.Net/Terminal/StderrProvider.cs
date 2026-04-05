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

    internal StderrProvider(TextWriter writer)
    {
        _writer = writer;
    }

    /// <summary>
    /// The stderr stream (TextWriter).
    /// <para>Corresponds to JS <c>stderr</c> property.</para>
    /// </summary>
    public TextWriter Writer => _writer;

    /// <summary>
    /// Raised when <see cref="Write"/> is called, allowing the host (InkApplication)
    /// to clear/restore Ink output around the external write.
    /// </summary>
    internal event Action<string>? WriteRequested;

    /// <summary>
    /// Write any string to stderr while preserving Ink's output.
    /// <para>
    /// Corresponds to JS <c>write(data)</c> from <c>useStderr()</c>.
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
