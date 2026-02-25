using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Consent;

public class ConsentTableRow(
    XmlDocumentNavigator navigator,
    InfrequentPropertiesData<Consents.ConsentInfrequentProperties> infrequentProperties
) : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator _,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var collapsibleRow = new StructuredDetails();

        if (navigator.EvaluateCondition("f:policy"))
        {
            collapsibleRow.Add(new CollapsibleDetail(new LocalizedLabel("consent.policy-detail"), new PolicyDetailsTable()));
        }

        if (navigator.EvaluateCondition("f:provision"))
        {
            collapsibleRow.Add(new CollapsibleDetail(new LocalizedLabel("consent.provision"), new Provision.Provision(), IsHideable: false));
        }

        if (navigator.EvaluateCondition("f:text"))
        {
            collapsibleRow.Add(new CollapsibleDetail(new EhdsiDisplayLabel(LabelCodes.OriginalNarrative), new Narrative("f:text")));
        }

        var row =
            new TableRow([
                new TableCell([
                    new CommaSeparatedBuilder("f:category", _ => [new CodeableConcept()])
                ]),
                new TableCell([new ChangeContext("f:scope", new CodeableConcept())]),
                new If(
                    _ => infrequentProperties.ContainsAnyOf(Consents.ConsentInfrequentProperties.Policy,
                        Consents.ConsentInfrequentProperties.PolicyRule),
                    new TableCell([
                        new Choose([
                            new When("f:policyRule",
                                new ChangeContext("f:policyRule", new CodeableConcept())
                            ),
                            new When("f:policy", new LocalizedLabel("general.see-detail"))
                        ])
                    ])
                ),
                new If(_ => infrequentProperties.Contains(Consents.ConsentInfrequentProperties.Status),
                    new TableCell([
                        new EnumIconTooltip("f:status", "http://hl7.org/fhir/consent-state-codes",
                            new EhdsiDisplayLabel(LabelCodes.Status))
                    ])
                ),
                new If(_ => infrequentProperties.Contains(Consents.ConsentInfrequentProperties.Text),
                    new NarrativeCell()
                ),
            ], collapsibleRow, idSource: navigator);
        return await row.Render(navigator, renderer, context);
    }
}