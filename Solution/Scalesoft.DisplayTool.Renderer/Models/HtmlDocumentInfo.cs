using Scalesoft.DisplayTool.Renderer.Models.Enums;

namespace Scalesoft.DisplayTool.Renderer.Models;

public class HtmlDocumentInfo
{
    public required List<string> Styles { get; set; } = [];

    public required List<string> Scripts { get; set; } = [];
    
    public required string Content { get; set; }
    
    public RenderMode? RenderMode { get; set; }
    
    public bool IsEmbeddable { get; set; }
}
