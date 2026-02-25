using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using DisplayTool.DocSignAuthority.Service.Exceptions;
using DisplayTool.DocSignAuthority.Service.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Jose;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisplayTool.DocSignAuthority.Service.FhirManipulation;

public class JsonDocumentFormatManipulator : IDocumentFormatManipulator
{
    public DocumentType Type => DocumentType.FhirJson;

    private readonly IOptions<SigningConfiguration> m_signingConfiguration;
    private readonly ILogger<JsonDocumentFormatManipulator> m_logger;

    public JsonDocumentFormatManipulator(
        IOptions<SigningConfiguration> signingConfiguration,
        ILoggerFactory loggerFactory
    )
    {
        m_signingConfiguration = signingConfiguration;
        m_logger = loggerFactory.CreateLogger<JsonDocumentFormatManipulator>();
    }

    public RootResourceResult? SelectRootElement(string documentContent)
    {
        var doc = LoadJsonObject(documentContent, "Failed to select root element");

        var arr = new JArray(doc); // wrap in array to allow filtering with JSONPath
        var rootResource = arr.SelectToken("$[?(@.resourceType !== '')]");
        if (rootResource == null)
        {
            return null;
        }

        var id = rootResource.SelectToken("id")?.ToString();
        var resourceType = rootResource.SelectToken("resourceType")?.ToString();
        if (resourceType == null)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError("Failed to select root element resource type. Invalid FHIR JSON document.");
            }

            throw new DocumentSignatureException("Invalid FHIR JSON document.");
        }

        return new RootResourceResult
        {
            ResourceName = resourceType,
            Content = rootResource.ToString(),
            Id = id,
        };
    }

    public Signature GetSignature(
        string payload,
        string canonicalizationUrl,
        RSA privateKey,
        X509Certificate2 certificate,
        DateTime signingTime
    )
    {
        var signConfig = m_signingConfiguration.Value;

        //see https://build.fhir.org/ig/HL7/davinci-ecdx/signed-document-bundle-example.html for specification/guideline
        const string signatureCommitmentSystem = "urn:iso-astm:E1762-95:2013";
        const string signatureCommitmentId = "1.2.840.10065.1.12.1.5";
        const string signatureCommitmentDescription = "Verification Signature";
        const string signatureCommitmentQualifyingInfo = "Verification of medical record integrity";

        var cert = Convert.ToBase64String(certificate.RawData);
        var certChain = new[] { cert };
        var timestamp = signingTime.ToString("O");
        var signerCommitments = new[]
        {
            new SignersCommitment
            {
                CommitmentIdentifier = new CommitmentId
                    { Id = signatureCommitmentId, Description = signatureCommitmentDescription },
                CommitmentQualifications = [signatureCommitmentQualifyingInfo],
            }
        };
        var headers = new Dictionary<string, object>
        {
            { "kty", "RS" },
            { "srCms", signerCommitments },
            { "sigT", timestamp },
            { "x5c", certChain },
        };

        using (privateKey)
        {
            var token = JWT.Encode(payload, privateKey, JwsAlgorithm.RS256, extraHeaders: headers);

            var splitToken = token.Split('.');
            splitToken[1] = string.Empty; // remove payload for a detached signature
            token = string.Join(".", splitToken);

            var tokenBytes = Encoding.UTF8.GetBytes(token);

            var signature = new Signature
            {
                Type = [new Coding(signatureCommitmentSystem, signatureCommitmentId, signatureCommitmentDescription)],
                When = new DateTimeOffset(signingTime),
                Who = new ResourceReference
                {
                    Display = signConfig.SignorDisplay, Identifier = new Identifier(null, certificate.SubjectName.Name)
                },
                TargetFormat = $"application/fhir+json;canonicalization={canonicalizationUrl}",
                SigFormat = "application/jose",
                Data = tokenBytes,
            };

            return signature;
        }
    }

    public string ReplaceRootBundlePlaceholder(string rootResourceContent, string bundleIdLookup, Bundle resultBundle)
    {
        var fhirJson = FhirJsonSerializer.Default.SerializeToString(resultBundle);
        var doc = LoadJsonObject(fhirJson, "Failed to parse serialized FHIR");
        var replacingToken = LoadJsonObject(rootResourceContent, "Failed to parse resource being signed");

        var arr = new JArray(doc); // wrap in array to allow filtering with JSONPath
        var placeholderBundle =
            arr.SelectToken(
                $"$[?(@.resourceType == 'Bundle')].entry[?(@.resource.resourceType == 'Bundle' && @.resource.id == '{bundleIdLookup}')].resource");
        if (placeholderBundle == null)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError("Failed to compose the result document - unable to find the placeholder Bundle.");
            }

            throw new DocumentSignatureException("Failed to compose the result document.");
        }

        placeholderBundle.Replace(replacingToken);

        return doc.ToString();
    }

    public string? GetEncapsulatedBundleSignature(string documentContent)
    {
        var doc = LoadJsonObject(documentContent, "Failed to parse serialized FHIR");

        var arr = new JArray(doc); // wrap in array to allow filtering with JSONPath

        var provenanceSignature =
            arr.SelectToken(
                "$[?(@.resourceType == 'Bundle')].entry[?(@.resource.resourceType == 'Provenance')].resource.signature[0]");

        return provenanceSignature?.ToString();
    }

    public string? GetEncapsulatedBundleProvenanceTarget(string documentContent)
    {
        var doc = LoadJsonObject(documentContent, "Failed to parse serialized FHIR provenance target");

        var arr = new JArray(doc); // wrap in array to allow filtering with JSONPath

        var provenanceTargetValue =
            arr.SelectToken(
                "$[?(@.resourceType == 'Bundle')].entry[?(@.resource.resourceType == 'Provenance')].resource.target[0].reference");
        if (provenanceTargetValue == null)
        {
            return null;
        }

        var provenanceTarget = provenanceTargetValue.ToString().Split('/');
        var targetResourceType = provenanceTarget[0];
        var targetId = provenanceTarget[1];

        var targetResource = arr.SelectToken(
            $"$[?(@.resourceType == 'Bundle')].entry[?(@.resource.resourceType == '{targetResourceType}' && @.resource.id == '{targetId}')].resource");

        return targetResource?.ToString();
    }

    public DocumentSignatureVerificationResult Validate(string signatureTarget, string signatureContent)
    {
        Signature signatureParsed;
        try
        {
            signatureParsed = FhirJsonDeserializer.DEFAULT.Deserialize<Signature>(signatureContent);
        }
        catch (DeserializationFailedException e)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError(e, "Failed to parse serialized FHIR signature. Invalid JSON document.");
            }

            throw new DocumentSignatureException("Invalid JSON document.");
        }

        var jwtBytes = signatureParsed.Data;
        if (jwtBytes == null)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError(
                    "Failed to validate FHIR JSON signature. Unable to get base64 decoded signature data.");
            }

            throw new DocumentSignatureException("Invalid signature.");
        }

        var decodedToken = Encoding.UTF8.GetString(jwtBytes);
        var splitToken = decodedToken.Split('.');
        splitToken[1] = Convert.ToBase64String(Encoding.UTF8.GetBytes(signatureTarget)); // re-add payload
        decodedToken = string.Join(".", splitToken);
        IDictionary<string, object>? jwtHeaders;
        try
        {
            jwtHeaders = JWT.Headers(decodedToken);
        }
        catch (FormatException e)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError(e,
                    "Failed to validate FHIR JSON signature. Unable to get base64 decoded signature data.");
            }

            throw new DocumentSignatureException("Invalid signature.");
        }

        if (!jwtHeaders.TryGetValue("x5c", out var certChain))
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError(
                    "Failed to validate FHIR JSON signature. Unable to get x5c JWT header with a public key.");
            }

            throw new DocumentSignatureException("Invalid signature.");
        }

        var certs = certChain as List<object>;
        if (certs?.FirstOrDefault() is not string certB64)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError(
                    "Failed to validate FHIR JSON signature. Unable to get public key data from x5c JWT header.");
            }

            throw new DocumentSignatureException("Invalid signature.");
        }

        var cert = new X509Certificate2(Convert.FromBase64String(certB64));
        try
        {
            JWT.Verify(decodedToken, cert.GetRSAPublicKey());

            return new DocumentSignatureVerificationResult
            {
                IsValid = true,
                SignedAt = signatureParsed.When,
                Signor = signatureParsed.Who as ResourceReference,
            };
        }
        catch (IntegrityException e)
        {
            if (m_logger.IsEnabled(LogLevel.Information))
            {
                m_logger.LogInformation(e, "Signature is not valid.");
            }

            return new DocumentSignatureVerificationResult
            {
                IsValid = false,
                SignedAt = signatureParsed.When,
                Signor = signatureParsed.Who as ResourceReference,
            };
        }
        catch (Exception e) when (e is JoseException or InvalidAlgorithmException)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError(
                    "Failed to validate FHIR JSON signature. Invalid JWT signature value.");
            }

            throw new DocumentSignatureException("Invalid signature.");
        }
    }

    public IntegratedSignaturePreprocessResult SelectIntegratedSignatureResourceParts(string rootResourceContent)
    {
        var doc = LoadJsonObject(rootResourceContent, "Failed to pre-process resource being signed - remove id, meta");

        var arr = new JArray(doc); // wrap in array to allow filtering with JSONPath
        var rootResource = arr.SelectToken("$[?(@.resourceType !== '')]");
        if (rootResource is not JObject rootResourceObject)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError(
                    "Failed to pre-process resource being signed - remove id, meta. Failed to find root resource.");
            }

            throw new DocumentSignatureException("Invalid FHIR JSON document.");
        }

        var idToken = rootResourceObject.Property("id");
        var idTokenValue = (idToken?.Value as JValue)?.Value;
        if (idTokenValue is not string idString || string.IsNullOrEmpty(idString))
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError(
                    "Failed to pre-process resource being signed. Root resource has no id value.");
            }

            throw new DocumentSignatureException("Invalid FHIR JSON document.");
        }

        var metaToken = rootResourceObject.Property("meta");
        var provenanceToken =
            rootResourceObject.SelectToken(
                $"$.entry[?(@.resource.resourceType == 'Provenance' && @.resource.target[0].reference == 'Bundle/{idString}')]");
        if (idToken != null)
        {
            rootResourceObject.Remove(idToken.Name);
        }

        if (metaToken != null)
        {
            rootResourceObject.Remove(metaToken.Name);
        }

        string? signatureContent = null;
        if (provenanceToken != null)
        {
            provenanceToken.Remove();
            signatureContent = provenanceToken.SelectToken("$.resource.signature[0]")?.ToString();
        }

        return new IntegratedSignaturePreprocessResult
        {
            IdNode = idToken,
            MetaNode = metaToken,
            SignatureContent = signatureContent,
            ContentToSign = rootResource.ToString(),
        };
    }

    public string AddMetaIdSignature(string payload, Provenance provenance, object? metaContent, object? idContent)
    {
        var doc = LoadJsonObject(payload, "Failed to re-add meta, id elements and to add provenance.");
        var provenanceJson = FhirJsonSerializer.Default.SerializeToString(provenance);
        var provenanceToken = LoadJsonObject(provenanceJson, "Failed to parse serialized FHIR signature");

        if (idContent is JToken idContentToken)
        {
            doc.Add(idContentToken);
        }

        if (metaContent is JToken metaContentToken)
        {
            doc.Add(metaContentToken);
        }

        var entries = doc.SelectToken("$.entry");
        if (entries is not JArray entriesArray)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError(
                    "Failed to populate signature - bundle has no entries.");
            }

            throw new DocumentSignatureException("Invalid FHIR JSON document.");
        }

        var entryResource = new JObject { { "resource", provenanceToken } };

        entriesArray.Add(entryResource);

        return doc.ToString();
    }

    private JObject LoadJsonObject(string serialized, string errMsg)
    {
        JObject doc;
        try
        {
            doc = JObject.Parse(serialized);
        }
        catch (JsonReaderException e)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError(e, "{errMsg}. Invalid JSON document.", errMsg);
            }

            throw new DocumentSignatureException("Invalid JSON document.");
        }

        return doc;
    }

    private class CommitmentId
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonProperty("desc")]
        [JsonPropertyName("desc")]
        public required string Description { get; set; }
    }

    private class SignersCommitment
    {
        [JsonProperty("commId")]
        [JsonPropertyName("commId")]
        public required CommitmentId CommitmentIdentifier { get; set; }

        [JsonProperty("commQuals")]
        [JsonPropertyName("commQuals")]
        public required string[] CommitmentQualifications { get; set; }
    }
}