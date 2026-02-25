using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Utils;

public class InfrequentPropertiesDataInContext<T> : InfrequentPropertiesData<T> where T : notnull
{
    public XmlDocumentNavigator Navigator { get; set; }

    public InfrequentPropertiesDataInContext(XmlDocumentNavigator navigator, InfrequentPropertiesData<T> props) : base(
        props.InfrequentProperties)
    {
        Navigator = navigator;
    }

    public InfrequentPropertiesDataInContext(XmlDocumentNavigator navigator)
    {
        Navigator = navigator;
    }
}