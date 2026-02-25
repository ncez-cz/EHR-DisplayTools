namespace Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;

public class ReferenceNamingWidgetModel
{
    public Widget? LabelOverride { get; init; }
    
    public ReferenceNamingWidgetType Type { get; init; }
    
    public FlexDirection Direction { get; init; }
    
    public NameValuePair.NameValuePairSize Size { get; init; }
    
    public NameValuePair.NameValuePairStyle Style { get; init; }
}
public enum ReferenceNamingWidgetType
{
    Link, 
    NameValuePair,
}