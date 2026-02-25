using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Allergy;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.FunctionalStatus.ClinicallImpression;

public class ClinicalImpressionRow(
    XmlDocumentNavigator item,
    InfrequentPropertiesData<ClinicalImpressionInfrequentProperties> infrequentProperties,
    List<XmlDocumentNavigator>? allExistingProblems = null
) : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<XmlDocumentNavigator> existingAllergies = [];
        List<XmlDocumentNavigator> existingConditions = [];

        if (allExistingProblems != null)
        {
            foreach (var existingProblem in allExistingProblems)
            {
                if (existingProblem?.Node?.Name?.Contains("Condition") == true)
                {
                    existingConditions.Add(existingProblem);
                }
                else if (existingProblem != null)
                {
                    existingAllergies.Add(existingProblem);
                }
            }
        }

        var tableRefs = new StructuredDetails();

        var conditionTableRef = existingConditions.Count > 0
            ? new Conditions(existingConditions, new LocalizedLabel("general.related-condition"))
            : null;
        var allergyTableRef = existingAllergies.Count > 0
            ? new AllergiesAndIntolerances(existingAllergies)
            : null;

        if (conditionTableRef != null)
        {
            tableRefs.Add(
                new CollapsibleDetail(
                    new LocalizedLabel("general.related-conditions-detail"),
                    conditionTableRef
                )
            );
        }

        if (allergyTableRef != null)
        {
            tableRefs.Add(
                new CollapsibleDetail(
                    new LocalizedLabel("general.related-allergies-detail"),
                    allergyTableRef
                )
            );
        }

        if (navigator.EvaluateCondition("f:encounter"))
        {
            var encounterNarrative = ReferenceHandler.GetSingleNodeNavigatorFromReference(navigator,
                "f:encounter", "f:text");

            tableRefs.Add(
                new CollapsibleDetail(
                    new LocalizedLabel("node-names.Encounter"),
                    ShowSingleReference.WithDefaultDisplayHandler(nav => [new EncounterCard(nav, false, false)],
                        "f:encounter"),
                    encounterNarrative != null
                        ?
                        [
                            new NarrativeCollapser(encounterNarrative.GetFullPath())
                        ]
                        : null,
                    encounterNarrative != null
                        ? new NarrativeModal(encounterNarrative.GetFullPath())
                        : null
                )
            );
        }

        if (navigator.EvaluateCondition("f:text"))
        {
            tableRefs.Add(
                new CollapsibleDetail(
                    new EhdsiDisplayLabel(LabelCodes.OriginalNarrative),
                    new Narrative("f:text")
                )
            );
        }

        Widget tree = new TableRow([
            new If(_ => infrequentProperties.Contains(ClinicalImpressionInfrequentProperties.Code),
                new TableCell([new CommaSeparatedBuilder("f:code", _ => [new CodeableConcept()])])
            ),
            new If(_ => infrequentProperties.Contains(ClinicalImpressionInfrequentProperties.Effective),
                new TableCell([new Chronometry("effective")])
            ),
            new If(_ => infrequentProperties.Contains(ClinicalImpressionInfrequentProperties.Date),
                new TableCell([new ShowDateTime("f:date")])
            ),
            new If(_ => infrequentProperties.Contains(ClinicalImpressionInfrequentProperties.Summary),
                new TableCell([new Optional("f:summary", new Text("@value"))])
            ),
            new If(_ => infrequentProperties.Contains(ClinicalImpressionInfrequentProperties.Problem),
                new TableCell([
                    new ConcatBuilder("f:problem", _ =>
                    [
                        new AnyReferenceNamingWidget()
                    ], new LineBreak())
                ])
            ),
            new If(_ => infrequentProperties.Contains(ClinicalImpressionInfrequentProperties.Status),
                new TableCell([
                    new EnumIconTooltip("f:status", "http://hl7.org/fhir/event-status",
                        new EhdsiDisplayLabel(LabelCodes.Status))
                ])
            ),
            new If(_ => infrequentProperties.Contains(ClinicalImpressionInfrequentProperties.Text),
                new NarrativeCell()
            ),
        ], tableRefs, idSource: item);

        return await tree.Render(navigator, renderer, context);
    }
}