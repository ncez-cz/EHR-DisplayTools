using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Immunization;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Person;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ImmunizationEvaluation : SequentialResourceBase<ImmunizationEvaluation>, IResourceWidget
{
    public static string ResourceType => "ImmunizationEvaluation";
    public static bool HasBorderedContainer(Widget widget) => true;

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var cardContent = new List<Widget>
        {
            new Container([
                // id must be rendered by the parent class
                // ignore identifier
                new NameValuePair([new LocalizedLabel("immunization-evaluation.status")], [
                    new Choose(
                    [
                        new When("f:status/@value='completed'",
                            new LocalizedLabel("immunization-evaluation.status.completed")),
                        new When("f:status/@value='entered-in-error'",
                            new LocalizedLabel("immunization-evaluation.status.entered-in-error")),
                    ]),
                ]),
                new Optional("f:date",
                    new NameValuePair([new EhdsiDisplayLabel(LabelCodes.Date)], [new ShowDateTime()])),
                new Optional("f:authority",
                    new NameValuePair([new LocalizedLabel("immunization-evaluation.authority")],
                    [
                        ShowSingleReference.WithDefaultDisplayHandler(nav =>
                            [new Container([new PersonOrOrganization(nav)], idSource: nav)]),
                    ])),
                new NameValuePair([new LocalizedLabel("immunization-evaluation.targetDisease")],
                    [new ChangeContext("f:targetDisease", new CodeableConcept())]),
            ], optionalClass: "name-value-pair-wrapper w-fit-content"),
            // ignore patient
            new Collapser([new LocalizedLabel("immunization-evaluation.immunizationEvent")],
            [
                ShowSingleReference.WithDefaultDisplayHandler(nav => [new Immunizations([nav])], "f:immunizationEvent")
            ]),
            new Container([
                new NameValuePair([new LocalizedLabel("immunization-evaluation.doseStatus")],
                    [new ChangeContext("f:doseStatus", new CodeableConcept())]),
                new Optional("f:doseStatusReason", new NameValuePair(
                    [new LocalizedLabel("immunization-evaluation.doseStatusReason")], [
                        new ItemListBuilder(".", ItemListType.Unordered, _ =>
                        [
                            new CodeableConcept(),
                        ]),
                    ])),
                new Optional("f:description",
                    new NameValuePair([new LocalizedLabel("immunization-evaluation.description")],
                        [new Text("@value")])),
                new Optional("f:series",
                    new NameValuePair([new LocalizedLabel("immunization-evaluation.series")], [new Text("@value")])),
                new Condition("f:doseNumberPositiveInt | f:doseNumberString",
                    new NameValuePair([new EhdsiDisplayLabel(LabelCodes.DoseNumber)],
                        [new OpenTypeElement(null, "doseNumber")])), // positiveInt | string
                new Condition("f:seriesDosesPositiveInt | f:seriesDosesString",
                    new NameValuePair([new LocalizedLabel("immunization-evaluation.seriesDoses")],
                        [new OpenTypeElement(null, "seriesDoses")])), // positiveInt | string
            ], optionalClass: "name-value-pair-wrapper w-fit-content"),
        };

        var widget = new Card(null, new Container(cardContent));

        return widget.Render(navigator, renderer, context);
    }
}