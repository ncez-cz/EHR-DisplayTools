using Scalesoft.DisplayTool.Shared.Translation;

namespace Scalesoft.DisplayTool.Renderer.Utils.Language;

public class TranslatorWrapper
{
    private readonly ICodeTranslator m_translator;

    public TranslatorWrapper(ICodeTranslator translator)
    {
        m_translator = translator;
    }

    public async Task<string?> GetCodedValue(
        string code,
        string codeSystem,
        string language,
        string fallbackLanguage,
        bool isValueSet = false
    )
    {
        var entry = await m_translator.GetCodedValue(code, codeSystem, language, fallbackLanguage, isValueSet);
        if (entry == null)
        {
            return null;
        }


        var result = entry.Translations.TryGetValue(language, out var translation)
            ? translation
            : entry.Translations.GetValueOrDefault(fallbackLanguage);
        return result;
    }
}