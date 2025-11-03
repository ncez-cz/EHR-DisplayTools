using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ShowTiming(string path = ".", FlexDirection nameValuePairDirection = FlexDirection.Row) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        if (IsDataAbsent(navigator, path))
        {
            return new AbsentData(path).Render(navigator, renderer, context);
        }

        if (context.RenderMode == RenderMode.Documentation)
        {
            return Task.FromResult<RenderResult>(navigator.SelectSingleNode(path).GetFullPath());
        }

        var tree = new ChangeContext(path,
            new Concat([
                new Condition("f:event",
                    new NameValuePair([new ConstantText("Datum")],
                        [new CommaSeparatedBuilder("f:event", _ => [new ShowDateTime()]),],
                        direction: nameValuePairDirection)
                ),

                new Condition("f:repeat/f:*[starts-with(name(), 'bounds')]",
                    new ChangeContext("f:repeat", new NameValuePair(new Choose([
                        new When("f:boundsDuration", new ConstantText("Délka trvání")),
                        new When("f:boundsRange", new ConstantText("Rozsah")),
                        new When("f:boundsPeriod", new ConstantText("Doba")),
                    ]), new OpenTypeElement(null, "bounds"), direction: nameValuePairDirection))
                ),

                new Condition("f:repeat/f:frequency | f:repeat/f:period | f:repeat/f:periodUnit",
                    new NameValuePair(
                        [
                            new ConstantText("Četnost")
                        ],
                        [
                            new Optional("f:repeat/f:frequency",
                                new Text("@value"),
                                new ConstantText(" "),
                                new DisplayLabel(LabelCodes.Times)
                            ),
                            new Optional("f:repeat/f:period",
                                new ConstantText(" "),
                                new DisplayLabel(LabelCodes.Every),
                                new ConstantText(" "),
                                new Text("@value"),
                                new ConstantText(" ")
                            ),
                            new Optional("f:repeat/f:periodUnit",
                                new EnumLabel("@value", "http://hl7.org/fhir/ValueSet/units-of-time")
                            )
                        ], direction: nameValuePairDirection
                    )
                ),

                new Condition("f:repeat/f:timeOfDay",
                    new NameValuePair([new ConstantText("Časy")],
                        [new CommaSeparatedBuilder("f:repeat/f:timeOfDay", _ => [new Text("@value")])],
                        direction: nameValuePairDirection)
                ),

                new Condition("f:repeat/f:dayOfWeek",
                    new NameValuePair([new ConstantText("Dny v týdnu")],
                        [new CommaSeparatedBuilder("f:repeat/f:dayOfWeek", _ => [new Text("@value")])],
                        direction: nameValuePairDirection)
                ),

                // TODO implement count, countMax, duration, durationUnit, frequencyMax, periodMax, offset, code #DT-267

                new Condition("f:repeat/f:when",
                    new NameValuePair([new ConstantText("Denní doby")],
                        [new CommaSeparatedBuilder("f:repeat/f:when", _ => [new Text("@value")])],
                        direction: nameValuePairDirection)
                ),
            ])
        );

        return tree.Render(navigator, renderer, context);
    }
}