using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Observation;

public class PrimitiveType(string? prefix = "value", string? suffix = "") : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var nav = navigator.SelectSingleNode($"f:{prefix}{suffix}");

        var widget = new Choose(
        [
            new When($"f:{prefix}Uri", new Text($"f:{prefix}Uri/@value")),
            new When($"f:{prefix}Url", new Text($"f:{prefix}Uri/@value")),
            new When($"f:{prefix}String", new Text($"f:{prefix}String/@value")),
            new When($"f:{prefix}Boolean",
                new ShowBoolean(new EhdsiDisplayLabel(LabelCodes.No), new EhdsiDisplayLabel(LabelCodes.Yes), $"f:{prefix}Boolean")),
            new When($"f:{prefix}Decimal", new ShowDecimal($"f:{prefix}Decimal")),
            new When($"f:{prefix}Integer", new Text($"f:{prefix}Integer/@value")),
            new When($"f:{prefix}Integer64", new Text($"f:{prefix}Integer64/@value")),
            new When($"f:{prefix}Time", new ShowTime($"f:{prefix}Time")),
            new When($"f:{prefix}Date", new ShowDateTime($"f:{prefix}Date")),
            new When($"f:{prefix}DateTime", new ShowDateTime($"f:{prefix}DateTime")),
            new When($"f:{prefix}Canonical", new Text($"f:{prefix}Canonical/@value")),
            new When($"f:{prefix}Id", new Text($"f:{prefix}Id/@value")),
            new When($"f:{prefix}Instant", new ShowInstant($"f:{prefix}Instant")),
            new When($"f:{prefix}Markdown", new Markdown($"f:{prefix}Markdown/@value")),
            new When($"f:{prefix}Oid", new Text($"f:{prefix}Oid/@value")),
            new When($"f:{prefix}PositiveInt", new Text($"f:{prefix}PositiveInt/@value")),
            new When($"f:{prefix}UnsignedInt", new Text($"f:{prefix}UnsignedInt/@value")),
            new When($"f:{prefix}Uuid", new Text($"f:{prefix}Uuid/@value")),
        ], new TextContainer(TextStyle.Regular, [
            new LocalizedLabel("errors.unsupported-coded-content"),
            new ConstantText(": "),
            new TextContainer(TextStyle.Small,
            [
                new LineBreak(),
                new ConstantText(nav.Node?.OuterXml ?? string.Empty),
            ]),
            new LineBreak()
        ]));

        return widget.Render(navigator, renderer, context);
    }
}