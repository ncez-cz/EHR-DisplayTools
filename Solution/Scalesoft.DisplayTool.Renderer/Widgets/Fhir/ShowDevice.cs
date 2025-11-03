using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Devices;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ShowDevice(List<XmlDocumentNavigator> items) : Widget, IResourceWidget
{
    public static string ResourceType => "Device";
    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return [new ShowDevice(items)];
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentOptions =
            InfrequentProperties.Evaluate<InfrequentPropertiesPaths>(items);

        var widgetTree = new List<Widget>
        {
            new Table(
                [
                    new TableHead([
                        new TableRow([
                            new If(
                                _ => infrequentOptions.ContainsAnyOf(InfrequentPropertiesPaths.DistinctIdentifier,
                                    InfrequentPropertiesPaths.SerialNumber),
                                new HideableDetails(new TableCell([new DisplayLabel(LabelCodes.DeviceId)],
                                    TableCellType.Header))
                            ),
                            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Type),
                                new TableCell([new DisplayLabel(LabelCodes.DeviceName)], TableCellType.Header)
                            ),
                            new If(_ => infrequentOptions.ContainsAnyOf(InfrequentPropertiesPaths.Manufacturer,
                                    InfrequentPropertiesPaths.DeviceName, InfrequentPropertiesPaths.ModelNumber,
                                    InfrequentPropertiesPaths.SerialNumber, InfrequentPropertiesPaths.Specialization,
                                    InfrequentPropertiesPaths
                                        .ExpirationDate),
                                new TableCell([new DisplayLabel(LabelCodes.DeviceOrImplant)], TableCellType.Header)
                            ),
                            new If(
                                _ => infrequentOptions.Contains(InfrequentPropertiesPaths.Version),
                                new HideableDetails(new TableCell([new ConstantText("Dodatečné informace")],
                                    TableCellType.Header))
                            ),
                            new If(
                                _ => infrequentOptions.Contains(InfrequentPropertiesPaths.Status),
                                new HideableDetails(new TableCell([new DisplayLabel(LabelCodes.Status)],
                                    TableCellType.Header))
                            ),
                            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Text),
                                new NarrativeCell(false, TableCellType.Header)
                            )
                        ])
                    ]),
                    new TableBody([
                        ..items.Select(item =>
                        {
                            var collapsibleRow = new StructuredDetails();

                            if (item.EvaluateCondition("f:text"))
                            {
                                collapsibleRow.Add(
                                    new CollapsibleDetail(
                                        new DisplayLabel(LabelCodes.OriginalNarrative),
                                        new Narrative("f:text")
                                    )
                                );
                            }

                            return new TableRow([
                                new If(
                                    _ => infrequentOptions.ContainsAnyOf(InfrequentPropertiesPaths.DistinctIdentifier,
                                        InfrequentPropertiesPaths.SerialNumber, InfrequentPropertiesPaths.Type,
                                        InfrequentPropertiesPaths.Manufacturer, InfrequentPropertiesPaths.DeviceName,
                                        InfrequentPropertiesPaths.ModelNumber, InfrequentPropertiesPaths.SerialNumber,
                                        InfrequentPropertiesPaths.Specialization,
                                        InfrequentPropertiesPaths
                                            .ExpirationDate),
                                    new DeviceInfo(item, infrequentOptions, false, false)
                                ),
                                new If(
                                    _ => infrequentOptions.ContainsAnyOf(InfrequentPropertiesPaths.Version),
                                    new HideableDetails(new AdditionalDeviceInfo(item, false, false))
                                ),
                                new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Status),
                                    new HideableDetails(new TableCell([
                                        new EnumIconTooltip("f:status", "http://hl7.org/fhir/device-status",
                                            new DisplayLabel(LabelCodes.Status))
                                    ]))
                                ),
                                new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Text),
                                    new NarrativeCell(narrativePath: item.SelectSingleNode("f:text").GetFullPath())
                                )
                            ], collapsibleRow, idSource: item);
                        })
                    ]),
                ],
                true
            )
        };

        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }

    public enum InfrequentPropertiesPaths
    {
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
        Text
    }
}