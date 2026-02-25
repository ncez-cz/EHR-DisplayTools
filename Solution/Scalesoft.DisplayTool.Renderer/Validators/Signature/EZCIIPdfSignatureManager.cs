using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Scalesoft.DisplayTool.Renderer.ConfigurationUtils;
using Scalesoft.DisplayTool.Shared.Configuration;
using Scalesoft.EZCAII.Client;

namespace Scalesoft.DisplayTool.Renderer.Validators.Signature;

public class EZCIIPdfSignatureManager : IPdfSignatureManager
{
    private readonly SignDocumentClient m_signDocumentClient;
    private readonly ExternalServicesConfiguration m_externalServicesConfiguration;
    private readonly ILogger<EZCIIPdfSignatureManager> m_logger;

    public EZCIIPdfSignatureManager(
        SignDocumentClient signDocumentClient,
        ExternalServicesConfiguration externalServicesConfiguration,
        ILogger<EZCIIPdfSignatureManager> logger
    )
    {
        m_signDocumentClient = signDocumentClient;
        m_externalServicesConfiguration = externalServicesConfiguration;
        m_logger = logger;
    }

    public SignatureProvider SignatureProvider => SignatureProvider.EZCAII;

    public async Task<DocumentSigningOperationResult> SignPdfFileAsync(byte[] document)
    {
        if (!ConfigValidator.TryGetValidConfig(
                m_externalServicesConfiguration.DocumentSignature.EZCAIIConfiguration, m_logger, out var config))
        {
            return DocumentSigningOperationResult.Error(null);
        }

        var b64Content = Convert.ToBase64String(document);
        const string fileName = "document.pdf";
        var documentContract = new RequestSignDocumentModelDto
        {
            DocumentContent = b64Content,
            DocumentType = DocumentTypeEnum.PADES,
            FileName = fileName,
            IsSignatureWithTimestamp = true,
            SourceSystem = config.ApiCallerName,
            CertificateId = config.CertificateId,
            StorageId = config.StorageId,
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

            return DocumentSigningOperationResult.Error(null);
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

            return DocumentSigningOperationResult.Error(null);
        }

        var signedDocument = Convert.FromBase64String(response.Document.SignedDocument);

        return DocumentSigningOperationResult.Success(signedDocument);
    }
}