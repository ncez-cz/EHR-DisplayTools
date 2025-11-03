using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public delegate Task<SectionBuildResult> SectionContentBuilderAsync(
    List<XmlDocumentNavigator> items,
    string? type,
    bool hasMultipleResourceTypes
);

public delegate SectionBuildResult SectionContentBuilder(
    List<XmlDocumentNavigator> items,
    string? type,
    bool hasMultipleResourceTypes
);

public class SectionBuildResult
{
    public Widget Content { get; set; }
    public Widget? TitleAppend { get; set; }

    public SectionBuildResult(Widget content, Widget? titleAppend = null)
    {
        Content = content;
        TitleAppend = titleAppend;
    }

    public static implicit operator SectionBuildResult(Widget content)
    {
        return new SectionBuildResult(content);
    }
}

/// <summary>
///     Represents a section for rendering IPS (International Patient Summary) views, designed to process
///     and render coded sections based on provided navigators and associated data.
///     <br />
///     This widget expects to be called in the context of a Composition.
/// </summary>
/// <param name="codedSectionBuilder">
///     A delegate that builds the section's content.
///     If the section contains multiple resource types, codedSectionBuilder is called once for each resource type.
///     Takes a list of <see cref="XmlDocumentNavigator" />s, optionally the name of the current resource type,
///     and a boolean indicating whether there are multiple resource types, returns a Widget.
/// </param>
/// <param name="code">Loinc code identifying this section</param>
public class FhirSection(
    CodedValueDefinition? code,
    SectionContentBuilderAsync codedSectionBuilder,
    Func<List<XmlDocumentNavigator>, Severity?> getSeverity,
    LocalizedAbbreviations? titleAbbreviations
) : Widget
{
    private static readonly SectionContentBuilderAsync m_defaultBuilder = (items, type, hasMultipleResourceTypes) =>
        Task.FromResult<SectionBuildResult>(new AnyResource(items, type,
            displayResourceType: hasMultipleResourceTypes));

    public FhirSection(
        CodedValueDefinition? code,
        SectionContentBuilder codedSectionBuilder,
        Func<List<XmlDocumentNavigator>, Severity?> getSeverity,
        LocalizedAbbreviations? titleAbbreviations
    ) : this(code,
        (items, type, hasMultipleResourceTypes) =>
            Task.FromResult(codedSectionBuilder(items, type, hasMultipleResourceTypes)), getSeverity,
        titleAbbreviations)
    {
    }

    public FhirSection(
        CodedValueDefinition code,
        SectionContentBuilder? codedSectionBuilder = null,
        Severity? severity = null,
        LocalizedAbbreviations? titleAbbreviations = null
    )
        : this(code,
            codedSectionBuilder != null
                ? (items, type, hasMultipleResourceTypes) =>
                    Task.FromResult(codedSectionBuilder(items, type, hasMultipleResourceTypes))
                : m_defaultBuilder, _ => severity, titleAbbreviations)
    {
    }

    public FhirSection() : this(
        null,
        m_defaultBuilder,
        _ => null,
        null
    )
    {
    }

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var sectionNav = code == null
            ? navigator
            : navigator.SelectSingleNode(
                $"f:section[f:code/f:coding/f:system[@value='{code.CodeSystem}'] and f:code/f:coding/f:code[@value='{code.Code}']]"
            );
        if (sectionNav.Node == null)
        {
            return string.Empty;
        }

        var narrativeModal = new NarrativeModal();

        var tree = new ChangeContext(
            sectionNav,
            new MultiReference(async navigators =>
            {
                var sectionContentWithTypes =
                    await BuildSectionContent(sectionNav, navigators, navigator, context, renderer);
                var anyStructuredContent = sectionContentWithTypes.ContentType.Any(x =>
                    x is SectionElements.Entries or SectionElements.EmptyReason or SectionElements.Subsection);
                Widget[] sectionContent;
                if (anyStructuredContent)
                {
                    sectionContent = [..sectionContentWithTypes.Content, new NarrativeCollapser()];
                }
                else
                {
                    sectionContent = [..sectionContentWithTypes.Content, new NarrativeCard()];
                }

                var codeIsLoinc = sectionNav.EvaluateCondition("f:code/f:coding/f:system[@value='http://loinc.org']");
                var titleProp = new ChangeContext("f:code",
                    new CodeableConcept(
                        preferredCodeSystemOverride: codeIsLoinc
                            ? "http://ncez.mzcr.cz/CodeSystem/ehr-display-tool-labels"
                            : null
                    )
                );

                List<Widget> title = [titleProp];
                if (sectionContentWithTypes.TitleAppend.Count != 0)
                {
                    title.Add(new ConstantText("  "));
                    title.AddRange(sectionContentWithTypes.TitleAppend);
                }

                return new Section(
                    ".",
                    null,
                    title,
                    sectionContent,
                    idSource: sectionNav,
                    titleAbbreviations: titleAbbreviations,
                    severity: getSeverity(navigators),
                    narrativeModal: anyStructuredContent ? narrativeModal : null
                );
            })
        );

        return await tree.Render(navigator, renderer, context);
    }

    private async Task<SectionContent> BuildSectionContent(
        XmlDocumentNavigator sectionNav,
        List<XmlDocumentNavigator> entryNavs,
        XmlDocumentNavigator navigator,
        RenderContext context,
        IWidgetRenderer renderer
    )
    {
        var sectionContent = new List<Widget>();
        var sectionContentType = new List<SectionElements>();
        var sectionTitleAppend = new List<Widget>();

        if (sectionNav.EvaluateCondition("f:author"))
        {
            sectionContentType.Add(SectionElements.Author);
            sectionContent.Add(new NameValuePair(
                [new ConstantText("Autor sekce")],
                [
                    new CommaSeparatedBuilder(
                        "f:author",
                        _ => new AnyReferenceNamingWidget()
                    ),
                ]
            ));
        }

        if (sectionNav.EvaluateCondition("f:focus"))
        {
            sectionContentType.Add(SectionElements.Focus);
            sectionContent.Add(new ChangeContext(
                "f:focus",
                new NameValuePair(
                    new ConstantText("Subjekt"),
                    new AnyReferenceNamingWidget()
                )
            ));
        }

        if (entryNavs.Count != 0)
        {
            var groups = entryNavs
                .GroupBy(n => n.Node?.Name).ToList();
            sectionContentType.Add(SectionElements.Entries);

            var sectionBuildResult = await Task.WhenAll(groups.Select(async group =>
                await codedSectionBuilder(group.ToList(), group.Key, groups.Count > 1)
            ));

            sectionTitleAppend.AddRange(sectionBuildResult.Select(x => x.TitleAppend).WhereNotNull());
            sectionContent.Add(
                new Concat(
                    // Process each different coded section separately
                    sectionBuildResult.Select(x => x.Content).ToArray()
                )
            );
        }
        else if (sectionNav.EvaluateCondition("f:emptyReason"))
        {
            sectionContentType.Add(SectionElements.EmptyReason);
            sectionContent.Add(
                new Widgets.Alert(
                    new NameValuePair(
                        [new ConstantText("Důvod absence údajů")],
                        [new ChangeContext("f:emptyReason", new CodeableConcept())]
                    ),
                    Severity.Info
                )
            );
        }

        if (sectionNav.EvaluateCondition("f:section"))
        {
            sectionContentType.Add(SectionElements.Subsection);
            sectionContent.Add(
                new ConcatBuilder(
                    "f:section",
                    _ =>
                    [
                        new FhirSection(),
                    ]
                )
            );
        }

        return new SectionContent(sectionContent, sectionContentType, sectionTitleAppend);
    }

    private enum SectionElements
    {
        Author,
        Focus,
        Entries,
        EmptyReason,
        Subsection
    }

    private class SectionContent(
        List<Widget> content,
        List<SectionElements> contentType,
        List<Widget>? titleAppend = null
    )
    {
        public List<Widget> Content { get; } = content;
        public List<SectionElements> ContentType { get; } = contentType;

        public List<Widget> TitleAppend { get; } = titleAppend ?? [];
    }
}