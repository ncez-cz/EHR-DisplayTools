using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Person;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class Medication(bool displayAsCard = true) : SequentialResourceBase<Medication>, IResourceWidget
{
    public static string ResourceType => "Medication";
    [UsedImplicitly] public static bool RequiresExternalTitle => true;

    public Medication() : this(true)
    {
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var cardContent = new List<Widget>();
        // id must be rendered by the parent class
        // implicitRules just contains a URL to a set of rules, and has little value to the end user 
        // if (navigator.EvaluateCondition("f:implicitRules"))
        // {
        //     cardContent.Add(new NameValuePair([new ConstantText("Dodatečná pravidla")], [new Text("f:implicitRules/@value")]));
        // }
        // ignore language
        // ignore extension
        // ignore identifier

        if (navigator.EvaluateCondition("f:text"))
        {
            cardContent.Add(new NarrativeModal());
        }

        if (navigator.EvaluateCondition("f:status"))
        {
            cardContent.Add(new HideableDetails(new EnumIconTooltip("f:status",
                "http://hl7.org/fhir/CodeSystem/medication-status",
                new DisplayLabel(LabelCodes.Status))));
        }

        if (navigator.EvaluateCondition("f:code"))
        {
            cardContent.Add(new NameValuePair([new ConstantText("Kód léku")],
                [new ChangeContext("f:code", new CodeableConcept())]));
        }

        if (navigator.EvaluateCondition("f:manufacturer"))
        {
            cardContent.Add(new HideableDetails(new NameValuePair([new ConstantText("Výrobce")],
            [
                ShowSingleReference.WithDefaultDisplayHandler(
                    x => [new Card(null, new PersonOrOrganization(x), idSource: x)],
                    "f:manufacturer")
            ])));
        }

        if (navigator.EvaluateCondition("f:form"))
        {
            cardContent.Add(new NameValuePair([new ConstantText("Forma")],
                [new ChangeContext("f:form", new CodeableConcept())]));
        }

        if (navigator.EvaluateCondition("f:amount"))
        {
            cardContent.Add(new HideableDetails(new NameValuePair([new ConstantText("Množství")],
                [new ChangeContext("f:amount", new ShowRatio())])));
        }

        if (navigator.EvaluateCondition("f:ingredient"))
        {
            cardContent.Add(new NameValuePair([new ConstantText("Látky")], [
                new CommaSeparatedBuilder("f:ingredient", (_, _, nav) =>
                [
                    new Container([
                        // ignore extension
                        new Optional("f:itemCodeableConcept", new CodeableConcept()),
                        new Optional("f:itemReference",
                            ShowSingleReference.WithDefaultDisplayHandler(x =>
                            [
                                new AnyResource([x], x.Node?.Name, displayResourceType: false)
                            ])), // Substance | Medication
                    ], ContainerType.Span, idSource: nav)
                ])
            ]));
        }

        if (navigator.EvaluateCondition("f:batch"))
        {
            cardContent.Add(new NameValuePair([new ConstantText("Šarže")], [
                new Container([
                    // ignore extension
                    new Condition("f:batch/f:lotNumber",
                        new NameValuePair([new ConstantText("Číslo šarže")], [new Text("f:batch/f:lotNumber/@value")])),
                    new Condition("f:batch/f:expirationDate",
                        new NameValuePair([new ConstantText("Datum expirace")],
                            [new ChangeContext("f:batch/f:expirationDate", new ShowDateTime())])),
                ])
            ]));
        }

        if (navigator.EvaluateCondition("f:text"))
        {
            cardContent.Add(
                new NarrativeCollapser()
            );
        }

        if (cardContent.Count >= 2 && navigator.EvaluateCondition("f:text"))
        {
            cardContent[0] = new Row([cardContent[1], cardContent[0]], flexContainerClasses: "align-items-center");
            cardContent.RemoveAt(1);
        }


        var widget = new If(_ => displayAsCard,
            new Card(null, new Container(cardContent)
            )).Else(
            new Container(cardContent)
        );

        return widget.Render(navigator, renderer, context);
    }
}