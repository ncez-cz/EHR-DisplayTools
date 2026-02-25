using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Questionnaire;

public class QuestionnaireResponseRow(
    InfrequentPropertiesData<QuestionnaireResponseInfrequentProperties> infrequentProperties
) : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var collapsibleRow = QuestionnaireResponseCollapsibleDetail.Build(navigator);

        var tree = new TableRow([
            new If(_ => infrequentProperties.Contains(QuestionnaireResponseInfrequentProperties.Identifier),
                new TableCell([
                    new Optional("f:identifier", new ShowIdentifier())
                ])),
            new If(_ => infrequentProperties.Contains(QuestionnaireResponseInfrequentProperties.Authored),
                new TableCell([
                    new Optional("f:authored", new ShowDateTime()),
                ])),
            new If(_ => infrequentProperties.Contains(QuestionnaireResponseInfrequentProperties.Author),
                new TableCell([
                    new Optional("f:author", new AnyReferenceNamingWidget())
                ])),
            new If(_ => infrequentProperties.Contains(QuestionnaireResponseInfrequentProperties.Source),
                new TableCell([
                    new Optional("f:source", new AnyReferenceNamingWidget())
                ])),
            new If(_ => infrequentProperties.Contains(QuestionnaireResponseInfrequentProperties.Status),
                new TableCell([
                    new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/questionnaire-answers-status",
                        new EhdsiDisplayLabel(LabelCodes.Status)),
                ])
            ),
            new If(_ => infrequentProperties.Contains(QuestionnaireResponseInfrequentProperties.Text),
                new NarrativeCell()
            ),
        ], collapsibleRow, idSource: navigator);

        var result = await tree.Render(navigator, renderer, context);
        return result;
    }
}