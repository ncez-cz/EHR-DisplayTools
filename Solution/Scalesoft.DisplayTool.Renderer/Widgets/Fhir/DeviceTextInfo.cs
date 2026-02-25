using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class DeviceTextInfo(string xpath = ".", bool compact = false) : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties.Evaluate<DeviceCardInfrequentProperties>(navigator);

        // Since all the properties are optional, we can skip rendering if none are present.
        if (infrequentProperties.Count == 0)
        {
            return RenderResult.NullResult;
        }

        var deviceNavigator = navigator.SelectSingleNode(xpath);

        List<Widget> deviceProperties =
        [
            // Basic device information
            new If(_ => infrequentProperties.Contains(DeviceCardInfrequentProperties.DeviceName) && !compact,
                new NameValuePair(
                    new EhdsiDisplayLabel(LabelCodes.DeviceName),
                    new Text("f:deviceName/f:name/@value")
                )
            ),

            // Device identifier
            new If(_ => infrequentProperties.Contains(DeviceCardInfrequentProperties.Identifier) && !compact,
                new NameValuePair(
                    new EhdsiDisplayLabel(LabelCodes.DeviceId),
                    new ShowIdentifier("f:identifier")
                )
            ),

            // Manufacturer details
            new If(_ => infrequentProperties.Contains(DeviceCardInfrequentProperties.Manufacturer),
                new NameValuePair(
                    new LocalizedLabel("device.manufacturer"),
                    new Text("f:manufacturer/@value")
                )
            ),

            // Serial number
            new If(_ => infrequentProperties.Contains(DeviceCardInfrequentProperties.SerialNumber),
                new NameValuePair(
                    new LocalizedLabel("device.serialNumber"),
                    new Text("f:serialNumber/@value")
                )
            ),

            new If(
                _ => infrequentProperties.Count > 0 &&
                     infrequentProperties.All(x => x == DeviceCardInfrequentProperties.Text),
                new Container([
                    new Card(null, new Narrative("f:text"))
                ])
            ),
        ];

        var container = new Concat(deviceProperties);

        return await container.Render(deviceNavigator, renderer, context);
    }
}

public enum DeviceCardInfrequentProperties
{
    DeviceName,
    Identifier,
    Status,
    Manufacturer,
    SerialNumber,
    Text,
}