using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;
using RenderMode = Scalesoft.DisplayTool.Renderer.Models.Enums.RenderMode;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class LocalNodeName(string? resourceType = null, bool isPlural = false) : Widget
{
    private string? m_resourceType = resourceType;

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        if (context.RenderMode == RenderMode.Documentation)
        {
            return Task.FromResult<RenderResult>(navigator.GetFullPath());
        }

        m_resourceType ??= navigator.Node?.LocalName;
        Widget nodeName = m_resourceType switch
        {
            "AllergyIntolerance" => new LocalizedLabel("node-names.AllergyIntolerance"),
            "Appointment" => isPlural
                ? new LocalizedLabel("node-names.Appointment-pl")
                : new LocalizedLabel("node-names.Appointment-sgl"),
            "Attachment" => isPlural
                ? new LocalizedLabel("node-names.Attachment-pl")
                : new LocalizedLabel("node-names.Attachment-sgl"),
            "Binary" => isPlural
                ? new LocalizedLabel("node-names.Binary-pl")
                : new LocalizedLabel("node-names.Binary-sgl"),
            "BodyStructure" => new EhdsiDisplayLabel(LabelCodes.BodySite),
            "CarePlan" => isPlural
                ? new LocalizedLabel("node-names.CarePlan-pl")
                : new LocalizedLabel("node-names.CarePlan-sgl"),
            "CareTeam" => isPlural
                ? new LocalizedLabel("node-names.CareTeam-pl")
                : new LocalizedLabel("node-names.CareTeam-sgl"),
            "Communication" => new LocalizedLabel("node-names.Communication"),
            "CommunicationRequest" => isPlural
                ? new LocalizedLabel("node-names.CommunicationRequest-pl")
                : new LocalizedLabel("node-names.CommunicationRequest-sgl"),
            "Condition" => isPlural
                ? new LocalizedLabel("node-names.Condition-pl")
                : new LocalizedLabel("node-names.Condition-sgl"),
            "Consent" => isPlural
                ? new LocalizedLabel("node-names.Consent-pl")
                : new LocalizedLabel("node-names.Consent-sgl"),
            "Contract" => isPlural
                ? new LocalizedLabel("node-names.Contract-pl")
                : new LocalizedLabel("node-names.Contract-sgl"),
            "Coverage" => isPlural
                ? new LocalizedLabel("node-names.Coverage-pl")
                : new LocalizedLabel("node-names.Coverage-sgl"),
            "DetectedIssue" => isPlural
                ? new LocalizedLabel("node-names.DetectedIssue-pl")
                : new LocalizedLabel("node-names.DetectedIssue-sgl"),
            "Device" => new LocalizedLabel("node-names.Device"),
            "DeviceRequest" => isPlural
                ? new LocalizedLabel("node-names.DeviceRequest-pl")
                : new LocalizedLabel("node-names.DeviceRequest-sgl"),
            "DeviceUseStatement" => new LocalizedLabel("node-names.DeviceUseStatement"),
            "DocumentReference" => isPlural
                ? new LocalizedLabel("node-names.DocumentReference-pl")
                : new LocalizedLabel("node-names.DocumentReference-sgl"),
            "DiagnosticReport" => isPlural
                ? new LocalizedLabel("node-names.DiagnosticReport-pl")
                : new LocalizedLabel("node-names.DiagnosticReport-sgl"),
            "Encounter" => new LocalizedLabel("node-names.Encounter"),
            "EpisodeOfCare" => isPlural
                ? new LocalizedLabel("node-names.EpisodeOfCare-pl")
                : new LocalizedLabel("node-names.EpisodeOfCare-sgl"),
            "Evidence" => isPlural
                ? new LocalizedLabel("node-names.Evidence-pl")
                : new LocalizedLabel("node-names.Evidence-sgl"),
            "EvidenceVariable" => isPlural
                ? new LocalizedLabel("node-names.EvidenceVariable-pl")
                : new LocalizedLabel("node-names.EvidenceVariable-sgl"),
            "FamilyMemberHistory" => new LocalizedLabel("node-names.FamilyMemberHistory"),
            "Flag" => new LocalizedLabel("node-names.Flag"),
            "Goal" => isPlural ? new LocalizedLabel("node-names.Goal-pl") : new LocalizedLabel("node-names.Goal-sgl"),
            "HealthcareService" => isPlural
                ? new LocalizedLabel("node-names.HealthcareService-pl")
                : new LocalizedLabel("node-names.HealthcareService-sgl"),
            "ImagingStudy" => isPlural
                ? new LocalizedLabel("node-names.ImagingStudy-pl")
                : new LocalizedLabel("node-names.ImagingStudy-sgl"),
            "Immunization" => new LocalizedLabel("node-names.Immunization"),
            "ImmunizationRecommendation" => new LocalizedLabel("node-names.ImmunizationRecommendation"),
            "ImmunizationEvaluation" => new LocalizedLabel("node-names.ImmunizationEvaluation"),
            "Media" => new LocalizedLabel("node-names.Media"),
            "Medication" => new LocalizedLabel("node-names.Medication"),
            "MedicationAdministration" => new LocalizedLabel("node-names.MedicationAdministration"),
            "MedicationDispense" => new LocalizedLabel("node-names.MedicationDispense"),
            "MedicationRequest" => isPlural
                ? new LocalizedLabel("node-names.MedicationRequest-pl")
                : new LocalizedLabel("node-names.MedicationRequest-sgl"),
            "MedicationStatement" => isPlural
                ? new LocalizedLabel("node-names.MedicationStatement-pl")
                : new LocalizedLabel("node-names.MedicationStatement-sgl"),
            "NutritionOrder" => isPlural
                ? new LocalizedLabel("node-names.NutritionOrder-pl")
                : new LocalizedLabel("node-names.NutritionOrder-sgl"),
            "Observation" => isPlural
                ? new LocalizedLabel("node-names.Observation-pl")
                : new LocalizedLabel("node-names.Observation-sgl"),
            "Organization" => new LocalizedLabel("node-names.Organization"),
            "List" => isPlural ? new LocalizedLabel("node-names.List-pl") : new LocalizedLabel("node-names.List-sgl"),
            "Location" => new LocalizedLabel("node-names.Location"),
            "Patient" => isPlural
                ? new LocalizedLabel("node-names.Patient-pl")
                : new LocalizedLabel("node-names.Patient-sgl"),
            "Practitioner" => isPlural
                ? new LocalizedLabel("node-names.Practitioner-pl")
                : new LocalizedLabel("node-names.Practitioner-sgl"),
            "PractitionerRole" => isPlural
                ? new LocalizedLabel("node-names.PractitionerRole-pl")
                : new LocalizedLabel("node-names.PractitionerRole-sgl"),
            "Procedure" => isPlural
                ? new LocalizedLabel("node-names.Procedure-pl")
                : new LocalizedLabel("node-names.Procedure-sgl"),
            "Provenance" => new LocalizedLabel("node-names.Provenance"),
            "QuestionnaireResponse" => isPlural
                ? new LocalizedLabel("node-names.QuestionnaireResponse-pl")
                : new LocalizedLabel("node-names.QuestionnaireResponse-sgl"),
            "RelatedPerson" => isPlural
                ? new LocalizedLabel("node-names.RelatedPerson-pl")
                : new LocalizedLabel("node-names.RelatedPerson-sgl"),
            "RequestGroup" => isPlural
                ? new LocalizedLabel("node-names.RequestGroup-pl")
                : new LocalizedLabel("node-names.RequestGroup-sgl"),
            "RiskAssessment" => new LocalizedLabel("node-names.RiskAssessment"),
            "ServiceRequest" => isPlural
                ? new LocalizedLabel("node-names.ServiceRequest-pl")
                : new LocalizedLabel("node-names.ServiceRequest-sgl"),
            "Substance" => isPlural
                ? new LocalizedLabel("node-names.Substance-pl")
                : new LocalizedLabel("node-names.Substance-sgl"),
            "Specimen" => isPlural
                ? new LocalizedLabel("node-names.Specimen-pl")
                : new LocalizedLabel("node-names.Specimen-sgl"),
            "Task" => isPlural ? new LocalizedLabel("node-names.Task-pl") : new LocalizedLabel("node-names.Task-sgl"),
            "VisionPrescription" => isPlural
                ? new LocalizedLabel("node-names.VisionPrescription-pl")
                : new LocalizedLabel("node-names.VisionPrescription-sgl"),
            "ClinicalImpression" => isPlural
                ? new LocalizedLabel("node-names.ClinicalImpression-pl")
                : new LocalizedLabel("node-names.ClinicalImpression-sgl"),
            "Group" => isPlural
                ? new LocalizedLabel("node-names.Group-pl")
                : new LocalizedLabel("node-names.Group-sgl"),
            "MessageHeader" => isPlural
                ? new LocalizedLabel("node-names.MessageHeader-pl")
                : new LocalizedLabel("node-names.MessageHeader-sgl"),
            "Composition" => isPlural
                ? new LocalizedLabel("node-names.Composition-pl")
                : new LocalizedLabel("node-names.Composition-sgl"),
            _ => isPlural
                ? new LocalizedLabel("node-names.unsupported-pl")
                : new LocalizedLabel("node-names.unsupported-sgl"),
        };

        return nodeName.Render(navigator, renderer, context);
    }
}