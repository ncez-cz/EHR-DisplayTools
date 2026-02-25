using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.MedicationResources.MedicationAdministrationSection;

public class MedicationAdministrationDosageCard : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var dosageNavigator = navigator.SelectSingleNode("f:dosage");

        var dosageInfrequentOptions =
            InfrequentProperties.Evaluate<InfrequentDosagePropertiesPaths>(dosageNavigator);

        if (dosageInfrequentOptions.Count == 0)
        {
            return Task.FromResult(RenderResult.NullResult);
        }

        var widget = new Card(
            new LocalizedLabel("dosage.dosage-information"), new FlexList([
                new If(_ => dosageInfrequentOptions.Contains(InfrequentDosagePropertiesPaths.Route), new NameValuePair(
                    [new EhdsiDisplayLabel(LabelCodes.AdministrationRoute)],
                    [
                        new Optional("f:dosage/f:route", new CodeableConcept())
                    ], direction: FlexDirection.Column)),
                new If(_ => dosageInfrequentOptions.Contains(InfrequentDosagePropertiesPaths.Site), new NameValuePair(
                    [new EhdsiDisplayLabel(LabelCodes.BodySite)],
                    [
                        new Optional("f:dosage/f:site", new CodeableConcept())
                    ], direction: FlexDirection.Column)),
                new If(_ => dosageInfrequentOptions.Contains(InfrequentDosagePropertiesPaths.Method), new NameValuePair(
                    [new LocalizedLabel("dosage.method")],
                    [
                        new Optional("f:dosage/f:method", new CodeableConcept())
                    ], direction: FlexDirection.Column)),
                new If(_ => dosageInfrequentOptions.Contains(InfrequentDosagePropertiesPaths.Dose), new NameValuePair(
                    [new LocalizedLabel("dosage.dose")],
                    [
                        new ConstantText(" "), new ShowQuantity("f:dosage/f:dose")
                    ], direction: FlexDirection.Column)),
                new If(_ => dosageInfrequentOptions.Contains(InfrequentDosagePropertiesPaths.RateRatio),
                    new NameValuePair([new LocalizedLabel("dosage.rate")],
                    [
                        new ConstantText(" "), new ShowRatio("f:dosage/f:rateRatio")
                    ], direction: FlexDirection.Column)),
                new If(_ => dosageInfrequentOptions.Contains(InfrequentDosagePropertiesPaths.RateQuantity),
                    new NameValuePair([new LocalizedLabel("dosage.rate")],
                    [
                        new ConstantText(" "), new ShowQuantity("f:dosage/f:rateQuantity")
                    ], direction: FlexDirection.Column)),
                new If(_ => dosageInfrequentOptions.Contains(InfrequentDosagePropertiesPaths.Text), new NameValuePair(
                    [new LocalizedLabel("dosage.text")],
                    [
                        new Optional("f:dosage/f:text", new Text("@value"))
                    ], direction: FlexDirection.Column)),
            ], FlexDirection.Row, flexContainerClasses: "column-gap-6 row-gap-1", idSource: navigator),
            optionalClass: "m-0");

        return widget.Render(navigator, renderer, context);
    }

    public enum InfrequentDosagePropertiesPaths
    {
        Route,
        Text,
        Site,
        Method,
        Dose,
        RateRatio,
        RateQuantity,
    }
}