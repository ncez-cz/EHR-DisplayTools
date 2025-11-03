using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class Button(
    string? text = null,
    string? onClick = null,
    ButtonStyle style = ButtonStyle.Primary,
    string? tooltip = null,
    bool isDisabled = false,
    string? cssClass = null,
    ButtonVariant variant = ButtonVariant.Default,
    SupportedIcons? icon = null,
    SupportedIcons? altIcon = null,
    Dictionary<string, object>? additionalAttributes = null
) : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var renderedIcon = icon == null ? string.Empty : IconHelper.GetInstance(icon.Value, context);
        var renderedAltIcon = altIcon == null ? string.Empty : IconHelper.GetInstance(altIcon.Value, context);

        var viewModel = new ViewModel
        {
            InputId = Id,
            Text = text,
            OnClick = onClick,
            Style = style,
            Tooltip = tooltip,
            IsDisabled = isDisabled,
            CustomClass = cssClass,
            Variant = variant,
            Icon = variant == ButtonVariant.CollapseSection
                ? IconHelper.GetInstance(SupportedIcons.ChevronUp, context)
                : renderedIcon,
            AdditionalAttributes = additionalAttributes ?? new Dictionary<string, object>(),
            LevelOfDocumentDetail = context.LevelOfDetail,
            AltIcon = renderedAltIcon
        };

        var view = await renderer.RenderButton(viewModel);

        return new RenderResult(view, []);
    }

    public class ViewModel : ViewModelBase
    {
        public uint InputId { get; set; }

        public required string? Text { get; set; }

        public string? OnClick { get; set; }

        public ButtonStyle Style { get; set; }

        public string? Tooltip { get; set; }

        public bool IsDisabled { get; set; }

        public ButtonVariant Variant { get; set; } = ButtonVariant.Default;

        public required string Icon { get; init; }

        public Dictionary<string, object> AdditionalAttributes { get; set; } = new();

        public LevelOfDetail LevelOfDocumentDetail { get; set; }

        public string? AltIcon { get; set; }
    }
}

public enum ButtonStyle
{
    Primary,
    Secondary,
    Success,
    Danger,
    Warning,
    Info,
    Outline,
}

public enum ButtonVariant
{
    Default,
    CollapseSection,
    ToggleDetails,
    Modal
}