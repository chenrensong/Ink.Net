// 1:1 port of instances.ts
// Store all instances of Ink to ensure that consecutive render() calls
// use the same instance of Ink and don't create a new one.
//
// This map has to be stored in a separate file, because render creates instances,
// but InkApp should delete itself from the map on unmount.

using System.Runtime.CompilerServices;

namespace Ink.Net;

/// <summary>
/// Global instance registry. Maps output streams to their active <see cref="InkApp"/>
/// instances so consecutive render calls reuse the same instance.
/// Uses <see cref="ConditionalWeakTable{TKey,TValue}"/> as the C# equivalent
/// of JavaScript's <c>WeakMap</c>.
/// </summary>
public static class Instances
{
    private static readonly ConditionalWeakTable<TextWriter, InkApp> Map = new();

    /// <summary>
    /// Try to retrieve an existing <see cref="InkApp"/> associated with
    /// <paramref name="stdout"/>.
    /// </summary>
    public static bool TryGet(TextWriter stdout, out InkApp? instance)
    {
        return Map.TryGetValue(stdout, out instance);
    }

    /// <summary>
    /// Associate <paramref name="instance"/> with <paramref name="stdout"/>,
    /// replacing any previous association.
    /// </summary>
    public static void Set(TextWriter stdout, InkApp instance)
    {
        Map.AddOrUpdate(stdout, instance);
    }

    /// <summary>
    /// Remove the association for <paramref name="stdout"/>.
    /// </summary>
    public static bool Remove(TextWriter stdout)
    {
        return Map.Remove(stdout);
    }
}
