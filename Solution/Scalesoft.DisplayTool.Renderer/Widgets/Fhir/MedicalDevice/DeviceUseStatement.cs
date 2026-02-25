using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.MedicalDevice;

public class DeviceUseStatement(bool skipIdPopulation = true)
    : AlternatingBackgroundColumnResourceBase<DeviceUseStatement>, IResourceWidget
{
    public static string ResourceType => "DeviceUseStatement";

    public static bool HasBorderedContainer(Widget widget) => false;

    public DeviceUseStatement() : this(true)
    {
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties.Evaluate<DeviceUseInfrequentProperties>(navigator);

        var nameValuePairs = new FlexList([
            infrequentProperties.Optional(DeviceUseInfrequentProperties.Device,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        Direction = FlexDirection.Column,
                        Style = NameValuePair.NameValuePairStyle.Primary,
                        LabelOverride = new LocalizedLabel("device-use-statement.device"),
                    }
                )
            ),
            infrequentProperties.Condition(DeviceUseInfrequentProperties.Timing,
                new NameValuePair(
                    new LocalizedLabel("device-use-statement.timing"),
                    new OpenTypeElement(null, "timing"),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )), // Timing | Period | dateTime
            new If(_ => infrequentProperties.Contains(DeviceUseInfrequentProperties.BasedOn),
                new HideableDetails(
                    new NameValuePair(
                        new LocalizedLabel("device-use-statement.basedOn"),
                        new CommaSeparatedBuilder("f:basedOn", _ => [new AnyReferenceNamingWidget()]),
                        style: NameValuePair.NameValuePairStyle.Primary,
                        direction: FlexDirection.Column
                    ))
            ),
            new If(_ => infrequentProperties.Contains(DeviceUseInfrequentProperties.DerivedFrom),
                new HideableDetails(
                    new NameValuePair(
                        new LocalizedLabel("device-use-statement.derivedFrom"),
                        new CommaSeparatedBuilder("f:derivedFrom", _ => [new AnyReferenceNamingWidget()]),
                        style: NameValuePair.NameValuePairStyle.Primary,
                        direction: FlexDirection.Column
                    ))
            ),
            infrequentProperties.Optional(DeviceUseInfrequentProperties.BodySite,
                new NameValuePair(
                    new EhdsiDisplayLabel(LabelCodes.BodySite),
                    new CodeableConcept(),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )),
            new If(_ => infrequentProperties.ContainsAnyOf(
                    DeviceUseInfrequentProperties.ReasonCode,
                    DeviceUseInfrequentProperties.ReasonReference
                ),
                new NameValuePair(
                    [new LocalizedLabel("device-use-statement.reason")],
                    [
                        new CommaSeparatedBuilder("f:reasonCode", _ => [new CodeableConcept()]),
                        new Condition("f:reasonCode and f:reasonReference", new ConstantText(", ")),
                        new CommaSeparatedBuilder("f:reasonReference", _ => [new AnyReferenceNamingWidget()]),
                    ],
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )
            ),
        ], FlexDirection.Row, flexContainerClasses: "column-gap-6 row-gap-1");

        ResourceSummaryModel? summary = null;
        var deviceNav = ReferenceHandler.GetSingleNodeNavigatorFromReference(navigator, "f:device", ".");
        if (deviceNav != null)
        {
            summary = ReferenceHandler.GetResourceSummary(deviceNav);
        }

        var title = summary?.Value ?? new LocalizedLabel("device-use-statement");
        var resultWidget = new Concat([
            new Row([
                    new Heading([
                        new Container([
                            title,
                            infrequentProperties.Condition(DeviceUseInfrequentProperties.Timing,
                                new Container([new ConstantText("  ")], ContainerType.Span, optionalClass: "pre"),
                                new TextContainer(TextStyle.Light, new Chronometry("timing"))),
                            new EnumIconTooltip("f:status", "http://hl7.org/fhir/device-statement-status",
                                new EhdsiDisplayLabel(LabelCodes.Status))
                        ], optionalClass: "blue-color d-flex align-items-center"),
                    ], HeadingSize.H5, customClass: "m-0"),
                    new NarrativeModal(alignRight: false),
                ], flexContainerClasses: "gap-1 align-items-center", flexWrap: false,
                idSource: skipIdPopulation ? null : new IdentifierSource(navigator)),
            new FlexList([
                nameValuePairs,
                ThematicBreak.SurroundedThematicBreak(
                    infrequentProperties, [
                        DeviceUseInfrequentProperties.Device,
                        DeviceUseInfrequentProperties.Timing,
                        DeviceUseInfrequentProperties.BasedOn,
                        DeviceUseInfrequentProperties.DerivedFrom,
                        DeviceUseInfrequentProperties.BodySite,
                        DeviceUseInfrequentProperties.ReasonCode,
                        DeviceUseInfrequentProperties.ReasonReference,
                    ], [
                        DeviceUseInfrequentProperties.Note,
                        DeviceUseInfrequentProperties.Text,
                    ]
                ),
                new Condition("f:note",
                    new NameValuePair(
                        new LocalizedLabel("device-use-statement.notes"),
                        new CommaSeparatedBuilder("f:note", _ => [new ShowAnnotationCompact()]),
                        style: NameValuePair.NameValuePairStyle.Secondary
                    )
                ),
                new Condition("f:text", new NarrativeCollapser()),
            ], FlexDirection.Column, flexContainerClasses: "px-2 gap-1"),
        ]);

        return resultWidget.Render(navigator, renderer, context);
    }

    public enum DeviceUseInfrequentProperties
    {
        [HiddenInSimpleMode] BasedOn,
        ReasonCode,
        ReasonReference,
        BodySite,
        [HiddenInSimpleMode] DerivedFrom,
        [NarrativeDisplayType] Text,
        Note,
        Device,
        [OpenType("timing")] Timing,
    }
}