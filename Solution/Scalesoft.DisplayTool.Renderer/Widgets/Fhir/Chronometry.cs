using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

/// <summary>
///     Displays timing[x], onset[x], performed[x] and similar FHIR properties.
/// </summary>
/// <param name="prefix">The (common) beginning part of each possible option's path ("timing" for timing[x])</param>
/// <param name="showFromPrefix">If true, a "from " prefix is added to {prefix}DateTime's value (used for onset[x])</param>
public class Chronometry(string? prefix, bool showFromPrefix = false, string? path = null) : Widget
{
    private string? m_path = path;

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        if (m_path != null)
        {
            m_path += "/";
        }

        var widget = new Choose(
        [
            new When($"{m_path}f:{prefix}DateTime",
                new If(_ => showFromPrefix,
                    new Condition(
                        $"{m_path}f:{prefix}DateTime/@value",
                        new EhdsiDisplayLabel(LabelCodes.From),
                        new ConstantText(" ")
                    )
                ),
                new ShowDateTime($"{m_path}f:{prefix}DateTime")
            ),
            new When($"{m_path}f:{prefix}Age", new ShowAge($"{m_path}f:{prefix}Age")),
            new When($"{m_path}f:{prefix}Period", new ShowPeriod($"{m_path}f:{prefix}Period")),
            new When($"{m_path}f:{prefix}Duration", new ShowDuration($"{m_path}f:{prefix}Duration")),
            new When($"{m_path}f:{prefix}Range", new ShowRange($"{m_path}f:{prefix}Range")),
            new When($"{m_path}f:{prefix}Timing",
                new ShowTiming($"{m_path}f:{prefix}Timing",
                    nameValuePairStyle: NameValuePair.NameValuePairStyle.Initial)),
            new When($"{m_path}f:{prefix}Date", new ShowDateTime($"{m_path}f:{prefix}Date")),
            new When($"{m_path}f:{prefix}String", new AbsentDataTextCheck($"{m_path}f:{prefix}String", new Text())),
        ]);

        return widget.Render(navigator, renderer, context);
    }
}