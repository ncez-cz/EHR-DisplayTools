using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ShowTiming(
    string path = ".",
    FlexDirection nameValuePairDirection = FlexDirection.Row,
    NameValuePair.NameValuePairStyle nameValuePairStyle = NameValuePair.NameValuePairStyle.Secondary
) : Widget
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
                    new NameValuePair([new LocalizedLabel("timing.event")],
                        [new CommaSeparatedBuilder("f:event", _ => [new ShowDateTime()]),],
                        direction: nameValuePairDirection, style: nameValuePairStyle)
                ),

                new Condition("f:repeat/f:*[starts-with(name(), 'bounds')]",
                    new ChangeContext("f:repeat", new NameValuePair(new Choose([
                        new When("f:boundsDuration", new LocalizedLabel("timing.boundsDuration")),
                        new When("f:boundsRange", new LocalizedLabel("timing.boundsRange")),
                        new When("f:boundsPeriod", new LocalizedLabel("timing.boundsPeriod")),
                    ]), new OpenTypeElement(null, "bounds"), direction: nameValuePairDirection, style: nameValuePairStyle))
                ),

                new Condition("f:repeat/f:frequency | f:repeat/f:period | f:repeat/f:periodUnit",
                    new NameValuePair(
                        [
                            new LocalizedLabel("timing.repeat")
                        ],
                        [
                            new Optional("f:repeat/f:frequency",
                                new Text("@value"),
                                new ConstantText(" "),
                                new EhdsiDisplayLabel(LabelCodes.Times)
                            ),
                            new Optional("f:repeat/f:period",
                                new ConstantText(" "),
                                new EhdsiDisplayLabel(LabelCodes.Every),
                                new ConstantText(" "),
                                new Text("@value"),
                                new ConstantText(" ")
                            ),
                            new Optional("f:repeat/f:periodUnit",
                                new EnumLabel("@value", "http://hl7.org/fhir/ValueSet/units-of-time")
                            )
                        ], direction: nameValuePairDirection, style: nameValuePairStyle
                    )
                ),

                new Condition("f:repeat/f:timeOfDay",
                    new NameValuePair([new LocalizedLabel("timing.repeat.timeOfDay")],
                        [new CommaSeparatedBuilder("f:repeat/f:timeOfDay", _ => [new Text("@value")])],
                        direction: nameValuePairDirection, style: nameValuePairStyle)
                ),

                new Condition("f:repeat/f:dayOfWeek",
                    new NameValuePair([new LocalizedLabel("timing.repeat.daysOfWeek")],
                        [new CommaSeparatedBuilder("f:repeat/f:dayOfWeek", _ => [new Text("@value")])],
                        direction: nameValuePairDirection, style: nameValuePairStyle)
                ),

                // TODO implement count, countMax, duration, durationUnit, frequencyMax, periodMax, offset, code #DT-267

                new Condition("f:repeat/f:when",
                    new NameValuePair([new LocalizedLabel("timing.repeat.when")],
                        [new CommaSeparatedBuilder("f:repeat/f:when", _ => [new Text("@value")])],
                        direction: nameValuePairDirection, style: nameValuePairStyle)
                ),
            ])
        );

        return tree.Render(navigator, renderer, context);
    }
}