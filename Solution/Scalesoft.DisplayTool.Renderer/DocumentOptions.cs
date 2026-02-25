using Scalesoft.DisplayTool.Renderer.Utils.Language;

namespace Scalesoft.DisplayTool.Renderer;

public class DocumentOptions
{
    public bool ValidateDocument { get; set; }

    /// <summary>
    /// Validation against terminology server
    /// </summary>
    public bool ValidateCodeValues { get; set; }

    public object? WarningsAndErrors { get; set; }

    public bool ValidateDigitalSignature { get; set; }

    public LanguageOptions? LanguageOption { get; set; }

    public required bool PreferTranslationsFromDocument { get; set; }
}