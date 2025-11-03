using System.Xml;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;
using Scalesoft.DisplayTool.Renderer.DocumentRenderers.Tools;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Validators;
using Scalesoft.DisplayTool.Renderer.Validators.Signature;
using Scalesoft.DisplayTool.Renderer.Widgets;
using Scalesoft.DisplayTool.Renderer.Widgets.Cda;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;
using Scalesoft.DisplayTool.Shared.Translation;

namespace Scalesoft.DisplayTool.Renderer.DocumentRenderers;

public class CdaDocumentRenderer : SpecificDocumentRendererBase
{
    public override InputFormat InputFormat => InputFormat.Cda;

    private readonly IWidgetRenderer m_widgetRenderer;
    private readonly ILogger<CdaDocumentRenderer> m_logger;
    private readonly ICodeTranslator m_translator;
    private readonly Language m_language;
    private readonly ILoggerFactory m_loggerFactor;

    public CdaDocumentRenderer(
        IWidgetRenderer widgetRenderer,
        DocumentValidatorProvider documentValidatorProvider,
        ILogger<CdaDocumentRenderer> logger,
        HtmlToPdfConverter htmlToPdfConverter,
        ICodeTranslator translator,
        Language language,
        IDocumentSignatureValidationManager documentSignatureValidationManager,
        ILoggerFactory loggerFactor
    ) : base(documentValidatorProvider, htmlToPdfConverter, documentSignatureValidationManager)
    {
        m_widgetRenderer = widgetRenderer;
        m_logger = logger;
        m_translator = translator;
        m_language = language;
        m_loggerFactor = loggerFactor;
    }

    public override async Task<DocumentResult> RenderAsync(
        byte[] fileContent,
        OutputFormat outputFormat,
        DocumentOptions options,
        DocumentType documentType,
        RenderMode renderMode = RenderMode.Standard,
        LevelOfDetail levelOfDetail = LevelOfDetail.Simplified
    )
    {
        if (outputFormat != OutputFormat.Html && outputFormat != OutputFormat.Pdf)
        {
            throw new NotSupportedException();
        }

        var validationResult = new ValidationResultModel();
        if (options.ValidateDocument)
        {
            validationResult = await GetValidator().ValidateDocumentAsync(fileContent, null);
            if (validationResult.ErrorMessage != null)
            {
                return CreateResultForError(validationResult.ErrorMessage);
            }
        }

        XPathNavigator? navigator = null;
        try
        {
            using var stream = new MemoryStream(fileContent);
            using var xr = XmlReader.Create(stream);
            var document = new XPathDocument(xr, XmlSpace.Preserve);
            navigator = document.CreateNavigator();
        }
        catch (XmlException ex)
        {
            var message = "Došlo k chybě při zpracovávání dokumentu";
            m_logger.LogError(ex, message);
            return new DocumentResult
            {
                Content = Array.Empty<byte>(),
                Errors = [message],
                Warnings = [],
                IsRenderedSuccessfully = false,
            };
        }

        var root = new XmlDocumentNavigator(navigator);
        root.AddNamespace("n1", "urn:hl7-org:v3");
        root.AddNamespace("ns1", "urn:hl7-org:v3");
        root.AddNamespace("fhir", "http://hl7.org/fhir");
        root.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
        root.AddNamespace("pharm", "urn:hl7-org:pharm");

        var context = new RenderContext(
            m_translator,
            m_language,
            m_loggerFactor,
            documentType,
            renderMode,
            options.PreferTranslationsFromDocument,
            levelOfDetail);

        List<Widget> widgets =
        [
            new RootWidget(),
            new Container(
                new LazyWidget(() =>
                    context.RenderedIcons.Select(x => new RawText(IconHelper.GetOriginal(x))).ToList<Widget>()),
                optionalClass: "icon-reservoir")
        ];
        var validationWidget = new ValidationResult(validationResult);
        var validationRenderResult = await validationWidget.Render(root, m_widgetRenderer, context);

        var renderResult = await widgets.RenderConcatenatedResult(root, m_widgetRenderer, context);
        var htmlContent =
            await m_widgetRenderer.WrapWithLayout(renderResult.Content, validationRenderResult.Content, renderMode);
        var renderedDocumentContent = await CreateOutputDocumentAsync(fileContent, htmlContent, outputFormat);
        var errors = renderResult.Errors.Where(x => x.Severity >= ErrorSeverity.Fatal)
            .Select(x => x.Message ?? x.Kind.ToString()).ToList();
        if (!string.IsNullOrEmpty(renderedDocumentContent.Error))
        {
            errors.Add(renderedDocumentContent.Error);
        }

        var documentResult = new DocumentResult
        {
            Content = renderedDocumentContent.Content,
            Errors = errors,
            Warnings = renderResult.Errors.Where(x => x.Severity <= ErrorSeverity.Warning)
                .Select(x => x.Message ?? x.Kind.ToString()).ToList(),
            IsRenderedSuccessfully = renderResult.MaxSeverity is null or < ErrorSeverity.Fatal,
        };

        return documentResult;
    }
}