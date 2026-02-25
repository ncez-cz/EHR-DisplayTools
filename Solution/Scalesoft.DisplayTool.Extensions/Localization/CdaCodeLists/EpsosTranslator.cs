using Scalesoft.DisplayTool.Shared.Configuration;
using Scalesoft.DisplayTool.Shared.Translation;

namespace Scalesoft.DisplayTool.Extensions.Localization.CdaCodeLists;

public class EpsosTranslator(ITranslationsStorage storage, KnownOidsConfiguration? knownOidsConfiguration) : ICodeTranslator
{
    public Task<TranslationEntry?> GetCodedValue(
        string code,
        string codeSystem,
        string userLang,
        string defaultUserLang,
        bool isValueSet
    )
    {
        var systemUrl = codeSystem;
        if (!codeSystem.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) && knownOidsConfiguration != null)
        {
            systemUrl = knownOidsConfiguration.GetValueOrDefault(codeSystem, systemUrl);
        }
        
        var result = isValueSet
            ? storage.GetConceptByCodeAndValueSet(code, systemUrl)
            : storage.GetConceptByCodeAndSystem(code, systemUrl);

        if (result == null)
        {
            return Task.FromResult<TranslationEntry?>(null);
        }

        return Task.FromResult<TranslationEntry?>(
            new TranslationEntry
            {
                Code = result.Code,
                System = result.System,
                Translations = result.Translations,
                Properties = result.Properties,
            }
        );
    }
}