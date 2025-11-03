using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Devices;

public class DeviceInfo(
    XmlDocumentNavigator item,
    InfrequentPropertiesData<ShowDevice.InfrequentPropertiesPaths>? infrequentProperties = null,
    bool addIdentifier = true,
    bool addVisualIdentifiers = true
) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        navigator = item;

        List<Widget> tree =
        [
            new If(
                _ => infrequentProperties == null || infrequentProperties.ContainsAnyOf(
                    ShowDevice.InfrequentPropertiesPaths.DistinctIdentifier,
                    ShowDevice.InfrequentPropertiesPaths.SerialNumber),
                new HideableDetails(new TableCell(
                    [
                        new Choose(
                            [
                                new When(
                                    "f:distinctIdentifier",
                                    new Text("f:distinctIdentifier/@value")
                                ),
                            ],
                            new Optional("f:serialNumber", new Text("@value"))
                        )
                    ],
                    idSource: addIdentifier ? navigator : (IdentifierSource?)null
                ))
            ),
            new If(
                _ => infrequentProperties == null ||
                     infrequentProperties.Contains(ShowDevice.InfrequentPropertiesPaths.Type),
                new TableCell([
                    new CommaSeparatedBuilder("f:type", _ => [new CodeableConcept()]),
                ], visualIdSource: addVisualIdentifiers ? navigator : (IdentifierSource?)null)
            ),
            new If(
                _ => infrequentProperties == null || infrequentProperties.ContainsAnyOf(
                    ShowDevice.InfrequentPropertiesPaths.Manufacturer, ShowDevice.InfrequentPropertiesPaths.DeviceName,
                    ShowDevice.InfrequentPropertiesPaths.ModelNumber, ShowDevice.InfrequentPropertiesPaths.SerialNumber,
                    ShowDevice.InfrequentPropertiesPaths.Specialization,
                    ShowDevice.InfrequentPropertiesPaths
                        .ExpirationDate),
                new TableCell(DeviceParsingInfo.CompactRenderingWidgets,
                    visualIdSource: addVisualIdentifiers ? navigator : (IdentifierSource?)null)
            ),
        ];

        return tree.RenderConcatenatedResult(navigator, renderer, context);
    }
}