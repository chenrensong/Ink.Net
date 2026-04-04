// Tests ported from focus.tsx
// Covers: FocusManager — add, remove, activate, deactivate, focusNext, focusPrevious, focus, enable/disable
using Ink.Net.Input;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Focus management tests aligned with JS focus.tsx test suite.</summary>
public class FocusTests
{
    // ── Basic registration ──────────────────────────────────────────

    [Fact]
    public void DoNotFocusOnRegisterWhenAutoFocusIsOff()
    {
        var fm = new FocusManager();
        fm.Add("first");
        fm.Add("second");
        fm.Add("third");

        Assert.Null(fm.ActiveId);
    }

    [Fact]
    public void FocusFirstComponentWithAutoFocus()
    {
        var fm = new FocusManager();
        fm.Add("first", new FocusOptions { AutoFocus = true });
        fm.Add("second");
        fm.Add("third");

        Assert.Equal("first", fm.ActiveId);
    }

    [Fact]
    public void AutoFocusOnlyFirstComponent()
    {
        var fm = new FocusManager();
        fm.Add("first", new FocusOptions { AutoFocus = true });
        fm.Add("second", new FocusOptions { AutoFocus = true });

        // Only the first autoFocus takes effect when nothing is focused
        Assert.Equal("first", fm.ActiveId);
    }

    // ── Tab navigation ──────────────────────────────────────────────

    [Fact]
    public void SwitchFocusToFirstComponentOnTab()
    {
        var fm = new FocusManager();
        fm.Add("first");
        fm.Add("second");
        fm.Add("third");

        fm.HandleInput("\t");

        Assert.Equal("first", fm.ActiveId);
    }

    [Fact]
    public void SwitchFocusToNextComponentOnTab()
    {
        var fm = new FocusManager();
        fm.Add("first");
        fm.Add("second");
        fm.Add("third");

        fm.HandleInput("\t"); // -> first
        fm.HandleInput("\t"); // -> second

        Assert.Equal("second", fm.ActiveId);
    }

    [Fact]
    public void WrapAroundOnTab()
    {
        var fm = new FocusManager();
        fm.Add("first", new FocusOptions { AutoFocus = true });
        fm.Add("second");
        fm.Add("third");

        fm.HandleInput("\t"); // -> second
        fm.HandleInput("\t"); // -> third
        fm.HandleInput("\t"); // wrap -> first

        Assert.Equal("first", fm.ActiveId);
    }

    [Fact]
    public void SkipDisabledComponentOnTab()
    {
        var fm = new FocusManager();
        fm.Add("first", new FocusOptions { AutoFocus = true });
        var second = fm.Add("second");
        fm.Add("third");

        fm.Deactivate("second");
        fm.HandleInput("\t"); // skip second -> third

        Assert.Equal("third", fm.ActiveId);
    }

    // ── Shift+Tab navigation ────────────────────────────────────────

    [Fact]
    public void SwitchFocusToPreviousOnShiftTab()
    {
        var fm = new FocusManager();
        fm.Add("first", new FocusOptions { AutoFocus = true });
        fm.Add("second");
        fm.Add("third");

        fm.HandleInput("\t"); // -> second
        fm.HandleInput("\u001B[Z"); // -> first

        Assert.Equal("first", fm.ActiveId);
    }

    [Fact]
    public void WrapAroundOnShiftTab()
    {
        var fm = new FocusManager();
        fm.Add("first", new FocusOptions { AutoFocus = true });
        fm.Add("second");
        fm.Add("third");

        fm.HandleInput("\u001B[Z"); // wrap -> third

        Assert.Equal("third", fm.ActiveId);
    }

    [Fact]
    public void SkipDisabledComponentOnShiftTab()
    {
        var fm = new FocusManager();
        fm.Add("first", new FocusOptions { AutoFocus = true });
        fm.Add("second");
        fm.Add("third");

        fm.Deactivate("second");
        fm.HandleInput("\u001B[Z"); // skip second -> third
        fm.HandleInput("\u001B[Z"); // wrap -> first

        Assert.Equal("first", fm.ActiveId);
    }

    // ── Remove / Unregister ─────────────────────────────────────────

    [Fact]
    public void ResetFocusWhenFocusedComponentUnregisters()
    {
        var fm = new FocusManager();
        var first = fm.Add("first", new FocusOptions { AutoFocus = true });
        fm.Add("second");

        first.Dispose(); // Remove "first"

        Assert.Null(fm.ActiveId);
    }

    [Fact]
    public void FocusFirstAfterFocusedComponentUnregisters()
    {
        var fm = new FocusManager();
        var first = fm.Add("first", new FocusOptions { AutoFocus = true });
        fm.Add("second");
        fm.Add("third");

        first.Dispose(); // Remove "first"

        fm.HandleInput("\t"); // -> second (first available)

        Assert.Equal("second", fm.ActiveId);
    }

    // ── Enable / Disable ────────────────────────────────────────────

    [Fact]
    public void DisableFocusPreventsFocusChange()
    {
        var fm = new FocusManager();
        fm.Add("first", new FocusOptions { AutoFocus = true });
        fm.Add("second");

        fm.DisableFocus();
        fm.HandleInput("\t"); // should be ignored

        Assert.Null(fm.ActiveId);
    }

    [Fact]
    public void EnableFocusReenablesFocusManagement()
    {
        var fm = new FocusManager();
        fm.Add("first", new FocusOptions { AutoFocus = true });
        fm.Add("second");

        fm.DisableFocus();
        fm.EnableFocus();
        fm.HandleInput("\t"); // -> first

        Assert.Equal("first", fm.ActiveId);
    }

    // ── Manual focus ────────────────────────────────────────────────

    [Fact]
    public void ManuallyFocusNext()
    {
        var fm = new FocusManager();
        fm.Add("first", new FocusOptions { AutoFocus = true });
        fm.Add("second");
        fm.Add("third");

        fm.FocusNext(); // -> second

        Assert.Equal("second", fm.ActiveId);
    }

    [Fact]
    public void ManuallyFocusPrevious()
    {
        var fm = new FocusManager();
        fm.Add("first", new FocusOptions { AutoFocus = true });
        fm.Add("second");
        fm.Add("third");

        fm.FocusPrevious(); // wrap -> third

        Assert.Equal("third", fm.ActiveId);
    }

    [Fact]
    public void FocusByIdSwitchesActiveId()
    {
        var fm = new FocusManager();
        fm.Add("first");
        fm.Add("second");

        fm.Focus("second");

        Assert.Equal("second", fm.ActiveId);
    }

    [Fact]
    public void FocusByIdDoesNothingForUnknownId()
    {
        var fm = new FocusManager();
        fm.Add("first");

        fm.Focus("nonexistent");

        Assert.Null(fm.ActiveId);
    }

    // ── IsFocused helper ────────────────────────────────────────────

    [Fact]
    public void IsFocusedReturnsTrueForActiveComponent()
    {
        var fm = new FocusManager();
        fm.Add("first", new FocusOptions { AutoFocus = true });
        fm.Add("second");

        Assert.True(fm.IsFocused("first"));
        Assert.False(fm.IsFocused("second"));
    }

    // ── FocusRegistration.IsFocused ─────────────────────────────────

    [Fact]
    public void FocusRegistrationIsFocused()
    {
        var fm = new FocusManager();
        var reg = fm.Add("first", new FocusOptions { AutoFocus = true });
        fm.Add("second");

        Assert.True(reg.IsFocused);

        fm.FocusNext(); // -> second
        Assert.False(reg.IsFocused);
    }

    // ── ActiveIdChanged event ───────────────────────────────────────

    [Fact]
    public void ActiveIdChangedEventFires()
    {
        var fm = new FocusManager();
        fm.Add("first");
        fm.Add("second");

        string? changedTo = null;
        fm.ActiveIdChanged += id => changedTo = id;

        fm.HandleInput("\t"); // -> first

        Assert.Equal("first", changedTo);
    }

    // ── Edge cases ──────────────────────────────────────────────────

    [Fact]
    public void DoesNotCrashWhenFocusingNextWithNoFocusables()
    {
        var fm = new FocusManager();
        fm.FocusNext(); // Should not throw
        Assert.Null(fm.ActiveId);
    }

    [Fact]
    public void DoesNotCrashWhenFocusingPreviousWithNoFocusables()
    {
        var fm = new FocusManager();
        fm.FocusPrevious(); // Should not throw
        Assert.Null(fm.ActiveId);
    }

    [Fact]
    public void FocusesFirstNonDisabledComponent()
    {
        var fm = new FocusManager();
        fm.Add("first");
        fm.Add("second");
        fm.Add("third");

        fm.Deactivate("first");
        fm.Deactivate("second");

        fm.HandleInput("\t"); // should skip to "third"

        Assert.Equal("third", fm.ActiveId);
    }

    [Fact]
    public void SkipsDisabledWhenWrappingAround()
    {
        var fm = new FocusManager();
        fm.Add("first");
        fm.Add("second", new FocusOptions { AutoFocus = true });
        fm.Add("third");

        fm.Deactivate("first");
        fm.HandleInput("\t"); // -> third
        fm.HandleInput("\t"); // wrap, skip first -> second

        Assert.Equal("second", fm.ActiveId);
    }

    [Fact]
    public void SkipsDisabledWhenWrappingFromFront()
    {
        var fm = new FocusManager();
        fm.Add("first", new FocusOptions { AutoFocus = true });
        fm.Add("second");
        fm.Add("third");

        fm.Deactivate("third");
        fm.HandleInput("\u001B[Z"); // wrap, skip third -> second

        Assert.Equal("second", fm.ActiveId);
    }
}
