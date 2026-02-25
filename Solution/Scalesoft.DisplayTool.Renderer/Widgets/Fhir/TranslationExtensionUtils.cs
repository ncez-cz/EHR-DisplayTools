using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public static class TranslationExtensionUtils
{
    public static XmlDocumentNavigator? GetTranslationNavigator(
        XmlDocumentNavigator navigator,
        string lang,
        string shortLang
    )
    {
        if (navigator.Node == null)
        {
            return null;
        }

        var extension = navigator
            .SelectAllNodes("f:extension[@url='http://hl7.org/fhir/StructureDefinition/translation']")
            .FirstOrDefault(x =>
                x.EvaluateCondition(
                    $"f:extension[@url='lang' and (f:valueCode/@value='{lang}' or f:valueCode/@value='{shortLang}')]"));
        var translationContent = extension?.SelectSingleNode("f:extension[@url='content']");

        return translationContent?.Node != null ? translationContent : null;
    }

    public static bool ResourceHasLanguage(XmlDocumentNavigator navigator)
    {
        return navigator.EvaluateCondition("ancestor::f:resource/*/f:language");
    }

    public static bool ResourceHasCorrectLanguage(XmlDocumentNavigator navigator, string lang, string shortLang)
    {
        return navigator.EvaluateCondition(
            $"ancestor::f:resource/*/f:language[@value='{lang}' or @value='{shortLang}']");
    }

    public static bool DocumentHasCorrectLanguage(XmlDocumentNavigator navigator, string lang, string shortLang)
    {
        return navigator.EvaluateCondition($"/f:Bundle/f:language[@value='{lang}' or @value='{shortLang}']");
    }

    public static Widget? TranslateWithHierarchy(
        XmlDocumentNavigator navigator,
        string lang,
        string shortLang,
        Widget? originalValue
    )
    {
        // First, try translations from extensions
        var translation = GetTranslationNavigator(navigator, lang, shortLang);
        if (translation?.Node != null)
        {
            return new ChangeContext(translation, new OpenTypeElement(null)); // string | markdown
        }

        // Next, try checking if the current resource has the correct language
        var resourceHasLanguage = ResourceHasLanguage(navigator);
        var resourceHasCorrectLanguage =
            ResourceHasCorrectLanguage(navigator, lang, shortLang);

        // Next, try checking if the current resource has no language, but the document has the right language
        var documentHasCorrectLanguage =
            DocumentHasCorrectLanguage(navigator, lang, shortLang);

        if (resourceHasCorrectLanguage || (!resourceHasLanguage && documentHasCorrectLanguage))
        {
            if (originalValue != null)
            {
                return originalValue;
            }
        }

        return null;
    }
}