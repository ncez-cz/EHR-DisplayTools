using System.Reflection;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;

public static class SupportedResourceProvider
{
    public static readonly Dictionary<string, ResourceWidgetDescriptor> SupportedResources = GetSupportedResources();

    private static Dictionary<string, ResourceWidgetDescriptor> GetSupportedResources()
    {
        var resourceWidgetTypes = typeof(IResourceWidget).Assembly.GetTypes()
            .Where(x => x.IsAssignableTo(typeof(IResourceWidget)))
            .Where(t => !t.IsAbstract);

        Dictionary<string, ResourceWidgetDescriptor> resourceDescriptors = [];
        foreach (var type in resourceWidgetTypes)
        {
            var resourceType =
                type.GetProperty(nameof(IResourceWidget.ResourceType), BindingFlags.Static | BindingFlags.Public)
                    ?.GetValue(null) as string ?? string.Empty;
            var requiresTitle =
                type.GetProperty(
                        nameof(IResourceWidget.RequiresExternalTitle),
                        BindingFlags.Static | BindingFlags.Public
                    )
                    ?.GetValue(null) as bool? ?? false;

            var instantiateDelegate = GetMethod<InstantiateDelegate>(type, nameof(IResourceWidget.InstantiateMultiple));
            var renderSummaryDelegate = GetMethod<RenderSummaryDelegate>(type, nameof(IResourceWidget.RenderSummary));

            if (instantiateDelegate == null || renderSummaryDelegate == null)
            {
                continue;
            }

            resourceDescriptors.Add(
                resourceType,
                new ResourceWidgetDescriptor
                {
                    RequiresExternalTitle = requiresTitle,
                    Instantiate = instantiateDelegate,
                    RenderSummary = renderSummaryDelegate,
                }
            );
        }

        return resourceDescriptors;
    }

    private static TDelegate? GetMethod<TDelegate>(Type type, string name) where TDelegate : Delegate
    {
        foreach (var method in type.GetMethods())
        {
            if (method.Name != name || !method.IsStatic || !method.IsPublic)
            {
                continue;
            }

            // Try to bind the method to the delegate.
            // If it fails, the method is not compatible (incorrect number of parameters, etc.), and we skip it.
            if (Delegate.CreateDelegate(typeof(TDelegate), method, false) is TDelegate boundDelegate)
            {
                return boundDelegate;
            }
        }

        if (type.BaseType != null)
        {
            return GetMethod<TDelegate>(type.BaseType, name);
        }

        return GetMethod<TDelegate>(typeof(IResourceWidget), name);
    }
}