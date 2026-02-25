using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;

public class EncounterSection(XmlDocumentNavigator navigator, LocalizedLabel title) : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator _, 
        IWidgetRenderer renderer,
        RenderContext context)
    {
        if (context.DocumentType != DocumentType.EmsReport && context.DocumentType != DocumentType.DischargeReport)
        {
            return RenderResult.NullResult;
        }
        
        var section = new Section(
            ".",
            null,
            [title],
            [
                new Choose([
                        new When("f:type",
                            new ChangeContext("f:type",
                                new Row([
                                    new Heading([new CodeableConcept()],
                                        HeadingSize.H5,
                                        customClass: "blue-color"
                                    ),
                                ])
                            )
                        ),
                        new When("f:serviceType",
                            new ChangeContext("f:serviceType",
                                new Row([
                                    new Heading([new CodeableConcept()],
                                        HeadingSize.H5,
                                        customClass: "blue-color"
                                    ),
                                ])
                            )
                        )
                    ],
                    new ChangeContext("f:class",
                        new Row([
                            new Heading([new Coding()],
                                HeadingSize.H5,
                                customClass: "blue-color"
                            ),
                        ])
                    )
                ),
                
                new Condition("f:reasonCode or f:reasonReference",
                    new Row([
                        new Heading([
                            new Concat([
                                new Condition("f:reasonCode",
                                    new CommaSeparatedBuilder("f:reasonCode", _ => new CodeableConcept())),
                                new Condition("f:reasonReference",
                                    new CommaSeparatedBuilder("f:reasonReference",
                                        _ => new AnyReferenceNamingWidget())),
                            ]),
                        ], size: HeadingSize.H6),
                    ])
                ),
            ],
            idSource: navigator
        );
        
        return await section.Render(navigator, renderer, context);
    }
}