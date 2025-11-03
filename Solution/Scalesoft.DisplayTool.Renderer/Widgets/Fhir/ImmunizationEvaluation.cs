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

    public override Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer, RenderContext context)
    {
        var cardContent = new List<Widget>
        {
            // id must be rendered by the parent class
            // ignore identifier
            new NameValuePair([new ConstantText("Stav")], [
                new Choose(
                [
                    new When("f:status/@value='completed'", new ConstantText("Dokončeno")),
                    new When("f:status/@value='entered-in-error'", new ConstantText("Zadáno omylem")),
                ]),
            ]),
            // ignore patient
            new Optional("f:date", new NameValuePair([new DisplayLabel(LabelCodes.Date)], [new ShowDateTime()])),
            new Optional("f:authority",
                new NameValuePair([new ConstantText("Zodpovědná organizace")],
                [
                    ShowSingleReference.WithDefaultDisplayHandler(nav => [new Container([new PersonOrOrganization(nav)], idSource: nav)])
                ])),
            new NameValuePair([new ConstantText("Očkování proti")], [new ChangeContext("f:targetDisease", new CodeableConcept())]),
            new Collapser([new ConstantText("Vyhodnocované očkování")], [],
                [ShowSingleReference.WithDefaultDisplayHandler(nav => [new Immunizations([nav])], "f:immunizationEvent")]),
            new NameValuePair([new ConstantText("Stav dávky")], [new ChangeContext("f:doseStatus", new CodeableConcept())]),
            new Optional("f:doseStatusReason", new NameValuePair([new ConstantText("Důvod stavu dávky")], [
                new ItemListBuilder(".", ItemListType.Unordered, _ =>
                [
                    new CodeableConcept(),
                ]),
            ])),
            new Optional("f:description", new NameValuePair([new ConstantText("Popis")], [new Text("@value")])),
            new Optional("f:series", new NameValuePair([new ConstantText("Název očkovací řady")], [new Text("@value")])),
            new Condition("f:doseNumberPositiveInt | f:doseNumberString",
                new NameValuePair([new DisplayLabel(LabelCodes.DoseNumber)],
                    [new OpenTypeElement(null, "doseNumber")])), // positiveInt | string
            new Condition("f:seriesDosesPositiveInt | f:seriesDosesString",
                new NameValuePair([new ConstantText("Celkový počet dávek")],
                    [new OpenTypeElement(null, "seriesDoses")])), // positiveInt | string
        };

        var widget = new Card(null, new Container(cardContent));

        return widget.Render(navigator, renderer, context);
    }
}