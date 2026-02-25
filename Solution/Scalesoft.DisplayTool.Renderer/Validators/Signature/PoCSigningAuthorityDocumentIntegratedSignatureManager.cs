using Microsoft.Extensions.Logging;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Shared.Configuration;
using Scalesoft.DisplayTool.Shared.Translation;
using Scalesoft.DocSignAuthority.Client;

namespace Scalesoft.DisplayTool.Renderer.Validators.Signature;

public class PoCSigningAuthorityDocumentIntegratedSignatureManager : PoCSigningAuthorityDocumentSignatureManagerBase,
    IFhirDocumentSignatureManager
{
    private readonly Client m_client;

    public PoCSigningAuthorityDocumentIntegratedSignatureManager(
        Client client,
        ICodeTranslator translator,
        Language language,
        ILogger<PoCSigningAuthorityDocumentIntegratedSignatureManager> logger
    ) : base(translator, language, logger)
    {
        m_client = client;
    }

    public SignatureProvider SignatureProvider => SignatureProvider.PoCSigningAuthority;
    
    protected override Task<DocumentSignatureVerificationResponseContract> VerifyAsync(
        DocumentSignContract requestContract
    )
    {
        return m_client.ApiSignatureIntegratedVerifyAsync(requestContract);
    }
}