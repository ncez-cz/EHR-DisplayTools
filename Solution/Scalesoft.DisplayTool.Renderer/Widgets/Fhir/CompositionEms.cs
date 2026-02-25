using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Observation;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.PatientSection;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Sections;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class CompositionEms : Widget
{
    private readonly SectionBuilder
        m_vitalSignsSection = (sectionNav, _, _) =>
        {
            return new ChangeContext(sectionNav,
                new MultiReference(navigators =>
                {
                    var vitalSigns = navigators.Where(x => x.EvaluateCondition(VitalSigns.XPathCondition)).ToList();
                    var vitalSignsSection = new Section(
                        ".",
                        null,
                        [new LocalizedLabel("ems.section-vital-signs-title")],
                        [new VitalSigns(vitalSigns)]
                    );
                    
                    var otherFindings = navigators.Where(x => !x.EvaluateCondition(VitalSigns.XPathCondition)).ToList();
                    var otherFindingsSection = new Section(
                        ".",
                        null,
                        [new LocalizedLabel("ems.section-other-findings-title")],
                        [..otherFindings.Select(x => new ChangeContext(
                                x,
                                new ObservationCard()
                            )
                        )]
                    );

                    return new Concat([vitalSignsSection, otherFindingsSection]);
                }));
;        };
    
    public override async Task<RenderResult> Render(
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
            
            new DynamicSectionBuilder(
                defaultType: PredefinedSectionType.AnyResource,
                overrides: new List<SectionDefinition>
                {
                    new(null, SectionType.Custom, customBuilder: (_, _, _) =>
                        ShowSingleReference.WithDefaultDisplayHandler(
                            x => [
                                new EncounterSection(x, new LocalizedLabel("ems.composition-encounter-section-title")),
                            ], "f:encounter"
                        )
                    ),
                    new(null, SectionType.Custom, customBuilder: (_, _, _) => new EmsMissionDetailsSection()),
                    new(LoincSectionCodes.Allergies, SectionType.AnyResource, Severity.Warning),
                    new(LoincSectionCodes.PhysicalFindings, SectionType.Custom, customBuilder: m_vitalSignsSection),
                    new(LoincSectionCodes.Mission, SectionType.Ignore),
                    new(LoincSectionCodes.Timeline, SectionType.Ignore),
                }
            ),

            new FhirFooter(),
        ];
        return await widgets.RenderConcatenatedResult(navigator, renderer, context);
    }
}