// -----------------------------------------------------------------------
// 1:1 Port from Ink (JS) test/screen-reader.tsx
// -----------------------------------------------------------------------

using Ink.Net.Builder;
using Ink.Net.Dom;
using Ink.Net.Styles;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Tests for screen reader output rendering.</summary>
public class ScreenReaderTests
{
    private static readonly RenderToStringOptions ScreenReader = new()
    {
        Columns = 100,
        IsScreenReaderEnabled = true,
    };

    [Fact]
    public void RenderTextForScreenReaders()
    {
        // JS: <Box aria-label="Hello World"><Text>Not visible to screen readers</Text></Box>
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(ariaLabel: "Hello World", children: new[]
            {
                b.Text("Not visible to screen readers"),
            }),
        }, ScreenReader);

        Assert.Equal("Hello World", output);
    }

    [Fact]
    public void RenderTextForScreenReadersWithAriaHidden()
    {
        // JS: <Box aria-hidden><Text>Not visible to screen readers</Text></Box>
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(ariaHidden: true, children: new[]
            {
                b.Text("Not visible to screen readers"),
            }),
        }, ScreenReader);

        Assert.Equal("", output);
    }

    [Fact]
    public void RenderTextForScreenReadersWithAriaRole()
    {
        // JS: <Box aria-role="button"><Text>Click me</Text></Box>
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(ariaRole: AccessibilityRole.Button, children: new[]
            {
                b.Text("Click me"),
            }),
        }, ScreenReader);

        Assert.Equal("button: Click me", output);
    }

    [Fact]
    public void RenderSelectInputForScreenReaders()
    {
        // JS: Complex select input with list, listitems, selected state
        var items = new[] { "Red", "Green", "Blue" };

        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(
                style: new InkStyle { FlexDirection = FlexDirectionMode.Column },
                ariaRole: AccessibilityRole.List,
                children: new TreeNode[]
                {
                    b.Text("Select a color:"),
                }.Concat(items.Select((item, index) =>
                {
                    bool isSelected = index == 1;
                    string screenReaderLabel = $"{index + 1}. {item}";
                    return b.Box(
                        ariaLabel: screenReaderLabel,
                        ariaRole: AccessibilityRole.Listitem,
                        ariaState: new AccessibilityState { Selected = isSelected },
                        children: new[] { b.Text(item) });
                })).ToArray()),
        }, ScreenReader);

        Assert.Equal(
            "list: Select a color:\nlistitem: 1. Red\nlistitem: (selected) 2. Green\nlistitem: 3. Blue",
            output);
    }

    [Fact]
    public void RenderAriaLabelOnlyTextForScreenReaders()
    {
        // JS: <Text aria-label="Screen-reader only" />
        string output = InkApp.RenderToString(b => new[]
        {
            b.Text("", ariaLabel: "Screen-reader only"),
        }, ScreenReader);

        Assert.Equal("Screen-reader only", output);
    }

    [Fact]
    public void RenderAriaLabelOnlyBoxForScreenReaders()
    {
        // JS: <Box aria-label="Screen-reader only" />
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(ariaLabel: "Screen-reader only"),
        }, ScreenReader);

        Assert.Equal("Screen-reader only", output);
    }

    [Fact]
    public void SkipNodesWithDisplayNoneInScreenReaderOutput()
    {
        // JS: <Box><Box display="none"><Text>Hidden</Text></Box><Text>Visible</Text></Box>
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(children: new[]
            {
                b.Box(style: new InkStyle { Display = DisplayMode.None }, children: new[]
                {
                    b.Text("Hidden"),
                }),
                b.Text("Visible"),
            }),
        }, ScreenReader);

        Assert.Equal("Visible", output);
    }

    [Fact]
    public void RenderMultipleTextComponents()
    {
        // JS: <Box flexDirection="column"><Text>Hello</Text><Text>World</Text></Box>
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(style: new InkStyle { FlexDirection = FlexDirectionMode.Column }, children: new[]
            {
                b.Text("Hello"),
                b.Text("World"),
            }),
        }, ScreenReader);

        Assert.Equal("Hello\nWorld", output);
    }

    [Fact]
    public void RenderNestedBoxComponentsWithText()
    {
        // JS: <Box flexDirection="column"><Text>Hello</Text><Box><Text>World</Text></Box></Box>
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(style: new InkStyle { FlexDirection = FlexDirectionMode.Column }, children: new[]
            {
                b.Text("Hello"),
                b.Box(children: new[] { b.Text("World") }),
            }),
        }, ScreenReader);

        Assert.Equal("Hello\nWorld", output);
    }

    [Fact]
    public void RenderWithAriaStateBusy()
    {
        // JS: <Box aria-state={{busy: true}}><Text>Loading</Text></Box>
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(ariaState: new AccessibilityState { Busy = true }, children: new[]
            {
                b.Text("Loading"),
            }),
        }, ScreenReader);

        Assert.Equal("(busy) Loading", output);
    }

    [Fact]
    public void RenderWithAriaStateChecked()
    {
        // JS: <Box aria-role="checkbox" aria-state={{checked: true}}><Text>Accept terms</Text></Box>
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(ariaRole: AccessibilityRole.Checkbox,
                  ariaState: new AccessibilityState { Checked = true },
                  children: new[] { b.Text("Accept terms") }),
        }, ScreenReader);

        Assert.Equal("checkbox: (checked) Accept terms", output);
    }

    [Fact]
    public void RenderWithAriaStateDisabled()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(ariaRole: AccessibilityRole.Button,
                  ariaState: new AccessibilityState { Disabled = true },
                  children: new[] { b.Text("Submit") }),
        }, ScreenReader);

        Assert.Equal("button: (disabled) Submit", output);
    }

    [Fact]
    public void RenderWithAriaStateExpanded()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(ariaRole: AccessibilityRole.Combobox,
                  ariaState: new AccessibilityState { Expanded = true },
                  children: new[] { b.Text("Select") }),
        }, ScreenReader);

        Assert.Equal("combobox: (expanded) Select", output);
    }

    [Fact]
    public void RenderWithAriaStateMultiline()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(ariaRole: AccessibilityRole.Textbox,
                  ariaState: new AccessibilityState { Multiline = true },
                  children: new[] { b.Text("Hello") }),
        }, ScreenReader);

        Assert.Equal("textbox: (multiline) Hello", output);
    }

    [Fact]
    public void RenderWithAriaStateMultiselectable()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(ariaRole: AccessibilityRole.Listbox,
                  ariaState: new AccessibilityState { Multiselectable = true },
                  children: new[] { b.Text("Options") }),
        }, ScreenReader);

        Assert.Equal("listbox: (multiselectable) Options", output);
    }

    [Fact]
    public void RenderWithAriaStateReadonly()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(ariaRole: AccessibilityRole.Textbox,
                  ariaState: new AccessibilityState { Readonly = true },
                  children: new[] { b.Text("Hello") }),
        }, ScreenReader);

        Assert.Equal("textbox: (readonly) Hello", output);
    }

    [Fact]
    public void RenderWithAriaStateRequired()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(ariaRole: AccessibilityRole.Textbox,
                  ariaState: new AccessibilityState { Required = true },
                  children: new[] { b.Text("Name") }),
        }, ScreenReader);

        Assert.Equal("textbox: (required) Name", output);
    }

    [Fact]
    public void RenderWithAriaStateSelected()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(ariaRole: AccessibilityRole.Option,
                  ariaState: new AccessibilityState { Selected = true },
                  children: new[] { b.Text("Blue") }),
        }, ScreenReader);

        Assert.Equal("option: (selected) Blue", output);
    }

    [Fact]
    public void RenderMultiLineText()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(style: new InkStyle { FlexDirection = FlexDirectionMode.Column }, children: new[]
            {
                b.Text("Line 1"),
                b.Text("Line 2"),
            }),
        }, ScreenReader);

        Assert.Equal("Line 1\nLine 2", output);
    }

    [Fact]
    public void RenderNestedMultiLineText()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(style: new InkStyle { FlexDirection = FlexDirectionMode.Row }, children: new[]
            {
                b.Box(style: new InkStyle { FlexDirection = FlexDirectionMode.Column }, children: new[]
                {
                    b.Text("Line 1"),
                    b.Text("Line 2"),
                }),
            }),
        }, ScreenReader);

        Assert.Equal("Line 1\nLine 2", output);
    }

    [Fact]
    public void RenderNestedRow()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(style: new InkStyle { FlexDirection = FlexDirectionMode.Column }, children: new[]
            {
                b.Box(style: new InkStyle { FlexDirection = FlexDirectionMode.Row }, children: new[]
                {
                    b.Text("Line 1"),
                    b.Text("Line 2"),
                }),
            }),
        }, ScreenReader);

        Assert.Equal("Line 1 Line 2", output);
    }

    [Fact]
    public void RenderMultiLineTextWithRoles()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(style: new InkStyle { FlexDirection = FlexDirectionMode.Column },
                  ariaRole: AccessibilityRole.List,
                  children: new[]
            {
                b.Box(ariaRole: AccessibilityRole.Listitem, children: new[]
                {
                    b.Text("Item 1"),
                }),
                b.Box(ariaRole: AccessibilityRole.Listitem, children: new[]
                {
                    b.Text("Item 2"),
                }),
            }),
        }, ScreenReader);

        Assert.Equal("list: listitem: Item 1\nlistitem: Item 2", output);
    }

    [Fact]
    public void RenderListboxWithMultiselectableOptions()
    {
        string output = InkApp.RenderToString(b => new[]
        {
            b.Box(
                style: new InkStyle { FlexDirection = FlexDirectionMode.Column },
                ariaRole: AccessibilityRole.Listbox,
                ariaState: new AccessibilityState { Multiselectable = true },
                children: new[]
                {
                    b.Box(ariaRole: AccessibilityRole.Option,
                          ariaState: new AccessibilityState { Selected = true },
                          children: new[] { b.Text("Option 1") }),
                    b.Box(ariaRole: AccessibilityRole.Option,
                          ariaState: new AccessibilityState { Selected = false },
                          children: new[] { b.Text("Option 2") }),
                    b.Box(ariaRole: AccessibilityRole.Option,
                          ariaState: new AccessibilityState { Selected = true },
                          children: new[] { b.Text("Option 3") }),
                }),
        }, ScreenReader);

        Assert.Equal(
            "listbox: (multiselectable) option: (selected) Option 1\noption: Option 2\noption: (selected) Option 3",
            output);
    }
}
