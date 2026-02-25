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
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;
using Scalesoft.DisplayTool.Shared.Translation;

namespace Scalesoft.DisplayTool.Renderer.DocumentRenderers;

public abstract class FhirDocumentRendererBase : SpecificDocumentRendererBase
{
    private readonly IWidgetRenderer m_widgetRenderer;
    private readonly ILogger<FhirDocumentRendererBase> m_logger;
    private readonly ICodeTranslator m_translator;
    private readonly Language m_language;
    private readonly IFhirDocumentSignatureManager m_fhirDocumentSignatureManager;
    private readonly ILoggerFactory m_loggerFactory;

    protected FhirDocumentRendererBase(
        IWidgetRenderer widgetRenderer,
        DocumentValidatorProvider documentValidatorProvider,
        ILogger<FhirDocumentRendererBase> logger,
        InputFormat inputFormat,
        HtmlToPdfConverter htmlToPdfConverter,
        ICodeTranslator translator,
        Language language,
        IPdfSignatureManager pdfSignatureManager,
        IFhirDocumentSignatureManager fhirDocumentSignatureManager,
        ILoggerFactory loggerFactory
    ) : base(documentValidatorProvider, htmlToPdfConverter, pdfSignatureManager)
    {
        InputFormat = inputFormat;
        m_translator = translator;
        m_language = language;
        m_fhirDocumentSignatureManager = fhirDocumentSignatureManager;
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
        bool isEmbeddable,
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

        var documentTypeForSignatureValidation = MapDocumentType(InputFormat);

        var signatureValidationResult =
            await m_fhirDocumentSignatureManager.ValidateSignatureAsync(fileContent,
                documentTypeForSignatureValidation);

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
            var message = "An error occurred while processing the document.";

            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError(ex, message);
            }

            return new DocumentResult
            {
                Content = [],
                Errors = [message, ex.Message],
                Warnings = [],
                IsRenderedSuccessfully = false,
            };
        }

        var root = new XmlDocumentNavigator(navigator);
        AddFhirNamespaces(root);

        var hasProvenanceWithSignature = root.EvaluateCondition("f:Bundle/f:entry/f:resource/f:Provenance/f:signature");
        if (hasProvenanceWithSignature)
        {
            var provenanceTarget =
                ReferenceHandler.GetSingleNodeNavigatorFromReference(root,
                    "f:Bundle/f:entry/f:resource/f:Provenance[1]/f:target[1]", ".");
            var firstInnerBundle = root.SelectSingleNode("f:Bundle/f:entry/f:resource/f:Bundle[1]");
            if (provenanceTarget?.Node?.ComparePosition(firstInnerBundle.Node) == XmlNodeOrder.Same)
            {
                var sr = new StringReader(firstInnerBundle.Node?.OuterXml ?? string.Empty);
                var newDoc = new XPathDocument(sr);
                var newNav = newDoc.CreateNavigator();
                root = new XmlDocumentNavigator(newNav);
                AddFhirNamespaces(root);
            }
        }

        if (signatureValidationResult != null)
        {
            root.SignatureValidationResult = signatureValidationResult;
        }

        var renderContext = new RenderContext(
            m_translator,
            m_language,
            m_loggerFactory,
            documentType,
            renderMode,
            options.PreferTranslationsFromDocument,
            levelOfDetail
        );

        var subjectId = root.SelectSingleNode("f:Bundle/f:entry/f:resource/f:Composition/f:subject/f:reference/@value")
            .Node?.InnerXml;
        if (subjectId != null)
        {
            root.CompositionSubjectId = subjectId;
        }

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

        var resource = root.SelectSingleNode("/f:Bundle/f:entry[1]/f:resource/*[1]");
        if (resource.Node == null)
        {
            // Try rendering a raw resource without a composition
            resource = root.SelectSingleNode("/*[1]");
            if (resource.Node == null)
            {
                return new DocumentResult
                {
                    Content = [],
                    Errors = ["No resource found."],
                    Warnings = [],
                    IsRenderedSuccessfully = false,
                };
            }
        }

        if (ResourceIdentifier.TryFromNavigator(resource, out var compositionId))
        {
            renderContext.AddRenderedResource(resource, compositionId, out _);
        }

        if (documentType != DocumentType.AnyBundle && resource.Node.Name != "Composition")
        {
            return new DocumentResult
            {
                Content = [],
                Errors = ["Missing required Composition resource."],
                Warnings = [],
                IsRenderedSuccessfully = false,
            };
        }

        Widget widget;

        // If we're dealing with a raw resource without a composition, render it as a generic resource widget
        if (resource.Node.Name != "Composition" && resource.Node.Name != "Bundle")
        {
            widget = new Concat([new FhirHeader(), new AnyResource(resource)]);
        }
        else
        {
            widget = documentType switch
            {
                DocumentType.PatientSummary => new ChangeContext(resource, new CompositionIps()),
                DocumentType.DischargeReport => new ChangeContext(resource, new CompositionHdr()),
                DocumentType.ImagingOrder => new ChangeContext(resource, new CompositionImagingOrder()),
                DocumentType.Laboratory => new ChangeContext(resource, new CompositionLab()),
                DocumentType.LaboratoryOrder => new ChangeContext(resource, new CompositionLabOrder()),
                DocumentType.ImagingReport => new ChangeContext(resource, new CompositionImg()),
                DocumentType.AnyBundle => new ChangeContext(resource, new AnyBundle()),
                DocumentType.EmsReport => new ChangeContext(resource, new CompositionEms()),
                _ => throw new NotSupportedException($"Unknown document type: {documentType}"),
            };
        }

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
            await m_widgetRenderer.WrapWithLayout(renderResult.Content, validationRenderResult.Content, renderMode,
                isEmbeddable);

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

    private FhirDocumentFormat MapDocumentType(InputFormat inputFormat)
    {
        switch (inputFormat)
        {
            case InputFormat.FhirXml:
                return FhirDocumentFormat.FhirXml;
            case InputFormat.FhirJson:
                return FhirDocumentFormat.FhirJson;
            default:
                throw new ArgumentOutOfRangeException(nameof(inputFormat), inputFormat, null);
        }
    }

    private void AddFhirNamespaces(XmlDocumentNavigator root)
    {
        root.AddNamespace("f", "http://hl7.org/fhir");
        root.AddNamespace("xhtml", "http://www.w3.org/1999/xhtml");
    }
}