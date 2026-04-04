// Ink.Net Examples — ported from JS Ink examples
// Usage: dotnet run -- <example-name>
// Each example is a separate file matching the JS examples/ directory structure.

using Ink.Net.Examples;

var exampleName = args.Length > 0 ? args[0] : "help";

switch (exampleName.ToLowerInvariant())
{
    case "borders":
        Borders.Run();
        break;
    case "backgrounds":
    case "box-backgrounds":
        BoxBackgrounds.Run();
        break;
    case "justify-content":
        JustifyContentExample.Run();
        break;
    case "table":
        Table.Run();
        break;
    case "counter":
        await Counter.RunAsync();
        break;
    case "incremental-rendering":
        await IncrementalRendering.RunAsync();
        break;
    case "use-focus":
        await UseFocusExample.RunAsync();
        break;
    case "use-focus-with-id":
        await UseFocusWithIdExample.RunAsync();
        break;
    case "use-input":
        await UseInputExample.RunAsync();
        break;
    case "select-input":
        await SelectInputExample.RunAsync();
        break;
    case "terminal-resize":
        await TerminalResizeExample.RunAsync();
        break;
    case "chat":
        await ChatExample.RunAsync();
        break;
    case "static":
        await StaticExample.RunAsync();
        break;
    case "use-stdout":
        await UseStdoutExample.RunAsync();
        break;
    case "use-stderr":
        await UseStderrExample.RunAsync();
        break;
    case "cursor-ime":
        await CursorImeExample.RunAsync();
        break;
    case "subprocess-output":
        await SubprocessOutputExample.RunAsync();
        break;
    case "aria":
        await AriaExample.RunAsync();
        break;
    case "render-throttle":
        await RenderThrottleExample.RunAsync();
        break;
    case "router":
        await RouterExample.RunAsync();
        break;
    default:
        Console.WriteLine("Ink.Net Examples");
        Console.WriteLine("================");
        Console.WriteLine("Usage: dotnet run -- <example>");
        Console.WriteLine();
        Console.WriteLine("Available examples:");
        Console.WriteLine("  borders                - Border styles showcase");
        Console.WriteLine("  box-backgrounds        - Background color examples");
        Console.WriteLine("  justify-content        - Flexbox justifyContent demo");
        Console.WriteLine("  table                  - Table layout with percentage widths");
        Console.WriteLine("  counter                - Live counter (interactive, Ctrl+C to exit)");
        Console.WriteLine("  incremental-rendering  - Live dashboard with progress bars");
        Console.WriteLine("  use-focus              - Focus management with Tab/Shift+Tab");
        Console.WriteLine("  use-focus-with-id      - Focus by ID (press 1/2/3 or Tab)");
        Console.WriteLine("  use-input              - Arrow key movement (interactive)");
        Console.WriteLine("  select-input           - List selection with Up/Down arrows");
        Console.WriteLine("  terminal-resize        - Terminal size display (resize to update)");
        Console.WriteLine("  chat                   - Chat app (type + Enter to send)");
        Console.WriteLine("  static                 - Incremental test results");
        Console.WriteLine("  use-stdout             - Writing to stdout periodically");
        Console.WriteLine("  use-stderr             - Writing to stderr periodically");
        Console.WriteLine("  cursor-ime             - Cursor positioning with IME support");
        Console.WriteLine("  subprocess-output      - Subprocess output display");
        Console.WriteLine("  aria                   - Accessibility (aria-role/state/label)");
        Console.WriteLine("  render-throttle        - Render throttling with maxFps");
        Console.WriteLine("  router                 - Multi-page navigation (state machine)");
        break;
}
