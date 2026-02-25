using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Consent.Provision;

public class Provision : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var provisionsList = new Container([
            new ItemListBuilder("f:provision", ItemListType.Unordered, (_, nav) =>
            [
                new Container([
                    // Type indicator
                    new TypeIndicator("f:type", new Dictionary<string, SupportedIcons>
                    {
                        { "permit", SupportedIcons.Check },
                        { "deny", SupportedIcons.Cross }
                    }, "http://hl7.org/fhir/consent-provision-type"),

                    // Actors
                    new ConcatBuilder("f:actor", i =>
                    [
                        new Concat([
                            new If(_ => i == 0,
                                new ConstantText(" "),
                                new EhdsiDisplayLabel(LabelCodes.For)
                            ),
                            new ChangeContext("f:role", new CodeableConcept()),
                            new ChangeContext("f:reference",
                                new TextContainer(TextStyle.Bold, [new AnyReferenceNamingWidget()]))
                        ])
                    ], ", "),
                    
                    // Purpose
                    new Condition("f:purpose",
                        new Container([
                            new Concat(
                            [
                                new ConstantText(" "), new EhdsiDisplayLabel(LabelCodes.For),
                                new ConstantText(" "),
                                new LocalizedLabel("consent.provision.purpose"),
                                new ConstantText(" "),
                            ]),
                            new CommaSeparatedBuilder("f:purpose", _ => [new Coding()])
                        ], ContainerType.Span)
                    ),

                    // Security Labels
                    new Condition("f:securityLabel",
                        new Container([
                            new ConstantText(" ["),
                            new LocalizedLabel("consent.provision.security"),
                            new ConstantText(": "),
                            new CommaSeparatedBuilder("f:securityLabel", _ => [new Coding()]),
                            new ConstantText("]")
                        ], ContainerType.Span)
                    ),

                    // Classes (resource types)
                    new Condition("f:class",
                        new Container([
                            new LocalizedLabel("consent.provision.class"),
                            new CommaSeparatedBuilder("f:class", _ => [new Coding()])
                        ], ContainerType.Span)
                    ),

                    // Actions
                    new Condition("f:action",
                        new Container([
                            new LocalizedLabel("consent.provision.action"),
                            new CommaSeparatedBuilder("f:action", _ => [new CodeableConcept()])
                        ], ContainerType.Span)
                    ),

                    // Exception codes
                    new Condition("f:code",
                        new Container([
                            new ConstantText(", "),
                            new LocalizedLabel("consent.provision.code"),
                            new ConstantText(" "),
                            new CommaSeparatedBuilder("f:code", _ => [new CodeableConcept()])
                        ], ContainerType.Span)
                    ),

                    // Period
                    new Optional("f:period",
                        new Container([
                            new ConstantText(" ("),
                            new ShowPeriod(),
                            new ConstantText(")")
                        ], ContainerType.Span)
                    ),

                    // Handle nested provisions recursively
                    new Condition("f:provision",
                        new Container([
                            new Provision()
                        ], ContainerType.Span)
                    ),
                ]),
            ]),
        ]);

        return await provisionsList.Render(navigator, renderer, context);
    }
}