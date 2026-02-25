namespace Scalesoft.DisplayTool.Shared.Translation;

public interface ICodeTranslator
{
    public Task<TranslationEntry?> GetCodedValue(
        string code,
        string codeSystem,
        string language,
        string fallbackLanguage,
        bool isValueSet = false
    );
}