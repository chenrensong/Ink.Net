// -----------------------------------------------------------------------
// AlternateScreenTests.cs — Tests for the AlternateScreen terminal feature.
// Aligned with ink/test/alternate-screen-example.tsx and ink.tsx behavior.
// -----------------------------------------------------------------------

using Ink.Net.Terminal;
using System.Text;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>
/// Tests for <see cref="AlternateScreen"/>.
/// <para>Verifies ANSI escape sequences for enter/exit and cursor management.</para>
/// </summary>
public class AlternateScreenTests
{
    [Fact]
    public void Enter_WritesEnterEscapeAndHidesCursor()
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        var screen = new AlternateScreen(writer);

        screen.Enter();

        var expected = AlternateScreen.EnterAlternateScreenEscape + CursorHelpers.HideCursorEscape;
        Assert.Equal(expected, sb.ToString());
        Assert.True(screen.IsActive);
    }

    [Fact]
    public void Exit_WritesExitEscapeAndShowsCursor()
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        var screen = new AlternateScreen(writer);

        screen.Enter();
        sb.Clear();

        screen.Exit();

        var expected = AlternateScreen.ExitAlternateScreenEscape + CursorHelpers.ShowCursorEscape;
        Assert.Equal(expected, sb.ToString());
        Assert.False(screen.IsActive);
    }

    [Fact]
    public void Enter_DoesNotWriteIfAlreadyActive()
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        var screen = new AlternateScreen(writer);

        screen.Enter();
        sb.Clear();

        screen.Enter(); // second Enter should be no-op

        Assert.Empty(sb.ToString());
    }

    [Fact]
    public void Exit_DoesNotWriteIfNotActive()
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        var screen = new AlternateScreen(writer);

        screen.Exit(); // Exit without Enter should be no-op

        Assert.Empty(sb.ToString());
    }

    [Fact]
    public void Dispose_ExitsIfActive()
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        var screen = new AlternateScreen(writer);

        screen.Enter();
        sb.Clear();

        screen.Dispose();

        var expected = AlternateScreen.ExitAlternateScreenEscape + CursorHelpers.ShowCursorEscape;
        Assert.Equal(expected, sb.ToString());
    }

    [Fact]
    public void Dispose_DoesNothingIfNotActive()
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        var screen = new AlternateScreen(writer);

        screen.Dispose();

        Assert.Empty(sb.ToString());
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        var screen = new AlternateScreen(writer);

        screen.Enter();
        sb.Clear();

        screen.Dispose();
        sb.Clear();

        screen.Dispose(); // second Dispose should be no-op

        Assert.Empty(sb.ToString());
    }

    [Fact]
    public void ShouldEnable_ReturnsTrueWhenRequestedAndInteractive()
    {
        Assert.True(AlternateScreen.ShouldEnable(requested: true, interactive: true));
    }

    [Fact]
    public void ShouldEnable_ReturnsFalseWhenNotRequested()
    {
        Assert.False(AlternateScreen.ShouldEnable(requested: false, interactive: true));
    }

    [Fact]
    public void ShouldEnable_ReturnsFalseWhenNotInteractive()
    {
        Assert.False(AlternateScreen.ShouldEnable(requested: true, interactive: false));
    }

    [Fact]
    public void EnterAfterDispose_DoesNothing()
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        var screen = new AlternateScreen(writer);

        screen.Dispose();
        sb.Clear();

        screen.Enter(); // should be no-op after dispose

        Assert.Empty(sb.ToString());
        Assert.False(screen.IsActive);
    }

    [Fact]
    public void ExitAfterDispose_DoesNothing()
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        var screen = new AlternateScreen(writer);

        screen.Enter();
        screen.Dispose();
        sb.Clear();

        screen.Exit(); // should be no-op after dispose

        Assert.Empty(sb.ToString());
    }
}
