using System.Globalization;
using System.Text;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class SvgGraph : Widget
{
    private readonly double m_origin;
    private readonly double m_period;
    private readonly int m_dimensions;
    private readonly string m_data;
    private readonly double? m_lowerLimit;
    private readonly double? m_upperLimit;
    private readonly double m_factor;
    private readonly IdentifierSource? m_idSource;
    private readonly IdentifierSource? m_visualIdSource;

    public SvgGraph(
        double origin,
        double period,
        int dimensions,
        string data,
        double? lowerLimit,
        double? upperLimit,
        double factor,
        IdentifierSource? idSource = null,
        IdentifierSource? visualIdSource = null
    )
    {
        m_origin = origin;
        m_period = period;
        m_dimensions = dimensions;
        m_data = data;
        m_lowerLimit = lowerLimit;
        m_upperLimit = upperLimit;
        m_factor = factor;
        m_idSource = idSource;
        m_visualIdSource = visualIdSource;
    }

    private static readonly string[] m_colors =
        ["lightblue", "pink", "orchid", "cyan", "goldenrod", "limegreen", "orangered"];

    private const string DefaultColor = "black";
    private const string UpperLimitColor = "purple";
    private const string LowerLimitColor = "orange";
    private const int InternalWidth = 500;
    private const int InternalHeight = 500;
    private const int InternalPadding = 80;
    private const int GraphWidth = InternalWidth - 2 * InternalPadding;
    private const int GraphHeight = InternalHeight - 2 * InternalPadding;

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        if (context.RenderMode == RenderMode.Documentation)
        {
            return navigator.GetFullPath();
        }

        var belowLowerLimit = m_lowerLimit * m_factor + m_origin - 1;
        var aboveUpperLimit = m_upperLimit * m_factor + m_origin + 1;

        var values = m_data.Split(' ')
            .Where(s => double.TryParse(s, CultureInfo.InvariantCulture, out _) || s == "U" || s == "L" || s == "E")
            .Select(s =>
                double.TryParse(s, CultureInfo.InvariantCulture, out var num) ? (object)(num * m_factor + m_origin) : s)
            .ToList();

        var dimensionsData = Enumerable.Range(0, m_dimensions)
            .Select(_ => new List<double?>())
            .ToList();
        for (var i = 0; i < values.Count; i++)
        {
            var dimensionIndex = i % m_dimensions;
            if (values[i] is double)
            {
                dimensionsData[dimensionIndex].Add((double)values[i]);
            }
            else
                switch ((string)values[i])
                {
                    case "U" when aboveUpperLimit != null:
                        dimensionsData[dimensionIndex].Add((double)aboveUpperLimit);
                        break;
                    case "L" when belowLowerLimit != null:
                        dimensionsData[dimensionIndex].Add((double)belowLowerLimit);
                        break;
                    case "E":
                        dimensionsData[dimensionIndex].Add(null);
                        break;
                }
        }

        if (values.Count == 0)
        {
            return RenderResult.NullResult;
        }

        var valueOverLimit = m_data.Split(' ').Contains("U");
        var valueUnderLimit = m_data.Split(' ').Contains("L");

        var maxValue = dimensionsData.SelectMany(d => d).Where(val =>
                val != null && !double.IsNaN((double)val) && !val.Equals(belowLowerLimit) &&
                !val.Equals(aboveUpperLimit))
            .DefaultIfEmpty(double.MinValue).Max();
        var minValue = dimensionsData.SelectMany(d => d).Where(val =>
                val != null && !double.IsNaN((double)val) && !val.Equals(aboveUpperLimit) &&
                !val.Equals(belowLowerLimit))
            .DefaultIfEmpty(double.MaxValue).Min();

        var dataRange = maxValue - minValue;
        if (dataRange == 0) dataRange = 1;
        var zoomFactor = GraphHeight / dataRange;

        var totalTime = values.Count * m_period;

        var labels = await DrawLabels(m_period, minValue, maxValue, InternalHeight, InternalPadding, zoomFactor, values,
            totalTime,
            GraphWidth,
            InternalWidth, context);

        var graphData = DrawGraphData(dimensionsData, InternalPadding, GraphWidth, InternalHeight, minValue, zoomFactor,
            belowLowerLimit,
            aboveUpperLimit);

        var legendX = InternalWidth - InternalPadding;
        var legendY = GraphHeight - InternalPadding;

        var legendEntries = await DrawLegend(m_dimensions, legendX, legendY, valueOverLimit, UpperLimitColor,
            valueUnderLimit,
            LowerLimitColor, m_lowerLimit, m_upperLimit, m_factor, m_origin, context);


        var viewModel = new ViewModel
        {
            SvgWidth = InternalWidth,
            SvgHeight = InternalHeight,
            SvgPadding = InternalPadding,
            Labels = labels,
            Lines = graphData.PolyLineData,
            Triangles = graphData.TriangleData,
            LegendEntries = legendEntries
        };

        HandleIds(context, navigator, viewModel, m_idSource, m_visualIdSource);
        var view = await renderer.RenderSvgGraph(viewModel);

        return view;
    }

    private static async Task<List<AxisLabel>> DrawLabels(
        double period,
        double? minValue,
        double? maxValue,
        int height,
        int padding,
        double? zoomFactor,
        List<object> values,
        double totalTime,
        int graphWidth,
        int width,
        RenderContext renderContext
    )
    {
        List<AxisLabel> labels = [];

        var yLabelCount = 5;
        for (var i = 0; i <= yLabelCount; i++)
        {
            var labelValue = minValue + i * (maxValue - minValue) / yLabelCount ?? 0;
            var yPosition = height - padding - (labelValue - minValue) * zoomFactor;


            labels.Add(new AxisLabel
            {
                X = (padding - 10).ToString(CultureInfo.InvariantCulture),
                Y = ((yPosition ?? 0) + 4).ToString(CultureInfo.InvariantCulture),
                Label = labelValue.ToString("G4", CultureInfo.InvariantCulture),
            });
        }

        var xLabelInterval = Math.Max(1, values.Count / 10);
        for (var i = 0; i < values.Count; i += xLabelInterval)
        {
            var xPosition = padding + i * period / totalTime * graphWidth;
            var timeLabel = i * period;

            labels.Add(new AxisLabel
            {
                X = xPosition.ToString(CultureInfo.InvariantCulture),
                Y = (height - padding + 20).ToString(CultureInfo.InvariantCulture),
                Label = timeLabel.ToString("G4", CultureInfo.InvariantCulture),
            });
        }


        labels.Add(
            new AxisLabel
            {
                X = (width / 2).ToString(CultureInfo.InvariantCulture),
                Y = (height - 10).ToString(CultureInfo.InvariantCulture),
                Label = await LocalizedLabel.GetTranslatedValue("graph.time", renderContext) + " (ms)"
            }
        );

        labels.Add(
            new AxisLabel
            {
                X = "15",
                Y = (height / 2).ToString(CultureInfo.InvariantCulture),
                Label = await LocalizedLabel.GetTranslatedValue("graph.value", renderContext),
                Attributes = new Dictionary<string, object>
                {
                    { "transform", $"rotate(-90, 15, {(height / 2).ToString(CultureInfo.InvariantCulture)})" }
                }
            }
        );

        return labels;
    }


    private static (List<PolylineData> PolyLineData, List<TriangleData> TriangleData) DrawGraphData(
        List<List<double?>> dimensionsData,
        int padding,
        int graphWidth,
        int height,
        double? minValue,
        double? zoomFactor,
        double? belowLowerLimit,
        double? aboveUpperLimit
    )
    {
        List<PolylineData> lines = [];
        List<TriangleData> triangles = [];

        // rendering of graph data
        for (var dimensionNum = 0; dimensionNum < dimensionsData.Count; dimensionNum++)
        {
            var dimension = dimensionsData[dimensionNum];
            var color = (dimensionNum < m_colors.Length) ? m_colors[dimensionNum] : DefaultColor;

            var points = new StringBuilder();

            for (var i = 0; i < dimension.Count; i++)
            {
                if (!dimension[i].Equals(aboveUpperLimit) && !dimension[i].Equals(belowLowerLimit) &&
                    dimension[i] != null)
                {
                    var x = padding + (i / (dimension.Count - 1.0)) * graphWidth;
                    var y = height - padding - (dimension[i] - minValue) * zoomFactor;


                    points.AppendFormat(CultureInfo.InvariantCulture, "{0:0.###},{1:0.###} ", x, y);
                }
            }

            lines.Add(new PolylineData
            {
                Color = color,
                Points = points.ToString()
            });
        }

        // rendering of above and below limit values
        if (belowLowerLimit != null && aboveUpperLimit != null)
        {
            foreach (var dimension in dimensionsData)
            {
                for (var i = 0; i < dimension.Count; i++)
                {
                    var x = padding + (i / (dimension.Count - 1.0)) * graphWidth;
                    if (dimension[i].Equals(aboveUpperLimit))
                    {
                        triangles.Add(DrawTriangle(10, 15, x, padding, UpperLimitColor, true));
                    }
                    else if (dimension[i].Equals(belowLowerLimit))
                    {
                        triangles.Add(DrawTriangle(10, 15, x, height - padding - 20, LowerLimitColor, false));
                    }
                }
            }
        }

        return (lines, triangles);
    }

    private static TriangleData DrawTriangle(
        double baseWidth,
        double height,
        double xPosition,
        double yPosition,
        string color,
        bool pointsUpwards
    )
    {
        var y1 = pointsUpwards ? yPosition - height : yPosition + height;
        var x2 = xPosition + baseWidth;
        var x3 = xPosition - baseWidth;

        return new TriangleData
        {
            Points =
                $"{xPosition.ToString(CultureInfo.InvariantCulture)},{y1.ToString(CultureInfo.InvariantCulture)} " +
                $"{x2.ToString(CultureInfo.InvariantCulture)},{yPosition.ToString(CultureInfo.InvariantCulture)} " +
                $"{x3.ToString(CultureInfo.InvariantCulture)},{yPosition.ToString(CultureInfo.InvariantCulture)}",
            Color = color
        };
    }


    private static async Task<List<LegendEntry>> DrawLegend(
        int dimensions,
        int legendX,
        int legendY,
        bool valueOverLimit,
        string upperLimitColor,
        bool valueUnderLimit,
        string lowerLimitColor,
        double? lowerLimit,
        double? upperLimit,
        double factor,
        double origin,
        RenderContext renderContext
    )
    {
        List<LegendEntry> legendEntries = [];

        for (var i = 0; i < dimensions; i++)
        {
            var color = (i < m_colors.Length) ? m_colors[i] : DefaultColor;

            legendEntries.Add(new LegendEntry
            {
                Color = color,
                Label = $"{await LocalizedLabel.GetTranslatedValue("graph.data", renderContext)} {i + 1}",
                X = legendX,
                Y = legendY
            });

            legendY += 25;
        }

        var upperLimitExplanationText =
            await LocalizedLabel.GetTranslatedValue("graph.value-above-limit", renderContext) + " ({0})";
        var lowerLimitExplanationText =
            await LocalizedLabel.GetTranslatedValue("graph.value-below-limit", renderContext) + " ({0})";

        if (lowerLimit != null && upperLimit != null)
        {
            var x = InternalWidth - 200;
            if (valueOverLimit)
            {
                var y = 20;

                legendEntries.Add(new LegendEntry
                {
                    Color = upperLimitColor,
                    Label =
                        string.Format(upperLimitExplanationText, Math.Round((double)(upperLimit * factor + origin))),
                    X = x,
                    Y = y
                });
            }

            if (valueUnderLimit)
            {
                var y = InternalHeight - 30;

                legendEntries.Add(new LegendEntry
                {
                    Color = lowerLimitColor,
                    Label =
                        string.Format(lowerLimitExplanationText, Math.Round((double)(lowerLimit * factor + origin))),
                    X = x,
                    Y = y
                });
            }
        }

        return legendEntries;
    }

    public class ViewModel : ViewModelBase
    {
        public required int SvgWidth { get; init; }
        public required int SvgHeight { get; init; }
        public required int SvgPadding { get; init; }

        public List<AxisLabel> Labels { get; init; } = [];
        public List<PolylineData> Lines { get; init; } = [];
        public List<TriangleData> Triangles { get; init; } = [];
        public List<LegendEntry> LegendEntries { get; init; } = [];
    }

    public class AxisLabel
    {
        public required string X { get; init; }
        public required string Y { get; init; }
        public required string Label { get; init; }
        public Dictionary<string, object> Attributes { get; init; } = new();
    }

    public class PolylineData
    {
        public required string Color { get; init; }
        public required string Points { get; init; }
    }

    public class TriangleData
    {
        public required string Points { get; init; }
        public required string Color { get; init; }
    }

    public class LegendEntry
    {
        public required string Color { get; init; }
        public required string Label { get; init; }
        public required double X { get; init; }
        public required double Y { get; init; }
    }
}