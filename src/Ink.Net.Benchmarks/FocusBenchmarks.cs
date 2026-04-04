// Focus management benchmarks — measures focus registration, navigation, and lookup.
// Supplements JS benchmarks by covering the new focus subsystem.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Ink.Net.Input;

namespace Ink.Net.Benchmarks;

/// <summary>
/// Focus management benchmarks — measures registration, Tab navigation, and focus lookup.
/// <para>
/// No direct JS benchmark equivalent; covers the focus subsystem added in Ink.Net.
/// </para>
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class FocusBenchmarks
{
    [Params(5, 20, 100)]
    public int FocusableCount { get; set; }

    [Benchmark(Description = "Focus: register N items")]
    public FocusManager RegisterItems()
    {
        var fm = new FocusManager();
        for (int i = 0; i < FocusableCount; i++)
        {
            fm.Add($"item-{i}");
        }
        return fm;
    }

    [Benchmark(Description = "Focus: cycle through N items (FocusNext)")]
    public void CycleNext()
    {
        var fm = new FocusManager();
        for (int i = 0; i < FocusableCount; i++)
        {
            fm.Add($"item-{i}");
        }

        // Cycle through all items twice
        for (int i = 0; i < FocusableCount * 2; i++)
        {
            fm.FocusNext();
        }
    }

    [Benchmark(Description = "Focus: cycle through N items (FocusPrevious)")]
    public void CyclePrevious()
    {
        var fm = new FocusManager();
        for (int i = 0; i < FocusableCount; i++)
        {
            fm.Add($"item-{i}");
        }

        for (int i = 0; i < FocusableCount * 2; i++)
        {
            fm.FocusPrevious();
        }
    }

    [Benchmark(Description = "Focus: direct focus by ID")]
    public void DirectFocus()
    {
        var fm = new FocusManager();
        for (int i = 0; i < FocusableCount; i++)
        {
            fm.Add($"item-{i}");
        }

        // Focus each item directly
        for (int i = 0; i < FocusableCount; i++)
        {
            fm.Focus($"item-{i}");
        }
    }

    [Benchmark(Description = "Focus: IsFocused lookup")]
    public bool IsFocusedLookup()
    {
        var fm = new FocusManager();
        for (int i = 0; i < FocusableCount; i++)
        {
            fm.Add($"item-{i}");
        }

        fm.FocusNext(); // Focus first item

        bool result = false;
        for (int i = 0; i < FocusableCount; i++)
        {
            result = fm.IsFocused($"item-{i}");
        }
        return result;
    }

    [Benchmark(Description = "Focus: Tab navigation via HandleInput")]
    public void TabNavigation()
    {
        var fm = new FocusManager();
        for (int i = 0; i < FocusableCount; i++)
        {
            fm.Add($"item-{i}", new FocusOptions { AutoFocus = i == 0 });
        }

        // Simulate Tab presses
        for (int i = 0; i < FocusableCount; i++)
        {
            fm.HandleInput("\t");
        }
    }
}
