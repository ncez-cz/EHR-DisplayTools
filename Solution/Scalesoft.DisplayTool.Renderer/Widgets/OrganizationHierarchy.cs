using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class OrganizationHierarchy : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var widget = new NameValuePair(new ConstantText("Organizace:"),
            new OrganizationHierarchyItem(),
            direction: FlexDirection.Row
        );

        return widget.Render(navigator, renderer, context);
    }
}

public class OrganizationHierarchyItem : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties = InfrequentProperties.Evaluate<HierarchyInfrequentProperties>([navigator]);

        var widget = new Concat([
            InfrequentProperties.Optional(infrequentProperties, HierarchyInfrequentProperties.PartOfOrServiceProvider,
                new Concat([
                    ShowSingleReference.WithDefaultDisplayHandler(_ =>
                    [
                        new OrganizationHierarchyItem(),
                        new If(t => t.EvaluateCondition("f:partOf|f:serviceProvider"),
                            new Icon(SupportedIcons.ChevronRight)),
                    ]),
                ])
            ),
            new AnyReferenceNamingWidget("f:partOf|f:serviceProvider"),
        ]);

        return widget.Render(navigator, renderer, context);
    }
}

public enum HierarchyInfrequentProperties
{
    [PropertyPath("f:serviceProvider|f:partOf")]
    PartOfOrServiceProvider,
}