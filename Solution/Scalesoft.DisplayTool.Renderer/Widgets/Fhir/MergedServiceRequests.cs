using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class MergedServiceRequests : Widget, IResourceWidget
{
    private readonly IList<XmlDocumentNavigator> m_items;
    private readonly bool m_keepTitleInResource;
    private readonly List<Widget> m_titleAppendWidgets = [];

    public MergedServiceRequests(
        IList<XmlDocumentNavigator> items,
        out Widget titleAppend,
        bool keepTitleInResource = true
    )
    {
        m_items = items;
        m_keepTitleInResource = keepTitleInResource;
        titleAppend = new LazyWidget(() => m_titleAppendWidgets);
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var widgets = new List<Widget>();
        var serviceKvps = m_items.Select(x =>
            new KeyValuePair<ServiceRequestKeys.ServiceRequestKey, XmlDocumentNavigator>(
                ServiceRequestKeyMapper.MapRequestKey(x), x)).ToList();
        var allServiceRequestProps = Enum.GetValues<ServiceRequestProperties>().ToHashSet();

        switch (serviceKvps.Count)
        {
            case 0:
                break;
            case 1:
            {
                var (key, serviceReqNav) = serviceKvps.ElementAt(0);
                var displayedProps = new HashSet<ServiceRequestProperties>(allServiceRequestProps);
                if (key.Occurence != null)
                {
                    m_titleAppendWidgets.Add(
                        new ChangeContext(serviceReqNav,
                            new TextContainer(TextStyle.Light, new Chronometry("occurrence")))
                    );
                    if (!m_keepTitleInResource)
                    {
                        displayedProps.Remove(ServiceRequestProperties.Occurrence);
                    }
                }

                widgets.Add(new ChangeContext(serviceReqNav, new ServiceRequest(displayedProps)));
                break;
            }
            case > 1:
            {
                var sharedProps = new HashSet<ServiceRequestProperties>(allServiceRequestProps);
                var keys = serviceKvps.Select(x => x.Key).ToList();
                for (var i = 0; i < keys.Count - 1; i++)
                {
                    var currentKey = keys[i];
                    var nextKey = keys[i + 1];
                    var compareResult = currentKey.Compare(nextKey);
                    sharedProps.IntersectWith(compareResult);
                }

                var serviceReqNav = serviceKvps.First().Value;
                var sharedDisplayedProps = new HashSet<ServiceRequestProperties>(sharedProps);
                if (sharedDisplayedProps.Contains(ServiceRequestProperties.Occurrence))
                {
                    m_titleAppendWidgets.Add(
                        new ChangeContext(serviceReqNav,
                            new TextContainer(TextStyle.Light, new Chronometry("occurrence")))
                    );
                    if (!m_keepTitleInResource)
                    {
                        sharedDisplayedProps.Remove(ServiceRequestProperties.Occurrence);
                    }
                }

                var differentProps = new HashSet<ServiceRequestProperties>(allServiceRequestProps);
                differentProps.ExceptWith(sharedProps);

                var serviceRequestsOfDifferingProps = serviceKvps.Select(x => x.Value)
                    .Select(x => new ChangeContext(x,
                        new ServiceRequest(differentProps,
                            nameValuePairStyle: NameValuePair.NameValuePairStyle.Secondary)));
                widgets.AddRange(serviceRequestsOfDifferingProps);
                if (sharedDisplayedProps.Count > 0)
                {
                    var serviceRequestOfSharedProps =
                        new ChangeContext(serviceReqNav, new ServiceRequest(sharedDisplayedProps));
                    widgets.Add(new ThematicBreak());
                    widgets.Add(serviceRequestOfSharedProps);
                }

                break;
            }
        }

        return new Container(widgets, optionalClass: "merged-service-requests").Render(navigator, renderer, context);
    }

    public static string ResourceType => "ServiceRequest";

    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return [new MergedServiceRequests(items, out _)];
    }

    public static async Task<(Widget serviceRequest, Widget title)> AppendDate(
        IList<XmlDocumentNavigator> serviceRequestNavs,
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var mergedServiceRequests = new MergedServiceRequests(serviceRequestNavs, out var titleAppend, false);
        var widgetContent = await mergedServiceRequests.Render(navigator, renderer, context);
        var prerenderedWidgetContent = new PassthroughWidget(widgetContent);

        return (prerenderedWidgetContent, titleAppend);
    }
}