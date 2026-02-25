using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Utils;

[AttributeUsage(AttributeTargets.Field)]
public class EnumValueSetAttribute : InfrequentPropertyNegativeAttribute
{
    public string EnumDefinitionUri { get; }

    public EnumValueSetAttribute(string enumDefinitionUri)
    {
        EnumDefinitionUri = enumDefinitionUri;
    }

    public override bool ShouldRemove(List<XmlDocumentNavigator> items, string originalPath)
    {
        var propertyItems = items
            .Where(x => x.EvaluateCondition(originalPath))
            .Select(x => x.SelectSingleNode(originalPath))
            .ToList();

        var filter = propertyItems.Any(ShouldRemove);

        return filter;
    }


    /// <summary>
    ///     Returns true when the given XML node should be filtered out because all the
    ///     present enum candidates (code/display/value) are explicitly mapped and those
    ///     mappings intentionally have a null icon value.
    ///     Notes:
    ///     - If a candidate can't be converted to the enum (missing/unexpected value), it is NOT
    ///     considered filtered (we treat that as "good").
    ///     - For "f:coding" nodes we require each coding node to have at least one candidate and
    ///     every non-null candidate for every coding must be an explicit null-mapping to return true.
    /// </summary>
    private bool ShouldRemove(XmlDocumentNavigator item)
    {
        // codeable concept/coding case
        if (item.EvaluateCondition("f:coding"))
        {
            //TODO we will change the way we handle coding nodes in the future
            foreach (var coding in item.SelectAllNodes("f:coding"))
            {
                var code = coding.SelectSingleNode("f:code/@value").Node?.Value ?? string.Empty;
                var codeEnum = code.ToLower().ToEnum<SupportedCodes>();

                if (codeEnum == null)
                {
                    return false;
                }

                var found = EnumIconTooltip.TryGetIcon(codeEnum.Value, EnumDefinitionUri, out var icon);

                // If mapping does not exist, or mapping exists and icon is non-null -> not filtered
                if (!found)
                {
                    return false;
                }

                // If we found the icon, and it is null, that means we don't want to show it
                if (icon != null)
                {
                    return false;
                }
            }

            // All coding nodes had only explicit-null mappings -> filtered
            return true;
        }

        // code case
        if (item.EvaluateCondition("@value"))
        {
            var value = item.SelectSingleNode("@value").Node?.Value ?? string.Empty;
            var valueEnum = value.ToLower().ToEnum<SupportedCodes>();

            // missing/unexpected value -> not filtered
            if (valueEnum == null)
            {
                return false;
            }

            var found = EnumIconTooltip.TryGetIcon(valueEnum.Value, EnumDefinitionUri, out var icon);
            return found && icon == null;
        }

        return false;
    }
}