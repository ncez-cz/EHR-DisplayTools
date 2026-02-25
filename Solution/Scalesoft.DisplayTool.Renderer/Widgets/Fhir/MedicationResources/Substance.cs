using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.MedicationResources;

public class Substance : SequentialResourceBase<Substance>, IResourceWidget
{
    public static string ResourceType => "Substance";
    [UsedImplicitly] public static bool RequiresExternalTitle => true;

    public static bool HasBorderedContainer(Widget widget) => false;

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<Widget> tree =
        [
            new Optional("f:code", new CodeableConcept()),
            new Condition("f:ingredient", new ConstantText(" - ")),
            new CommaSeparatedBuilder("f:ingredient", _ =>
            [
                new Optional("f:quantity", new ShowRatio()),
                new Choose([
                    new When("f:substanceCodeableConcept",
                        new Optional("f:itemCodeableConcept", new CodeableConcept())),
                    new When("f:substanceReference",
                        ShowSingleReference.WithDefaultDisplayHandler(
                            x => [new ChangeContext(x, new Container([new Substance()], idSource: x))],
                            "f:substanceReference")
                    ),
                ]),
            ]),
        ];

        return tree.RenderConcatenatedResult(navigator, renderer, context);
    }
}