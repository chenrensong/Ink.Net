// Tests ported from write-synchronized.tsx
using Ink.Net.Terminal;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Synchronized write tests aligned with JS test/write-synchronized.tsx.</summary>
public class SynchronizedWriteTests
{
    [Fact]
    public void BsuIsExpectedSequence()
    {
        Assert.Equal("\u001B[?2026h", SynchronizedWrite.Bsu);
    }

    [Fact]
    public void EsuIsExpectedSequence()
    {
        Assert.Equal("\u001B[?2026l", SynchronizedWrite.Esu);
    }

    [Fact]
    public void ShouldSynchronizeReturnsTrueForInteractiveTty()
    {
        // isRedirected=false means TTY, interactive=true
        Assert.True(SynchronizedWrite.ShouldSynchronize(isRedirected: false, interactive: true));
    }

    [Fact]
    public void ShouldSynchronizeReturnsFalseForNonInteractiveTty()
    {
        Assert.False(SynchronizedWrite.ShouldSynchronize(isRedirected: false, interactive: false));
    }

    [Fact]
    public void ShouldSynchronizeReturnsFalseForNonTty()
    {
        // isRedirected=true means not TTY
        Assert.False(SynchronizedWrite.ShouldSynchronize(isRedirected: true, interactive: true));
    }

    [Fact]
    public void ShouldSynchronizeReturnsFalseForNonTtyWhenInteractiveNotSpecified()
    {
        Assert.False(SynchronizedWrite.ShouldSynchronize(isRedirected: true));
    }
}
