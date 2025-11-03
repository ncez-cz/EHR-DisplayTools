using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Alert;

public class FlagResource(List<XmlDocumentNavigator> items) : Widget, IResourceWidget
{
    public static string ResourceType => "Flag";
    [UsedImplicitly]
    public static bool RequiresExternalTitle => true;

    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return [new FlagResource(items)];
    }
    
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var headerCells = new List<Widget>();

        var listOfPresentResources = Enum.GetValues<InfrequentPropertiesPaths>().Where(HasAtLeastOneResource).ToList();

        //Mandatory
        headerCells.Add(new TableCell([new DisplayLabel(LabelCodes.Code)], TableCellType.Header));
        headerCells.Add(new TableCell([new ConstantText("Subjekt")], TableCellType.Header));

        AddHeaderIfPresent(InfrequentPropertiesPaths.Identifier, new ConstantText("Identifikátor"), headerCells,
            listOfPresentResources);
        AddHeaderIfPresent(InfrequentPropertiesPaths.Category, new ConstantText("Kategorie"), headerCells,
            listOfPresentResources);
        AddHeaderIfPresent(InfrequentPropertiesPaths.Period, new DisplayLabel(LabelCodes.Date), headerCells,
            listOfPresentResources);
        AddHeaderIfPresent(InfrequentPropertiesPaths.Encounter, new ConstantText(Labels.Encounter), headerCells,
            listOfPresentResources);
        AddHeaderIfPresent(InfrequentPropertiesPaths.Author, new DisplayLabel(LabelCodes.Author), headerCells,
            listOfPresentResources);
        AddHeaderIfPresent(InfrequentPropertiesPaths.Status, new DisplayLabel(LabelCodes.Status), headerCells,
            listOfPresentResources);

        if (listOfPresentResources.Contains(InfrequentPropertiesPaths.Text))
        {
            headerCells.Add(new NarrativeCell(false, TableCellType.Header));
        }

        var table = new Table(
            [
                new TableHead([
                    new TableRow(headerCells)
                ]),
                ..items.Select(x => new TableBody([new FlagResourceRowBuilder(x, listOfPresentResources)])),
            ],
            true
        );

        var result = await table.Render(navigator, renderer, context);

        var isStatus = navigator.EvaluateCondition("f:status");
        var isCode = navigator.EvaluateCondition("f:code");
        var isSubject = navigator.EvaluateCondition("f:subject");

        if (!isStatus)
        {
            result.Errors.Add(ParseError.MissingValue(navigator.SelectSingleNode("f:status").GetFullPath()));
        }

        if (!isCode)
        {
            result.Errors.Add(ParseError.MissingValue(navigator.SelectSingleNode("f:code").GetFullPath()));
        }

        if (!isSubject)
        {
            result.Errors.Add(ParseError.MissingValue(navigator.SelectSingleNode("f:subject").GetFullPath()));
        }

        return result;
    }

    private bool HasAtLeastOneResource(InfrequentPropertiesPaths resource)
    {
        return items.Any(x =>
            InfrequentProperties.Evaluate<InfrequentPropertiesPaths>([x])
                .Contains(resource)
        );
    }

    void AddHeaderIfPresent(
        InfrequentPropertiesPaths resource,
        Widget headerWidget,
        List<Widget> headers,
        List<InfrequentPropertiesPaths> presentResources
    )
    {
        if (presentResources.Contains(resource))
            headers.Add(new TableCell([headerWidget], TableCellType.Header));
    }

    private class FlagResourceRowBuilder(
        XmlDocumentNavigator item,
        List<InfrequentPropertiesPaths> listOfPresentResources
    ) : Widget
    {
        public override Task<RenderResult> Render(
            XmlDocumentNavigator navigator,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            var infrequentOptions =
                InfrequentProperties.Evaluate<InfrequentPropertiesPaths>([item]);

            var rowDetails = new StructuredDetails();
            if (item.EvaluateCondition("f:text"))
            {
                rowDetails.AddCollapser(new DisplayLabel(LabelCodes.OriginalNarrative), new Narrative("f:text"));
            }

            var tableRowContent = new List<Widget>
            {
                //Mandatory
                new TableCell([new Optional("f:code", new CodeableConcept())]),
                new TableCell([
                    new TextContainer(TextStyle.Regular,
                        ReferenceHandler.BuildAnyReferencesNaming(item, "f:subject", context, renderer))
                ])
            };

            if (infrequentOptions.Contains(InfrequentPropertiesPaths.Identifier))
                tableRowContent.Add(
                    new TableCell([
                        new TextContainer(TextStyle.Regular, [
                            new TextContainer(TextStyle.Regular,
                                [new CommaSeparatedBuilder("f:identifier", _ => [new ShowIdentifier()])]),
                            new LineBreak(),
                        ])
                    ]));
            else if (infrequentOptions.Contains(InfrequentPropertiesPaths.Id))
            {
                tableRowContent.Add(
                    new TableCell([
                        new NameValuePair([new ConstantText("Technický identifikátor podani")],
                        [
                            new Optional("f:id", new Text("@value"))
                        ])
                    ]));
            }
            else
            {
                tableRowContent.Add(new TableCell([new ConstantText("Identifikátor podání není specifikován")]));
            }

            AddOptionalCell(tableRowContent, infrequentOptions,
                InfrequentPropertiesPaths.Category,
                () => new TableCell([new Optional("f:category", new CodeableConcept())])
            );

            AddOptionalCell(tableRowContent, infrequentOptions,
                InfrequentPropertiesPaths.Period,
                () => new TableCell([new Optional("f:period", new ShowPeriod())])
            );
            AddOptionalCell(tableRowContent, infrequentOptions,
                InfrequentPropertiesPaths.Encounter,
                () =>
                    new TableCell([
                        new TextContainer(TextStyle.Regular,
                            [new CommaSeparatedBuilder("f:encounter", _ => [new AnyReferenceNamingWidget()])])
                    ])
            );
            AddOptionalCell(tableRowContent, infrequentOptions,
                InfrequentPropertiesPaths.Author,
                () =>
                    new TableCell([
                        new TextContainer(TextStyle.Regular,
                            [new CommaSeparatedBuilder("f:author", _ => [new AnyReferenceNamingWidget()])])
                    ])
            );
            AddOptionalCell(tableRowContent, infrequentOptions, InfrequentPropertiesPaths.Status,
                () =>
                    new TableCell([
                        new EnumIconTooltip("f:status", "http://hl7.org/fhir/flag-status",
                            new DisplayLabel(LabelCodes.Status))
                    ]));
            if (infrequentOptions.Contains(InfrequentPropertiesPaths.Text))
            {
                tableRowContent.Add(new NarrativeCell());
            }

            return new TableRow(tableRowContent, rowDetails, idSource: item).Render(item, renderer, context);
        }

        void AddOptionalCell(
            List<Widget> tableRowContent,
            InfrequentPropertiesData<InfrequentPropertiesPaths> infrequentOptions,
            InfrequentPropertiesPaths property,
            Func<TableCell> presentCellFactory
        )
        {
            if (listOfPresentResources.Contains(property))
            {
                if (infrequentOptions.Contains(property))
                {
                    tableRowContent.Add(presentCellFactory());
                }
                else
                {
                    tableRowContent.Add(new TableCell([]));
                }
            }
        }
    }

    private enum InfrequentPropertiesPaths
    {
        Identifier,
        Id,
        Category,
        Period,
        Encounter,
        Author,
        Text,

        [EnumValueSet("http://hl7.org/fhir/flag-status")]
        Status
    }
}