using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Cda;

public class PatientSectionWidget : Widget
{
    public override Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer,
        RenderContext context)
    {
        List<Widget> widgetTree =
        [
            new Section("/n1:ClinicalDocument/n1:recordTarget/n1:patientRole", "Patient info section is missing !", [
                new EhdsiDisplayLabel(LabelCodes.Patient)
            ], [
                new Container([
                    new ChangeContext("/n1:ClinicalDocument/n1:recordTarget/n1:patientRole", new PatientDataWidget()),
                ], ContainerType.Div),
                new ExtendedCdaHeaderWidget()
            ], [
                new ChangeContext("/n1:ClinicalDocument/n1:recordTarget/n1:patientRole",
                    new TextContainer(TextStyle.None,
                        [new ChangeContext("n1:patient/n1:name/n1:family", new Widget51())]),
                    new TextContainer(TextStyle.None,
                        [new ChangeContext("n1:patient/n1:name/n1:given", new Widget52())]),
                    new TextContainer(TextStyle.None,
                    [
                        new WidgetWithVariables(new ShowTsWidget(), [new Variable("node", "n1:patient/n1:birthTime")])
                    ])
                ),
            ], titleAbbreviations: ("P", "P")),

        ];

        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}