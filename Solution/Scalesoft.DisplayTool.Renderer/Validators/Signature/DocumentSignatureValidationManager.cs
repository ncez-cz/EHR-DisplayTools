using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scalesoft.DisplayTool.Shared.Configuration;
using Scalesoft.EZCAII.Client;

namespace Scalesoft.DisplayTool.Renderer.Validators.Signature;

public class DocumentSignatureValidationManager : IDocumentSignatureValidationManager
{
    private readonly SignDocumentClient m_signDocumentClient;
    private readonly ValidateDocumentClient m_validateDocumentClient;
    private readonly ExternalServicesConfiguration m_externalServicesConfiguration;
    private readonly ILogger<DocumentSignatureValidationManager> m_logger;

    public DocumentSignatureValidationManager(
        SignDocumentClient signDocumentClient,
        ValidateDocumentClient validateDocumentClient,
        ExternalServicesConfiguration externalServicesConfiguration,
        ILogger<DocumentSignatureValidationManager> logger
    )
    {
        m_signDocumentClient = signDocumentClient;
        m_validateDocumentClient = validateDocumentClient;
        m_externalServicesConfiguration = externalServicesConfiguration;
        m_logger = logger;
    }

    public async Task<DocumentSigningOperationResult> SignPdfFileAsync(byte[] document)
    {
        var config = m_externalServicesConfiguration.DocumentSignatureValidation;
        var validationResult = new List<ValidationResult>();
        var configIsValid = Validator.TryValidateObject(config, new ValidationContext(config), validationResult, true);
        if (!configIsValid)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError("Invalid configuration, missing {param}",
                    string.Join(", ", validationResult.Select(x => x.ErrorMessage)));
            }

            return DocumentSigningOperationResult.Error();
        }

        var b64Content = Convert.ToBase64String(document);
        const string fileName = "document.pdf";
        var documentContract = new RequestSignDocumentModelDto
        {
            DocumentContent = b64Content,
            DocumentType = DocumentTypeEnum.PADES,
            FileName = fileName,
            IsSignatureWithTimestamp = true,
            SourceSystem = config.ApiCallerName!, // assume these properties are not null after validation
            CertificateId = config.CertificateId!.Value,
            StorageId = config.StorageId!.Value,
        };
        if (!string.IsNullOrEmpty(config.EncryptedCertificatePassword))
        {
            documentContract.CryptedPassword = config.EncryptedCertificatePassword;
        }

        var contract = new RequestSignDocument
        {
            Authentication = new RequesAuthenticationModelDto(),
            Document = documentContract,
        };

        ResponseSignDocument response;
        try
        {
            response = await m_signDocumentClient.DocumentPOST4Async(contract);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or ApiException)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError(e, "Error while signing document");
            }

            return DocumentSigningOperationResult.Error();
        }

        if (m_logger.IsEnabled(LogLevel.Debug))
        {
            m_logger.LogDebug("Received current message during document signing: {msg}", response.Message);
        }

        if (!response.Success)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError("Error while signing document: {message}", response.ErrorCode);
            }

            return DocumentSigningOperationResult.Error();
        }

        var signedDocument = Convert.FromBase64String(response.Document.SignedDocument);

        return DocumentSigningOperationResult.Success(signedDocument, response.Document.DocumentId);
    }

    public async Task<DocumentSignatureValidationOperationResult> ValidateAdesSignatureAsync(byte[] document)
    {
        var config = m_externalServicesConfiguration.DocumentSignatureValidation;
        var validationResult = new List<ValidationResult>();
        var configIsValid = Validator.TryValidateObject(config, new ValidationContext(config), validationResult, true);
        if (!configIsValid)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError("Invalid configuration, missing {param}",
                    string.Join(", ", validationResult.Select(x => x.ErrorMessage)));
            }

            return DocumentSignatureValidationOperationResult.Error();
        }

        var b64Content = Convert.ToBase64String(document);
        const string fileName = "signed-document.pdf";

        var documentContract = new RequestValidateDocumentModelDto
        {
            SignedDocumentContent = b64Content,
            SignedFileName = fileName,
            DocumentValidationType = DocumentVerificationTypeEnum.ADES,
            SourceSystem = config.ApiCallerName!, // assume these properties are not null after validation
            StorageId = config.StorageId!.Value,
        };

        var contract = new RequestValidateDocument
        {
            Authentication = new RequesAuthenticationModelDto(),
            Document = documentContract,
        };

        ResponseValidateDocument response;
        try
        {
            response = await m_validateDocumentClient.DocumentPOST8Async(contract);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or ApiException)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError(e, "Error while validating document");
            }

            return DocumentSignatureValidationOperationResult.Error();
        }

        if (m_logger.IsEnabled(LogLevel.Debug))
        {
            m_logger.LogDebug("Received current message during document signing: {msg}", response.Message);
        }

        if (!response.Success)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError("Error while validating document: {message}", response.ErrorCode);
            }

            return DocumentSignatureValidationOperationResult.Error();
        }

        var isValid = response.Document.IsValid == DocumentSignatureValidityEnum.Valid;

        return DocumentSignatureValidationOperationResult.Success(
            isValid,
            response.Document.ReportId,
            response.Document.DocumentId
        );
    }
}