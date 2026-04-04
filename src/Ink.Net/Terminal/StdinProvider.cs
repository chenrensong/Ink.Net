// -----------------------------------------------------------------------
// <copyright file="StdinProvider.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) StdinContext.ts + useStdin.ts
//   Provides access to stdin stream and raw mode control.
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Terminal;

/// <summary>
/// Provides access to the stdin stream and raw mode control.
/// <para>Corresponds to JS <c>useStdin()</c> / <c>StdinContext</c>.</para>
/// </summary>
public sealed class StdinProvider : IDisposable
{
    private readonly Stream _inputStream;
    private bool _isRawMode;
    private bool _disposed;

    /// <summary>
    /// Creates a new stdin provider.
    /// </summary>
    /// <param name="inputStream">The input stream. Default: <c>Console.OpenStandardInput()</c>.</param>
    /// <param name="isRawModeSupported">Whether raw mode is supported.</param>
    public StdinProvider(Stream? inputStream = null, bool isRawModeSupported = true)
    {
        _inputStream = inputStream ?? Console.OpenStandardInput();
        IsRawModeSupported = isRawModeSupported;
    }

    /// <summary>
    /// The raw stdin input stream.
    /// <para>Corresponds to JS <c>stdin</c> property.</para>
    /// </summary>
    public Stream InputStream => _inputStream;

    /// <summary>
    /// Whether raw mode is currently enabled.
    /// <para>Corresponds to JS <c>isRawModeSupported</c>.</para>
    /// </summary>
    public bool IsRawModeSupported { get; }

    /// <summary>
    /// Whether raw mode is currently active.
    /// </summary>
    public bool IsRawMode => _isRawMode;

    /// <summary>
    /// Enable or disable raw mode.
    /// <para>
    /// Corresponds to JS <c>setRawMode(value)</c> from <c>useStdin()</c>.
    /// </para>
    /// </summary>
    public void SetRawMode(bool value)
    {
        if (!IsRawModeSupported)
            return;

        _isRawMode = value;
        RawModeChanged?.Invoke(value);
    }

    /// <summary>
    /// Event fired when raw data is received from stdin.
    /// </summary>
    public event Action<string>? DataReceived;

    /// <summary>
    /// Event fired when raw mode state changes.
    /// </summary>
    public event Action<bool>? RawModeChanged;

    /// <summary>
    /// Feed raw input data (used for testing or manual stdin piping).
    /// </summary>
    public void EmitData(string data)
    {
        DataReceived?.Invoke(data);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_isRawMode)
        {
            _isRawMode = false;
            RawModeChanged?.Invoke(false);
        }
    }
}
