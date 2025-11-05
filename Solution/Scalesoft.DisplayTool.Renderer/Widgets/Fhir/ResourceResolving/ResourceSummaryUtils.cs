using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;

public static class ResourceSummaryUtils
{
    public static ResourceSummaryModel? SummaryByHumanName(XmlDocumentNavigator navigator, string path = "f:name")
    {
        var nameNavs = navigator.SelectAllNodes(path).ToList();
        if (nameNavs.Count == 0)
        {
            return null;
        }

        var result = new List<Widget>();

        for (var i = 0; i < nameNavs.Count; i++)
        {
            var nameNav = nameNavs[i];
            if (nameNav.EvaluateCondition("(f:given and f:family) or (not(f:text/@value) and (f:family or f:given))"))
            {
                var val = new Container([
                    new ConcatBuilder("f:prefix/@value", _ =>
                    [
                        new Text()
                    ], " "),
                    new ConstantText(" "),
                    new ConcatBuilder("f:given/@value", _ =>
                    [
                        new Text()
                    ], " "),
                    new ConstantText(" "),
                    new ConcatBuilder("f:family/@value", _ =>
                    [
                        new Text()
                    ], " "),
                    new ConstantText(" "),
                    new ConcatBuilder("f:suffix/@value", _ =>
                    [
                        new Text()
                    ], ", "),
                ], ContainerType.Span);

                result.Add(new ChangeContext(nameNav, val));
            }
            else if (nameNav.EvaluateCondition("f:text/@value"))
            {
                var name = nameNav.SelectSingleNode("f:text/@value").Node?.Value;
                if (name != null)
                {
                    result.Add(new ConstantText(name));
                }
            }

            if (i < nameNavs.Count - 1)
            {
                result.Add(new ConstantText("; "));
            }
        }

        if (result.Count != 0)
        {
            return new ResourceSummaryModel
            {
                Value = new Container(result, ContainerType.Span),
            };
        }

        return null;
    }
}