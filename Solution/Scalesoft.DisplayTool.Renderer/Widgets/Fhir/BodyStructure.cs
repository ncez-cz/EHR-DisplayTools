using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class BodyStructure(
    NameValuePair.NameValuePairStyle nameValuePairStyle = NameValuePair.NameValuePairStyle.Primary
) : Widget, IResourceWidget
{
    public static string ResourceType => "BodyStructure";

    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return items.Select(x => new ChangeContext(x, new Container(new BodyStructure(), idSource: x)))
            .ToList<Widget>();
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<Widget> tree =
        [
            new Choose([
                    // Show structured data, fallback to narrative text
                    new When(
                        "f:morphology or f:extension[@url='http://hl7.org/fhir/5.0/StructureDefinition/extension-BodyStructure.includedStructure.laterality']/f:valueCodeableConcept or f:location",
                        new Optional(
                            "f:morphology",
                            new NameValuePair([new ConstantText("Morfologie")],
                            [
                                new CodeableConcept()
                            ], style: nameValuePairStyle)
                        ),
                        new Optional(
                            "f:extension[@url='http://hl7.org/fhir/5.0/StructureDefinition/extension-BodyStructure.includedStructure.laterality']/f:valueCodeableConcept",
                            new NameValuePair([new ConstantText("Lateralita")],
                            [
                                new CodeableConcept()
                            ], style: nameValuePairStyle)),
                        new Condition(
                            "f:location",
                            new NameValuePair([new ConstantText("Lokalizace")],
                            [
                                new ConcatBuilder("f:locationQualifier", _ => [new CodeableConcept()], " "),
                                new ConstantText(" "),
                                new ChangeContext("f:location", new CodeableConcept()),
                            ], style: nameValuePairStyle)
                        )),
                ],
                new Optional("f:text", new Narrative())),
        ];
        return tree.RenderConcatenatedResult(navigator, renderer, context);
    }
}