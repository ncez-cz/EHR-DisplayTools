namespace Scalesoft.DisplayTool.Renderer.Utils;

public static class SetExtensions
{
    public static bool ContainsAny<T>(this ISet<T> set, params T[] items)
    {
        return items.Any(set.Contains);
    }
}