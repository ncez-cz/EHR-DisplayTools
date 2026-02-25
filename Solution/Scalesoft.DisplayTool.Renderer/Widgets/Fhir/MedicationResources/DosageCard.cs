using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.MedicationResources;

public class DosageCard(string path = "f:dosage", bool bodyOnly = false) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var dosageBody = new ConcatBuilder(path, (_, _, nav, nextNav) =>
            [
                new FlexList([
                    new Optional("f:text",
                        new NameValuePair(
                            [new LocalizedLabel("dosage.dosage-instruction")],
                            [
                                new Text("@value"),
                                new LineBreak(),
                                new ChangeContext(nav,
                                    new Condition("f:asNeededCodeableConcept | f:asNeededBoolean",
                                        new Optional("f:asNeededCodeableConcept",
                                            new LocalizedLabel("dosage.asNeededCodeableConcept"),
                                            new CodeableConcept()),
                                        new Optional("f:asNeededBoolean",
                                            children:
                                            [
                                                new ShowBoolean(
                                                    onFalse: new LocalizedLabel("dosage.asNeededBoolean.false"),
                                                    onTrue: new LocalizedLabel("dosage.asNeededBoolean.true")
                                                ),
                                            ]
                                        )
                                    )
                                ),
                            ],
                            direction: FlexDirection.Column,
                            style: NameValuePair.NameValuePairStyle.Secondary
                        )
                    ),
                    new Optional("f:route",
                        new NameValuePair(
                            new EhdsiDisplayLabel(LabelCodes.AdministrationRoute),
                            new CodeableConcept(),
                            direction: FlexDirection.Column,
                            style: NameValuePair.NameValuePairStyle.Secondary
                        )
                    ),
                    new Optional("f:timing",
                        new FlexList([
                            new ShowTiming(nameValuePairDirection: FlexDirection.Column,
                                nameValuePairStyle: NameValuePair.NameValuePairStyle.Secondary)
                        ], FlexDirection.Row, flexContainerClasses: "column-gap-6")
                    ),
                    new Condition("f:doseAndRate",
                        new NameValuePair(
                            new LocalizedLabel("dosage.doseAndRate"),
                            new DoseAndRate(),
                            direction: FlexDirection.Column,
                            style: NameValuePair.NameValuePairStyle.Secondary
                        )
                    ),
                    new Optional("f:sequence",
                        new NameValuePair(
                            new LocalizedLabel("dosage.sequence"),
                            new Text("@value"),
                            direction: FlexDirection.Column,
                            style: NameValuePair.NameValuePairStyle.Secondary
                        )
                    ),
                    new Condition("f:additionalInstruction",
                        new NameValuePair(
                            new LocalizedLabel("dosage.additionalInstruction"),
                            new CommaSeparatedBuilder("f:additionalInstruction", _ => [new CodeableConcept()]),
                            direction: FlexDirection.Column,
                            style: NameValuePair.NameValuePairStyle.Secondary
                        )
                    ),
                ], FlexDirection.Row, flexContainerClasses: "column-gap-6 row-gap-1"),
                new If(
                    _ =>
                    {
                        const string conditionXpath =
                            "f:text or f:route or f:timing or f:doseAndRate or f:sequence or f:additionalInstruction";
                        return nextNav?.EvaluateCondition(conditionXpath) ==
                            true && nav.EvaluateCondition(conditionXpath);
                    },
                    new ThematicBreak()
                ),
            ]
        );

        Widget widget = bodyOnly
            ? dosageBody
            : new Card(
                new LocalizedLabel("dosage.dosage-information"),
                new Concat([
                    dosageBody,
                ])
            );

        return widget.Render(navigator, renderer, context);
    }
}