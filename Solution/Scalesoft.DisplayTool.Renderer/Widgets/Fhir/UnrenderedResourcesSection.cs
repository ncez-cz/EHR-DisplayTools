using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

/// <summary>
///     This widget shows resources contained in the document that weren't rendered elsewhere
///     (instances of AnyReferenceNaming do not count)
///     <br />
///     This widget should be the last drawn widgets - otherwise
/// </summary>
public class UnrenderedResourcesSection : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var unrendered = context.GetUnrenderedResources();
        if (unrendered.Count == 0)
        {
            return string.Empty;
        }

        var grouped = unrendered.GroupBy(x => x.Node?.Name);

        var widgets = grouped.Select(x => new AnyResource(x.ToList(), x.Key)).Cast<Widget>().ToList();

        var tree = new Section(".", null, [new LocalizedLabel("general.other-resources")],
            widgets, titleAbbreviations: SectionTitleAbbreviations.Other, customClass: "other-section-margin",
            isCollapsed: true);

        return await tree.Render(navigator, renderer, context);
    }
}