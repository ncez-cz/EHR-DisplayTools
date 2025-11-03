using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Questionnaire;

public class QuestionnaireResponses(List<XmlDocumentNavigator> items) : Widget, IResourceWidget
{
   public static string ResourceType => "QuestionnaireResponse";
   [UsedImplicitly]
   public static bool RequiresExternalTitle => true;

   public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
   {
       return [new QuestionnaireResponses(items)];
   }
    
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties.Evaluate<QuestionnaireResponseInfrequentProperties>(items);


        var tree = new Table(
            [
                new TableHead([
                    new TableRow([
                        new If(_ => infrequentProperties.Contains(QuestionnaireResponseInfrequentProperties.Identifier),
                            new TableCell([new ConstantText("Identifikátor")], TableCellType.Header)),
                        new If(_ => infrequentProperties.Contains(QuestionnaireResponseInfrequentProperties.Authored),
                            new TableCell([new ConstantText("Datum vyplnění")], TableCellType.Header)),
                        new If(_ => infrequentProperties.Contains(QuestionnaireResponseInfrequentProperties.Author),
                            new TableCell([new DisplayLabel(LabelCodes.Author)], TableCellType.Header)),
                        new If(_ => infrequentProperties.Contains(QuestionnaireResponseInfrequentProperties.Source),
                            new TableCell([new ConstantText("Respondent")], TableCellType.Header)),
                        new If(_ => infrequentProperties.Contains(QuestionnaireResponseInfrequentProperties.Status),
                            new TableCell([new ConstantText("Stav")], TableCellType.Header)
                        ),
                        new If(_ => infrequentProperties.Contains(QuestionnaireResponseInfrequentProperties.Text),
                            new NarrativeCell(false, TableCellType.Header)
                        )
                    ]),
                ]),
                ..items.Select(x => new ChangeContext(x, new QuestionnaireResponseRow(infrequentProperties))),
            ],
            true
        );

        return tree.Render(navigator, renderer, context);
    }
}

public enum QuestionnaireResponseInfrequentProperties
{
    Identifier,
    Authored,
    Author,
    Source,
    Text,

    [EnumValueSet("http://hl7.org/fhir/questionnaire-answers-status")]
    Status
}