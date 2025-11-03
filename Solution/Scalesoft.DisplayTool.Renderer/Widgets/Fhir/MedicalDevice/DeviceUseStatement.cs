using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.MedicalDevice;

public class DeviceUseStatement(List<XmlDocumentNavigator> items) : Widget, IResourceWidget
{
    public static string ResourceType => "DeviceUseStatement";
    [UsedImplicitly]
    public static bool RequiresExternalTitle => true;

    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return [new DeviceUseStatement(items)];
    }
    
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentOptions = InfrequentProperties.Evaluate<InfrequentPropertiesPaths>(items);

        List<Widget> headerRow =
        [
            new TableCell([new ConstantText("Četnost použití")], TableCellType.Header),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.BodySite),
                new TableCell([new DisplayLabel(LabelCodes.BodySite)], TableCellType.Header)),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.ReasonCode) ||
                        infrequentOptions.Contains(InfrequentPropertiesPaths.ReasonReference),
                new TableCell([new ConstantText("Důvod")], TableCellType.Header)),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Status),
                new TableCell([new DisplayLabel(LabelCodes.Status)], TableCellType.Header)
            ),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Text),
                new NarrativeCell(false, TableCellType.Header)
            )
        ];

        var table = new Table(
            [
                new TableHead([
                    new TableRow(headerRow)
                ]),
                ..items.Select(x => new TableBody([new DeviceUseStatementRow(x, infrequentOptions)])),
            ],
            true
        );

        return table.Render(navigator, renderer, context);
    }

    private class DeviceUseStatementRow(
        XmlDocumentNavigator item,
        InfrequentPropertiesData<InfrequentPropertiesPaths> parentInfrequentOptions
    ) : Widget
    {
        public override async Task<RenderResult> Render(
            XmlDocumentNavigator navigator,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            var infrequentOptions =
                InfrequentProperties.Evaluate<InfrequentPropertiesPaths>([item]);

            var collapsibleRow = new StructuredDetails();

            if (item.EvaluateCondition("f:text"))
            {
                collapsibleRow.AddCollapser(new DisplayLabel(LabelCodes.OriginalNarrative), new Narrative("f:text"));
            }

            if (item.EvaluateCondition("f:device"))
            {
                collapsibleRow.AddCollapser(new ConstantText("Informace o zařízení"),
                    ShowSingleReference.WithDefaultDisplayHandler(x => [new ShowDevice([x])], "f:device"));
            }

            var tableCells = new List<Widget>
            {
                new TableCell([new OpenTypeElement(null, "timing")]), // Timing | Period | dateTime
                new If(_ => parentInfrequentOptions.Contains(InfrequentPropertiesPaths.BodySite),
                    new TableCell([
                        new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.BodySite),
                            new Optional("f:bodySite", new CodeableConcept()), new LineBreak())
                    ])),
                new If(_ => parentInfrequentOptions.Contains(InfrequentPropertiesPaths.ReasonCode) ||
                            parentInfrequentOptions.Contains(InfrequentPropertiesPaths.ReasonReference),
                    new TableCell([
                        new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.ReasonCode),
                            new ItemListBuilder("f:reasonCode",
                                ItemListType.Unordered,
                                _ => [new CodeableConcept()]), new LineBreak()),
                        new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.ReasonReference),
                            new NameValuePair([new ConstantText("Odkaz na důvod")],
                            [
                                new ItemListBuilder("f:reasonReference", ItemListType.Unordered,
                                    _ => [new AnyReferenceNamingWidget()])
                            ]))
                    ])),
                new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Status),
                    new TableCell([
                            new EnumIconTooltip("f:status", "http://hl7.org/fhir/device-use-statement-status",
                                new DisplayLabel(LabelCodes.Status))
                        ]
                    )),
                new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Text),
                    new NarrativeCell()
                )
            };

            var result = await new TableRow(tableCells, collapsibleRow, idSource: item).Render(item, renderer, context);

            var isStatus = item.EvaluateCondition("f:status");
            if (!isStatus)
            {
                result.Errors.Add(ParseError.MissingValue(item.SelectSingleNode("f:status").GetFullPath()));
            }

            var isDevice = item.EvaluateCondition("f:device");
            if (!isDevice)
            {
                result.Errors.Add(ParseError.MissingValue(item.SelectSingleNode("f:device").GetFullPath()));
            }

            var isTiming = item.EvaluateCondition("f:*[starts-with(name(), 'timing')]");
            if (!isTiming)
            {
                result.Errors.Add(
                    ParseError.MissingValue(item.SelectSingleNode("*[starts-with(local-name(), 'timing')]")
                        .GetFullPath()));
            }

            return result;
        }
    }

    private enum InfrequentPropertiesPaths
    {
        ReasonCode,
        ReasonReference,
        BodySite,
        Text,

        [EnumValueSet("http://hl7.org/fhir/device-use-statement-status")]
        Status
    }
}