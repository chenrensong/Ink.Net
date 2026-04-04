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
        break;
}
