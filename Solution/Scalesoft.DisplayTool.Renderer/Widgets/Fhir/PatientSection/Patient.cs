using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.PatientSection;

public class Patient(XmlDocumentNavigator navigator) : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator _,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<ParseError> errors = [];
        var configManager = new ResourceConfiguration();
        var selectionRulesParseResult = configManager.ProcessConfigurations(navigator);
        errors.AddRange(selectionRulesParseResult.Errors);

        var narrativeModal = new NarrativeModal();
        var sectionContent = new List<Widget>();
        sectionContent.AddRange([
            new PatientDetails(),
            new CoreActors(),
            new NarrativeCollapser(),
        ]);

        List<Widget> tree =
        [
            new Section(".", null, [new EhdsiDisplayLabel(LabelCodes.Patient)], sectionContent,
                [
                    new Concat([
                        new HumanName("f:name", unformattedName: true),
                        new Condition("f:birthDate", new ConstantText(" ")),
                        new ShowDateTime("f:birthDate"),
                    ])
                ], idSource: navigator, titleAbbreviations: ("P", "P"),
                narrativeModal: narrativeModal
            ),
        ];

        var result = await tree.RenderConcatenatedResult(navigator, renderer, context);
        errors.AddRange(result.Errors);

        if (!result.HasValue || errors.MaxSeverity() >= ErrorSeverity.Fatal)
        {
            return errors;
        }

        return new RenderResult(result.Content, errors);
    }
}