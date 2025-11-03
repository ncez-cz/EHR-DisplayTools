using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.PatientSection;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class CompositionImagingOrder : Widget
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
                CodedValueDefinition.LoincValue(LoincSectionCodes.RequestedImagingStudies),
                async (items, type, types) =>
                    await ImagingStudiesSectionBuilder(items, navigator, renderer, context),
                _ => null,
                SectionTitleAbbreviations.RequestedImagingStudies
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.RadiologyReason),
                titleAbbreviations: SectionTitleAbbreviations.RadiologyReason
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.Coverage),
                titleAbbreviations: SectionTitleAbbreviations.Coverage
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.Appointment),
                titleAbbreviations: SectionTitleAbbreviations.Appointment
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.PlanOfCare),
                titleAbbreviations: SectionTitleAbbreviations.HealthMaintenanceCarePlan
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.ImplantComponent),
                titleAbbreviations: SectionTitleAbbreviations.ImplantComponent
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.AdditionalDocuments),
                titleAbbreviations: SectionTitleAbbreviations.AdditionalDocuments
            ),
            new FhirSection(
                CodedValueDefinition.LoincValue(LoincSectionCodes.ClinicalInformation),
                titleAbbreviations: SectionTitleAbbreviations.SupportingInformation
            ),
            new FhirFooter(),
        ];
        return widgets.RenderConcatenatedResult(navigator, renderer, context);
    }

    private async Task<SectionBuildResult> ImagingStudiesSectionBuilder(
        List<XmlDocumentNavigator> items,
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var requestsWithTitle = await MergedServiceRequests.AppendDate(items, navigator, renderer, context);

        return new SectionBuildResult(requestsWithTitle.serviceRequest, requestsWithTitle.title);
    }
}