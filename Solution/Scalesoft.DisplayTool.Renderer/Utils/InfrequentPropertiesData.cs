using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Scalesoft.DisplayTool.Renderer.Utils;

public class InfrequentPropertiesData<T> where T : notnull
{
    private readonly Dictionary<T, string> m_infrequentProperties = new();
    public IReadOnlyDictionary<T, string> InfrequentProperties => m_infrequentProperties.AsReadOnly();

    protected InfrequentPropertiesData(IReadOnlyDictionary<T, string> infrequentProperties)
    {
        m_infrequentProperties = new Dictionary<T, string>(infrequentProperties);
    }

    public InfrequentPropertiesData()
    {
    }

    public void Add(T property, string path)
    {
        m_infrequentProperties.TryAdd(property, path);
    }

    public void AddAll(T property, IEnumerable<string> path)
    {
        foreach (var pathItem in path)
        {
            m_infrequentProperties.TryAdd(property, pathItem);
        }
    }

    public void Remove(T property)
    {
        m_infrequentProperties.Remove(property);
    }

    public bool TryGet(T property, [NotNullWhen(true)] out string? xpath)
    {
        return m_infrequentProperties.TryGetValue(property, out xpath);
    }

    public bool Contains(T property)
    {
        return m_infrequentProperties.ContainsKey(property);
    }

    public bool ContainsAnyOf(params T[] properties)
    {
        return properties.Any(x => m_infrequentProperties.ContainsKey(x));
    }

    public bool ContainsAllOf(params T[] properties)
    {
        return properties.All(x => m_infrequentProperties.ContainsKey(x));
    }

    public bool HasAnyOfGroup(string groupName)
    {
        return m_infrequentProperties.Keys
            .Select(property =>
            {
                var name = property.ToString();
                return string.IsNullOrEmpty(name) ? null : typeof(T).GetField(name);
            })
            .Select(field => field?.GetCustomAttribute<GroupAttribute>())
            .Any(groupAttribute => groupAttribute?.GroupName == groupName);
    }

    public bool ContainsOnly(params T[] items)
    {
        var sourceSet = m_infrequentProperties.Keys.ToHashSet();
        var itemsSet = items.ToHashSet();

        return sourceSet.SetEquals(itemsSet);
    }

    public int Count => m_infrequentProperties.Count;

    public bool All(Func<T, bool> predicate) => m_infrequentProperties.Keys.All(predicate);
}