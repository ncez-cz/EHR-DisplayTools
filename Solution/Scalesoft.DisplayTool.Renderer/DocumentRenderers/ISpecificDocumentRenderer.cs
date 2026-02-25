using Scalesoft.DisplayTool.Renderer.Models.Enums;

namespace Scalesoft.DisplayTool.Renderer.DocumentRenderers;

public interface ISpecificDocumentRenderer
{
    public InputFormat InputFormat { get; }

    public Task<DocumentResult> RenderAsync(
        byte[] fileContent,
        OutputFormat outputFormat,
        DocumentOptions options,
        DocumentType documentType,
        bool isEmbeddable,
        RenderMode renderMode = RenderMode.Standard,
        LevelOfDetail levelOfDetail = LevelOfDetail.Simplified
    );
}