using System.Text;
using System.Text.RegularExpressions;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Utils;

public static partial class ReferenceHandler
{
    /// <summary>
    ///     Generates a collapsible content structure for references found at the given path, using the specified content
    ///     builder.
    ///     Creates two widgets if applicable: one for references with content and one for references without content.
    /// </summary>
    /// <param name="contentBuilder">Function to build content widgets from a list of references.</param>
    /// <param name="navigator">XML navigator to traverse the document.</param>
    /// <param name="referencePath">XPath to locate references in the given navigator.</param>
    /// <param name="referenceTitle">Title displayed for the reference section.</param>
    /// <returns>
    ///     A dictionary with two widgets if references with and without content exist, or one if only one type is found.
    /// </returns>
    public static StructuredDetails SingleTypeReferencesCollapsibleContent(
        Func<List<XmlDocumentNavigator>, Widget> contentBuilder,
        XmlDocumentNavigator navigator,
        string referencePath,
        string referenceTitle,
        RenderContext context
    )
    {
        var referencesWithContent = GetContentFromReferences(navigator, referencePath);
        var referencesWithoutContent = GetReferencesWithoutContent(navigator, referencePath);

        if (!navigator.EvaluateCondition(referencePath))
        {
            return new StructuredDetails();
        }

        var collapsibleRow = new StructuredDetails();

        if (referencesWithContent.Count > 0)
        {
            collapsibleRow.Add(
                new CollapsibleDetail(
                    new ConstantText(referenceTitle),
                    new MultiReference(contentBuilder, $"{referencePath}/f:reference")
                )
            );
        }

        AddNoContent(referencesWithoutContent, collapsibleRow, referenceTitle, context);

        return collapsibleRow;
    }


    public static List<Widget> BuildCollapserByMultireference(
        Func<List<ReferenceNavigatorOrDisplay>, Widget> contentBuilder,
        XmlDocumentNavigator navigator,
        RenderContext context,
        string referencePath,
        string referenceTitle
    )
    {
        if (context.RenderMode == RenderMode.Documentation)
        {
            return [new ConstantText(navigator.SelectSingleNode(referencePath).GetFullPath())];
        }

        var referencesWithResolvedResources = GetContentFromReferences(navigator, referencePath);
        var referencesWithDisplay = GetReferencesWithDisplayValue(navigator, referencePath);
        var referencesWithoutContent = GetReferencesWithoutContent(navigator, referencePath);

        if (!navigator.EvaluateCondition(referencePath))
        {
            return new StructuredDetails().Build();
        }

        var collapsibleRow = new StructuredDetails();

        if (referencesWithResolvedResources.Count != 0)
        {
            collapsibleRow.Add(
                new CollapsibleDetail(
                    new ConstantText(referenceTitle),
                    new MultiReference(
                        navs => contentBuilder(navs.Select(y => new ReferenceNavigatorOrDisplay(y))
                            .ToList()),
                        $"{referencePath}/f:reference")
                )
            );
        }

        if (referencesWithDisplay.Count != 0)
        {
            var displayValues = referencesWithDisplay.Select(x => x.SelectSingleNode("f:display/@value").Node?.InnerXml)
                .WhereNotNull().ToList();
            var referenceData = displayValues.Select(x => new ReferenceNavigatorOrDisplay(x)).ToList();
            collapsibleRow.Add(
                new CollapsibleDetail(
                    new ConstantText($"{referenceTitle} - text"),
                    contentBuilder(referenceData)
                )
            );
        }

        AddNoContent(referencesWithoutContent, collapsibleRow, referenceTitle, context);

        return collapsibleRow.Build();
    }


    /// <summary>
    ///     Retrieves references with resolved referenced resources from the given XML navigator based on the specified
    ///     reference path.
    /// </summary>
    /// <returns>
    ///     A list of the actual resolved resources associated with each reference
    /// </returns>
    public static List<XmlDocumentNavigator> GetContentFromReferences(
        XmlDocumentNavigator navigator,
        string referencePath
    )
    {
        return FilterReferences(referencePath, navigator, ReferenceResolveType.HasReferencedResource);
    }

    /// <summary>
    ///     Gets a list of key-value pairs where the key leads to the "original" navigator
    ///     (the one that contains the `reference` element)
    ///     and the value has the navigator with the referenced content (resource)
    /// </summary>
    public static List<KeyValuePair<XmlDocumentNavigator, XmlDocumentNavigator>> GetReferencesWithContent(
        XmlDocumentNavigator navigator,
        string referencePath
    )
    {
        var references = navigator.SelectAllNodes(referencePath).ToList();
        List<KeyValuePair<XmlDocumentNavigator, XmlDocumentNavigator>> results = [];

        foreach (var originalNavigator in references)
        {
            var fullId = originalNavigator.SelectSingleNode("f:reference/@value").Node?.InnerXml;

            if (string.IsNullOrEmpty(fullId))
            {
                continue;
            }

            if (!TryBuildReferenceXpath(fullId, out var xpath))
            {
                continue;
            }

            var resource = navigator.SelectSingleNode(xpath);

            if (resource.Node == null)
            {
                continue;
            }


            results.Add(
                new KeyValuePair<XmlDocumentNavigator, XmlDocumentNavigator>(originalNavigator, resource));
        }

        return results;
    }

    /// <summary>
    ///     Retrieves references without content (resolved resource or display value) from the given XML navigator based on the
    ///     specified reference path.
    /// </summary>
    /// <returns>
    ///     A list of references definitions that do not have associated content.
    /// </returns>
    public static List<XmlDocumentNavigator> GetReferencesWithoutContent(
        XmlDocumentNavigator navigator,
        string referencePath
    )
    {
        return FilterReferences(referencePath, navigator, ReferenceResolveType.NoContent);
    }

    /// <summary>
    ///     Retrieves references with display value only from the given XML navigator based on the specified reference path.
    /// </summary>
    /// <returns>
    ///     A list of references definitions that only have associated display value.
    /// </returns>
    public static List<XmlDocumentNavigator> GetReferencesWithDisplayValue(
        XmlDocumentNavigator navigator,
        string referencePath
    )
    {
        return FilterReferences(referencePath, navigator, ReferenceResolveType.HasDisplay);
    }

    /// <summary>
    ///     Builds a widget displaying the name of a given XML reference.
    ///     If the reference has content, by default, it expects the name to be a type code.
    ///     If the reference does not have a defined name, it provides a fallback message.
    /// </summary>
    /// <param name="reference">The XML reference to extract the name from.</param>
    /// <param name="context">Rendering context.</param>
    /// <param name="doesExist">Indicates whether the reference has associated content.</param>
    /// <param name="pathToExistingReference">
    ///     To specify the path to the name value within existing reference, default:
    ///     "f:code/f:coding/f:display/@value"
    /// </param>
    /// <returns>A widget displaying the reference name or an appropriate fallback message.</returns>
    public static Widget BuildReferenceNameWidget(
        XmlDocumentNavigator reference,
        RenderContext context,
        bool doesExist = true,
        string pathToExistingReference = "f:code/f:coding/f:display"
    )
    {
        var id = reference.SelectSingleNode("f:reference/@value");
        var name = doesExist
            ? reference.SelectSingleNode($"{pathToExistingReference}/@value")
            : reference.SelectSingleNode("f:display/@value").Node != null
                ? reference.SelectSingleNode("f:display/@value")
                : id;

        var widget = name.Node?.InnerXml != null
            ? new ConstantText(name.Node?.InnerXml!)
            : new ConstantText(id.Node?.InnerXml ?? "Chybějící jméno a id reference");

        if (context.RenderMode == RenderMode.Documentation)
        {
            return new ConstantText(name.GetFullPath());
        }

        if (!doesExist)
        {
            return new TextContainer(TextStyle.Regular,
            [
                widget, new TextContainer(TextStyle.Muted,
                [
                    new ConstantText(" "),
                    new Tooltip([], [
                        new ConstantText("Reference na chybějící obsah"),
                    ], icon: new Icon(SupportedIcons.Gear)),
                ]),
            ]);
        }

        return widget;
    }

    private static List<XmlDocumentNavigator> FilterReferences(
        string referencePath,
        XmlDocumentNavigator navigator,
        ReferenceResolveType resolveType
    )
    {
        var references = navigator.SelectAllNodes(referencePath).ToList();
        List<XmlDocumentNavigator> referencesWithResolvedResource = [];
        List<XmlDocumentNavigator> referencesWithDisplay = [];
        List<XmlDocumentNavigator> referencesWithoutContent = [];

        if (references.Count <= 0)
        {
            return [];
        }

        foreach (var reference in references)
        {
            var display = reference.SelectSingleNode("f:display/@value").Node?.InnerXml;
            var referenceUri = reference.SelectSingleNode("f:reference/@value").Node?.InnerXml;

            if (string.IsNullOrEmpty(referenceUri))
            {
                if (string.IsNullOrEmpty(display))
                {
                    referencesWithoutContent.Add(reference);
                }
                else
                {
                    referencesWithDisplay.Add(reference);
                }

                continue;
            }

            if (!TryBuildReferenceXpath(referenceUri, out var xpath))
            {
                referencesWithoutContent.Add(reference);
                continue;
            }

            var resourceExists = navigator.EvaluateCondition(xpath);

            if (resourceExists)
            {
                var resource = navigator.SelectSingleNode(xpath);
                referencesWithResolvedResource.Add(resource);
            }
            else
            {
                referencesWithoutContent.Add(reference);
            }
        }

        switch (resolveType)
        {
            case ReferenceResolveType.HasReferencedResource:
                return referencesWithResolvedResource;
            case ReferenceResolveType.HasDisplay:
                return referencesWithDisplay;
            case ReferenceResolveType.NoContent:
                return referencesWithoutContent;
            default:
                throw new ArgumentOutOfRangeException(nameof(resolveType), resolveType, null);
        }
    }


    /// <summary>
    ///     Mostly based on https://build.fhir.org/bundle.html#references, but differs in handling relative references.
    ///     The described algorithm did not work in most example documents we have available, because fullUrl is either not
    ///     present or is an oid.
    ///     For this reason, we try to find the resources based on only resource type and id instead.
    /// </summary>
    /// <param name="referenceUri">urn, absolute, relative, or local uri identifying the required entry/resource</param>
    /// <param name="xpath">
    ///     Outputs the xpath leading to the required resource if the referenceUri is valid, empty string
    ///     otherwise
    /// </param>
    /// <returns>true when referenceUri is valid (but not necessarily present), false otherwise</returns>
    public static bool TryBuildReferenceXpath(string referenceUri, out string xpath)
    {
        // Purely local references - look for a "contained" resource within the current resource
        if (referenceUri.StartsWith('#'))
        {
            var id = referenceUri[1..];
            xpath = $"ancestor::f:resource[position() = 1]/descendant::f:contained/*[f:id/@value='{id}']";
            return true;
        }

        // Urns - look for a resource with a fullUrl that matches the given URN
        if (referenceUri.StartsWith("urn:", StringComparison.InvariantCultureIgnoreCase))
        {
            xpath = $"/f:Bundle/f:entry[f:fullUrl/@value='{referenceUri}']/f:resource/*";

            return true;
        }

        var regex = ReferenceRegex();
        var match = regex.Match(referenceUri);
        if (!match.Success)
        {
            xpath = string.Empty;
            return false;
        }

        var baseUrl = match.Groups["baseUrl"].Value;
        var versionlessUrl = match.Groups["versionlessUrl"].Value;
        var resourceType = match.Groups["resourceType"].Value;
        var resourceId = match.Groups["id"].Value;
        var versionId = match.Groups["versionId"].Value;
        var localId = match.Groups["localId"].Value;

        // Resource type and id are the only mandatory parts of a non-local, non-urn reference. Without them, the reference is invalid.
        if (string.IsNullOrEmpty(resourceType) || string.IsNullOrEmpty(resourceId))
        {
            xpath = string.Empty;
            return false;
        }

        var path = new StringBuilder();


        // Find the correct entry
        // For absolute references, look for a fullUrl that matches the given URL.
        if (!string.IsNullOrEmpty(baseUrl))
        {
            path.Append($"/f:Bundle/f:entry[f:fullUrl/@value='{versionlessUrl}']");
        }
        // For relative references, look for a resource with a matching resource type and id, or a matching fullUrl
        else
        {
            path.Append(
                $"/f:Bundle/f:entry[f:resource/f:{resourceType}/f:id/@value='{resourceId}'or f:fullUrl/@value='{versionlessUrl}']"
            );
        }

        // Go inside the actual resource
        path.Append("/f:resource/*");

        // If the reference is versioned, look for a versionId that matches the given version.
        if (!string.IsNullOrEmpty(versionId))
        {
            path.Append($"[f:meta/f:versionId/@value='{versionId}']");
        }

        // If the reference contains a local ID, go inside a contained resource with a matching id.
        if (!string.IsNullOrEmpty(localId))
        {
            path.Append($"/f:contained/*[f:id/@value='{localId}']");
        }

        xpath = path.ToString();
        return true;
    }

    /// ///
    /// <summary>
    ///     Constructs a widget that displays the name of an individual based on a given XML reference.
    ///     This is specifically for individual resource types such as Patient, Practitioner, Person, or Group.
    ///     For more details, see: https://hl7.org/fhir/R4/resourcelist.html
    /// </summary>
    /// <param name="navigator">The XML navigator pointing to the specific reference node.</param>
    /// <param name="context">Rendering context.</param>
    /// <param name="referenceType">The XPath to the reference (e.g., "f:performer").</param>
    /// <returns>
    ///     A widget containing the full name of the referenced individual (if available), their ID if the name is not
    ///     available,
    ///     or the string "Unknown" if the reference cannot be resolved.
    /// </returns>
    public static Widget BuildIReferenceNameByHumanNameType(
        XmlDocumentNavigator navigator,
        RenderContext context,
        string referenceType
    )
    {
        var referencesWithContent = GetContentFromReferences(navigator, referenceType);
        if (referencesWithContent.Count == 1)
        {
            var referencedResource = referencesWithContent.First();
            var givenName = BuildReferenceNameWidget(referencedResource, context, true, "f:name/f:given");
            var familyName = BuildReferenceNameWidget(referencedResource, context, true, "f:name/f:family");
            return new TextContainer(TextStyle.Regular,
                [givenName, new ConstantText(" "), familyName]);
        }

        var referencesWithoutContent = GetReferencesWithoutContent(navigator, referenceType);
        if (referencesWithoutContent.Count == 1)
        {
            return BuildReferenceNameWidget(referencesWithoutContent[0], context, false);
        }

        return new TextContainer(TextStyle.Muted, [new ConstantText("Neznámé jméno")]);
    }

    private static void AddNoContent(
        List<XmlDocumentNavigator> referencesWithoutContent,
        StructuredDetails collapsibleRow,
        string referenceTitle,
        RenderContext context
    )
    {
        if (referencesWithoutContent.Count == 0)
        {
            return;
        }

        var textList = new List<Widget>();
        foreach (var reference in referencesWithoutContent)
        {
            textList.Add(BuildReferenceNameWidget(reference, context, doesExist: false));
            textList.Add(new LineBreak());
        }

        collapsibleRow.Add(
            new CollapsibleDetail(
                new ConstantText($"{referenceTitle} s chybějícím obsahem"),
                new Table(textList)
            )
        );
    }

    private enum ReferenceResolveType
    {
        HasReferencedResource,
        HasDisplay,
        NoContent,
    }

    public static string GetReferenceName(
        string referencePath,
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var reference = navigator.SelectSingleNode($"{referencePath}/@value").Node?.InnerXml;

        var typeNode = navigator.SelectSingleNode($"{referencePath}/../f:type/@value").Node;

        if (typeNode != null)
        {
            return typeNode.Value;
        }

        if (reference == null)
        {
            return string.Empty;
        }

        // Handle cases where reference starts with '#' (contained resource)
        if (reference.StartsWith('#'))
        {
            var id = reference[1..];
            var containedNode =
                navigator.SelectSingleNode(
                    $"ancestor::f:resource[position() = 1]/descendant::f:contained/*[f:id/@value='{id}']");
            return containedNode.Node != null
                ?
                // Return the local name of the contained resource element
                new ChangeContext(reference, new LocalNodeName()).Render(navigator, renderer, context).Result.Content ??
                string.Empty
                : string.Empty;
        }

        // Handle standard references like "Practitioner/example"
        var parts = reference.Split('/');
        return parts.Length >= 1
            ?
            // Return the first part (resource type) in lowercase
            parts[0]
            :
            // If we can't parse the reference, return empty string
            string.Empty;
    }

    /// <summary>
    ///     Builds a list of reference widgets, both with and without embedded content.
    ///     Handles:
    ///     - Extracting display names, identifiers, URLs, and human-readable names.
    ///     - Providing fallbacks when display names are missing.
    ///     - Structuring and styling reference outputs semantically for UI rendering.
    ///     - Managing XPath logic encapsulation.
    ///     The resulting list groups widgets semantically—each index contains one or more widgets wrapped
    ///     in a container to represent a distinct reference block.
    /// </summary>
    /// <param name="customFallbackName">Fallback name widget to use if regular fallback fails, before identifier  fallback</param>
    /// <param name="widgetModel">Specify how the result should be rendered.</param>
    /// <returns>
    ///     A list of <see cref="Widget" /> elements, each wrapped in a <see cref="Container" /> to represent
    ///     a semantically separated reference group suitable for rendering.
    /// </returns>
    public static Widget BuildAnyReferencesNaming(
        XmlDocumentNavigator navigator,
        string path,
        RenderContext context,
        IWidgetRenderer renderer,
        bool showOptionalDetails = false,
        Widget? customFallbackName = null,
        ReferenceNamingWidgetModel? widgetModel = null
    )
    {
        if (context.RenderMode == RenderMode.Documentation)
        {
            List<string> namePathsForDocumentation = [];

            var withContentDocumentation = GetReferencesWithContent(navigator, path);
            foreach (var (originalNav, reference) in withContentDocumentation)
            {
                var displayNameNav = originalNav.SelectSingleNode("f:display/@value");
                if (displayNameNav.Node != null)
                {
                    namePathsForDocumentation.Add(displayNameNav.GetFullPath());
                    continue;
                }

                var fallback = GetFallbackDisplayName(reference, reference.Node?.Name, showOptionalDetails);

                if (fallback.Navigator != null)
                {
                    namePathsForDocumentation.Add(fallback.Navigator.GetFullPath());
                }
            }

            var noContentTotalDocumentation = GetReferencesWithoutContent(navigator, path)
                .Concat(GetReferencesWithDisplayValue(navigator, path));
            foreach (var reference in noContentTotalDocumentation)
            {
                var displayNav = reference.SelectSingleNode("f:display/@value");
                if (displayNav.Node != null)
                {
                    namePathsForDocumentation.Add(displayNav.GetFullPath());
                    continue;
                }

                var fallback = GetFallbackDisplayName(reference, showKind: showOptionalDetails);
                if (fallback.Navigator != null)
                {
                    namePathsForDocumentation.Add(fallback.Navigator.GetFullPath());
                }
            }

            return namePathsForDocumentation.Count > 0
                ? new ConstantText(string.Join(", ", namePathsForDocumentation))
                : new ConstantText(navigator.GetFullPath());
        }

        List<Widget> resultSeparatedSemantically = [];
        var withContent = GetReferencesWithContent(navigator, path);
        var withoutContent = GetReferencesWithoutContent(navigator, path);
        var withoutContentWithDisplayName = GetReferencesWithDisplayValue(navigator, path);

        foreach (var (originalNav, reference) in withContent)
        {
            var refTypeWidget = BuildMutedWidget(new ChangeContext(reference, new LocalNodeName())
                .Render(navigator, renderer, context).Result.Content);
            List<Widget> resultAltogether = [];

            string? resourceHref = null;
            Widget? hrefErrorMessage = null;

            if (ResourceIdentifier.TryFromNavigator(reference, out var identifier))
            {
                if (context.TryGetReferenceHref(identifier, out var href, out var error))
                {
                    resourceHref = href;
                }
                else
                {
                    var message = error switch
                    {
                        // The first two cases shouldn't happen, since we're only checking valid references
                        GetResourceHrefError.MissingId => "Chybějící / neplatný identifikátor",
                        GetResourceHrefError.MissingReferencedResource => "Reference na chybějící obsah",
                        GetResourceHrefError.NonUniqueReference => "Nejednoznačná reference",
                        _ => throw new ArgumentOutOfRangeException(),
                    };

                    hrefErrorMessage = BuildMutedWidget(message);
                }
            }

            var displayName = originalNav.SelectSingleNode("f:display/@value").Node?.InnerXml;
            Widget? resourceSummaryLabel = null;
            Widget? resourceSummaryValue = null;

            if (reference.Node?.Name is { } nodeName)
            {
                resourceSummaryLabel = new LocalNodeName(nodeName);

                var (summaryLabel, summaryValue) = GetResourceSummary(reference);
                if (summaryLabel != null)
                {
                    resourceSummaryLabel = summaryLabel;
                }

                resourceSummaryValue = summaryValue;
            }

            if (!string.IsNullOrEmpty(displayName))
            {
                resultAltogether.Add(new ConstantText(displayName));
                if (resourceHref == null)
                {
                    resultAltogether.Add(refTypeWidget);
                    if (hrefErrorMessage != null)
                    {
                        resultAltogether.Add(hrefErrorMessage);
                    }
                }
            }
            else if (resourceSummaryValue != null)
            {
                resultAltogether.Add(resourceSummaryValue);
            }
            else
            {
                var fallbackName = GetFallbackDisplayName(reference, reference.Node?.Name, showOptionalDetails,
                    customFallbackName: customFallbackName).Widget;

                if (fallbackName != null)
                {
                    resultAltogether.Add(fallbackName);
                }

                if (hrefErrorMessage != null)
                {
                    resultAltogether.Add(hrefErrorMessage);
                }
            }


            Widget result;
            if (resourceHref == null)
            {
                result = new Concat(resultAltogether);
            }
            else if (widgetModel?.Type == ReferenceNamingWidgetType.NameValuePair)
            {
                if (widgetModel.LabelOverride != null)
                {
                    resourceSummaryLabel = widgetModel.LabelOverride;
                }

                result = new NameValuePair(
                    resourceSummaryLabel ?? new NullWidget(),
                    new Link(new Concat(resultAltogether), resourceHref, optionalClass: "d-inline-block"),
                    direction: widgetModel.Direction,
                    size: widgetModel.Size,
                    style: widgetModel.Style
                );
            }
            else
            {
                result = new Link(new Concat(resultAltogether), resourceHref, optionalClass: "d-inline-block");
            }

            resultSeparatedSemantically.Add(result);
        }

        List<XmlDocumentNavigator> noContentTotal = [..withoutContent, ..withoutContentWithDisplayName];
        foreach (var reference in noContentTotal)
        {
            var idType = reference.SelectSingleNode("f:reference/@value")?.Node?.InnerXml.Split('/')[0];
            var localizedType = new LocalNodeName(idType);
            var refTypeWidget = BuildMutedWidget(localizedType);
            List<Widget> resultAltogether = [];

            var displayVal = reference.SelectSingleNode("f:display/@value")?.Node?.InnerXml;

            var display = reference.EvaluateCondition("f:display")
                ? displayVal != null ? new ConstantText($"{displayVal} ") : null
                : null;

            var naming = GetFallbackDisplayName(reference, showKind: showOptionalDetails,
                customFallbackName: customFallbackName);
            var contentToDisplay = display ?? naming.Widget;

            if (contentToDisplay != null)
            {
                resultAltogether.Add(new Container([contentToDisplay], ContainerType.Span));
                if (display != null && idType != null)
                {
                    resultAltogether.Add(refTypeWidget);
                }

                resultAltogether.Add(
                    new Concat([
                        new ConstantText(" "),
                        new Tooltip([], [
                            new TextContainer(TextStyle.Muted, new ConstantText("Reference na chybějící obsah")),
                        ], icon: new Icon(SupportedIcons.Gear)),
                    ])
                );
            }
            else
            {
                var fallbackName = BuildReferenceNameWidget(reference, context, false);
                resultAltogether.Add(fallbackName);
            }

            resultSeparatedSemantically.Add(new Container(resultAltogether,
                optionalClass: "d-inline-flex flex-wrap column-gap-1 align-items-center"));
        }

        return new Concat(resultSeparatedSemantically, ", ");
    }

    public static (Widget? Label, Widget? Value) GetResourceSummary(XmlDocumentNavigator resource)
    {
        var nodeName = resource.Node?.Name;
        if (nodeName == null || !SupportedResourceProvider.SupportedResources.TryGetValue(nodeName, out var descriptor))
        {
            return (null, null);
        }

        var summaryModel = descriptor.RenderSummary(resource);
        return (summaryModel?.Label, summaryModel?.Value);
    }

    private static Widget BuildMutedWidget(string? typeName) =>
        BuildMutedWidget(new ConstantText($" ({typeName}) "));

    private static Widget BuildMutedWidget(Widget content) =>
        new HideableDetails(
            ContainerType.Span,
            new TextContainer(TextStyle.Muted, [content])
        );

    public static (XmlDocumentNavigator? Navigator, Widget? Widget) GetFallbackDisplayName(
        XmlDocumentNavigator reference,
        string? typeName = null,
        bool showKind = false,
        Widget? customFallbackName = null
    )
    {
        //HumanName
        var humanNameSummary = ResourceSummaryUtils.SummaryByHumanName(reference);
        if (humanNameSummary != null)
        {
            var name = reference.SelectSingleNode("f:name");
            return (name, humanNameSummary.Value);
        }

        //Name 
        if (reference.EvaluateCondition("f:name/@value"))
        {
            var name = reference.SelectSingleNode("f:name/@value");
            return (name, new ConstantText(name.Node?.Value ?? ""));
        }

        //Codeable 
        if (reference.EvaluateCondition("f:code/f:coding/f:display/@value"))
        {
            var codeable = reference.SelectSingleNode("f:code/f:coding/f:display/@value");
            return (codeable, new ConstantText(codeable.Node?.Value ?? ""));
        }

        //Url 
        if (reference.EvaluateCondition("f:url/@value"))
        {
            var urlNode = reference.SelectSingleNode("f:url/@value");
            var url = urlNode.Node?.Value;
            return (urlNode, url != null
                ? new TextContainer(TextStyle.Regular,
                [
                    new If(_ => showKind,
                        new TextContainer(TextStyle.Muted, [new ConstantText("(URL)")])
                    ),

                    new TextContainer(TextStyle.Regular, [new ConstantText(url)]),
                ])
                : null);
        }

        // if a custom name is present, prefer it to an identifier 
        if (customFallbackName != null)
        {
            return (null, customFallbackName);
        }

        //Identifier
        if (reference.EvaluateCondition("f:identifier/f:value/@value"))
        {
            var identifierNode = reference.SelectSingleNode("f:identifier");

            return (identifierNode, new ChangeContext(identifierNode, new ShowIdentifier()));
        }

        //ID fallback 
        if (reference.EvaluateCondition("f:id/@value"))
        {
            var idNode = reference.SelectSingleNode("f:id/@value");
            var id = idNode.Node?.InnerXml;
            return
                (idNode,
                    new TextContainer(TextStyle.Regular, [
                        new If(_ => showKind,
                            new TextContainer(TextStyle.Muted, [new ConstantText("(Technický identifikátor)")]),
                            new ConstantText(" ")
                        ),
                        new TextContainer(TextStyle.Regular, [new ConstantText($"{typeName}/{id}")]),
                    ])
                );
        }

        //ID from reference fallback 
        if (reference.EvaluateCondition("f:reference/@value"))
        {
            var idNode = reference.SelectSingleNode("f:reference/@value");
            var id = idNode.Node?.InnerXml;
            return
            (
                idNode,
                new TextContainer(TextStyle.Regular, [
                    new If(_ => showKind,
                        new TextContainer(TextStyle.Muted, [new ConstantText("(Technický identifikátor)")]),
                        new ConstantText(" ")
                    ),
                    new TextContainer(TextStyle.Regular, [new ConstantText($"{id}")]),
                ])
            );
        }

        return (null, null);
    }


    /// <summary>
    ///     Retrieves the XMLDocumentNavigator of a single node from a referenced resource.
    /// </summary>
    /// <param name="navigator">The XML navigator positioned at the element containing the reference.</param>
    /// <param name="referencePath">The XPath to the reference element (e.g., "f:subject").</param>
    /// <param name="nodePath">The XPath to the desired node within the referenced resource (e.g., "f:birthDate/@value").</param>
    /// <returns>
    ///     The XMLDocumentNavigator of the node if found and the reference resolves to exactly one resource; otherwise,
    ///     null.
    /// </returns>
    public static XmlDocumentNavigator? GetSingleNodeNavigatorFromReference(
        XmlDocumentNavigator navigator,
        string referencePath,
        string nodePath
    )
    {
        var referencesWithContent = GetContentFromReferences(navigator, referencePath);

        if (referencesWithContent.Count != 1)
        {
            return null;
        }

        var referencedResourceNavigator = referencesWithContent[0];
        var node = referencedResourceNavigator.SelectSingleNode(nodePath);
        return node;
    }

    /// <summary>
    ///     Retrieves the XMLDocumentNavigators of multiple nodes from referenced resources.
    /// </summary>
    /// <param name="navigator">The XML navigator positioned at the element containing the references.</param>
    /// <param name="referencePath">The XPath to the reference element(s) (e.g., "f:subject").</param>
    /// <param name="nodePath">The XPath to the desired node(s) within each referenced resource (e.g., "f:birthDate/@value").</param>
    /// <returns>
    ///     A list of XMLDocumentNavigators for all matching nodes across all resolved references.
    ///     Returns an empty list if no references resolve or no matching nodes are found.
    /// </returns>
    public static List<XmlDocumentNavigator>? GetNodeNavigatorsFromReferences(
        XmlDocumentNavigator navigator,
        string referencePath,
        string nodePath
    )
    {
        var result = new List<XmlDocumentNavigator>();
        var referencesWithContent = GetContentFromReferences(navigator, referencePath);


        foreach (var nodes in referencesWithContent.Select(referencedResourceNavigator =>
                     referencedResourceNavigator.SelectAllNodes(nodePath).ToList()))
        {
            result.AddRange(nodes);
        }

        return result.Count == 0 ? null : result;
    }

    /// <summary>
    ///     Taken from https://build.fhir.org/references.html, added capture groups baseUrl, versionlessUrl, resourceType,
    ///     versionId, localId, id
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(
        """(?<versionlessUrl>(?<baseUrl>(http|https):\/\/([A-Za-z0-9\-\\\.\:\%\$]*\/)+)?(?<resourceType>Account|ActivityDefinition|AdverseEvent|AllergyIntolerance|Appointment|AppointmentResponse|AuditEvent|Basic|Binary|BiologicallyDerivedProduct|BodyStructure|Bundle|CapabilityStatement|CarePlan|CareTeam|CatalogEntry|ChargeItem|ChargeItemDefinition|Claim|ClaimResponse|ClinicalImpression|CodeSystem|Communication|CommunicationRequest|CompartmentDefinition|Composition|ConceptMap|Condition|Consent|Contract|Coverage|CoverageEligibilityRequest|CoverageEligibilityResponse|DetectedIssue|Device|DeviceDefinition|DeviceMetric|DeviceRequest|DeviceUseStatement|DiagnosticReport|DocumentManifest|DocumentReference|EffectEvidenceSynthesis|Encounter|Endpoint|EnrollmentRequest|EnrollmentResponse|EpisodeOfCare|EventDefinition|Evidence|EvidenceVariable|ExampleScenario|ExplanationOfBenefit|FamilyMemberHistory|Flag|Goal|GraphDefinition|Group|GuidanceResponse|HealthcareService|ImagingStudy|Immunization|ImmunizationEvaluation|ImmunizationRecommendation|ImplementationGuide|InsurancePlan|Invoice|Library|Linkage|List|Location|Measure|MeasureReport|Media|Medication|MedicationAdministration|MedicationDispense|MedicationKnowledge|MedicationRequest|MedicationStatement|MedicinalProduct|MedicinalProductAuthorization|MedicinalProductContraindication|MedicinalProductIndication|MedicinalProductIngredient|MedicinalProductInteraction|MedicinalProductManufactured|MedicinalProductPackaged|MedicinalProductPharmaceutical|MedicinalProductUndesirableEffect|MessageDefinition|MessageHeader|MolecularSequence|NamingSystem|NutritionOrder|Observation|ObservationDefinition|OperationDefinition|OperationOutcome|Organization|OrganizationAffiliation|Patient|PaymentNotice|PaymentReconciliation|Person|PlanDefinition|Practitioner|PractitionerRole|Procedure|Provenance|Questionnaire|QuestionnaireResponse|RelatedPerson|RequestGroup|ResearchDefinition|ResearchElementDefinition|ResearchStudy|ResearchSubject|RiskAssessment|RiskEvidenceSynthesis|Schedule|SearchParameter|ServiceRequest|Slot|Specimen|SpecimenDefinition|StructureDefinition|StructureMap|Subscription|Substance|SubstanceNucleicAcid|SubstancePolymer|SubstanceProtein|SubstanceReferenceInformation|SubstanceSourceMaterial|SubstanceSpecification|SupplyDelivery|SupplyRequest|Task|TerminologyCapabilities|TestReport|TestScript|ValueSet|VerificationResult|VisionPrescription)\/(?<id>[A-Za-z0-9\-\.]{1,64}))(\/_history\/(?<versionId>[A-Za-z0-9\-\.]{1,64}))?(#(?<localId>[A-Za-z0-9\-\.]{1,64}))?""",
        RegexOptions.Compiled
    )]
    private static partial Regex ReferenceRegex();
}