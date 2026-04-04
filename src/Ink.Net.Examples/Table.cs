// Ported from examples/table/table.tsx
using Ink.Net;
using Ink.Net.Builder;
using Ink.Net.Styles;

namespace Ink.Net.Examples;

/// <summary>
/// Table layout with percentage widths — ported from JS Ink examples/table/table.tsx.
/// </summary>
public static class Table
{
    public static void Run()
    {
        var users = new[]
        {
            (Id: 0, Name: "Alice", Email: "alice@example.com"),
            (Id: 1, Name: "Bob", Email: "bob@example.com"),
            (Id: 2, Name: "Charlie", Email: "charlie@example.com"),
            (Id: 3, Name: "Diana", Email: "diana@example.com"),
            (Id: 4, Name: "Eve", Email: "eve@example.com"),
        };

        var output = InkApp.RenderToString(b =>
        {
            var rows = new List<TreeNode>();

            // Header
            rows.Add(b.Box(children: new[]
            {
                b.Box(new InkStyle { Width = DimensionValue.Percent(10) }, new[] { b.Text("ID") }),
                b.Box(new InkStyle { Width = DimensionValue.Percent(50) }, new[] { b.Text("Name") }),
                b.Box(new InkStyle { Width = DimensionValue.Percent(40) }, new[] { b.Text("Email") }),
            }));

            // Data rows
            foreach (var user in users)
            {
                rows.Add(b.Box(children: new[]
                {
                    b.Box(new InkStyle { Width = DimensionValue.Percent(10) },
                        new[] { b.Text(user.Id.ToString()) }),
                    b.Box(new InkStyle { Width = DimensionValue.Percent(50) },
                        new[] { b.Text(user.Name) }),
                    b.Box(new InkStyle { Width = DimensionValue.Percent(40) },
                        new[] { b.Text(user.Email) }),
                }));
            }

            return new[]
            {
                b.Box(new InkStyle { FlexDirection = FlexDirectionMode.Column, Width = 80 },
                    rows.ToArray())
            };
        });

        Console.WriteLine(output);
    }
}
