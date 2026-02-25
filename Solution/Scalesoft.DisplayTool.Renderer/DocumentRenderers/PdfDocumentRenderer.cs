using System.Text;
using Microsoft.Extensions.Logging;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Models.Exceptions;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Validators;
using Scalesoft.DisplayTool.Renderer.Widgets;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;
using Scalesoft.DisplayTool.Shared.Translation;

namespace Scalesoft.DisplayTool.Renderer.DocumentRenderers;

public class PdfDocumentRenderer : ISpecificDocumentRenderer
{
    public InputFormat InputFormat => InputFormat.Pdf;

    private readonly IWidgetRenderer m_widgetRenderer;
    private readonly ICodeTranslator m_translator;
    private readonly Language m_language;
    private readonly ILoggerFactory m_loggerFactory;

    public PdfDocumentRenderer(IWidgetRenderer widgetRenderer, ICodeTranslator translator, Language language,
        ILoggerFactory loggerFactory)
    {
        m_widgetRenderer = widgetRenderer;
        m_translator = translator;
        m_language = language;
        m_loggerFactory = loggerFactory;
    }

    public async Task<DocumentResult> RenderAsync(
        byte[] fileContent,
        OutputFormat outputFormat,
        DocumentOptions options,
        DocumentType documentType,
        bool isEmbeddable,
        RenderMode renderMode = RenderMode.Standard,
        LevelOfDetail levelOfDetail = LevelOfDetail.Simplified
    )
    {
        if (PdfValidator.IsValidPdf(fileContent))
        {
            throw new FhirValidatorException(
                "Chyba: Dokument PDF není validní. Zkontrolujte správnost syntaxe a struktury");
        }

        if (outputFormat != OutputFormat.Html && outputFormat != OutputFormat.Pdf)
        {
            throw new NotSupportedException();
        }

        // Render PDF
        if (outputFormat == OutputFormat.Pdf)
        {
            return new DocumentResult
            {
                Content = fileContent,
                Errors = [],
                Warnings = [],
                IsRenderedSuccessfully = true
            };
        }

        var renderContext = new RenderContext(
            m_translator,
            m_language,
            m_loggerFactory,
            documentType,
            renderMode,
            options.PreferTranslationsFromDocument,
            levelOfDetail);

        // Render HTML
        var root = new XmlDocumentNavigator(null);
        root.AddNamespace("n1", "urn:hl7-org:v3");

        var pdfWidget = new PdfToHtml(fileContent);
        var renderResult = await pdfWidget.Render(root, m_widgetRenderer, renderContext);

        var htmlContent = await m_widgetRenderer.WrapWithLayout(renderResult.Content, null, renderMode);

        return new DocumentResult
        {
            Content = Encoding.UTF8.GetBytes(htmlContent),
            Errors = renderResult.Errors.Where(x => x.Severity >= ErrorSeverity.Fatal)
                .Select(x => x.Message ?? x.Kind.ToString()).ToList(),
            Warnings = renderResult.Errors.Where(x => x.Severity <= ErrorSeverity.Warning)
                .Select(x => x.Message ?? x.Kind.ToString()).ToList(),
            IsRenderedSuccessfully = renderResult.MaxSeverity is null or < ErrorSeverity.Fatal,
        };
    }
}