using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class EvidenceVariable : ColumnResourceBase<EvidenceVariable>, IResourceWidget
{
    public static string ResourceType => "EvidenceVariable";

    public static bool HasBorderedContainer(Widget widget) => true;

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var variableBadge = new PlainBadge(new LocalizedLabel("evidence-variable.variable-details"));
        var variableInfo = new Container([
            new Optional("f:type",
                new NameValuePair(
                    new LocalizedLabel("evidence-variable.type"),
                    new EnumLabel(".", "http://hl7.org/fhir/ValueSet/variable-type")
                )
            ),

            new ListBuilder("f:characteristic", FlexDirection.Row, (_, nav) =>
            {
                var infrequentProperties =
                    Widgets.InfrequentProperties.Evaluate<InfrequentProperties>(nav);

                var characteristicInfo = new Card(new LocalizedLabel("evidence-variable.characteristic"), new Container(
                [
                    new If(_ => infrequentProperties.Contains(InfrequentProperties.Description),
                        new NameValuePair(
                            new EhdsiDisplayLabel(LabelCodes.Description),
                            new Text("f:description/@value")
                        )
                    ),
                    new NameValuePair(
                        new LocalizedLabel("evidence-variable.characteristic.definice"),
                        new OpenTypeElement(null,
                            "definition") // Reference(Group) | canonical(ActivityDefinition) | CodeableConcept | Expression | DataRequirement | TriggerDefinition
                    ),
                    new If(_ => infrequentProperties.Contains(InfrequentProperties.Exclude),
                        new NameValuePair(
                            new LocalizedLabel("evidence-variable.characteristic.exclude"),
                            new ShowBoolean(new LocalizedLabel("general.no"),
                                new LocalizedLabel("general.yes"), "f:exclude")
                        )
                    ),
                    new If(_ => infrequentProperties.Contains(InfrequentProperties.ParticipantEffective),
                        new NameValuePair(
                            new LocalizedLabel("evidence-variable.characteristic.participantEffective"),
                            new Chronometry("participantEffective")
                        )
                    ),
                    new If(_ => infrequentProperties.Contains(InfrequentProperties.TimeFromStart),
                        new NameValuePair(
                            new LocalizedLabel("evidence-variable.characteristic.timeFromStart"),
                            new ShowDuration("f:timeFromStart")
                        )
                    ),
                    new If(_ => infrequentProperties.Contains(InfrequentProperties.GroupMeasure),
                        new NameValuePair(
                            new LocalizedLabel("evidence-variable.characteristic.groupMeasure"),
                            new EnumLabel("f:groupMeasure", "http://hl7.org/fhir/ValueSet/group-measure")
                        )
                    ),
                ], optionalClass: "name-value-pair-wrapper"));

                return [characteristicInfo];
            }, flexContainerClasses: "gap-2"),
        ]);


        var evidenceVariable = new Evidence(new LocalizedLabel("evidence-variable"), [
            new ThematicBreak(),
            variableBadge,
            variableInfo,
        ]);

        return evidenceVariable.Render(navigator, renderer, context);
    }

    private enum InfrequentProperties
    {
        Description,
        Exclude,
        [OpenType("participantEffective")] ParticipantEffective,
        TimeFromStart,
        GroupMeasure,
    }
}