using Scalesoft.DisplayTool.Renderer.Widgets;
using RenderMode = Scalesoft.DisplayTool.Renderer.Models.Enums.RenderMode;

namespace Scalesoft.DisplayTool.Renderer.Renderers;

public interface IWidgetRenderer
{
    public Task<string> RenderAlert(Alert.ViewModel viewModel);

    public Task<string> RenderPlainBadge(PlainBadge.ViewModel viewModel);

    public Task<string> RenderBadge(Badge.ViewModel viewModel);

    public Task<string> RenderCard(Card.ViewModel viewModel);

    public Task<string> RenderList(FlexList.ViewModel viewModel);

    public Task<string> RenderValidations(ValidationResult.ViewModel viewModel);

    public Task<string> RenderPdf(PdfToHtml.PdfModel viewModel);

    public Task<string> RenderHeading(Heading.ViewModel viewModel);

    public Task<string> RenderLink(Link.ViewModel viewModel);

    public Task<string> RenderNameValuePair(NameValuePair.ViewModel viewModel);

    public Task<string> RenderTable(Table.ViewModel viewModel);

    public Task<string> RenderTableCaption(TableCaption.ViewModel viewModel);

    public Task<string> RenderTableCell(TableCell.ViewModel viewModel);

    public Task<string> RenderTableRow(TableRow.ViewModel viewModel);

    public Task<string> RenderTableHead(TableHead.ViewModel viewModel);

    public Task<string> RenderTableFoot(TableFooter.ViewModel viewModel);

    public Task<string> RenderTableBody(TableBody.ViewModel viewModel);

    public Task<string> RenderTooltip(Tooltip.ViewModel viewModel);

    public Task<string> RenderContainer(Container.ViewModel viewModel);

    public Task<string> RenderNarrativeContainer(NarrativeContainer.ViewModel viewModel);

    public Task<string> RenderTextContainer(TextContainer.ViewModel viewModel);

    public Task<string> RenderLineBreak();

    public Task<string> RenderItemList(ItemList.ViewModel viewModel);

    public Task<string> WrapWithLayout(
        string? htmlContent,
        string? htmlValidationContent,
        RenderMode? renderMode = RenderMode.Standard,
        bool isEmbeddable = false
    );

    public Task<string> RenderSection(Section.ViewModel viewModel);

    public Task<string> RenderButton(Button.ViewModel viewModel);

    public Task<string> RenderThematicBreak(ThematicBreak.ViewModel viewModel);

    public Task<string> RenderImage(Image.ViewModel viewModel);

    public Task<string> RenderConstantText(ConstantText.ViewModel viewModel);

    public Task<string> RenderBarcode(Barcode.ViewModel viewModel);
    public Task<string> RenderSvgGraph(SvgGraph.ViewModel viewModel);
    public Task<string> RenderModal(Modal.ViewModel viewModel);
}