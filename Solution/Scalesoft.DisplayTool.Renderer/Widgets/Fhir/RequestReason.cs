using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Observation;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class RequestReason(XmlDocumentNavigator nav) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        Widget result;

        switch (nav.Node?.Name)
        {
            case "Condition":
                result = new Conditions([nav], new ConstantText("Problém"), true);
                break;
            case "Observation":
                result = new ChangeContext(nav, new ObservationCard(true));
                break;
            case "DiagnosticReport":
                result = new ChangeContext(nav, new DiagnosticReportCard(true));
                break;
            case "DocumentReference":
                result = new ConstantText(string.Empty);
                break;
            default:
                return Task.FromResult<RenderResult>(new ParseError
                {
                    Kind = ErrorKind.InvalidValue, Message = "Unknown request reason type",
                    Path = navigator.GetFullPath(),
                    Severity = ErrorSeverity.Warning
                });
        }


        return result.Render(navigator, renderer, context);
    }
}