using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ShowMultiReference(
    string path,
    Func<List<XmlDocumentNavigator>, string?, IList<Widget>> foundItemGroupBuilder,
    Func<IEnumerable<Widget>, IList<Widget>> brokenReferencesBuilder
) : Widget

{
    public ShowMultiReference(
        string path,
        Func<List<XmlDocumentNavigator>, string?, Widget> foundItemGroupBuilder,
        Func<IEnumerable<Widget>, Widget> brokenReferencesBuilder
    )
        : this(
            path,
            (foundNavigators, groupName) => [foundItemGroupBuilder(foundNavigators, groupName)],
            brokenReferences => [brokenReferencesBuilder(brokenReferences)])
    {
    }

    public ShowMultiReference(
        Func<List<XmlDocumentNavigator>, string?, IList<Widget>> foundItemGroupBuilder,
        string path = "."
    )
        : this(
            path,
            foundItemGroupBuilder,
            brokenReferences =>
            [
                new Container([
                    new TextContainer(TextStyle.Bold, new ConstantText("Položky bez obsahu")),
                    new ItemList(ItemListType.Unordered, [..brokenReferences]),
                ], ContainerType.Div, "resource-container")
            ]
        )
    {
    }

    public ShowMultiReference(
        string path = ".",
        bool displayResourceType = true
    )
        : this(
            path,
            ((foundNavigators, groupName) =>
            [
                new AnyResource(foundNavigators, groupName, displayResourceType: displayResourceType)
            ]),
            brokenReferences =>
            [
                new Container([
                    new TextContainer(TextStyle.Bold, new ConstantText("Položky bez obsahu")),
                    new ItemList(ItemListType.Unordered, [..brokenReferences]),
                ], ContainerType.Div, "resource-container")
            ]
        )
    {
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var validReferences = new MultiReference(navigators =>
                new Concat(navigators.GroupBy(n => n.Node?.Name)
                    .Select(group => foundItemGroupBuilder(group.ToList(), group.Key)).SelectMany(x => x).ToList())
            , $"{path}/f:reference"
        );

        var referencesWithoutContent = ReferenceHandler.GetReferencesWithoutContent(navigator, path);
        var displayOnlyReferences = ReferenceHandler.GetReferencesWithDisplayValue(navigator, path);

        var withoutContentWidgets =
            referencesWithoutContent.Select(x => ReferenceHandler.BuildReferenceNameWidget(x, context, false));
        var displayOnlyWidgets = displayOnlyReferences.Select(x => new ChangeContext(x, new Text("f:display/@value")));

        List<Widget> brokenReferencesWidgets = [..withoutContentWidgets, ..displayOnlyWidgets];

        List<Widget> widgets = [validReferences];
        if (brokenReferencesWidgets.Count > 0)
        {
            widgets.AddRange(brokenReferencesBuilder(brokenReferencesWidgets));
        }

        var result = widgets.RenderConcatenatedResult(navigator, renderer, context);
        return result;
    }
}