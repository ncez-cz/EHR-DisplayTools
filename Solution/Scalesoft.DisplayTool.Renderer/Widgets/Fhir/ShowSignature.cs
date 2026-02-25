using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ShowSignature(string path = "f:signature/") : Widget
{
    private string m_path = path;

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        if (!m_path.EndsWith('/'))
        {
            m_path += "/";
        }

        var mimetype = navigator.SelectSingleNode($"{m_path}f:sigFormat/@value").Node?.InnerXml;
        var data = navigator.SelectSingleNode($"{m_path}f:data//@value").Node?.InnerXml;
        var result = new Container([
            new If(_ => navigator.EvaluateCondition($"{m_path}f:who"), new Heading(
                [new AnyReferenceNamingWidget($"{m_path}f:who", showOptionalDetails: false)],
                HeadingSize.H5)),
            new If(_ => navigator.EvaluateCondition($"{m_path}f:type"), new NameValuePair(
                [new LocalizedLabel("signature.type")],
                [new CommaSeparatedBuilder($"{m_path}f:type", _ => [new Coding()])])),
            new If(_ => navigator.EvaluateCondition($"{m_path}f:when"), new NameValuePair(
                [new LocalizedLabel("signature.when")],
                [new ShowDateTime($"{m_path}f:when")])),
            new If(_ => navigator.EvaluateCondition($"{m_path}f:onBehalfOf"), new NameValuePair(
                [new LocalizedLabel("signature.onBehalfOf")],
                [new AnyReferenceNamingWidget($"{m_path}f:onBehalfOf")])),
            new If(_ => navigator.EvaluateCondition($"{m_path}f:data"), new Container([
                new Binary(width: "200px", mimeType: mimetype, base64data: data),
            ], idSource: new IdentifierSource())),
        ]);

        return result.Render(navigator, renderer, context);
    }
}