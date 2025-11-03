using System.Text;
using Scalesoft.DisplayTool.Renderer.DocumentRenderers.Tools;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Validators;
using Scalesoft.DisplayTool.Renderer.Validators.Signature;

namespace Scalesoft.DisplayTool.Renderer.DocumentRenderers;

public abstract class SpecificDocumentRendererBase : ISpecificDocumentRenderer
{
    private readonly DocumentValidatorProvider m_documentValidatorProvider;
    private readonly HtmlToPdfConverter m_htmlToPdfConverter;
    private readonly IDocumentSignatureValidationManager m_documentSignatureValidationManager;

    public abstract InputFormat InputFormat { get; }

    public abstract Task<DocumentResult> RenderAsync(
        byte[] fileContent,
        OutputFormat outputFormat,
        DocumentOptions options,
        DocumentType documentType,
        RenderMode renderMode = RenderMode.Standard,
        LevelOfDetail levelOdDetail = LevelOfDetail.Simplified
    );

    protected SpecificDocumentRendererBase(
        DocumentValidatorProvider documentValidatorProvider,
        HtmlToPdfConverter htmlToPdfConverter,
        IDocumentSignatureValidationManager documentSignatureValidationManager
    )
    {
        m_documentValidatorProvider = documentValidatorProvider;
        m_htmlToPdfConverter = htmlToPdfConverter;
        m_documentSignatureValidationManager = documentSignatureValidationManager;
    }

    protected IDocumentValidator GetValidator()
    {
        return m_documentValidatorProvider.GetValidator(InputFormat);
    }

    protected async Task<OutputDocumentModel> CreateOutputDocumentAsync(
        byte[] fileContent,
        string htmlContent,
        OutputFormat outputFormat
    )
    {
        byte[] renderedDocumentContent;
        string? errorMessage = null;
        switch (outputFormat)
        {
            case OutputFormat.Html:
                renderedDocumentContent = Encoding.UTF8.GetBytes(htmlContent);
                break;
            case OutputFormat.Pdf:
            {
                renderedDocumentContent =
                    await m_htmlToPdfConverter.ConvertHtmlToPdf(htmlContent, fileContent, InputFormat);
                var pdfSignResult =
                    await m_documentSignatureValidationManager.SignPdfFileAsync(renderedDocumentContent);
                if (pdfSignResult.OperationSuccess)
                {
                    renderedDocumentContent = pdfSignResult.SignedDocument;
                }
                else
                {
                    errorMessage = "Failed to sign the document";
                }

                break;
            }
            default:
                throw new NotSupportedException($"Unsupported output format: {outputFormat}");
        }

        return new OutputDocumentModel(renderedDocumentContent, errorMessage);
    }

    protected DocumentResult CreateResultForError(string errorMessage)
    {
        return new DocumentResult
        {
            Content = [],
            Errors = [errorMessage],
            Warnings = new List<string>(),
            IsRenderedSuccessfully = false,
        };
    }
}

public class OutputDocumentModel
{
    public OutputDocumentModel(byte[] content, string? error = null)
    {
        Content = content;
        Error = error;
    }

    public byte[] Content { get; set; }
    public string? Error { get; set; }
}