using Microsoft.Extensions.Logging;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;
using Scalesoft.DisplayTool.Shared.Translation;

namespace Scalesoft.DisplayTool.TermxTranslator;

public class TermxCodeTranslator : ICodeTranslator
{
    private readonly TermxApiClient m_client;
    private readonly ILogger<TermxCodeTranslator> m_logger;

    // Warning: Most of these mappings are not exact matches. Substitutions were made to similar / related systems
    // Which were actually present in termx at the time of writing.
    // Note that mappings without an explanation comment may not be correct - not all discrepancies were noted.
    private readonly Dictionary<string, string> m_oidToUrlMap = new()
    {
        { "2.16.840.1.113883.5.1150.1", "http://hl7.org/fhir/uv/ips/CodeSystem/absent-unknown-uv-ips" },
        { "2.16.840.1.113883.6.73", "http://www.whocc.no/atc" },
        // Should be http://terminology.hl7.org/CodeSystem/v3-AdministrativeGender but that doesn't exist in termx
        { "2.16.840.1.113883.5.1", "http://terminology.ehdsi.eu/ValueSet/eHDSIAdministrativeGender" },
        { "2.16.840.1.113883.6.96", "http://snomed.info/sct" },
        // This url corresponds to .1.1373, but .4.1373 wasn't found.
        { "2.16.840.1.113883.4.642.4.1373", "http://terminology.hl7.org/CodeSystem/allergyintolerance-clinical" },
        { "2.16.840.1.113883.6.1", "http://loinc.org" },
        // Should be http://terminology.hl7.org/CodeSystem/condition-ver-status which is not present in termx
        { "2.16.840.1.113883.4.642.4.1075", "http://terminology.ehdsi.eu/ValueSet/eHDSICertainty" },
        // There is a code system with this oid in termx, but it's empty
        { "1.0.3166.1", "https://ciselniky.dasta.mzcr.cz/CD_DS4/nclp_data/ds_DS/is3166_2.xml" },
        { "2.16.840.1.113883.4.642.4.130", "http://hl7.org/fhir/allergy-intolerance-criticality" },
        // Not verified
        { "0.4.0.127.0.16.1.1.2.1", "http://standardterms.edqm.eu" },
        // The actual code system (ISCO) is this one's base, but it doesn't exist in termx
        { "2.16.840.1.113883.2.9.6.2.7", "http://terminology.ehdsi.eu/ValueSet/eHDSIHealthcareProfessionalRole" },
        // Completely different OID, but similar name and likely values
        { "1.3.6.1.4.1.12559.11.10.1.3.1.44.2", "http://hl7.org/fhir/sid/icd-10" },
        // Not found at all
        // { "2.16.840.1.113883.3.6905.2", "" },
        { "1dd183a6-6d2b-4a9d-8f5d-be09d6bb5a6e", "https://ciselniky.dasta.mzcr.cz/CD_DS4/nclp_data/ds_DS/ejazyk.xml" },
        { "2.16.840.1.113883.5.1008", "http://terminology.ehdsi.eu/ValueSet/eHDSINullFlavor" },
        // This should be a more general RoleCode
        { "2.16.840.1.113883.5.111", "https://ciselniky.dasta.mzcr.cz/CD_DS4/nclp_data/ds_DS/evztah.xml" },
        // Should be orpha rare diseases? (Which is not translated)
        { "1.3.6.1.4.1.12559.11.10.1.3.1.44.5", "http://terminology.ehdsi.eu/ValueSet/eHDSIRareDisease" },
        { "2.16.840.1.113883.5.110", "http://terminology.ehdsi.eu/ValueSet/eHDSIRoleClass" },
        { "2.16.840.1.113883.5.1070", "http://terminology.ehdsi.eu/ValueSet/eHDSISubstitutionCode" },
        { "2.16.840.1.113883.5.1119", "http://terminology.ehdsi.eu/ValueSet/eHDSITelecomAddress" },
        { "2.16.840.1.113883.5.139", "http://terminology.ehdsi.eu/ValueSet/eHDSITimingEvent" },
        { "2.16.840.1.113883.6.8", "http://unitsofmeasure.org" },
        // Actually https://build.fhir.org/ig/HL7/UTG/CodeSystem-v3-Confidentiality.html which doesn't exist in termx.
        { "2.16.840.1.113883.5.25", "http://terminology.ehdsi.eu/ValueSet/v3.ConfidentialityClassification" },
        { "1.3.6.1.4.1.12559.11.10.1.3.1.44.4", "http://terminology.ehdsi.eu/CodeSystem/epSOSDisplayLabel" },
    };

    public TermxCodeTranslator(TermxApiClient client, ILogger<TermxCodeTranslator> logger)
    {
        m_client = client;
        m_logger = logger;
    }


    public async Task<string?> GetCodedValue(
        string fileName,
        string code,
        string codeSystem,
        string language,
        string fallbackLanguage,
        bool isValueSet
    )
    {
        try
        {
            var shortLanguage = language.Split('-')[0];

            var systemUrl = codeSystem;
            if (!codeSystem.StartsWith("http") &&
                !codeSystem.StartsWith("http://"))
            {
                if (m_oidToUrlMap.TryGetValue(codeSystem, out var value))
                {
                    systemUrl = value;
                }
                else
                {
                    try
                    {
                        systemUrl = await LookupCodeSystem(codeSystem);
                        m_oidToUrlMap.Add(codeSystem, systemUrl);
                    }
                    catch (HttpRequestException e)
                    {
                        if (m_logger.IsEnabled(LogLevel.Warning))
                        {
                            m_logger.LogWarning(
                                "Failed to fetch code system metadata for system {codeSystem}: {status}",
                                codeSystem,
                                (int?)e.StatusCode
                            );
                        }
                    }
                }
            }

            if (isValueSet)
            {
                return await TranslateValueSetCode(code, codeSystem, language, shortLanguage);
            }

            return await TranslateCodeSystemCode(code, systemUrl, language, shortLanguage);
        }
        catch (HttpRequestException e)
        {
            if (m_logger.IsEnabled(LogLevel.Information))
            {
                m_logger.LogInformation(
                    "Failed to get translation for code {code} in system {system}: {status}",
                    code,
                    codeSystem,
                    (int?)e.StatusCode
                );
            }
        }
        // XmlDocumentNavigator exceptions aren't documented, catch all exceptions to be safe.
        catch (Exception e)
        {
            if (m_logger.IsEnabled(LogLevel.Warning))
            {
                m_logger.LogWarning(
                    "Failed to fetch code translation for {code} in {codeSystem}: {message}",
                    code,
                    codeSystem,
                    e.Message
                );
            }
        }

        return null;
    }

    private async Task<string?> TranslateCodeSystemCode(
        string code,
        string codeSystem,
        string language,
        string shortLanguage
    )
    {
        var response = await m_client.LookupCodeSystemValue(code, codeSystem);

        var navigator = new XmlDocumentNavigator(response.CreateNavigator());
        navigator.AddNamespace("f", "http://hl7.org/fhir");

        var defaultDisplay = navigator
            .SelectSingleNode("f:Parameters/f:parameter[f:name/@value='display']/f:valueString/@value").Node?.Value;

        var designations = navigator.SelectAllNodes("f:Parameters/f:parameter[f:name/@value='designation']");
        var displayDesignations = designations.Where(x =>
            x.EvaluateCondition("f:part[f:name/@value='use' and f:valueCoding/f:code/@value='display']")
        );
        var withMatchingLanguage = displayDesignations.Where(x =>
            x.EvaluateCondition(
                $"f:part[f:name/@value='language' and f:valueString/@value='{language}' or f:name/@value='language' and f:valueString/@value='{shortLanguage}']"
            )
        );

        var selectedLanguageValue = withMatchingLanguage
            .Select(x => x.SelectSingleNode("f:part[f:name/@value='value']/f:valueString/@value"))
            .FirstOrDefault(x => x.Node?.Value != null)?.Node?.Value;

        if (selectedLanguageValue == null && defaultDisplay == null && m_logger.IsEnabled(LogLevel.Information))
        {
            m_logger.LogInformation("No translation found for {code} in {system}.", code, codeSystem);
        }

        return selectedLanguageValue ?? defaultDisplay;
    }


    private async Task<string?> TranslateValueSetCode(
        string code,
        string valueSet,
        string language,
        string shortLanguage
    )
    {
        var response = await m_client.ExpandValueSet(valueSet, code);

        var navigator = new XmlDocumentNavigator(response.CreateNavigator());
        navigator.AddNamespace("f", "http://hl7.org/fhir");

        // The display value can be found in three different places. The first should be localized, the rest may not be.
        var withMatchingLanguage = navigator.SelectSingleNode(
            $"""//f:concept[f:code/@value="{code}"]/f:designation[f:language/@value="{shortLanguage}" or f:language/@value="{language}"]/f:value/@value"""
        ).Node?.Value;
        var containsFallBack = navigator.SelectSingleNode($"""//f:contains[f:code/@value="{code}"]/f:display/@value""")
            .Node
            ?.Value;
        var conceptFallback = navigator.SelectSingleNode($"""//f:concept[code/@value="{code}"]/f:display/@value""").Node
            ?.Value;

        var result = withMatchingLanguage ?? containsFallBack ?? conceptFallback;


        if (result == null && m_logger.IsEnabled(LogLevel.Information))
        {
            m_logger.LogInformation("No translation found for {code} in {valueSet}.", code, valueSet);
        }

        return result;
    }


    private async Task<string> LookupCodeSystem(string oid)
    {
        var identifier = oid.StartsWith("urn:", StringComparison.CurrentCultureIgnoreCase) ? oid : $"urn:oid:{oid}";
        var response = await m_client.LookupCodeSystem(identifier);
        if (response == null)
        {
            throw new Exception($"Failed to lookup code system {identifier}");
        }

        var navigator = new XmlDocumentNavigator(response?.CreateNavigator());
        navigator.AddNamespace("f", "http://hl7.org/fhir");

        var urlNavigator = navigator.SelectSingleNode("f:Bundle/f:entry/f:resource/f:CodeSystem/f:url/@value");
        var url = urlNavigator.Node?.Value;
        if (url == null)
        {
            throw new Exception($"Failed to lookup code system {identifier}");
        }

        return url;
    }
}