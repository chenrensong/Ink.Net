// -----------------------------------------------------------------------
// Tests for StdoutProvider, StderrProvider, StdinProvider
// 1:1 Port concepts from Ink (JS) StdoutContext / useStdout / useStderr / useStdin
// -----------------------------------------------------------------------

using Ink.Net.Terminal;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Tests for stdout/stderr/stdin providers.</summary>
public class StdioProviderTests
{
    // ── StdoutProvider ──────────────────────────────────────────────

    [Fact]
    public void StdoutProvider_WritesToWriter()
    {
        var sw = new StringWriter();
        var provider = new StdoutProvider(sw);

        provider.Write("Hello from stdout");

        Assert.Equal("Hello from stdout", sw.ToString());
    }

    [Fact]
    public void StdoutProvider_UsesWriteCallback()
    {
        var sw = new StringWriter();
        string? callbackData = null;
        var provider = new StdoutProvider(sw, data => callbackData = data);

        provider.Write("test callback");

        Assert.Equal("test callback", callbackData);
        // Writer should NOT have been written to directly
        Assert.Equal("", sw.ToString());
    }

    [Fact]
    public void StdoutProvider_ExposesWriter()
    {
        var sw = new StringWriter();
        var provider = new StdoutProvider(sw);

        Assert.Same(sw, provider.Writer);
    }

    // ── StderrProvider ──────────────────────────────────────────────

    [Fact]
    public void StderrProvider_WritesToWriter()
    {
        var sw = new StringWriter();
        var provider = new StderrProvider(sw);

        provider.Write("Hello from stderr");

        Assert.Equal("Hello from stderr", sw.ToString());
    }

    [Fact]
    public void StderrProvider_UsesWriteCallback()
    {
        var sw = new StringWriter();
        string? callbackData = null;
        var provider = new StderrProvider(sw, data => callbackData = data);

        provider.Write("test callback");

        Assert.Equal("test callback", callbackData);
        Assert.Equal("", sw.ToString());
    }

    [Fact]
    public void StderrProvider_ExposesWriter()
    {
        var sw = new StringWriter();
        var provider = new StderrProvider(sw);

        Assert.Same(sw, provider.Writer);
    }

    // ── StdinProvider ───────────────────────────────────────────────

    [Fact]
    public void StdinProvider_EmitsData()
    {
        using var provider = new StdinProvider(isRawModeSupported: true);
        string? receivedData = null;
        provider.DataReceived += data => receivedData = data;

        provider.EmitData("hello");

        Assert.Equal("hello", receivedData);
    }

    [Fact]
    public void StdinProvider_SetRawMode()
    {
        using var provider = new StdinProvider(isRawModeSupported: true);

        Assert.False(provider.IsRawMode);

        provider.SetRawMode(true);
        Assert.True(provider.IsRawMode);

        provider.SetRawMode(false);
        Assert.False(provider.IsRawMode);
    }

    [Fact]
    public void StdinProvider_SetRawModeIgnoredWhenNotSupported()
    {
        using var provider = new StdinProvider(isRawModeSupported: false);

        provider.SetRawMode(true);
        Assert.False(provider.IsRawMode);
    }

    [Fact]
    public void StdinProvider_RawModeChangedEventFires()
    {
        using var provider = new StdinProvider(isRawModeSupported: true);
        bool? lastRawModeValue = null;
        provider.RawModeChanged += value => lastRawModeValue = value;

        provider.SetRawMode(true);
        Assert.True(lastRawModeValue);

        provider.SetRawMode(false);
        Assert.False(lastRawModeValue);
    }

    [Fact]
    public void StdinProvider_DisposeResetsRawMode()
    {
        var provider = new StdinProvider(isRawModeSupported: true);
        bool? lastRawModeValue = null;
        provider.RawModeChanged += value => lastRawModeValue = value;

        provider.SetRawMode(true);
        Assert.True(lastRawModeValue);

        provider.Dispose();
        Assert.False(lastRawModeValue);
    }

    [Fact]
    public void StdinProvider_ExposesInputStream()
    {
        var ms = new MemoryStream();
        using var provider = new StdinProvider(ms, isRawModeSupported: true);

        Assert.Same(ms, provider.InputStream);
    }

    [Fact]
    public void StdinProvider_IsRawModeSupported()
    {
        using var supported = new StdinProvider(isRawModeSupported: true);
        Assert.True(supported.IsRawModeSupported);

        using var notSupported = new StdinProvider(isRawModeSupported: false);
        Assert.False(notSupported.IsRawModeSupported);
    }
}
