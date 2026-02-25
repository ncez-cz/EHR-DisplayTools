using System.Reflection;
using Scalesoft.DisplayTool.Renderer.Utils;

namespace Scalesoft.DisplayTool.Renderer.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : class
    {
        return source.Where(item => item != null).Cast<T>();
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : struct
    {
        return source.Where(item => item != null).Select(x => x!.Value);
    }


    public static bool ContainsAnyOf<T>(this IEnumerable<T> source, params T[] items)
    {
        return source.Any(items.Contains);
    }

    public static bool ContainsAllOf<T>(this IEnumerable<T> source, params T[] items)
    {
        return items.All(source.Contains);
    }

    public static List<T> Flatten<T>(this List<List<T>> source, T? separator = null) where T : class
    {
        var result = new List<T>();
        for (var index = 0; index < source.Count; index++)
        {
            var list = source[index];
            result.AddRange(list);
            if (separator != null && index < source.Count - 1)
            {
                result.Add(separator);
            }
        }

        return result;
    }

    public static bool HasAnyOfGroup<T>(this IEnumerable<T> source, string groupName) where T : Enum
    {
        return source
            .Select(property => typeof(T).GetField(property.ToString()))
            .Select(field => field?.GetCustomAttribute<GroupAttribute>())
            .Any(groupAttribute => groupAttribute?.GroupName == groupName);
    }

    public static bool ContainsOnly<T>(this IEnumerable<T> source, params T[] items)
    {
        var sourceSet = source.ToHashSet();
        var itemsSet = items.ToHashSet();

        return sourceSet.SetEquals(itemsSet);
    }

    public static IEnumerable<T> Intersperse<T>(this IEnumerable<T> source, T element)
    {
        var first = true;
        foreach (var value in source)
        {
            if (!first)
            {
                yield return element;
            }

            yield return value;
            first = false;
        }
    }

    public static bool Only<T>(this ICollection<T> source, T item)
    {
        return source.Count == 1 && source.Contains(item);
    }
}