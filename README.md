<div align="center">
  <br>
  <h1>Ink.Net</h1>
  <p><strong>A declarative terminal rendering engine for .NET, ported from <a href="https://github.com/vadimdemedes/ink">Ink</a>.</strong></p>
  <p>Build beautiful CLI output using Flexbox layouts — powered by <a href="https://www.nuget.org/packages/Yoga.Net">Yoga.Net</a>.</p>
  <br>
</div>

---

> **Ink** is React for CLIs. **Ink.Net** brings the same Flexbox-based layout engine to C# and .NET, without the React dependency. It is a faithful 1:1 port of Ink's rendering pipeline — DOM tree → Yoga layout → Output buffer → Terminal string — optimized for **Native AOT** compatibility.

## Features

- 🧱 **Flexbox Layout** — Full Yoga layout support: `flexDirection`, `flexWrap`, `flexGrow`, `flexShrink`, `flexBasis`, `alignItems`, `alignSelf`, `alignContent`, `justifyContent`, `gap`
- 📏 **Dimensions** — `width`, `height`, `minWidth`, `minHeight`, `maxWidth`, `maxHeight` (absolute, percent, auto), `aspectRatio`
- 📦 **Box Model** — `margin`, `padding` (all edges), `overflow` (clip X/Y), borders (single, double, round, bold, classic, and custom)
- 🎨 **Colors** — Named colors, hex (`#FF0000`), RGB (`rgb(255,0,0)`), ANSI-256, dim, background color inheritance
- ✂️ **Text Handling** — Word-level wrapping, truncation (end, middle, start), CJK-aware string width, ANSI-preserving text processing
- 🖥️ **Terminal I/O** — Cursor control, synchronized writes, log-update, terminal size detection
- ⌨️ **Input Parsing** — Keypress parser, Kitty keyboard protocol, bracketed paste mode
- 🚀 **AOT-Compatible** — Designed for `PublishAot` / trimming from the start
- ✅ **315 Unit Tests** — Comprehensive xUnit test suite ported from Ink's original test files

## Quick Start

### Install

```xml
<PackageReference Include="Ink.Net" Version="1.0.0" />
```

Or add via CLI:
```sh
dotnet add package Ink.Net
```

### Basic Usage

```csharp
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;

// Render to string (for testing or static output)
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

### Live Terminal Rendering

```csharp
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;

// Render to the terminal (live, with updates)
using var instance = InkApp.Render(b => new[]
{
    b.Box(new InkStyle { BorderStyle = "round", Padding = 1 }, new[]
    {
        b.Text("Hello from Ink.Net!"),
    })
});

// Rerender with new content
instance.Rerender(b => new[]
{
    b.Box(new InkStyle { BorderStyle = "round", Padding = 1 }, new[]
    {
        b.Text("Updated content!"),
    })
});

// Clean up
instance.Unmount();
```

## API Reference

### `InkApp.RenderToString`

Renders a tree to a string — ideal for testing and static output.

```csharp
string output = InkApp.RenderToString(
    b => new[] { b.Text("Hello") },
    new RenderToStringOptions { Columns = 80 }
);
```

### `InkApp.Render`

Renders a tree to the terminal with live updates.

```csharp
using var instance = InkApp.Render(
    b => new[] { b.Text("Live!") },
    new RenderOptions { Columns = 80, Rows = 24 }
);
instance.Rerender(b => new[] { b.Text("Updated!") });
instance.Unmount();
```

### `TreeBuilder`

The imperative API for building the DOM tree:

| Method | Description |
|--------|-------------|
| `b.Text(content)` | Create a text node |
| `b.Box(style?, children?, transform?)` | Create a box (flex container) |
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
| `Padding` / `PaddingTop/Right/Bottom/Left` | `float?` | Padding (all edges) |
| `Position` | `PositionMode?` | `Relative`, `Absolute`, `Static` |
| `Top/Right/Bottom/Left` | `float?` | Position offsets |

</details>

<details>
<summary><strong>Borders</strong></summary>

| Property | Type | Description |
|----------|------|-------------|
| `BorderStyle` | `string?` | `"single"`, `"double"`, `"round"`, `"bold"`, `"singleDouble"`, `"doubleSingle"`, `"classic"`, or custom |
| `BorderColor` | `string?` | Border color |
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
    Width = DimensionValue.From(40),        // 40 columns
    Height = DimensionValue.Percent(50),    // 50% of parent
    MinWidth = DimensionValue.Auto,         // Auto
}
```

## Examples

Run the interactive examples:

```sh
cd src/Ink.Net.Examples
dotnet run -- borders         # Border styles demo
dotnet run -- backgrounds     # Background colors demo
dotnet run -- justify         # Justify content demo
dotnet run -- table           # Table layout demo
dotnet run -- simple          # Simple text demo
```

## Benchmarks

Run benchmarks with BenchmarkDotNet:

```sh
cd src/Ink.Net.Benchmarks
dotnet run -c Release -- --filter "*"
```

Available benchmark suites:

| Suite | Benchmarks |
|-------|------------|
| `RenderBenchmarks` | Simple text, complex tree, 1000 rerenders, table layout, bordered boxes |
| `TextBenchmarks` | String width (plain/ANSI), text wrapping, truncation modes |
| `OutputBenchmarks` | Output buffer write+get, ANSI writes, clipped writes |

## Project Structure

```
Ink.Net/src/
├── Ink.Net/                    # Core library
│   ├── Ansi/                   # ANSI tokenizer & sanitizer
│   ├── Builder/                # TreeBuilder (imperative DOM construction)
│   ├── Dom/                    # DOM tree (InkNode, DomElement, DomTree)
│   ├── Input/                  # Keypress & input parsing, Kitty protocol
│   ├── Rendering/              # Renderer, Output buffer, Colorizer, Borders
│   ├── Styles/                 # InkStyle, StyleApplier
│   ├── Terminal/               # Cursor, LogUpdate, SynchronizedWrite
│   ├── Text/                   # StringWidth, TextMeasurer, TextWrapper
│   └── InkApp.cs              # Main orchestration (render / renderToString)
├── Ink.Net.Tests/              # 315 xUnit tests (ported from Ink JS test suite)
├── Ink.Net.Examples/           # Interactive examples
├── Ink.Net.Benchmarks/         # BenchmarkDotNet benchmarks
└── ink.sln                     # Solution file
```

## Testing

```sh
cd src
dotnet test
```

All **315 tests** cover:

| Category | Tests |
|----------|-------|
| Components | Text, Box, Spacer, Newline, transforms |
| Flex Layout | Direction, wrap, grow, shrink, basis |
| Alignment | align-items, align-self, align-content, justify-content |
| Dimensions | Width/height, min/max, percent, aspect-ratio |
| Box Model | Margin, padding, gap, overflow, position |
| Borders | All styles, colors, individual edges, dim, custom |
| Background | Inheritance, colors (named/hex/rgb/ansi256), fill |
| Text | Colors, ANSI preservation, C1 handling, wrapping |
| ANSI | Tokenizer, sanitizer, colorizer |
| Rendering | Output buffer, NodeRenderer, InkRenderer |
| Input | Keypress parser, input parser, Kitty keyboard |
| Terminal | Cursor helpers |
| Integration | `RenderToString` end-to-end |

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

## Compatibility

| Feature | Status |
|---------|--------|
| .NET 8.0 | ✅ |
| Native AOT | ✅ (IsAotCompatible) |
| Trimming | ✅ (EnableTrimAnalyzer) |
| Windows | ✅ |
| Linux | ✅ |
| macOS | ✅ |

## Credits

- [Ink](https://github.com/vadimdemedes/ink) — The original React-based terminal rendering engine by Vadim Demedes
- [Yoga](https://yogalayout.dev/) — Facebook's cross-platform Flexbox layout engine
- [Yoga.Net](https://www.nuget.org/packages/Yoga.Net) — .NET binding for Yoga

## License

MIT
