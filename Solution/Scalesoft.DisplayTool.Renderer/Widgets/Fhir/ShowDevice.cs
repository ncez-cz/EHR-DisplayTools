using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ShowDevice(bool skipIdPopulation = true)
    : AlternatingBackgroundColumnResourceBase<ShowDevice>, IResourceWidget
{
    public static string ResourceType => "Device";

    public static bool HasBorderedContainer(Widget widget) => false;

    public ShowDevice() : this(true)
    {
    }

    public static ResourceSummaryModel? RenderSummary(XmlDocumentNavigator navigator)
    {
        Widget? label = null;
        if (navigator.EvaluateCondition("f:type"))
        {
            label = new ChangeContext(navigator, "f:type", new CodeableConcept());
        }

        if (navigator.EvaluateCondition("f:deviceName"))
        {
            return new ResourceSummaryModel
            {
                Label = label,
                Value = new ChangeContext(navigator,
                    new CommaSeparatedBuilder("f:deviceName/f:name/@value", _ => [new Text()])),
            };
        }

        var display = string.Empty;
        if (navigator.EvaluateCondition("f:manufacturer"))
        {
            if (display != string.Empty)
            {
                display += " ";
            }

            display += navigator.SelectSingleNode("f:manufacturer/@value").Node?.Value;
        }

        if (navigator.EvaluateCondition("f:modelNumber"))
        {
            if (display != string.Empty)
            {
                display += " ";
            }

            display += navigator.SelectSingleNode("f:modelNumber/@value").Node?.Value;
        }

        if (navigator.EvaluateCondition("f:serialNumber"))
        {
            var serial = navigator.SelectSingleNode("f:serialNumber/@value").Node?.Value;

            if (!string.IsNullOrEmpty(display))
            {
                display += $" ({serial})";
            }
            else
            {
                display += serial;
            }
        }

        if (!string.IsNullOrWhiteSpace(display))
        {
            return new ResourceSummaryModel
            {
                Label = label,
                Value = new ConstantText(display),
            };
        }

        if (label != null)
        {
            return new ResourceSummaryModel
            {
                Value = label,
            };
        }

        return null;
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties.Evaluate<DeviceInfrequentPropertiesPaths>(navigator);

        var nameValuePairs = new FlexList([
            new If(_ => infrequentProperties.Contains(DeviceInfrequentPropertiesPaths.UdiCarrier),
                new ConcatBuilder("f:udiCarrier", _ =>
                [
                    new NameValuePair(
                        [new LocalizedLabel("device.udiCarrier")],
                        [
                            new Choose(
                            [ // HRF includes the device identifier (UDI-DI), so if full UDI is present, UDI-DI is not displayed separately
                                new When("f:carrierHRF",
                                    new NameValuePair(new LocalizedLabel("device.udiCarrier.carrierHRF"),
                                        new Text("f:carrierHRF/@value"),
                                        style: NameValuePair.NameValuePairStyle.Secondary,
                                        direction: FlexDirection.Row)),
                            ], new Optional("f:deviceIdentifier",
                                new NameValuePair(new LocalizedLabel("device.udiCarrier.deviceIdentifier"),
                                    new Text("@value"), style: NameValuePair.NameValuePairStyle.Secondary,
                                    direction: FlexDirection.Row))),
                            new Optional("f:issuer",
                                new NameValuePair(new LocalizedLabel("device.udiCarrier.issuer"), new Text("@value"),
                                    style: NameValuePair.NameValuePairStyle.Secondary,
                                    direction: FlexDirection.Row)),
                            new Optional("f:jurisdiction",
                                new NameValuePair(new LocalizedLabel("device.udiCarrier.jurisdiction"),
                                    new Text("@value"),
                                    style: NameValuePair.NameValuePairStyle.Secondary,
                                    direction: FlexDirection.Row)),
                            new Optional("f:entryType",
                                new NameValuePair(new LocalizedLabel("device.udiCarrier.entryType"),
                                    new EnumLabel("@value", "http://hl7.org/fhir/udi-entry-type"),
                                    style: NameValuePair.NameValuePairStyle.Secondary,
                                    direction: FlexDirection.Row)),
                        ],
                        style: NameValuePair.NameValuePairStyle.Primary,
                        direction: FlexDirection.Column
                    )
                ])
            ),
            new If(
                _ => infrequentProperties.Contains(
                    DeviceInfrequentPropertiesPaths.DistinctIdentifier
                ),
                new HideableDetails(
                    new NameValuePair(
                        [new EhdsiDisplayLabel(LabelCodes.DeviceId)],
                        [
                            new Text("f:distinctIdentifier/@value"),
                        ],
                        style: NameValuePair.NameValuePairStyle.Primary,
                        direction: FlexDirection.Column
                    ))
            ),
            new If(
                _ => infrequentProperties.ContainsAnyOf(
                    DeviceInfrequentPropertiesPaths.SerialNumber),
                new HideableDetails(
                    new NameValuePair(
                        [new LocalizedLabel("device.serialNumber")],
                        [
                            new Text("f:serialNumber/@value"),
                        ],
                        style: NameValuePair.NameValuePairStyle.Primary,
                        direction: FlexDirection.Column
                    ))
            ),
            new If(
                _ => infrequentProperties.Contains(DeviceInfrequentPropertiesPaths.Type),
                new NameValuePair(
                    [new EhdsiDisplayLabel(LabelCodes.DeviceName)], [
                        new CommaSeparatedBuilder("f:type", _ => [new CodeableConcept()]),
                    ],
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )
            ),
            infrequentProperties.Optional(DeviceInfrequentPropertiesPaths.Manufacturer,
                new NameValuePair(
                    [new LocalizedLabel("device.manufacturer")], [new Text("@value")],
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )
            ),
            new If(
                _ => infrequentProperties.Contains(DeviceInfrequentPropertiesPaths.DeviceName),
                new NameValuePair([new LocalizedLabel("device.deviceName")],
                    [
                        new CommaSeparatedBuilder("f:deviceName", _ => [new Text("f:name/@value")]),
                    ],
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )
            ),
            infrequentProperties.Optional(DeviceInfrequentPropertiesPaths.ModelNumber,
                new NameValuePair(
                    [new LocalizedLabel("device.modelNumber")], [new Text("@value")],
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )
            ),
            new If(
                _ => infrequentProperties.ContainsAnyOf(DeviceInfrequentPropertiesPaths.Specialization),
                new NameValuePair(
                    [new LocalizedLabel("device.specialization")], [
                        new CommaSeparatedBuilder("f:specialization",
                            _ => [new Optional("f:systemType", new CodeableConcept())]),
                    ],
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )
            ),
            infrequentProperties.Optional(DeviceInfrequentPropertiesPaths.ExpirationDate,
                new NameValuePair(
                    [new LocalizedLabel("device.expirationDate")],
                    [new ShowDateTime(),],
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )
            ),
            new If(
                _ => infrequentProperties.ContainsAnyOf(DeviceInfrequentPropertiesPaths.Version),
                new ConcatBuilder("f:version", _ =>
                [
                    new NameValuePair([new LocalizedLabel("device.version")], [
                            new Optional("f:type", new CodeableConcept(), new ConstantText(" - ")),
                            new Optional("f:component", new ShowIdentifier(), new ConstantText(", ")),
                            new Optional("f:value", new Text("@value"))
                        ],
                        style: NameValuePair.NameValuePairStyle.Primary,
                        direction: FlexDirection.Column
                    ),
                ])
            ),
        ], FlexDirection.Row, flexContainerClasses: "column-gap-6 row-gap-1");

        var summary = RenderSummary(navigator);
        var title = summary?.Value ?? summary?.Label ?? new LocalizedLabel("device");

        var resultWidget = new Concat([
            new Row([
                    new Heading([
                        new Container([
                            title,
                            new EnumIconTooltip("f:status", "http://hl7.org/fhir/device-status",
                                new EhdsiDisplayLabel(LabelCodes.Status))
                        ], optionalClass: "blue-color d-flex align-items-center"),
                    ], HeadingSize.H5, customClass: "m-0"),
                    new NarrativeModal(alignRight: false),
                ], flexContainerClasses: "gap-1 align-items-center",
                idSource: skipIdPopulation ? null : new IdentifierSource(navigator)),
            new FlexList([
                nameValuePairs,
                new Condition("f:note", new NameValuePair([new LocalizedLabel("device.note")],
                [
                    new CommaSeparatedBuilder("f:note",
                        _ => [new ShowAnnotationCompact()]),
                ], style: NameValuePair.NameValuePairStyle.Secondary)),
                new Condition("f:text", new NarrativeCollapser()),
            ], FlexDirection.Column, flexContainerClasses: "px-2 gap-1"),
        ]);

        return resultWidget.Render(navigator, renderer, context);
    }

    private enum DeviceInfrequentPropertiesPaths
    {
        UdiCarrier,
        DistinctIdentifier,
        SerialNumber,
        Type,
        Manufacturer,
        DeviceName,
        ModelNumber,
        Specialization,
        ExpirationDate,

        [EnumValueSet("http://hl7.org/fhir/device-status")]
        Status,
        Note,
        Version,
        Text,
    }
}