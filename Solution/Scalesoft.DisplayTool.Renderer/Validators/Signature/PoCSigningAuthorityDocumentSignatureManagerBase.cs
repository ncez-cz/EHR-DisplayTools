using Microsoft.Extensions.Logging;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Widgets;
using Scalesoft.DisplayTool.Shared.Signature;
using Scalesoft.DisplayTool.Shared.Translation;
using Scalesoft.DocSignAuthority.Client;

namespace Scalesoft.DisplayTool.Renderer.Validators.Signature;

public abstract class PoCSigningAuthorityDocumentSignatureManagerBase(
    ICodeTranslator translator,
    Language language,
    ILogger logger
)
{
    private readonly TranslatorWrapper m_translator = new(translator);

    protected abstract Task<DocumentSignatureVerificationResponseContract> VerifyAsync(
        DocumentSignContract requestContract
    );

    public async Task<DocumentSignatureValidationOperationResult?> ValidateSignatureAsync(
        byte[] document,
        FhirDocumentFormat fhirDocumentFormat
    )
    {
        var b64Document = Convert.ToBase64String(document);
        var requestContract = new DocumentSignContract
        {
            Base64Document = b64Document,
            DocumentType = Map(fhirDocumentFormat),
        };
        try
        {
            var response = await VerifyAsync(requestContract);
            return DocumentSignatureValidationOperationResult.Success(response.IsValid, response.SignedAt,
                response.Signor?.DisplayElement?.Value);
        }
        catch (ApiException<DocumentSignatureVerificationErrorContract> e)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(e, "Validate FHIR document signature operation failed");
            }

            var errorLocalizationCode = GetValidationErrorLabel(e.Result);
            var errorLocalizationValue = await GetLocalizedLabelValueAsync(errorLocalizationCode);

            return DocumentSignatureValidationOperationResult.Error(errorLocalizationValue);
        }
        catch (ApiException e)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(e, "Verify FHIR document signature operation failed");
            }

            var genericError = await GetLocalizedGenericErrorAsync();
            return DocumentSignatureValidationOperationResult.Error(genericError);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(e, "Verify FHIR document signature operation failed");
            }

            var genericError = await GetLocalizedGenericErrorAsync();
            return DocumentSignatureValidationOperationResult.Error(genericError);
        }
    }

    private DocumentTypeContract Map(FhirDocumentFormat src)
    {
        return src switch
        {
            FhirDocumentFormat.FhirXml => DocumentTypeContract.FhirXml,
            FhirDocumentFormat.FhirJson => DocumentTypeContract.FhirJson,
            _ => throw new ArgumentOutOfRangeException(nameof(src), src, null)
        };
    }

    private string GetValidationErrorLabel(DocumentSignatureVerificationErrorContract error)
    {
        return error.ErrorCode switch
        {
            ErrorCodeContract.NoSignature => "signature.validate-error-no-signature",
            ErrorCodeContract.UnsupportedDocumentType => "signature.validate-error-unsupported-document-type",
            ErrorCodeContract.Other => "signature.validate-error-other",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private async Task<string> GetLocalizedLabelValueAsync(string code)
    {
        var translated = await m_translator.GetCodedValue(
            code,
            LocalizedLabel.Url,
            language.Primary.Code,
            language.Fallback.Code
        );

        return translated ?? code;
    }

    private async Task<string> GetLocalizedGenericErrorAsync()
    {
        return await GetLocalizedLabelValueAsync("signature.validate-error-other");
    }
}