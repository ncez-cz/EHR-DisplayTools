using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Observation;

public class VitalSigns(List<XmlDocumentNavigator> navs) : Widget
{
    public const string XPathCondition =
        "f:category/f:coding[f:system/@value='http://terminology.hl7.org/CodeSystem/observation-category' and f:code/@value='vital-signs']";

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var errors = new List<ParseError>();
        var validItems = new Dictionary<ObservationTimeFrame, List<XmlDocumentNavigator>>();

        foreach (var nav in navs)
        {
            var key = GetTimeFrame(nav, errors);

            if (key is null)
            {
                errors.Add(new ParseError
                {
                    Kind = ErrorKind.MissingValue,
                    Severity = ErrorSeverity.Warning,
                    Path = nav.GetFullPath()
                });
                continue;
            }

            if (!validItems.TryGetValue(key, out var items))
            {
                items = [];
                validItems.Add(key, items);
            }

            items.Add(nav);
        }

        var sortedWidgetRows = validItems
            .OrderByDescending(kvp => kvp.Key.Start ?? kvp.Key.End)
            .Select(kvp => new VitalSignRow(kvp.Value))
            .ToList<Widget>();

        var widget = new AlternatingBackgroundColumn(sortedWidgetRows);

        var widgetRender = await widget.Render(navigator, renderer, context);

        widgetRender.Errors.AddRange(errors);

        return widgetRender;
    }

    private class VitalSignRow(
        List<XmlDocumentNavigator> items
    ) : Widget
    {
        public override async Task<RenderResult> Render(
            XmlDocumentNavigator navigator,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            if (items.Count == 0)
            {
                return RenderResult.NullResult;
            }

            var column = new Column([
                new Row([
                        new Container([
                            new HeadingNoMargin([
                                new TextContainer(TextStyle.Bold,
                                [
                                    new LocalizedLabel("observation.measurement-date-time"),
                                    new ConstantText(" "),
                                    new TextContainer(TextStyle.Lowercase,
                                        // Assuming all the members have the same effective time - this should always be the case
                                        new ChangeContext(items.First(), new Chronometry("effective"))
                                    )
                                ]),
                                new EnumIconTooltip(
                                    "f:status",
                                    "http://hl7.org/fhir/ValueSet/observation-status",
                                    new EhdsiDisplayLabel(LabelCodes.Status)
                                )
                            ], HeadingSize.H5)
                        ], optionalClass: "blue-color"),
                        new NarrativeModal(alignRight: false),
                    ],
                    flexContainerClasses: "gap-1 align-items-center",
                    idSource: new IdentifierSource(navigator)
                ),
                new Row([
                        new ConcatBuilder(items, (_, _, resourceNavigator) =>
                        [
                            new If(_ => ShouldRenderMeasurement(resourceNavigator),
                                new VitalSignsNameValuePair()
                            ),
                            new ConcatBuilder("f:component", (_, _, componentNavigator, _) =>
                            [
                                new If(_ => ShouldRenderMeasurement(componentNavigator),
                                    new VitalSignsNameValuePair()
                                )
                            ], separator: new NullWidget()),
                            new ConcatBuilder("f:hasMember", _ =>
                            [
                                new AnyReferenceNamingWidget(widgetModel: new ReferenceNamingWidgetModel
                                {
                                    Direction = FlexDirection.Column,
                                    Style = NameValuePair.NameValuePairStyle.Primary,
                                    Type = ReferenceNamingWidgetType.NameValuePair
                                }),
                            ])
                        ]),
                    ],
                    flexContainerClasses: "column-gap-6 gap-1 px-2"
                ),
            ]);

            return await column.Render(navigator, renderer, context);
        }

        private bool ShouldRenderMeasurement(XmlDocumentNavigator navigator)
        {
            var properties = InfrequentProperties.Evaluate<ObservationInfrequentProperties>(navigator);

            return properties.Contains(ObservationInfrequentProperties.Value) ||
                   IsDataAbsent(navigator, "f:dataAbsentReason");
        }

        private class VitalSignsNameValuePair : Widget
        {
            public override Task<RenderResult> Render(
                XmlDocumentNavigator navigator,
                IWidgetRenderer renderer,
                RenderContext context
            )
            {
                var nameValuePair =
                    new NameValuePair(
                        new ChangeContext("f:code", new CodeableConcept()),
                        new If(_ => IsDataAbsent(navigator, "f:dataAbsentReason"),
                            new AbsentData("f:dataAbsentReason")
                        ).Else(
                            new OpenTypeElement(null)
                        ),
                        direction: FlexDirection.Column,
                        style: NameValuePair.NameValuePairStyle.Primary
                    );

                return nameValuePair.Render(navigator, renderer, context);
            }
        }
    }

    private static ObservationTimeFrame? GetTimeFrame(XmlDocumentNavigator nav, List<ParseError> errors)
    {
        var singleDate = ExtractDateFromNavigator(nav, errors, "f:effectiveDateTime/@value");
        if (singleDate.HasValue)
        {
            return new ObservationTimeFrame(Truncate(singleDate.Value), null, false);
        }

        var start = ExtractDateFromNavigator(nav, errors, "f:effectivePeriod/f:start/@value");
        var end = ExtractDateFromNavigator(nav, errors, "f:effectivePeriod/f:end/@value");

        if (!start.HasValue && !end.HasValue)
        {
            return null;
        }

        return new ObservationTimeFrame(Truncate(start), Truncate(end), true);
    }

    private static DateTime? ExtractDateFromNavigator(XmlDocumentNavigator nav, List<ParseError> errors, string xpath)
    {
        var localNavigator = nav.SelectSingleNode(xpath);
        var node = localNavigator.Node;
        if (node == null)
        {
            return null;
        }

        var valueStr = node.Value;

        if (DateTime.TryParse(valueStr, out var result))
        {
            return result;
        }

        errors.Add(new ParseError
        {
            Kind = ErrorKind.InvalidValue,
            Severity = ErrorSeverity.Warning,
            Path = localNavigator.GetFullPath()
        });

        return null;
    }

    private static DateTime? Truncate(DateTime? dateTime)
    {
        if (!dateTime.HasValue)
        {
            return null;
        }

        var dateTimeValue = dateTime.Value;
        return new DateTime(dateTimeValue.Year, dateTimeValue.Month, dateTimeValue.Day, dateTimeValue.Hour,
            dateTimeValue.Minute, 0, dateTimeValue.Kind);
    }

    private record ObservationTimeFrame(DateTime? Start, DateTime? End, bool IsPeriod);
}