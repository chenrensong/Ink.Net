<div align="center">
  <br>
  <br>
  <h1>Ink.Net</h1>
  <br>
  <br>
</div>

> A declarative terminal rendering engine for .NET — ported from [Ink](https://github.com/vadimdemedes/ink). Build and test your CLI output using components.

[![NuGet](https://img.shields.io/nuget/v/Ink.Net?logo=nuget)](https://www.nuget.org/packages/Ink.Net)
[![Build](https://img.shields.io/github/actions/workflow/status/nicologies/ink.net/dotnet.yml?logo=github)](https://github.com/chenrensong/Ink.Net/actions)

Ink.Net provides the same component-based UI building experience that [Ink](https://github.com/vadimdemedes/ink) offers for Node.js CLIs, but for .NET applications.
It uses [Yoga.Net](https://www.nuget.org/packages/Yoga.Net) to build Flexbox layouts in the terminal, so most CSS-like properties from Ink are available in Ink.Net as well.

Instead of React/JSX, Ink.Net uses an imperative `TreeBuilder` API to construct the UI tree. The rendering pipeline is a faithful 1:1 port of Ink's internals — DOM tree → Yoga layout → Output buffer → Terminal string — optimized for **Native AOT** compatibility.

For a maintained mapping of upstream `examples/`, `test/`, and subprocess fixtures, see [src/PARITY-TESTS.md](src/PARITY-TESTS.md).

---

## Install

```xml
<PackageReference Include="Ink.Net" Version="1.0.0" />
```

Or via CLI:

```sh
dotnet add package Ink.Net
```

> **Supported frameworks:** .NET 8.0, .NET 9.0, .NET 10.0

## Usage

```csharp
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;

var output = InkApp.RenderToString(b => new[]
{
    b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, Padding = 1 }, new[]
    {
        b.Text("Hello, Ink.Net!"),
        b.Box(new InkStyle { MarginTop = 1 }, new[]
        {
            b.Text("Flexbox layouts in the terminal."),
        }),
    })
});

Console.WriteLine(output);
```

Output:

```
                                                                                
  Hello, Ink.Net!                                                               
                                                                                
  Flexbox layouts in the terminal.                                              
                                                                                
```

## Contents

- [Components](#components)
  - [Text](#text)
  - [Box](#box)
  - [Static](#static)
  - [Newline](#newline)
  - [Spacer](#spacer)
- [API](#api)
  - [`InkApp.RenderToString`](#inkapprendetostring)
  - [`InkApp.Render`](#inkapprender)
  - [`TreeBuilder`](#treebuilder)
- [Styles](#styles)
  - [`InkStyle`](#inkstyle)
  - [`DimensionValue`](#dimensionvalue)
- [Accessibility](#accessibility)
- [Terminal I/O Providers](#terminal-io-providers)
- [Testing](#testing)
- [Examples](#examples)
- [Benchmarks](#benchmarks)
- [Architecture](#architecture)
- [Compatibility](#compatibility)

## Components

### Text

This component displays text. In Ink.Net, text content is always wrapped inside `b.Text(...)`.

```csharp
b.Text("I am green", new TextStyle { Color = "green" })
b.Text("I am bold", new TextStyle { Bold = true })
b.Text("I am italic", new TextStyle { Italic = true })
b.Text("I am underline", new TextStyle { Underline = true })
b.Text("I am strikethrough", new TextStyle { Strikethrough = true })
b.Text("I am inversed", new TextStyle { Inverse = true })
```

#### color

Type: `string`

Change text color. Supports named colors, hex (`#005cc5`), RGB (`rgb(232, 131, 136)`), and ANSI-256 codes.

```csharp
b.Text("Green", new TextStyle { Color = "green" })
b.Text("Blue", new TextStyle { Color = "#005cc5" })
b.Text("Red", new TextStyle { Color = "rgb(232, 131, 136)" })
```

#### backgroundColor

Type: `string`

Same as `color`, but for background.

```csharp
b.Text("Green BG", new TextStyle { BackgroundColor = "green" })
```

#### dimColor

Type: `bool`
Default: `false`

Dim the color (make it less bright).

#### bold

Type: `bool`
Default: `false`

Make the text bold.

#### italic

Type: `bool`
Default: `false`

Make the text italic.

#### underline

Type: `bool`
Default: `false`

Make the text underlined.

#### strikethrough

Type: `bool`
Default: `false`

Make the text crossed with a line.

#### inverse

Type: `bool`
Default: `false`

Invert background and foreground colors.

#### wrap

Type: `TextWrapMode`
Allowed values: `Wrap` `TruncateEnd` `TruncateMiddle` `TruncateStart`
Default: `Wrap`

This property tells Ink.Net to wrap or truncate text if its width is larger than the container.

```csharp
// Word wrapping (default)
b.Box(new InkStyle { Width = 7 }, new[] { b.Text("Hello World") })
//=> "Hello\nWorld"

// Truncate end
b.Box(new InkStyle { Width = 7, TextWrap = TextWrapMode.TruncateEnd }, new[] { b.Text("Hello World") })
//=> "Hello…"

// Truncate middle
b.Box(new InkStyle { Width = 7, TextWrap = TextWrapMode.TruncateMiddle }, new[] { b.Text("Hello World") })
//=> "He…ld"

// Truncate start
b.Box(new InkStyle { Width = 7, TextWrap = TextWrapMode.TruncateStart }, new[] { b.Text("Hello World") })
//=> "…World"
```

### Box

`Box` is an essential Ink.Net component to build your layout. It's like `<div style="display: flex">` in the browser.

```csharp
b.Box(new InkStyle { Margin = 2 }, new[]
{
    b.Text("This is a box with margin"),
})
```

#### Dimensions

##### width

Type: `DimensionValue`

Width of the element in spaces. You can also set it as a percentage.

```csharp
b.Box(new InkStyle { Width = 4 }, new[] { b.Text("X") })
//=> "X   "
```

```csharp
b.Box(new InkStyle { Width = 10 }, new[]
{
    b.Box(new InkStyle { Width = DimensionValue.Percent(50) }, new[] { b.Text("X") }),
    b.Text("Y"),
})
//=> "X    Y"
```

##### height

Type: `DimensionValue`

Height of the element in lines (rows). You can also set it as a percentage.

##### minWidth / minHeight / maxWidth / maxHeight

Type: `DimensionValue`

Sets minimum/maximum dimensions of the element.

##### aspectRatio

Type: `float?`

Defines the aspect ratio (width/height) for the element.

#### Padding

##### paddingTop / paddingBottom / paddingLeft / paddingRight

Type: `float?`
Default: `0`

Individual padding values for each edge.

##### PaddingX / PaddingY

Type: `float?`
Default: `0`

Horizontal/Vertical padding shortcuts.

##### Padding

Type: `float?`
Default: `0`

Padding on all sides.

```csharp
b.Box(new InkStyle { PaddingTop = 2 }, new[] { b.Text("Top") })
b.Box(new InkStyle { PaddingX = 2 }, new[] { b.Text("Left and right") })
b.Box(new InkStyle { Padding = 2 }, new[] { b.Text("All sides") })
```

#### Margin

##### marginTop / marginBottom / marginLeft / marginRight

Type: `float?`
Default: `0`

Individual margin values for each edge.

##### MarginX / MarginY

Type: `float?`
Default: `0`

Horizontal/Vertical margin shortcuts.

##### Margin

Type: `float?`
Default: `0`

Margin on all sides.

```csharp
b.Box(new InkStyle { MarginTop = 2 }, new[] { b.Text("Top") })
b.Box(new InkStyle { MarginX = 2 }, new[] { b.Text("Left and right") })
b.Box(new InkStyle { Margin = 2 }, new[] { b.Text("All sides") })
```

#### Gap

##### gap

Type: `float?`
Default: `0`

Size of the gap between an element's columns and rows. Shorthand for `columnGap` and `rowGap`.

```csharp
b.Box(new InkStyle { Gap = 1, Width = 3, FlexWrap = FlexWrapMode.Wrap }, new[]
{
    b.Text("A"),
    b.Text("B"),
    b.Text("C"),
})
// A B
//
// C
```

##### columnGap

Type: `float?`
Default: `0`

Size of the gap between an element's columns.

##### rowGap

Type: `float?`
Default: `0`

Size of the gap between an element's rows.

#### Flex

##### flexGrow

Type: `float?`
Default: `0`

See [flex-grow](https://css-tricks.com/almanac/properties/f/flex-grow/).

```csharp
b.Box(children: new[]
{
    b.Text("Label:"),
    b.Box(new InkStyle { FlexGrow = 1 }, new[] { b.Text("Fills all remaining space") }),
})
```

##### flexShrink

Type: `float?`
Default: `1`

See [flex-shrink](https://css-tricks.com/almanac/properties/f/flex-shrink/).

##### flexBasis

Type: `DimensionValue`

See [flex-basis](https://css-tricks.com/almanac/properties/f/flex-basis/).

##### flexDirection

Type: `FlexDirectionMode?`
Allowed values: `Row` `RowReverse` `Column` `ColumnReverse`

See [flex-direction](https://css-tricks.com/almanac/properties/f/flex-direction/).

```csharp
// Row (default)
b.Box(children: new[] {
    b.Box(new InkStyle { MarginRight = 1 }, new[] { b.Text("X") }),
    b.Text("Y"),
})
// X Y

// Column
b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column }, new[] {
    b.Text("X"),
    b.Text("Y"),
})
// X
// Y
```

##### flexWrap

Type: `FlexWrapMode?`
Allowed values: `NoWrap` `Wrap` `WrapReverse`

See [flex-wrap](https://css-tricks.com/almanac/properties/f/flex-wrap/).

```csharp
b.Box(new InkStyle { Width = 2, FlexWrap = FlexWrapMode.Wrap }, new[]
{
    b.Text("A"),
    b.Text("BC"),
})
// A
// BC
```

##### alignItems

Type: `AlignItemsMode?`
Allowed values: `FlexStart` `Center` `FlexEnd` `Stretch` `Baseline`

See [align-items](https://css-tricks.com/almanac/properties/a/align-items/).

##### alignSelf

Type: `AlignSelfMode?`
Default: `Auto`
Allowed values: `Auto` `FlexStart` `Center` `FlexEnd` `Stretch` `Baseline`

See [align-self](https://css-tricks.com/almanac/properties/a/align-self/).

##### alignContent

Type: `AlignContentMode?`
Default: `FlexStart`
Allowed values: `FlexStart` `FlexEnd` `Center` `Stretch` `SpaceBetween` `SpaceAround` `SpaceEvenly`

See [align-content](https://css-tricks.com/almanac/properties/a/align-content/).

##### justifyContent

Type: `JustifyContentMode?`
Allowed values: `FlexStart` `Center` `FlexEnd` `SpaceBetween` `SpaceAround` `SpaceEvenly`

See [justify-content](https://css-tricks.com/almanac/properties/j/justify-content/).

```csharp
b.Box(new InkStyle { JustifyContent = JustifyContentMode.SpaceBetween, Width = 10 }, new[]
{
    b.Text("X"),
    b.Text("Y"),
})
// [X        Y]
```

#### Position

##### position

Type: `PositionMode?`
Allowed values: `Relative` `Absolute` `Static`
Default: `Relative`

##### top / right / bottom / left

Type: `float?`

Offsets for positioned elements.

#### Visibility

##### display

Type: `DisplayMode?`
Allowed values: `Flex` `None`
Default: `Flex`

Set this property to `None` to hide the element.

##### overflow / overflowX / overflowY

Type: `OverflowMode?`
Allowed values: `Visible` `Hidden`
Default: `Visible`

Behavior for an element's overflow in horizontal/vertical direction.

#### Borders

##### borderStyle

Type: `string?`
Allowed values: `"single"` `"double"` `"round"` `"bold"` `"singleDouble"` `"doubleSingle"` `"classic"` or custom `BoxStyle`

Add a border with a specified style. Uses border styles from [cli-boxes](https://github.com/sindresorhus/cli-boxes).

```csharp
b.Box(new InkStyle { BorderStyle = "round", Padding = 1 }, new[]
{
    b.Text("Rounded border"),
})
```

##### borderColor

Type: `string?`

Change border color. Shorthand for all border edge colors.

##### borderTop / borderRight / borderBottom / borderLeft

Type: `bool?`
Default: `true`

Show/hide individual borders.

##### borderDimColor

Type: `bool?`
Default: `false`

Dim the border color.

#### Background

##### backgroundColor

Type: `string?`

Background color for the element. Accepts the same values as text `Color`. The background color fills the entire Box area and is inherited by child Text components unless they specify their own backgroundColor.

```csharp
b.Box(new InkStyle { BackgroundColor = "blue", Width = 20, Height = 3 }, new[]
{
    b.Text("Blue background"),
})
```

See example in [examples/BoxBackgrounds](src/Ink.Net.Examples/BoxBackgrounds.cs).

### Static

`Static` is a component for rendering content that is written once and never updated. It's useful for rendering logs, test results, or any content that should persist at the top of the output while the rest of the UI updates below.

```csharp
b.Static(new[]
{
    b.Text("Log: item 1 processed"),
    b.Text("Log: item 2 processed"),
})
```

Static content is rendered separately and appears above the dynamic output. The component accepts an optional `InkStyle` for customization.

```csharp
b.Static(
    children: new[]
    {
        b.Text("Test #1: passed"),
        b.Text("Test #2: failed"),
    },
    style: new InkStyle { Gap = 1 }
)
```

### Newline

Adds newline characters. Creates an empty block with specified height.

```csharp
b.Newline(2) // Two blank lines
```

### Spacer

A flexible space that expands along the major axis of its containing layout (`flexGrow = 1`).

```csharp
b.Box(children: new[]
{
    b.Text("Left"),
    b.Spacer(),
    b.Text("Right"),
})
```

## API

### `InkApp.RenderToString`

Render a tree to a string synchronously. Does not write to stdout. Useful for testing, generating documentation, or any scenario where you need the rendered output as a string.

```csharp
string output = InkApp.RenderToString(
    b => new[] { b.Text("Hello") },
    new RenderToStringOptions { Columns = 80 }
);
```

#### tree

Type: `Func<TreeBuilder, InkNode[]>`

Builder function that constructs the UI tree.

#### options

Type: `RenderToStringOptions`

##### columns

Type: `int`
Default: `80`

Width of the virtual terminal in columns. Controls where text wrapping occurs.

### `InkApp.Render`

Mount a tree and render to the terminal with live updates.

Returns: `InkInstance`

```csharp
using var instance = InkApp.Render(
    b => new[] { b.Text("Live!") },
    new RenderOptions { Columns = 80, Rows = 24 }
);
```

#### InkInstance

##### Rerender(tree)

Replace the root tree with a new one.

```csharp
instance.Rerender(b => new[] { b.Text("Updated!") });
```

##### Unmount()

Manually unmount the application.

```csharp
instance.Unmount();
```

### TreeBuilder

The imperative API for constructing the DOM tree:

| Method | Description |
|--------|-------------|
| `b.Text(content)` | Create a text node |
| `b.Text(content, style, transform, ariaLabel, ariaRole, ariaHidden, ariaState)` | Create a styled text node with optional ARIA attributes |
| `b.Box(style?, children?, transform?, ariaLabel?, ariaRole?, ariaHidden?, ariaState?)` | Create a box (flex container) with optional ARIA attributes |
| `b.Static(children?, style?)` | Create a static container for non-updating content |
| `b.Transform(transform, children)` | Apply output transformation to children |
| `b.Spacer()` | Flexible spacer (`flexGrow = 1`) |
| `b.Newline(count)` | Empty block with specified height |

### `InkStyle`

All Flexbox and visual style properties:

<details>
<summary><strong>Layout Properties</strong></summary>

| Property | Type | Description |
|----------|------|-------------|
| `FlexDirection` | `FlexDirectionMode?` | `Row`, `Column`, `RowReverse`, `ColumnReverse` |
| `FlexWrap` | `FlexWrapMode?` | `NoWrap`, `Wrap`, `WrapReverse` |
| `FlexGrow` | `float?` | Flex grow factor |
| `FlexShrink` | `float?` | Flex shrink factor |
| `FlexBasis` | `DimensionValue?` | Flex basis |
| `AlignItems` | `AlignItemsMode?` | Cross-axis alignment for children |
| `AlignSelf` | `AlignSelfMode?` | Cross-axis alignment for self |
| `AlignContent` | `AlignContentMode?` | Multi-line cross-axis alignment |
| `JustifyContent` | `JustifyContentMode?` | Main-axis alignment |
| `Gap` | `float?` | Gap between flex items |
| `RowGap` | `float?` | Row gap |
| `ColumnGap` | `float?` | Column gap |

</details>

<details>
<summary><strong>Dimensions</strong></summary>

| Property | Type | Description |
|----------|------|-------------|
| `Width` | `DimensionValue?` | Width (absolute, percent, auto) |
| `Height` | `DimensionValue?` | Height |
| `MinWidth` | `DimensionValue?` | Minimum width |
| `MinHeight` | `DimensionValue?` | Minimum height |
| `MaxWidth` | `DimensionValue?` | Maximum width |
| `MaxHeight` | `DimensionValue?` | Maximum height |
| `AspectRatio` | `float?` | Aspect ratio |

</details>

<details>
<summary><strong>Box Model</strong></summary>

| Property | Type | Description |
|----------|------|-------------|
| `Margin` / `MarginTop/Right/Bottom/Left` | `float?` | Margin (all edges) |
| `MarginX` / `MarginY` | `float?` | Horizontal / vertical margin |
| `Padding` / `PaddingTop/Right/Bottom/Left` | `float?` | Padding (all edges) |
| `PaddingX` / `PaddingY` | `float?` | Horizontal / vertical padding |
| `Position` | `PositionMode?` | `Relative`, `Absolute`, `Static` |
| `Top/Right/Bottom/Left` | `float?` | Position offsets |

</details>

<details>
<summary><strong>Borders</strong></summary>

| Property | Type | Description |
|----------|------|-------------|
| `BorderStyle` | `string?` | `"single"`, `"double"`, `"round"`, `"bold"`, `"singleDouble"`, `"doubleSingle"`, `"classic"`, or custom |
| `BorderColor` | `string?` | Border color |
| `BorderTopColor` / `BorderRightColor` / `BorderBottomColor` / `BorderLeftColor` | `string?` | Per-edge border color |
| `BorderDimColor` | `bool?` | Dim the border |
| `BorderTop/Right/Bottom/Left` | `bool?` | Show/hide individual borders |

</details>

<details>
<summary><strong>Visual</strong></summary>

| Property | Type | Description |
|----------|------|-------------|
| `Color` | `string?` | Text foreground color |
| `BackgroundColor` | `string?` | Background color (inherits to children) |
| `DimColor` | `bool?` | Dim text |
| `Display` | `DisplayMode?` | `Flex` or `None` |
| `Overflow` | `OverflowMode?` | Content overflow handling |
| `TextWrap` | `TextWrapMode?` | `Wrap`, `TruncateEnd`, `TruncateMiddle`, `TruncateStart` |

</details>

### `DimensionValue`

For width/height properties that support absolute, percent, and auto:

```csharp
new InkStyle
{
    Width = 40,                            // 40 columns (implicit conversion)
    Width = DimensionValue.From(40),       // 40 columns (explicit)
    Height = DimensionValue.Percent(50),   // 50% of parent
    MinWidth = DimensionValue.Auto,        // Auto
}
```

## Accessibility

Ink.Net supports ARIA attributes for screen reader output, ported from Ink's accessibility system.

### ARIA Attributes

Both `Box` and `Text` components accept ARIA parameters:

```csharp
// Checkbox with aria-role and aria-state
b.Box(
    ariaRole: AccessibilityRole.Checkbox,
    ariaState: new AccessibilityState { Checked = true },
    children: new[] { b.Text("[x] Accept terms") }
)

// Custom label for screen readers
b.Text("✓", ariaLabel: "Checkmark - task completed")

// Hide from screen readers
b.Text("Decorative border", ariaHidden: true)
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `ariaLabel` | `string?` | Label override for screen readers |
| `ariaRole` | `AccessibilityRole?` | `None`, `Checkbox`, `Radio`, `Button`, `Link`, `Heading`, `Img`, `Timer`, `ProgressBar` |
| `ariaHidden` | `bool` | Hide element from screen reader output |
| `ariaState` | `AccessibilityState?` | State properties: `Selected`, `Checked`, `Disabled`, `Expanded` |

### Screen Reader Mode

Enable screen reader output via options:

```csharp
// For RenderToString
var output = InkApp.RenderToString(b => new[] { b.Text("Hello") },
    new RenderToStringOptions { IsScreenReaderEnabled = true });

// For Render
var instance = InkApp.Render(buildFunc, new RenderOptions { IsScreenReaderEnabled = true });

// For InkApplication
var app = InkApplication.Create(buildFunc, new InkApplicationOptions { IsScreenReaderEnabled = true });
```

## Terminal I/O Providers

Ink.Net provides typed stream providers for terminal I/O, ported from Ink's `useStdin`, `useStdout`, and `useStderr` hooks.

### StdoutProvider

Access and write to stdout:

```csharp
var provider = new StdoutProvider(Console.Out);
provider.Write("Hello stdout");
```

### StderrProvider

Access and write to stderr:

```csharp
var provider = new StderrProvider(Console.Error);
provider.Write("Error message");
```

### StdinProvider

Access stdin with optional raw mode:

```csharp
using var provider = new StdinProvider();
provider.SetRawMode(true);
// ... read input
provider.SetRawMode(false);
```

### BoxMetrics

Measure element dimensions after layout:

```csharp
var metrics = BoxMetrics.Measure(element);
// metrics.Width, metrics.Height, metrics.Top, metrics.Left,
// metrics.Right, metrics.Bottom, metrics.PaddingTop, etc.
```

## Testing

Ink.Net components are simple to test with `InkApp.RenderToString`:

```csharp
using Ink.Net;

var output = InkApp.RenderToString(b => new[]
{
    b.Text("Hello World"),
});

Assert.Equal("Hello World", output);
```

Run the full test suite:

```sh
cd src
dotnet test
```

All **518 tests** cover:

| Category | Test Files | Tests |
|----------|-----------|-------|
| Components | `ComponentsTests` | Text, Box, Spacer, Newline, transforms |
| Static | `StaticComponentTests` | Static component rendering and output separation |
| Flex Layout | `FlexLayoutTests`, `FlexDirectionTests`, `FlexWrapTests`, `FlexTests` | Direction, wrap, grow, shrink, basis |
| Alignment | `FlexAlignItemsTests`, `FlexAlignSelfTests`, `FlexAlignContentTests`, `FlexJustifyContentTests` | All alignment modes |
| Dimensions | `WidthHeightTests` | Width/height, min/max, percent, aspect-ratio |
| Box Model | `MarginPaddingGapTests`, `GapTests`, `OverflowTests`, `PositionTests`, `DisplayTests` | Margin, padding, gap, overflow, position |
| Borders | `BorderTests`, `BorderRendererTests` | All styles, colors, edges, dim, custom |
| Background | `BackgroundTests` | Inheritance, named/hex/rgb/ansi256, fill |
| Text | `TextTests`, `TextWidthTests`, `MeasureTextTests` | Colors, ANSI preservation, wrapping, CJK/emoji width |
| ANSI | `AnsiTokenizerTests`, `AnsiSanitizerTests`, `ColorizerTests` | Tokenizer, sanitizer, colorizer |
| Rendering | `OutputTests`, `NodeRendererTests`, `InkRendererTests`, `RenderToStringTests` | Output buffer, rendering pipeline, end-to-end |
| Input | `KeypressParserTests`, `InputParserTests`, `KittyKeyboardTests` | Keypress parser, Kitty keyboard protocol |
| Terminal | `CursorHelpersTests`, `LogUpdateTests`, `SynchronizedWriteTests` | Cursor, log-update, synchronized output |
| Accessibility | `ScreenReaderTests` | aria-label, aria-role, aria-hidden, aria-state, screen reader output |
| Measurement | `MeasureElementTests`, `BoxMetricsTests` | Element dimensions, reactive metrics tracking |
| I/O Providers | `StdioProviderTests` | StdinProvider, StdoutProvider, StderrProvider |

### JS Test File Mapping

| JS Test File | C# Test File |
|-------------|-------------|
| `flex-direction.tsx` | `FlexDirectionTests.cs` |
| `flex-wrap.tsx` | `FlexWrapTests.cs` |
| `gap.tsx` | `GapTests.cs` |
| `margin.tsx` / `padding.tsx` | `MarginPaddingGapTests.cs` |
| `text-width.tsx` | `TextWidthTests.cs` |
| `measure-text.tsx` | `MeasureTextTests.cs` |
| `measure-element.tsx` | `MeasureElementTests.cs` |
| `write-synchronized.tsx` | `SynchronizedWriteTests.cs` |
| `log-update.tsx` | `LogUpdateTests.cs` |
| `borders.tsx` | `BorderTests.cs` |
| `display.tsx` | `DisplayTests.cs` |
| `overflow.tsx` | `OverflowTests.cs` |
| `flex.tsx` | `FlexTests.cs` |
| `screen-reader.tsx` | `ScreenReaderTests.cs` |
| `use-box-metrics.tsx` | `BoxMetricsTests.cs` |
| `Static` (component) | `StaticComponentTests.cs` |
| `use-stdin.ts` / `use-stdout.ts` / `use-stderr.ts` | `StdioProviderTests.cs` |

## Examples

Run the interactive examples:

```sh
cd src/Ink.Net.Examples
dotnet run -- <example>
```

Available examples:

| Example | Command | JS Equivalent |
|---------|---------|--------------|
| Border styles | `dotnet run -- borders` | `examples/borders/borders.tsx` |
| Background colors | `dotnet run -- box-backgrounds` | `examples/box-backgrounds/box-backgrounds.tsx` |
| Justify content | `dotnet run -- justify-content` | `examples/justify-content/justify-content.tsx` |
| Table layout | `dotnet run -- table` | `examples/table/table.tsx` |
| Live counter | `dotnet run -- counter` | `examples/counter/counter.tsx` |
| Incremental rendering | `dotnet run -- incremental-rendering` | `examples/incremental-rendering/incremental-rendering.tsx` |
| Focus management | `dotnet run -- use-focus` | `examples/use-focus/use-focus.tsx` |
| Focus by ID | `dotnet run -- use-focus-with-id` | `examples/use-focus-with-id/use-focus-with-id.tsx` |
| Keyboard input | `dotnet run -- use-input` | `examples/use-input/use-input.tsx` |
| List selection | `dotnet run -- select-input` | `examples/select-input/select-input.tsx` |
| Terminal resize | `dotnet run -- terminal-resize` | `examples/terminal-resize/terminal-resize.tsx` |
| Chat app | `dotnet run -- chat` | `examples/chat/chat.tsx` |
| Static output | `dotnet run -- static` | `examples/static/static.tsx` |
| Stdout writing | `dotnet run -- use-stdout` | `examples/use-stdout/use-stdout.tsx` |
| Stderr writing | `dotnet run -- use-stderr` | `examples/use-stderr/use-stderr.tsx` |
| Cursor/IME | `dotnet run -- cursor-ime` | `examples/cursor-ime/cursor-ime.tsx` |
| Subprocess output | `dotnet run -- subprocess-output` | `examples/subprocess-output/subprocess-output.tsx` |
| Accessibility (ARIA) | `dotnet run -- aria` | `examples/aria/aria.tsx` |
| Render throttle | `dotnet run -- render-throttle` | `examples/render-throttle/index.tsx` |
| Router navigation | `dotnet run -- router` | `examples/router/router.tsx` |

### JS Example File Mapping

| JS Example | C# File |
|-----------|---------|
| `borders/borders.tsx` | `Borders.cs` |
| `box-backgrounds/box-backgrounds.tsx` | `BoxBackgrounds.cs` |
| `justify-content/justify-content.tsx` | `JustifyContent.cs` |
| `table/table.tsx` | `Table.cs` |
| `counter/counter.tsx` | `Counter.cs` |
| `incremental-rendering/incremental-rendering.tsx` | `IncrementalRendering.cs` |
| `use-focus/use-focus.tsx` | `UseFocus.cs` |
| `use-focus-with-id/use-focus-with-id.tsx` | `UseFocusWithId.cs` |
| `use-input/use-input.tsx` | `UseInput.cs` |
| `select-input/select-input.tsx` | `SelectInput.cs` |
| `terminal-resize/terminal-resize.tsx` | `TerminalResize.cs` |
| `chat/chat.tsx` | `Chat.cs` |
| `static/static.tsx` | `StaticExample.cs` |
| `use-stdout/use-stdout.tsx` | `UseStdout.cs` |
| `use-stderr/use-stderr.tsx` | `UseStderr.cs` |
| `cursor-ime/cursor-ime.tsx` | `CursorIme.cs` |
| `subprocess-output/subprocess-output.tsx` | `SubprocessOutput.cs` |
| `aria/aria.tsx` | `Aria.cs` |
| `render-throttle/index.tsx` | `RenderThrottle.cs` |
| `router/router.tsx` | `Router.cs` |

> **Note:** `concurrent-suspense`, `suspense`, and `use-transition` are React-specific examples that cannot be directly ported to C#. `alternate-screen` depends on alternate screen buffer support which is not yet implemented.

## Benchmarks

Run benchmarks with BenchmarkDotNet:

```sh
cd src/Ink.Net.Benchmarks
dotnet run -c Release -- --filter "*"
```

Available benchmark suites:

| Suite | JS Equivalent | Benchmarks |
|-------|---------------|------------|
| `SimpleBenchmarks` | `benchmark/simple/simple.tsx` | Single render, 1K/10K rerenders |
| `StaticBenchmarks` | `benchmark/static/static.tsx` | Accumulated items rendering (10/100/500 items) |
| `TextBenchmarks` | — | String width (plain/CJK/ANSI/emoji), wrapping, truncation |
| `OutputBenchmarks` | — | Output buffer write+get, ANSI writes, clipping, large grid |
| `InputBenchmarks` | — | Keypress parsing, Kitty protocol, InputHandler dispatch |
| `FocusBenchmarks` | — | Focus registration, Tab navigation, direct focus |
| `TransformBenchmarks` | — | Output transform processing, nested transforms |

## Architecture

Ink.Net faithfully replicates the Ink rendering pipeline:

```
TreeBuilder → DOM Tree → Yoga Layout → NodeRenderer → Output Buffer → Terminal String
    │              │           │              │              │              │
 Box/Text     DomElement   YGNode     Write to grid    StyledChar[][]    ANSI output
 Spacer       TextNode     Layout     Clip regions     Style tracking    TrimEnd
 Newline      Style        Calculate  Background fill  Compose lines     Reset codes
```

1. **TreeBuilder** constructs an `InkNode` DOM tree imperatively
2. **StyleApplier** maps `InkStyle` properties to `YogaNode` style properties
3. **Yoga.Net** calculates the Flexbox layout (`YGNodeCalculateLayout`)
4. **NodeRenderer** walks the tree and writes to an `Output` buffer
5. **Output** manages a 2D grid of `StyledChar`, handling ANSI styles and clipping
6. The final string is composed with proper ANSI escape sequences and line trimming

## Project Structure

```
Ink.Net/src/
├── Ink.Net/                    # Core library
│   ├── Ansi/                   # ANSI tokenizer & sanitizer
│   ├── Builder/                # TreeBuilder (imperative DOM construction)
│   ├── Dom/                    # DOM tree (InkNode, DomElement, DomTree, AccessibilityInfo)
│   ├── Input/                  # Keypress & input parsing, Kitty protocol, FocusManager
│   ├── Rendering/              # Renderer, Output buffer, Colorizer, Borders, BoxMetrics
│   ├── Styles/                 # InkStyle, StyleApplier, enums
│   ├── Terminal/               # Cursor, LogUpdate, SynchronizedWrite, Stdio providers
│   ├── Text/                   # StringWidth, TextMeasurer, TextWrapper
│   ├── InkApp.cs              # Main orchestration (render / renderToString)
│   └── InkApplication.cs     # Full application coordinator (input, focus, cursor)
├── Ink.Net.Tests/              # 518 xUnit tests (ported from Ink JS test suite)
├── Ink.Net.Examples/           # 20 interactive examples (1:1 with JS examples)
├── Ink.Net.Benchmarks/         # 7 BenchmarkDotNet suites (1:1 with JS + extras)
└── ink.sln                     # Solution file
```

## Compatibility

| Feature | Status |
|---------|--------|
| .NET 8.0 | ✅ |
| .NET 9.0 | ✅ |
| .NET 10.0 | ✅ |
| Native AOT | ✅ (`IsAotCompatible`) |
| Trimming | ✅ (`EnableTrimAnalyzer`) |
| Windows | ✅ |
| Linux | ✅ |
| macOS | ✅ |

## Credits

- [Ink](https://github.com/vadimdemedes/ink) — The original React-based terminal rendering engine by [Vadim Demedes](https://github.com/vadimdemedes)
- [Yoga](https://yogalayout.dev/) — Facebook's cross-platform Flexbox layout engine
- [Yoga.Net](https://www.nuget.org/packages/Yoga.Net) — .NET binding for Yoga

## License

MIT
