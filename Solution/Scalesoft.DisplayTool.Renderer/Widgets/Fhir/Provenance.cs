using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class Provenance : RowResourceBase<Provenance>, IResourceWidget
{
    public static string ResourceType => "Provenance";

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentOptions =
            InfrequentProperties.Evaluate<InfrequentPropertiesPaths>([navigator]);

        var agentNav = navigator.SelectSingleNode("f:agent");
        var agentsInfrequentOptions =
            InfrequentProperties.Evaluate<AgentInfrequentPropertiesPaths>([agentNav]);

        var card = new Card(
                new Row(
                    [new ConstantText("Provenance"), new NarrativeModal()],
                    flexContainerClasses: "align-items-center"
                ),
                new Container(
                    [
                        new ChangeContext(
                            agentNav,
                            new PlainBadge(new DisplayLabel(LabelCodes.Author)),
                            new If(
                                _ => agentsInfrequentOptions.Contains(AgentInfrequentPropertiesPaths.Who),
                                new Heading(
                                    [new AnyReferenceNamingWidget("f:who", showOptionalDetails: false)],
                                    HeadingSize.H5
                                )
                            ),
                            new If(
                                _ => agentsInfrequentOptions.Contains(AgentInfrequentPropertiesPaths.Role),
                                new NameValuePair(
                                    [new ConstantText("Role")],
                                    [new CommaSeparatedBuilder("f:role", _ => new CodeableConcept())]
                                )
                            ),
                            new If(
                                _ => agentsInfrequentOptions.Contains(AgentInfrequentPropertiesPaths.Type),
                                new NameValuePair(
                                    [new ConstantText("Typ")],
                                    [new Optional("f:type", new CodeableConcept())]
                                )
                            ),
                            new If(
                                _ => agentsInfrequentOptions.Contains(AgentInfrequentPropertiesPaths.OnBehalfOf),
                                new NameValuePair(
                                    [new ConstantText("Na základě")],
                                    [new AnyReferenceNamingWidget("f:onBehalfOf")]
                                )
                            )
                        ),
                        new ThematicBreak(),
                        //Basic Info
                        new PlainBadge(new DisplayLabel(LabelCodes.Description)),
                        new If(
                            _ => infrequentOptions.Contains(InfrequentPropertiesPaths.Target),
                            new NameValuePair(
                                [new ConstantText("Ověřovaný záznam")],
                                [new AnyReferenceNamingWidget("f:target", showOptionalDetails: false)]
                            )
                        ),
                        new If(
                            _ => infrequentOptions.Contains(InfrequentPropertiesPaths.Occurred),
                            new NameValuePair(
                                [new ConstantText("Datum vzniku")],
                                [new OpenTypeElement(null, "occurred")]
                            )
                        ), // Period | dateTime
                        new If(
                            _ => infrequentOptions.Contains(InfrequentPropertiesPaths.Recorded),
                            new NameValuePair(
                                [new ConstantText("Datum zaznamenání")],
                                [new ShowDateTime("f:recorded")]
                            )
                        ),
                        new If(
                            _ => infrequentOptions.Contains(InfrequentPropertiesPaths.Reason),
                            new NameValuePair(
                                [new ConstantText("Důvod")],
                                [
                                    new ItemListBuilder(
                                        "f:reason",
                                        ItemListType.Unordered,
                                        _ => [new CodeableConcept()]
                                    ),
                                ]
                            )
                        ),
                        new If(
                            _ => infrequentOptions.Contains(InfrequentPropertiesPaths.Activity),
                            new NameValuePair(
                                [new ConstantText("Událost")],
                                [new Optional("f:reason", new CodeableConcept())]
                            )
                        ),
                        //Signature
                        new If(
                            _ => infrequentOptions.Contains(InfrequentPropertiesPaths.Signature),
                            new ThematicBreak(),
                            new PlainBadge(new ConstantText("Podpis")),
                            new ShowSignature("f:signature")
                        ),
                    ]
                ),
                footer: navigator.EvaluateCondition("f:text")
                    ? new NarrativeCollapser()
                    : null,
                optionalClass: context.RenderMode == RenderMode.Documentation ? null : "provenance-container"
            )
            ;

        return card.Render(navigator, renderer, context);
    }

    private enum InfrequentPropertiesPaths
    {
        Target, //Reference(Any)	Target Reference(s) (usually version specific)
        [OpenType("occurred")] Occurred, // 0..1		When the activity occurred
        Recorded, //	Σ	1..1	instant	When the activity was recorded / updated
        Reason, //		0..*	CodeableConcept	Reason the activity is occurring
        Activity, //		0..1	CodeableConcept	Activity that occurred
        Signature, //0..*	Signature	Signature on target
    }

    private enum AgentInfrequentPropertiesPaths
    {
        Type, //0..1	CodeableConcept	How the agent participated
        Role, //0..*	CodeableConcept	What the agents role was
        Who, // 1..1	Reference(Practitioner | PractitionerRole | RelatedPerson | Patient | Device | Organization)	Who participated
        OnBehalfOf, //onBehalfOf		0..1	Reference(Practitioner | PractitionerRole | RelatedPerson | Patient | Device | Organization)	Who the agent is representing
    }
}