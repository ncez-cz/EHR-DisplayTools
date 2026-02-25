using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

/// <summary>
///     This is an element, not a resource.
/// </summary>
public class RelatedArtifact : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties = InfrequentProperties.Evaluate<RelatedArtifactInfrequentProperties>(navigator);

        var url = navigator.SelectSingleNode("f:url/@value").Node?.Value ?? "";

        var card =
            new Card(null,
                new Concat([
                    new NameValuePair(
                        new LocalizedLabel("related-artifact.type"),
                        new EnumLabel("f:type", "http://hl7.org/fhir/ValueSet/related-artifact-type")
                    ),
                    infrequentProperties.Optional(RelatedArtifactInfrequentProperties.Label,
                        new NameValuePair(
                            new LocalizedLabel("related-artifact.label"),
                            new Text("@value")
                        )
                    ),
                    infrequentProperties.Optional(RelatedArtifactInfrequentProperties.Display,
                        new NameValuePair(
                            new LocalizedLabel("related-artifact.display"),
                            new Text("@value")
                        )
                    ),
                    infrequentProperties.Optional(RelatedArtifactInfrequentProperties.Citation,
                        new NameValuePair(
                            new LocalizedLabel("related-artifact.citation"),
                            new Markdown("@value")
                        )
                    ),
                    infrequentProperties.Optional(RelatedArtifactInfrequentProperties.Url,
                        new NameValuePair(
                            new LocalizedLabel("related-artifact.url"),
                            new Link(new Text("@value"), url)
                        )
                    ),
                    infrequentProperties.Optional(RelatedArtifactInfrequentProperties.Document,
                        new NameValuePair(
                            new LocalizedLabel("related-artifact.document"),
                            new Attachment()
                        )
                    ),
                    infrequentProperties.Optional(RelatedArtifactInfrequentProperties.Resource,
                        new NameValuePair(
                            new LocalizedLabel("related-artifact.resource"),
                            new AnyReferenceNamingWidget()
                        )
                    ),
                ])
            );

        return card.Render(navigator, renderer, context);
    }
}

public enum RelatedArtifactInfrequentProperties
{
    Label,
    Display,
    Citation,
    Url,
    Document,
    Resource,
}