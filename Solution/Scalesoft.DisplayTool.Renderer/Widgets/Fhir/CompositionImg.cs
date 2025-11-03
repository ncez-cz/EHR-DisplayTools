using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.PatientSection;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class CompositionImg : Widget
{
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

            new ConcatBuilder("f:section", _ =>
            [
                new Choose([
                    new When(
                        $"f:code/f:coding[f:system/@value='http://loinc.org' and f:code/@value='{LoincSectionCodes.RadiologyStudies}']",
                        new HideableDetails(new FhirSection())),
                    new When(
                        $"f:code/f:coding[f:system/@value='http://loinc.org' and f:code/@value='{LoincSectionCodes.RequestedImagingStudies}']",
                        new HideableDetails(new FhirSection(null,
                            async (items, type, types) =>
                                await ImagingStudiesSectionBuilder(items, navigator, renderer, context),
                            _ => null,
                            null))),
                ], new FhirSection()),
            ]),

            new FhirFooter(),
        ];
        return await widgets.RenderConcatenatedResult(navigator, renderer, context);
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