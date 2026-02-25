using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Cda;

public class ShowNameWidget(bool showInline = false) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        Widget structuredNameWidgets = new Row(CreateNameWidgets(navigator));
        List<Widget> widgetTree =
        [
            new Choose([
                new When("$name/n1:family", structuredNameWidgets),
            ], new Container([
                new PlainBadge(new EhdsiDisplayLabel(LabelCodes.FamilyName), Severity.Primary),
                new Heading([new Text("$name")], HeadingSize.H5),
            ])),
        ];
        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }

    private List<Widget> CreateNameWidgets(XmlDocumentNavigator navigator)
    {
        var nameParts = new List<Widget>();
        nameParts.AddRange(NamePartWidget("$name/n1:prefix", LabelCodes.Prefix, navigator));
        nameParts.AddRange(NamePartWidget("$name/n1:given", LabelCodes.GivenName, navigator));

        var familyNameWidget = NamePartWidget("$name/n1:family", LabelCodes.FamilyName, navigator);
        var suffixWidget = NamePartWidget("$name/n1:suffix", LabelCodes.Prefix, navigator);
        if (showInline)
        {
            nameParts.AddRange([new Concat([..familyNameWidget, ..suffixWidget], ", ")]);
        }
        else
        {
            nameParts.AddRange(familyNameWidget);
            nameParts.AddRange(suffixWidget);
        }

        return !showInline ? nameParts : [new Concat(nameParts)];
    }

    private List<Widget> NamePartWidget(string path, string labelCode, XmlDocumentNavigator navigator)
    {
        if (!navigator.EvaluateCondition(path))
        {
            return [];
        }

        if (!showInline)
        {
            return
            [
                new Container([
                    new PlainBadge(new EhdsiDisplayLabel(labelCode), Severity.Primary),
                    new Heading([new Text(path)], HeadingSize.H5)
                ])
            ];
        }

        return [new Text(path)];
    }
}