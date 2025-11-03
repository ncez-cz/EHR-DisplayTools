using System.Reflection;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public static class InfrequentProperties
{
    public static Builder<T> Optional<T>(
        InfrequentPropertiesData<T> presentProperties,
        T property,
        Func<List<XmlDocumentNavigator>, IList<Widget>> builder
    ) where T : notnull
    {
        return new Builder<T>(presentProperties, property, builder);
    }


    /// <summary>
    ///     If the given property is present, renders children in the context of the property.
    ///     If the property is present multiple times, children are evaluated once per occurrence and the result is
    ///     concatenated.
    /// </summary>
    /// <param name="presentProperties"></param>
    /// <param name="property"></param>
    /// <param name="children"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Builder<T> Optional<T>(
        InfrequentPropertiesData<T> presentProperties,
        T property,
        params Widget[] children
    ) where T : notnull
    {
        return new Builder<T>(
            presentProperties,
            property,
            items =>
                items.Select(x => new ChangeContext(x, children)).ToList<Widget>()
        );
    }

    /// <summary>
    ///     If the given property is present, renders children in the context of the property.
    ///     If the property is present multiple times, children are evaluated once per occurence and the result is
    ///     concatenated.
    /// </summary>
    /// <param name="presentProperties"></param>
    /// <param name="property"></param>
    /// <param name="children"></param>
    /// <param name="separator"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Builder<T> Optional<T>(
        Widget separator,
        InfrequentPropertiesData<T> presentProperties,
        T property,
        params Widget[] children
    ) where T : notnull
    {
        return new Builder<T>(
            presentProperties,
            property,
            items =>
                items.Select(x => new ChangeContext(x, children)).ToList<Widget>(),
            separator
        );
    }

    /// <summary>
    ///     If the given property is present, returns children. If the property is present multiple times,
    ///     children are evaluated once per occurence and the result is concatenated.
    /// </summary>
    /// <param name="presentProperties"></param>
    /// <param name="property"></param>
    /// <param name="children"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Builder<T> Condition<T>(
        InfrequentPropertiesData<T> presentProperties,
        T property,
        params Widget[] children
    ) where T : notnull
    {
        return new Builder<T>(
            presentProperties,
            property,
            _ => children
        );
    }

    /// <summary>
    ///     If the given property is present, builds builder for every instance of the property in the context of the property.
    /// </summary>
    /// <param name="presentProperties"></param>
    /// <param name="property"></param>
    /// <param name="builder"></param>
    /// <param name="separator"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Builder<T> Optional<T>(
        InfrequentPropertiesData<T> presentProperties,
        T property,
        Func<XmlDocumentNavigator, IList<Widget>> builder,
        Widget? separator = null
    ) where T : notnull
    {
        return new Builder<T>(
            presentProperties,
            property,
            items =>
                items.Select(x => new ChangeContext(x, builder(x).ToArray())).ToList<Widget>(),
            separator
        );
    }

    public class Builder<T>(
        InfrequentPropertiesData<T> presentProperties,
        T property,
        Func<List<XmlDocumentNavigator>, IList<Widget>> builder,
        Widget? separator = null
    ) : Widget where T : notnull
    {
        public override async Task<RenderResult> Render(
            XmlDocumentNavigator navigator,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            if (!presentProperties.TryGet(property, out var path))
            {
                return string.Empty;
            }

            var elements = navigator.SelectAllNodes(path).ToList();
            return await new Concat(builder(elements), separator).Render(navigator, renderer, context);
        }
    }

    public static InfrequentPropertiesData<T> Evaluate<T>(List<XmlDocumentNavigator> items)
        where T : Enum
    {
        var allProperties = Enum.GetValues(typeof(T)).Cast<T>().ToList();

        var infrequentProperties = new InfrequentPropertiesData<T>();

        foreach (var property in allProperties)
        {
            var propertyName = property.ToString();
            var lowerCamelCasePropertyName = char.ToLowerInvariant(propertyName[0]) + propertyName[1..];

            var field = typeof(T).GetField(property.ToString());
            var attributes = field?.GetCustomAttributes<InfrequentPropertyAttribute>().ToList() ?? [];
            var negativeAttributes = field?.GetCustomAttributes<InfrequentPropertyNegativeAttribute>().ToList() ?? [];

            List<string> infrequentPaths = [];

            foreach (var attribute in attributes)
            {
                var path = attribute.Evaluate(items);
                if (path != null)
                {
                    infrequentPaths.Add(path);
                }
            }

            // Only try matching by name if no attributes were found to avoid accidental matches
            if (attributes.Count == 0)
            {
                var propertyPath = $"f:{lowerCamelCasePropertyName}";
                if (items.Any(x => x.EvaluateCondition(propertyPath)))
                {
                    infrequentPaths.Add(propertyPath);
                }
            }

            foreach (var path in infrequentPaths.ToList())
            {
                foreach (var attribute in negativeAttributes)
                {
                    var filter = attribute.ShouldRemove(items, path);
                    if (filter)
                    {
                        infrequentPaths.Remove(path);
                    }
                }
            }

            infrequentProperties.AddAll(property, infrequentPaths);
        }

        return infrequentProperties;
    }
}