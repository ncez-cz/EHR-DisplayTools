using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Allergy;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.CareTeam;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Consent;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.FamilyMemberHistory;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Immunization;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.PlanOfCare;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.PatientSection;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
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

            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.Allergies),
                (x, _, _) => (new AllergiesAndIntolerances(x)),
                Severity.Warning,
                titleAbbreviations: SectionTitleAbbreviations.Allergies
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.AdvanceDirectives),
                (x, _, _) => new Consents(x),
                titleAbbreviations: SectionTitleAbbreviations.AdvanceDirectives
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.Medications),
                titleAbbreviations: SectionTitleAbbreviations.HistoryOfMedicationUse
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.Immunizations),
                (x, _, _) => new Immunizations(x),
                titleAbbreviations: SectionTitleAbbreviations.Immunizations
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.PastIllnessHx),
                (x, _, _) =>
                    new Conditions(x,
                        new DisplayLabel(LabelCodes.InactiveProblem)),
                _ => null,
                SectionTitleAbbreviations.HistoryOfPastIllness
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.Problems),
                (x, _, _) =>
                    new Conditions(x, new DisplayLabel(LabelCodes.ActiveProblem)),
                titleAbbreviations: SectionTitleAbbreviations.ActiveProblems
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.FunctionalStatus),
                titleAbbreviations: SectionTitleAbbreviations.FunctionalStatus
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.Alert),
                titleAbbreviations: SectionTitleAbbreviations.HealthConcerns
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.PlanOfCare),
                (x, _, _) => new FhirCarePlan(x),
                titleAbbreviations: SectionTitleAbbreviations.HealthMaintenanceCarePlan
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.PhysicalFindings),
                titleAbbreviations: SectionTitleAbbreviations.PhysicalFindings
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.SignificantResults),
                titleAbbreviations: SectionTitleAbbreviations.RelevantDiagnosticTests
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.HistoryOfMedicalDevices),
                titleAbbreviations: SectionTitleAbbreviations.HistoryOfMedicalDeviceUse
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.SignificantProcedures),
                (x, _, _) => new Procedures(x),
                titleAbbreviations: SectionTitleAbbreviations.HospitalDischargeProcedures
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.CareTeam),
                (x, _, _) => new CareTeams(x),
                titleAbbreviations: SectionTitleAbbreviations.PatientCareTeamInformation
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.AdmissionEvaluation),
                titleAbbreviations: SectionTitleAbbreviations.AdmissionEvaluation
            ), // entry is Resource
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.DischargeDetails),
                titleAbbreviations: SectionTitleAbbreviations.DischargeDetails
            ), // entry is Resource
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.FamilyHistory),
                (x, _, _) => new FamilyMembersHistory(x),
                titleAbbreviations: SectionTitleAbbreviations.HistoryOfFamilyMember
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.ProceduresHx),
                (x, _, _) => new Procedures(x),
                titleAbbreviations: SectionTitleAbbreviations.HistoryOfProcedures
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.DischargeMedications),
                titleAbbreviations: SectionTitleAbbreviations.DischargeMedication
            ),
            new FhirSection(
                // TODO?
                CodedValueDefinition.LoincValue(LoincSectionCodes.VitalSigns),
                (items, type, hasMultipleResourceTypes) => new AnyResource(
                    items,
                    type,
                    displayResourceType: hasMultipleResourceTypes
                ),
                titleAbbreviations: SectionTitleAbbreviations.VitalSigns
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.Synthesis),
                titleAbbreviations: SectionTitleAbbreviations.Synthesis
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.SocialHistory),
                titleAbbreviations: SectionTitleAbbreviations.SocialHistory
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.AdditionalDocuments),
                titleAbbreviations: SectionTitleAbbreviations.AdditionalDocuments),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.PaymentSources),
                (x, _, _) => 
                    new Coverages(x),
                titleAbbreviations: SectionTitleAbbreviations.PaymentSources
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.PhysicalExamByBodyAreas),
                titleAbbreviations: SectionTitleAbbreviations.PhysicalExamination
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.HospitalCourse),
                titleAbbreviations: SectionTitleAbbreviations.HospitalCourse
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.HospitalDischargeInstructions),
                titleAbbreviations: SectionTitleAbbreviations.HospitalDischargeInstructions
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.HospitalDischargePhysicalFindings),
                titleAbbreviations: SectionTitleAbbreviations.HospitalDischargePhysicalFindings
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.HistoryOfTravel),
                titleAbbreviations: SectionTitleAbbreviations.HistoryOfTravel
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.HistoryGeneral),
                titleAbbreviations: SectionTitleAbbreviations.GeneralHistory
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.Encounters),
                titleAbbreviations: SectionTitleAbbreviations.Encounters
            ),
            new FhirFooter(),
        ];
        return widgets.RenderConcatenatedResult(navigator, renderer, context);
    }
}