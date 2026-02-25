using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Allergy;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Consent;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Immunization;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.PatientSection;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.PlanOfCare;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Pregnancy;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Sections;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class CompositionIps : Widget
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
            new ShowSingleReference(x =>
            {
                if (x.ResourceReferencePresent)
                {
                    return [new Patient(x.Navigator)];
                }

                return [new ConstantText(x.ReferenceDisplay)];
            }, "f:subject"),

            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.Allergies),
                (x, _, _) => new AllergiesAndIntolerances(x),
                titleAbbreviations: SectionTitleAbbreviations.Allergies,
                severity: Severity.Secondary
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.Problems),
                (x, _, _) =>
                    new Conditions(x,
                        new EhdsiDisplayLabel(LabelCodes.ActiveProblem)),
                titleAbbreviations: SectionTitleAbbreviations.ActiveProblems
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.HistoryOfMedicalDevices),
                titleAbbreviations: SectionTitleAbbreviations.HistoryOfMedicalDeviceUse
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.ProceduresHx),
                titleAbbreviations: SectionTitleAbbreviations.HistoryOfProcedures
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.Medications),
                titleAbbreviations: SectionTitleAbbreviations.HistoryOfMedicationUse
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.FunctionalStatus),
                titleAbbreviations: SectionTitleAbbreviations.FunctionalStatus
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.VitalSigns),
                (items, type, hasMultipleResourceTypes) => new AnyResource(
                    items,
                    type,
                    displayResourceType: hasMultipleResourceTypes
                ),
                titleAbbreviations: SectionTitleAbbreviations.VitalSigns
            ),
            new ParentSection(
                new LocalizedLabel("custom-sections.personal-history"),
                titleAbbreviations: SectionTitleAbbreviations.PersonalHistory,
                [
                    new FhirSection(
                        CodedValueDefinition.LoincValue(LoincSectionCodes.PastIllnessHx),
                        (x, _, _) =>
                            new Conditions(x,
                                new EhdsiDisplayLabel(LabelCodes.InactiveProblem)),
                        titleAbbreviations: SectionTitleAbbreviations.HistoryOfPastIllness,
                        severity: Severity.Secondary
                    ),
                    new FhirSection(
                        CodedValueDefinition.LoincValue(LoincSectionCodes.Immunizations),
                        (x, _, _) =>
                            new Immunizations(x),
                        titleAbbreviations: SectionTitleAbbreviations.Immunizations
                    ),
                ]
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.SocialHistory),
                titleAbbreviations: SectionTitleAbbreviations.SocialHistory
            ),
            new ParentSection(
                new LocalizedLabel("custom-sections.gynecological-history"),
                titleAbbreviations: SectionTitleAbbreviations.GynecologicalHistory,
                [
                    new FhirSection(
                        CodedValueDefinition.LoincValue(LoincSectionCodes.PregnancyHx),
                        (x, _, _) => new FhirIpsPregnancy(x),
                        titleAbbreviations: SectionTitleAbbreviations.HistoryOfPregnancies
                    ),
                ]
            ),
            new ParentSection(
                new LocalizedLabel("custom-sections.patient-provided-data"),
                titleAbbreviations: SectionTitleAbbreviations.PatientProvidedData,
                [
                    new FhirSection(
                        CodedValueDefinition.LoincValue(LoincSectionCodes.AdvanceDirectives),
                        (x, _, _) => new Consents(x),
                        titleAbbreviations: SectionTitleAbbreviations.AdvanceDirectives
                    ),
                ]
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.SignificantResults),
                titleAbbreviations: SectionTitleAbbreviations.RelevantDiagnosticTests
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.PlanOfCare),
                (x, _, _) => new FhirCarePlan(x),
                titleAbbreviations: SectionTitleAbbreviations.HealthMaintenanceCarePlan
            ),
            new FhirFooter(),
        ];
        return widgets.RenderConcatenatedResult(navigator, renderer, context);
    }
}