using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;

public static class ResourceSummaryUtils
{
    public static ResourceSummaryModel? SummaryByHumanName(XmlDocumentNavigator navigator)
    {
        var resourceConfiguration = new ResourceConfiguration();
        var configurations = resourceConfiguration.ProcessConfigurations(navigator).Results;
        var path = configurations.First(r => r.Name == ResourceNames.Name).FormattedPath;

        var nameNav = navigator.SelectSingleNode(path);
        if (nameNav.Node == null)
        {
            return null;
        }

        Widget? result = null;

        if (nameNav.EvaluateCondition("(f:given and f:family) or (not(f:text/@value) and (f:family or f:given))"))
        {
            var val = new Container([
                new ConcatBuilder("f:prefix/@value", _ =>
                [
                    new Text(),
                ], " "),
                new ConstantText(" "),
                new ConcatBuilder("f:given/@value", _ =>
                [
                    new Text(),
                ], " "),
                new ConstantText(" "),
                new ConcatBuilder("f:family/@value", _ =>
                [
                    new Text(),
                ], " "),
                new ConstantText(" "),
                new ConcatBuilder("f:suffix/@value", _ =>
                [
                    new Text(),
                ], ", "),
            ], ContainerType.Span);

            result = new ChangeContext(nameNav, val);
        }
        else if (nameNav.EvaluateCondition("f:text/@value"))
        {
            var name = nameNav.SelectSingleNode("f:text/@value").Node?.Value;
            if (name != null)
            {
                result = new ConstantText(name);
            }
        }

        if (result == null)
        {
            return null;
        }

        return new ResourceSummaryModel
        {
            Value = new Container(result, ContainerType.Span),
        };
    }
}