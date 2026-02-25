using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Scalesoft.DisplayTool.Renderer.DocumentRenderers;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using RenderMode = Scalesoft.DisplayTool.Renderer.Models.Enums.RenderMode;

namespace Scalesoft.DisplayTool.Renderer;

public class DocumentRenderer
{
    private readonly IServiceProvider m_serviceProvider;

    public DocumentRenderer(DocumentRendererOptions options)
    {
        var loggerFactory = options.LoggerFactory ?? new NullLoggerFactory();
        m_serviceProvider =
            ServicesRegistration.CreateServiceProvider(loggerFactory,
                options.PdfRenderer, options.ExternalServicesConfiguration, options.KnownOidMappings);
    }

    public DocumentRenderer(IServiceProvider serviceProvider)
    {
        m_serviceProvider = serviceProvider;
    }


    public async Task<DocumentResult> RenderDocumentationAsync(
        byte[] fileContent,
        InputFormat inputFormat,
        OutputFormat outputFormat,
        DocumentType documentType,
        LanguageOptions languageOptions
    )
    {
        using var scope = m_serviceProvider.CreateScope();

        try
        {
            var specificDocumentRenderer = GetSpecificDocumentRenderer(scope.ServiceProvider, inputFormat);
            var options = new DocumentOptions
            {
                ValidateDocument = false,
                ValidateCodeValues = false,
                ValidateDigitalSignature = false,
                PreferTranslationsFromDocument = false,
                LanguageOption = languageOptions
            };

            if (options.LanguageOption != null)
            {
                var language = scope.ServiceProvider.GetRequiredService<Language>();
                language.Primary = options.LanguageOption;
            }

            var result = await specificDocumentRenderer.RenderAsync(
                fileContent,
                outputFormat,
                options,
                documentType,
                isEmbeddable: false,
                RenderMode.Documentation,
                levelOfDetail: LevelOfDetail.Detailed);

            return result;
        }
        catch (NotSupportedException)
        {
            return UnsupportedFormatResult(inputFormat, outputFormat);
        }
    }


    public async Task<DocumentResult> RenderAsync(
        byte[] fileContent,
        InputFormat inputFormat,
        OutputFormat outputFormat,
        bool isEmbeddable,
        DocumentOptions options,
        DocumentType documentType,
        LevelOfDetail levelOfDetail = LevelOfDetail.Simplified
    )
    {
        if (outputFormat == OutputFormat.Pdf && isEmbeddable)
        {
            return UnsupportedFormatResult(inputFormat, outputFormat, "PDF output format is not embeddable");
        }

        using var scope = m_serviceProvider.CreateScope();
        try
        {
            if (options.LanguageOption != null)
            {
                var language = scope.ServiceProvider.GetRequiredService<Language>();
                language.Primary = options.LanguageOption;
            }

            var specificDocumentRenderer = GetSpecificDocumentRenderer(scope.ServiceProvider, inputFormat);
            var result = await specificDocumentRenderer.RenderAsync(
                fileContent,
                outputFormat,
                options,
                documentType: documentType,
                isEmbeddable,
                levelOfDetail: levelOfDetail);

            return result;
        }
        catch (NotSupportedException)
        {
            return UnsupportedFormatResult(inputFormat, outputFormat);
        }
    }

    private ISpecificDocumentRenderer GetSpecificDocumentRenderer(
        IServiceProvider scopeServiceProvider,
        InputFormat inputFormat
    )
    {
        var renderers = scopeServiceProvider.GetRequiredService<IEnumerable<ISpecificDocumentRenderer>>();
        var renderer = renderers.FirstOrDefault(x => x.InputFormat == inputFormat);
        return renderer ?? throw new NotSupportedException();
    }

    private DocumentResult UnsupportedFormatResult(
        InputFormat inputFormat,
        OutputFormat outputFormat,
        string? errorMessage = null
    )
    {
        return new DocumentResult
        {
            Content = [],
            IsRenderedSuccessfully = false,
            Errors = [errorMessage ?? $"Rendering {outputFormat} from {inputFormat} is not supported"],
        };
    }
}