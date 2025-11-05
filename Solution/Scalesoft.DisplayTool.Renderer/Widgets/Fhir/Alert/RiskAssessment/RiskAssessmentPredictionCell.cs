using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Alert.RiskAssessment;

public class RiskAssessmentPredictionCell(XmlDocumentNavigator item) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var predictionNav = item.SelectSingleNode("f:prediction");
        var infrequentOptions = InfrequentProperties.Evaluate<InfrequentPropertiesPaths>([predictionNav]);

        var actorsTableCell = new TableCell(
        [
            new ChangeContext(predictionNav,
                new Container([
                    infrequentOptions.Contains(InfrequentPropertiesPaths.Outcome)
                        ? new NameValuePair([new ConstantText("Důsledek")],
                        [
                            new Optional("f:outcome", new CodeableConcept())
                        ])
                        : new NullWidget(),
                    infrequentOptions.Contains(InfrequentPropertiesPaths.Probability)
                        ? new NameValuePair([new ConstantText("Pravděpodobnost")],
                        [
                            new OpenTypeElement(null, "probability") // decimal | Range
                        ])
                        : new NullWidget(),
                    infrequentOptions.Contains(InfrequentPropertiesPaths.QualitativeRisk)
                        ? new NameValuePair([new ConstantText("Kvalitativní riziko")],
                        [
                            new Optional("f:qualitativeRisk", new CodeableConcept())
                        ])
                        : new NullWidget(),
                    infrequentOptions.Contains(InfrequentPropertiesPaths.RelativeRisk)
                        ? new NameValuePair([new ConstantText("Relativní riziko")],
                        [
                            new Optional("f:relativeRisk", new ShowDecimal())
                        ])
                        : new NullWidget(),
                    infrequentOptions.Contains(InfrequentPropertiesPaths.When)
                        ? new NameValuePair([new ConstantText("Období")],
                        [
                            new OpenTypeElement(null, "when") // Period | Range
                        ])
                        : new NullWidget(),
                    infrequentOptions.Contains(InfrequentPropertiesPaths.Rationale)
                        ? new NameValuePair([new ConstantText("Vysvětlení")],
                        [
                            new Optional("f:rationale", new Text("@value"))
                        ], direction: FlexDirection.Column)
                        : new NullWidget()
                ], optionalClass: "name-value-pair-wrapper")
            )
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
        /*Prediction 0..*	BackboneElement*/
        Outcome, //0..1	CodeableConcept	- Possible outcome for the subject
        [OpenType("probability")] Probability,
        QualitativeRisk, //	0..1	CodeableConcept	- Likelihood of specified outcome as a qualitative value
        RelativeRisk, //0..1	decimal	- Relative likelihood
        [OpenType("when")] When, //0..1 Timeframe or age range
        Rationale, //0..1	string	- Explanation of prediction
    }
}