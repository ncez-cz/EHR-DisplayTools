using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Sections;

/// <summary>
/// Dynamically render sections in order from the document using the default builder specified via 'defaultType'.
/// Section order, builder and severity can be overriden. 
/// </summary>
/// <param name="xpath"></param>
/// <param name="defaultType">Options -> AnyResource (Render entries as whole resources), Summary (Render entries using AnyReferenceNamingWidget) Ignore (Doesn't render at all) </param>
/// <param name="overrides">Overrides the default section type (= builder) and render sections in the order of this list, additionally allow to set severity of the section.
/// To create pseudo section, leave code of the SectionBuilder null, set type to 'Custom' and provide custom builder.</param>
public class DynamicSectionBuilder(
    string xpath = "f:section",
    PredefinedSectionType defaultType = PredefinedSectionType.AnyResource,
    List<SectionDefinition>? overrides = null
) : Widget
{
    private readonly SectionBuilder
        m_anyResourceBuilder = (_, _, severity) => new FhirSection(null, severity: severity);

    private readonly SectionBuilder m_summaryBuilder = (_, _, severity) => new FhirSection(
        null,
        (_, _, _) => new ListBuilder(
            "f:entry",
            FlexDirection.Row,
            _ =>
            [
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        Direction = FlexDirection.Column,
                        Style = NameValuePair.NameValuePairStyle.Primary,
                    }
                ),
            ],
            flexContainerClasses: "flex-wrap column-gap-4 row-gap-1"
        ),
        groupByResourceType: false,
        severity: severity
    );

    private SectionBuilder DefaultBuilder => defaultType switch
    {
        PredefinedSectionType.AnyResource => m_anyResourceBuilder,
        PredefinedSectionType.Summary => m_summaryBuilder,
        PredefinedSectionType.Ignore => (_, _, _) => new NullWidget(),
        _ => throw new ArgumentOutOfRangeException(nameof(defaultType), defaultType, null),
    };

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var sectionList = navigator.SelectAllNodes(xpath).ToList();

        List<Widget> resultWidgets = [];
        List<XmlDocumentNavigator> renderedSections = [];
        
        if (overrides != null)
        {
            foreach (var sectionOverride in overrides)
            {
                Widget resultWidget = new NullWidget();
            
                var sectionNav = sectionList.SingleOrDefault(x =>
                    x.SelectSingleNode("f:code/f:coding/f:code/@value").Node?.Value == sectionOverride.Code);
                
                if (sectionNav != null)
                {
                    if (sectionOverride.Type != SectionType.Ignore)
                    {
                        var selectedBuilder = sectionOverride.Type switch
                        {
                            SectionType.AnyResource => m_anyResourceBuilder,
                            SectionType.Summary => m_summaryBuilder,
                            SectionType.Custom when sectionOverride.CustomBuilder != null => sectionOverride.CustomBuilder,
                            _ => DefaultBuilder,
                        };
                        
                        resultWidget = new ChangeContext(sectionNav, selectedBuilder(sectionNav, severity: sectionOverride.Severity));
                    }
                
                    resultWidgets.Add(resultWidget); 
                    renderedSections.Add(sectionNav);
                    continue;
                }

                // Pseudo sections requires empty/null code, type Custom and custom builder provided.
                // (Pseudo sections = manually created sections that are not in the XML document)
                if (!string.IsNullOrEmpty(sectionOverride.Code) || sectionOverride.Type != SectionType.Custom|| sectionOverride.CustomBuilder == null)
                {
                    continue;
                }

                resultWidget = sectionOverride.CustomBuilder(navigator, severity: sectionOverride.Severity);
                resultWidgets.Add(resultWidget);
            }
        }
        
        sectionList.RemoveAll(x => renderedSections.Contains(x));
        
        Widget remainingWidgets = new ConcatBuilder(
            sectionList,
            (_, _, x) => [DefaultBuilder(x)]);

        resultWidgets.Add(remainingWidgets);
        
        return resultWidgets.RenderConcatenatedResult(navigator, renderer, context);
    }
}

public delegate Widget SectionBuilder(
    XmlDocumentNavigator nav,
    string? sectionCode = null,
    Severity? severity = null
);