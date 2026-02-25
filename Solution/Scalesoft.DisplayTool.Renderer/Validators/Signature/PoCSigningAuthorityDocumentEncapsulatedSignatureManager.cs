using Microsoft.Extensions.Logging;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Shared.Configuration;
using Scalesoft.DisplayTool.Shared.Translation;
using Scalesoft.DocSignAuthority.Client;

namespace Scalesoft.DisplayTool.Renderer.Validators.Signature;

public class PoCSigningAuthorityDocumentEncapsulatedSignatureManager : PoCSigningAuthorityDocumentSignatureManagerBase,
    IFhirDocumentSignatureManager
{
    private readonly Client m_client;

    public PoCSigningAuthorityDocumentEncapsulatedSignatureManager(
        Client client,
        ICodeTranslator translator,
        Language language,
        ILogger<PoCSigningAuthorityDocumentEncapsulatedSignatureManager> logger
    ) : base(translator, language, logger)
    {
        m_client = client;
    }

    public SignatureProvider SignatureProvider => SignatureProvider.PoCSigningAuthorityEncapsulated;

    protected override Task<DocumentSignatureVerificationResponseContract> VerifyAsync(
        DocumentSignContract requestContract
    )
    {
        return m_client.ApiSignatureEncapsulatedVerifyAsync(requestContract);
    }
}