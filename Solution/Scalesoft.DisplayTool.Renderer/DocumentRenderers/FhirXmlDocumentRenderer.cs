using Microsoft.Extensions.Logging;
using Scalesoft.DisplayTool.Renderer.DocumentRenderers.Tools;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Validators;
using Scalesoft.DisplayTool.Renderer.Validators.Signature;
using Scalesoft.DisplayTool.Shared.Translation;

namespace Scalesoft.DisplayTool.Renderer.DocumentRenderers;

public class FhirXmlDocumentRenderer : FhirDocumentRendererBase
{
    public FhirXmlDocumentRenderer(
        IWidgetRenderer widgetRenderer,
        DocumentValidatorProvider documentValidatorProvider,
        ILogger<FhirDocumentRendererBase> logger,
        HtmlToPdfConverter htmlToPdfConverter,
        ICodeTranslator translator,
        Language language,
        IPdfSignatureManager pdfSignatureManager,
        IFhirDocumentSignatureManager fhirDocumentSignatureManager,
        ILoggerFactory loggerFactory
    ) : base(widgetRenderer, documentValidatorProvider, logger, InputFormat.FhirXml, htmlToPdfConverter, translator,
        language, pdfSignatureManager, fhirDocumentSignatureManager, loggerFactory)
    {
    }

    protected override byte[] GetXmlBytes(byte[] fileContent)
    {
        return fileContent;
    }
}