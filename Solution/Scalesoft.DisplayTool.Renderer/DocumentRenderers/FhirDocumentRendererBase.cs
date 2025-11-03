using System.Text.Json;
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
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;
using Scalesoft.DisplayTool.Shared.Translation;

namespace Scalesoft.DisplayTool.Renderer.DocumentRenderers;

public abstract class FhirDocumentRendererBase : SpecificDocumentRendererBase
{
    private readonly IWidgetRenderer m_widgetRenderer;
    private readonly ILogger<FhirDocumentRendererBase> m_logger;
    private readonly ICodeTranslator m_translator;
    private readonly Language m_language;
    private readonly ILoggerFactory m_loggerFactory;

    protected FhirDocumentRendererBase(
        IWidgetRenderer widgetRenderer,
        DocumentValidatorProvider documentValidatorProvider,
        ILogger<FhirDocumentRendererBase> logger,
        InputFormat inputFormat,
        HtmlToPdfConverter htmlToPdfConverter,
        ICodeTranslator translator,
        Language language,
        IDocumentSignatureValidationManager documentSignatureValidationManager,
        ILoggerFactory loggerFactory
    ) : base(documentValidatorProvider, htmlToPdfConverter, documentSignatureValidationManager)
    {
        InputFormat = inputFormat;
        m_translator = translator;
        m_language = language;
        m_loggerFactory = loggerFactory;
        m_widgetRenderer = widgetRenderer;
        m_logger = logger;
    }

    public override InputFormat InputFormat { get; }

    protected abstract byte[] GetXmlBytes(byte[] fileContent);

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
            var xmlBytes = GetXmlBytes(fileContent);

            using var stream = new MemoryStream(xmlBytes);
            using var xr = XmlReader.Create(stream);
            var document = new XPathDocument(xr, XmlSpace.Preserve);
            navigator = document.CreateNavigator();
        }
        catch (Exception ex) when (ex is JsonException || ex is XmlException)
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
        root.AddNamespace("f", "http://hl7.org/fhir");
        root.AddNamespace("xhtml", "http://www.w3.org/1999/xhtml");

        var renderContext = new RenderContext(
            m_translator,
            m_language,
            m_loggerFactory,
            documentType,
            renderMode,
            options.PreferTranslationsFromDocument,
            levelOfDetail
        );

        var nodesWithIds = root.SelectAllNodes("f:Bundle/f:entry/f:resource//*[f:id[@value]]");
        foreach (var nodeWithId in nodesWithIds)
        {
            if (renderContext.TryAddResourceWithId(nodeWithId, "f:id/@value"))
            {
                continue;
            }

            if (m_logger.IsEnabled(LogLevel.Information))
            {
                m_logger.LogInformation("Failed to add id to collection of element {xpath}", nodeWithId.GetFullPath());
            }
        }

        var composition = root.SelectSingleNode("f:Bundle/f:entry/f:resource/f:Composition");
        if (ResourceIdentifier.TryFromNavigator(composition, out var compositionId))
        {
            renderContext.AddRenderedResource(composition, compositionId, out _);
        }

        if (composition.Node == null)
        {
            return new DocumentResult
            {
                Content = [],
                Errors = ["Missing required Composition resource"],
                Warnings = [],
                IsRenderedSuccessfully = false,
            };
        }

        Widget widget = documentType switch
        {
            DocumentType.PatientSummary => new ChangeContext(composition,
                new CompositionIps()),
            DocumentType.DischargeReport => new ChangeContext(composition,
                new CompositionHdr()),
            DocumentType.ImagingOrder => new ChangeContext(composition,
                new CompositionImagingOrder()),
            DocumentType.Laboratory => new ChangeContext(
                composition,
                new CompositionLab()
            ),
            DocumentType.LaboratoryOrder => new ChangeContext(
                composition,
                new CompositionLabOrder()
            ),
            DocumentType.ImagingReport => new ChangeContext(
                composition,
                new CompositionImg()
            ),
            _ => throw new NotSupportedException($"Unknown document type: {documentType}"),
        };

        List<Widget> widgets =
        [
            widget,
            new Container(
                [
                    new LazyWidget(() =>
                        renderContext.RenderedIcons.Select(x => new RawText(IconHelper.GetOriginal(x))).ToList<Widget>()
                    ),
                ],
                optionalClass: "icon-reservoir"
            ),
        ];

        var renderResult = await widgets.RenderConcatenatedResult(root, m_widgetRenderer, renderContext);
        var validationWidget = new ValidationResult(validationResult);

        var validationRenderResult = await validationWidget.Render(root, m_widgetRenderer, renderContext);

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