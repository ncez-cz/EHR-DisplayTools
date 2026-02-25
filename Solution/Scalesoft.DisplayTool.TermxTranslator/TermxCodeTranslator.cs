using Microsoft.Extensions.Logging;
using Scalesoft.DisplayTool.Shared.Configuration;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;
using Scalesoft.DisplayTool.Shared.Translation;

namespace Scalesoft.DisplayTool.TermxTranslator;

public class TermxCodeTranslator : ICodeTranslator
{
    private readonly TermxApiClient m_client;
    private readonly ILogger<TermxCodeTranslator> m_logger;
    private readonly KnownOidsConfiguration m_oidToUrlMap;
    

    public TermxCodeTranslator(TermxApiClient client, ILogger<TermxCodeTranslator> logger, KnownOidsConfiguration oidToUrlMap)
    {
        m_client = client;
        m_logger = logger;
        m_oidToUrlMap = oidToUrlMap;
    }


    public async Task<TranslationEntry?> GetCodedValue(
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
            if (!codeSystem.StartsWith("http"))
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

    private async Task<TranslationEntry?> TranslateCodeSystemCode(
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

        var translation = selectedLanguageValue ?? defaultDisplay;
        if (translation == null)
        {
            return null;
        }

        return new TranslationEntry
        {
            Code = code,
            System = codeSystem,
            Translations = { { language, translation } },
        };
    }


    private async Task<TranslationEntry?> TranslateValueSetCode(
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
        
        if (result == null)
        {
            return null;
        }

        return new TranslationEntry
        {
            Code = code,
            System = valueSet,
            Translations = { { language, result } },
        };
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