using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Observation;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.PatientSection;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Sections;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class CompositionHdr : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<Widget> widgets =
        [
            new FhirHeader(),

            new NarrativeCollapser(),

            ShowSingleReference.WithDefaultDisplayHandler(x => [new Patient(x)], "f:subject"),

            ShowSingleReference.WithDefaultDisplayHandler(
                x => [
                    new EncounterSection(x, new LocalizedLabel("hdr.composition-encounter-section-title")),
                ], "f:encounter"
            ),
            
            new ConcatBuilder("f:section", (_, _, nav) =>
            {
                if (!nav.EvaluateCondition("f:code[f:coding/f:system/@value='http://loinc.org']"))
                {
                    return
                    [
                        new FhirSection(),
                    ];
                }

                var sectionCodeValue = nav.SelectSingleNode("f:code/f:coding/f:code/@value").Node?.Value;

                Severity? severity = null;
                LocalizedAbbreviations? titleAbbreviations = sectionCodeValue == null
                    ? null
                    : HdrSectionAbbreviationProvider.GetAbbreviation(sectionCodeValue);
                SectionContentBuilder? codedSectionBuilder = null;

                switch (sectionCodeValue)
                {
                    case LoincSectionCodes.Allergies:
                        severity = Severity.Warning;
                        break;
                    case LoincSectionCodes.PastIllnessHx:
                        codedSectionBuilder = (x, _, _) =>
                            new Conditions(x,
                                new EhdsiDisplayLabel(LabelCodes.InactiveProblem));
                        break;
                    case LoincSectionCodes.Problems:
                        codedSectionBuilder = (x, _, _) =>
                            new Conditions(x,
                                new EhdsiDisplayLabel(LabelCodes.ActiveProblem));
                        break;
                    case LoincSectionCodes.VitalSigns:
                        codedSectionBuilder = (x, _, _) =>
                            new VitalSigns(x);
                        break;
                }

                return
                [
                    new FhirSection(null, codedSectionBuilder, severity, titleAbbreviations),
                ];
            }),

            new FhirFooter(),
        ];
        return widgets.RenderConcatenatedResult(navigator, renderer, context);
    }
}