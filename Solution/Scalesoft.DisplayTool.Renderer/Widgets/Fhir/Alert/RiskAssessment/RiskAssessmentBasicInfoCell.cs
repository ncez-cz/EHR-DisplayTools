using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Alert.RiskAssessment;

public class RiskAssessmentBasicInfoCell(XmlDocumentNavigator item) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentOptions = InfrequentProperties.Evaluate<InfrequentPropertiesPaths>([item]);

        var actorsTableCell = new TableCell([
            infrequentOptions.Contains(InfrequentPropertiesPaths.Condition)
                ? new NameValuePair([new ConstantText("Posuzovaný zdravotní stav")],
                [
                    new Optional("f:condition", new AnyReferenceNamingWidget())
                ])
                : new NullWidget(),
            infrequentOptions.Contains(InfrequentPropertiesPaths.Code)
                ? new NameValuePair([new ConstantText("Popis")],
                [
                    new Optional("f:code", new CodeableConcept())
                ])
                : new NullWidget(),
            infrequentOptions.Contains(InfrequentPropertiesPaths.Method)
                ? new NameValuePair([new ConstantText("Metoda posouzení")],
                [
                    new Optional("f:method", new CodeableConcept())
                ])
                : new NullWidget(),
            infrequentOptions.Contains(InfrequentPropertiesPaths.ReasonCode)
                ? new NameValuePair([new ConstantText("Důvody")],
                [
                    new ItemListBuilder("f:reasonCode", ItemListType.Unordered, _ => [new CodeableConcept()])
                ], direction: FlexDirection.Column)
                : new NullWidget(),
            infrequentOptions.Contains(InfrequentPropertiesPaths.ReasonReference)
                ? new NameValuePair([new ConstantText("Odkazy na důvod")],
                [
                    new ItemListBuilder("f:reasonReference", ItemListType.Unordered,
                        _ => [new AnyReferenceNamingWidget()])
                ])
                : new NullWidget(),
        ]);

        if (infrequentOptions.Count == 0)
        {
            actorsTableCell = new TableCell([
                new TextContainer(TextStyle.Muted, [new ConstantText("Informace nejsou k dispozici")])
            ]);
        }

        return actorsTableCell.Render(item, renderer, context);
    }

    private enum InfrequentPropertiesPaths
    {
        Condition, //0..1	Reference(Condition) - Condition assessed
        Code, //0..1	CodeableConcept - description
        ReasonCode, //0..*	CodeableConcept
        ReasonReference, //Reference(Condition | Observation | DiagnosticReport | DocumentReference)
        Method, //0..1	CodeableConcept - evaluation mechanism
    }
}