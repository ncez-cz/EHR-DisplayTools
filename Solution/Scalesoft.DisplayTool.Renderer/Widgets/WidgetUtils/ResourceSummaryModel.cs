using System.Diagnostics.CodeAnalysis;

namespace Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;

public class ResourceSummaryModel
{
    public Widget? Label { get; init; }
    public required Widget Value { get; init; }

    [SetsRequiredMembers]
    public ResourceSummaryModel(Widget? label, Widget value)
    {
        Label = label;
        Value = value;
    }

    public ResourceSummaryModel()
    {
    }
}