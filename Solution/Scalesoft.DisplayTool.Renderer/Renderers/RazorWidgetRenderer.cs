using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Providers;
using Scalesoft.DisplayTool.Renderer.Widgets;
using Scalesoft.DisplayTool.Renderer.Widgets.RazorComponents;
using RenderMode = Scalesoft.DisplayTool.Renderer.Models.Enums.RenderMode;

namespace Scalesoft.DisplayTool.Renderer.Renderers;

public class RazorWidgetRenderer : IWidgetRenderer
{
    private readonly IServiceProvider m_serviceProvider;
    private readonly ILoggerFactory m_loggerFactory;

    public RazorWidgetRenderer(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
    {
        m_serviceProvider = serviceProvider;
        m_loggerFactory = loggerFactory;
    }

    public async Task<string> RenderAlert(Alert.ViewModel viewModel)
    {
        return await RenderComponent<AlertView>(viewModel);
    }

    public async Task<string> RenderPlainBadge(PlainBadge.ViewModel viewModel)
    {
        return await RenderComponent<PlainBadgeView>(viewModel);
    }

    public async Task<string> RenderBadge(Badge.ViewModel viewModel)
    {
        return await RenderComponent<BadgeView>(viewModel);
    }

    public async Task<string> RenderCard(Card.ViewModel viewModel)
    {
        return await RenderComponent<CardView>(viewModel);
    }

    public async Task<string> RenderList(FlexList.ViewModel viewModel)
    {
        return await RenderComponent<FlexListView>(viewModel);
    }

    public async Task<string> RenderValidations(ValidationResult.ViewModel viewModel)
    {
        return await RenderComponent<ValidationResultView>(viewModel);
    }

    public async Task<string> RenderPdf(PdfToHtml.PdfModel viewModel)
    {
        return await RenderComponent<PdfToHtmlView>(viewModel);
    }

    public async Task<string> RenderHeading(Heading.ViewModel viewModel)
    {
        return await RenderComponent<HeadingView>(viewModel);
    }

    public async Task<string> RenderLink(Link.ViewModel viewModel)
    {
        return await RenderComponent<LinkView>(viewModel);
    }

    public async Task<string> RenderNameValuePair(NameValuePair.ViewModel viewModel)
    {
        return await RenderComponent<NameValuePairView>(viewModel);
    }

    public async Task<string> RenderTable(Table.ViewModel viewModel)
    {
        return await RenderComponent<TableView>(viewModel);
    }

    public async Task<string> RenderTableCaption(TableCaption.ViewModel viewModel)
    {
        return await RenderComponent<TableCaptionView>(viewModel);
    }

    public async Task<string> RenderTableCell(TableCell.ViewModel viewModel)
    {
        return await RenderComponent<TableCellView>(viewModel);
    }

    public async Task<string> RenderTableRow(TableRow.ViewModel viewModel)
    {
        return await RenderComponent<TableRowView>(viewModel);
    }

    public async Task<string> RenderTableHead(TableHead.ViewModel viewModel)
    {
        return await RenderComponent<TableHeadView>(viewModel);
    }

    public async Task<string> RenderTableFoot(TableFooter.ViewModel viewModel)
    {
        return await RenderComponent<TableFootView>(viewModel);
    }

    public async Task<string> RenderTableBody(TableBody.ViewModel viewModel)
    {
        return await RenderComponent<TableBodyView>(viewModel);
    }

    public async Task<string> RenderTooltip(Tooltip.ViewModel viewModel)
    {
        return await RenderComponent<TooltipView>(viewModel);
    }

    public async Task<string> RenderContainer(Container.ViewModel viewModel)
    {
        return await RenderComponent<ContainerView>(viewModel);
    }

    public async Task<string> RenderNarrativeContainer(NarrativeContainer.ViewModel viewModel)
    {
        return await RenderComponent<NarrativeView>(viewModel);
    }

    public async Task<string> RenderTextContainer(TextContainer.ViewModel viewModel)
    {
        return await RenderComponent<TextContainerView>(viewModel);
    }

    public async Task<string> RenderLineBreak()
    {
        return await RenderComponent<LineBreakView>(null);
    }

    public async Task<string> RenderItemList(ItemList.ViewModel viewModel)
    {
        return await RenderComponent<ItemListView>(viewModel);
    }

    public async Task<string> RenderSection(Section.ViewModel viewModel)
    {
        return await RenderComponent<SectionView>(viewModel);
    }

    public async Task<string> RenderButton(Button.ViewModel viewModel)
    {
        return await RenderComponent<ButtonView>(viewModel);
    }

    public async Task<string> RenderThematicBreak(ThematicBreak.ViewModel viewModel)
    {
        return await RenderComponent<ThematicBreakView>(viewModel);
    }

    public async Task<string> RenderImage(Image.ViewModel viewModel)
    {
        return await RenderComponent<ImageView>(viewModel);
    }

    public async Task<string> RenderConstantText(ConstantText.ViewModel viewModel)
    {
        return await RenderComponent<ConstantTextView>(viewModel);
    }

    public async Task<string> RenderBarcode(Barcode.ViewModel viewModel)
    {
        return await RenderComponent<BarcodeView>(viewModel);
    }

    public async Task<string> RenderSvgGraph(SvgGraph.ViewModel viewModel)
    {
        return await RenderComponent<SvgGraphView>(viewModel);
    }

    public async Task<string> RenderModal(Modal.ViewModel viewModel)
    {
        return await RenderComponent<ModalView>(viewModel);
    }


    public async Task<string> WrapWithLayout(
        string? htmlContent,
        string? htmlValidationContent,
        RenderMode? renderMode,
        bool isEmbeddable = false
    )
    {
        var frontendResourceProvider = new FrontendResourceProvider();
        var resources = frontendResourceProvider.ReadResources();
        var fullHtmlContent = htmlContent + htmlValidationContent;

        await using var htmlRenderer = new HtmlRenderer(m_serviceProvider, m_loggerFactory);

        var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
            {
                var dictionary = new Dictionary<string, object?>
                {
                    {
                        nameof(ResultHtmlDocument.DocumentInfo),
                        new HtmlDocumentInfo
                        {
                            Scripts = resources.Scripts,
                            Styles = resources.Styles,
                            Content = fullHtmlContent,
                            RenderMode = renderMode,
                            IsEmbeddable = isEmbeddable,
                        }
                    },
                };

                var parameters = ParameterView.FromDictionary(dictionary);
                var output = await htmlRenderer.RenderComponentAsync<ResultHtmlDocument>(parameters);

                return output.ToHtmlString();
            }
        );

        return html;
    }

    private async Task<string> RenderComponent<TComponent>(object? viewModel) where TComponent : ComponentBase
    {
        await using var htmlRenderer = new HtmlRenderer(m_serviceProvider, m_loggerFactory);
        var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
            {
                Dictionary<string, object?> dictionary = viewModel != null
                    ? new()
                    {
                        {
                            "Model", viewModel
                        },
                    }
                    : new();

                var parameters = ParameterView.FromDictionary(dictionary);
                var output = await htmlRenderer.RenderComponentAsync<TComponent>(parameters);

                return output.ToHtmlString();
            }
        );

        return html;
    }
}