using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

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
        m_resourceType ??= navigator.Node?.LocalName;
        Widget nodeName = m_resourceType switch
        {
            "AllergyIntolerance" => new ConstantText("Alergie a intolerance"),
            "Appointment" => new ConstantText(
                isPlural ? "Schůzky / objednání k lékaři" : "Schůzka / objednání k lékaři"),
            "Attachment" => new ConstantText(isPlural ? "Přílohy" : "Příloha"),
            "Binary" => new ConstantText(isPlural ? "Přílohy bez názvu" : "Příloha bez názvu"),
            "BodyStructure" => new DisplayLabel(LabelCodes.BodySite),
            "CarePlan" => new ConstantText(isPlural ? "Plány péče" : "Plan péče"),
            "CareTeam" => new ConstantText(isPlural ? "Pečovatelské týmy" : "Pečovatelský tým"),
            "CommunicationRequest" => new ConstantText("Žádost o komunikaci"),
            "Condition" => new ConstantText(isPlural ? "Potíže / Události" : "Potíž / Událost"),
            "Consent" => new ConstantText(isPlural ? "Souhlasy" : "Souhlas"),
            "Contract" => new ConstantText(isPlural ? "Smlouvy" : "Smlouva"),
            "Coverage" => new ConstantText(isPlural ? "Úhrady / Pokrytí" : "Úhrada / Pokrytí"),
            "DetectedIssue" => new ConstantText(isPlural ? "Zjištěné problémy" : "Zjištěný problém"),
            "Device" => new ConstantText("Zařízení"),
            "DeviceRequest" =>
                new ConstantText(isPlural ? "Žádosti o přístroj/zařízení" : "Žádost o přístroj/zařízení"),
            "DeviceUseStatement" => new ConstantText("Záznam o použití zařízení"),
            "DocumentReference" => new ConstantText(isPlural ? "Dokumenty" : "Dokument"),
            "DiagnosticReport" => new ConstantText(isPlural ? "Diagnostické zprávy" : "Diagnostická zpráva"),
            "Encounter" => new ConstantText(Labels.Encounter),
            "EpisodeOfCare" => new ConstantText(isPlural ? "Epizody péče" : "Epizoda péče"),
            "Evidence" => new ConstantText(isPlural ? "Důkazy" : "Důkaz"),
            "EvidenceVariable" => new ConstantText(isPlural ? "Proměnné důkazu" : "Proměnná důkazu"),
            "FamilyMemberHistory" => new ConstantText("Historie rodiny"),
            "Flag" => new ConstantText("Upozornění"),
            "Goal" => new ConstantText(isPlural ? "Cíle" : "Cíl"),
            "HealthcareService" => new ConstantText(isPlural ? "Zdravotnické služby" : "Zdravotnická služba"),
            "ImagingStudy" => new ConstantText(isPlural ? "Obrazové studie" : "Obrazová studie"),
            "Immunization" => new ConstantText("Očkování"),
            "ImmunizationRecommendation" => new ConstantText("Doporučení k očkování"),
            "ImmunizationEvaluation" => new ConstantText("Vyhodnocení očkování"),
            "Media" => new ConstantText("Média"),
            "Medication" => new ConstantText("Léky"),
            "MedicationAdministration" => new ConstantText("Podání léků"),
            "MedicationDispense" => new ConstantText("Výdej léků"),
            "MedicationRequest" => new RawText(isPlural ? "Žádosti o léky" : "Žádost o léky"),
            "MedicationStatement" => new ConstantText(isPlural ? "Záznamy o lécích" : "Záznam o lécích"),
            "NutritionOrder" => new ConstantText(isPlural ? "Výživové doporučení" : "Výživová doporučení"),
            "Observation" => new ConstantText("Výsledky pozorování"),
            "Organization" => new ConstantText("Organizace"),
            "List" => new ConstantText(isPlural ? "Seznamy" : "Seznam"),
            "Location" => new ConstantText("Lokace"),
            "Patient" => new ConstantText(isPlural ? "Pacitenti" : "Pacient"),
            "Practitioner" => new ConstantText(isPlural ? "Lékaři" : "Lékař"),
            "PractitionerRole" => new ConstantText(isPlural ? "Lékaři" : "Lékař"),
            "Procedure" => new ConstantText(isPlural ? "Procedury" : "Procedura"),
            "Provenance" => new ConstantText("Provenance"),
            "QuestionnaireResponse" => new ConstantText(isPlural ? "Odpovědi na dotazníky" : "Odpověď na dotazník"),
            "RelatedPerson" => new ConstantText(isPlural ? "Související osoby" : "Související osoba"),
            "RequestGroup" => new ConstantText(isPlural ? "Skupiny požadavků" : "Skupina požadavků"),
            "RiskAssessment" => new ConstantText("Hodnocení rizik"),
            "ServiceRequest" => new ConstantText(isPlural ? "Žádosti o službu" : "Žádost o službu"),
            "Substance" => new ConstantText(isPlural ? "Látky" : "Látka"),
            "Specimen" => new ConstantText(isPlural ? "Vzorky" : "Vzorek"),
            "Task" => new ConstantText(isPlural ? "Úkoly" : "Úkol"),
            "VisionPrescription" => new ConstantText(isPlural
                ? "Recepty na brýle / kontaktní čočky"
                : "Recept na brýle / kontaktní čočky"),
            "ClinicalImpression" => new ConstantText("Klinické hodnocení"),
            "Group" => new ConstantText(isPlural ? "Skupiny" : "Skupina"),
            _ => new ConstantText(isPlural ? "Nepodporované záznamy" : "Nepodporovaný záznam"),
        };

        return nodeName.Render(navigator, renderer, context);
    }
}