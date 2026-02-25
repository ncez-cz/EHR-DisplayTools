using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;
using Scalesoft.DisplayTool.Shared.Translation;

namespace Scalesoft.DisplayTool.Renderer.Models;

public class RenderContext
{
    public readonly ICodeTranslator Translator;

    public readonly TranslatorWrapper TranslatorWrapper;

    public readonly Language Language;

    public readonly ILoggerFactory LoggerFactory;

    public readonly DocumentType DocumentType;

    public RenderMode RenderMode { get; }

    private readonly List<KeyValuePair<ResourceIdentifier, XmlDocumentNavigator>> m_resourcesWithIds = [];

    private readonly List<ResourceIdentifier> m_renderedResources = [];

    public IReadOnlyList<KeyValuePair<ResourceIdentifier, XmlDocumentNavigator>> ResourcesWithIds =>
        m_resourcesWithIds.AsReadOnly();

    public IReadOnlyList<ResourceIdentifier> RenderedResources => m_renderedResources.AsReadOnly();

    public IdentifierDisplayTitleMapping IdentifierDisplayTitleMapping { get; } = new();

    public bool PreferTranslationsFromDocument { get; }

    public HashSet<SupportedIcons> RenderedIcons { get; } = [];
    
    public LevelOfDetail LevelOfDetail { get; set; }

    public RenderContext(
        ICodeTranslator translator,
        Language language,
        ILoggerFactory loggerFactory,
        DocumentType documentType,
        RenderMode renderMode,
        bool preferTranslationsFromDocument,
        LevelOfDetail levelOfDetail = LevelOfDetail.Simplified
    )
    {
        Translator = translator;
        Language = language;
        LoggerFactory = loggerFactory;
        DocumentType = documentType;
        RenderMode = renderMode;
        PreferTranslationsFromDocument = preferTranslationsFromDocument;
        LevelOfDetail = levelOfDetail;
        TranslatorWrapper = new TranslatorWrapper(translator);
    }

    /// <summary>
    ///     Try adding a node to a collection of resources that have ids.
    /// </summary>
    /// <param name="nav"></param>
    /// <returns>
    ///     True if the navigator with resource was added to the collection, false otherwise - resource has no id, XML node is missing,
    ///     element was already added etc.
    /// </returns>
    public bool TryAddResourceWithId(XmlDocumentNavigator nav, string idXpath)
    {
        if (ResourceIdentifier.TryFromNavigator(nav, out var id, idXpath))
        {
            m_resourcesWithIds.Add(new KeyValuePair<ResourceIdentifier, XmlDocumentNavigator>(id, nav));

            return true;
        }

        return false;
    }

    public bool AddRenderedResource(
        XmlDocumentNavigator nav,
        ResourceIdentifier id,
        [NotNullWhen(false)] out RenderedResourceAddFailReason? failReason,
        bool isInHideableContext = false
    )
    {
        var resourceIdValuePair = m_resourcesWithIds.FirstOrDefault(x => x.Key.Equals(id));
        if (!resourceIdValuePair.IsDefault())
        {
            var resourceWithId = resourceIdValuePair.Value;
            if (resourceWithId.Node == null || nav.Node == null)
            {
                failReason = RenderedResourceAddFailReason.MissingXmlNode;
                return false;
            }

            if (!resourceWithId.Node.IsSamePosition(nav.Node))
            {
                failReason = RenderedResourceAddFailReason.DifferentPositionInDocument;
                return false;
            }

            if (!isInHideableContext)
            {
                m_renderedResources.Add(id);
            }

            failReason = null;
            return true;
        }

        failReason = RenderedResourceAddFailReason.MissingReferencedResource;
        return false;
    }

    public bool TryGetReferenceHref(
        XmlDocumentNavigator referenceNode,
        [NotNullWhen(true)] out string? href,
        [NotNullWhen(false)] out GetResourceHrefError? error
    )
    {
        var referenceValue = referenceNode.SelectSingleNode("f:reference/@value").Node?.Value;

        if (referenceValue == null || !ResourceIdentifier.TryFromReference(referenceValue, out var identifier))
        {
            href = null;
            error = GetResourceHrefError.MissingId;
            return false;
        }

        return TryGetReferenceHref(identifier, out href, out error);
    }

    public bool TryGetReferenceHref(
        ResourceIdentifier identifier,
        [NotNullWhen(true)] out string? href,
        [NotNullWhen(false)] out GetResourceHrefError? error
    )
    {
        var matches = m_resourcesWithIds.Where(x => x.Key.Equals(identifier)).ToList();
        switch (matches.Count)
        {
            case < 1:
                href = null;
                error = GetResourceHrefError.MissingReferencedResource;
                return false;
            case > 1:
                href = null;
                error = GetResourceHrefError.NonUniqueReference;
                return false;
            case 1:
                var match = matches[0];
                href = $"#{match.Key.BuildId()}";
                error = null;
                return true;
        }
    }

    public List<XmlDocumentNavigator> GetUnrenderedResources()
    {
        var unrenderedResources =
            m_resourcesWithIds
                .Where(x => !m_renderedResources.Any(y => y.Equals(x.Key)))
                .Select(x => x.Value)
                .ToList();

        return unrenderedResources;
    }

    public bool IsResourceRendered(XmlDocumentNavigator nav)
    {
        var referenceValue = ReferenceHandler.GetReferencesWithContent(nav, ".");
        if (ResourceIdentifier.TryFromNavigator(referenceValue.First().Value, out var identifier))
        {
            var result = m_renderedResources.Any(x => x.Equals(identifier));
            return result;
        }
        
        return false;
    }
}