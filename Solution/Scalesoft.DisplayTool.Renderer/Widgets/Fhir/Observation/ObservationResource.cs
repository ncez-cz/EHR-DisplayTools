using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Observation;

/// <summary>
///     Automatically selects the best Observation widget for the given resource, based on its profile
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class ObservationResource : IResourceWidget
{
    public static string ResourceType => "Observation";

    public static ResourceSummaryModel? RenderSummary(XmlDocumentNavigator navigator)
    {
        Widget? label = null;

        var infrequentProperties = InfrequentProperties.Evaluate<ObservationInfrequentProperties>(navigator);

        if (navigator.EvaluateCondition("f:code"))
        {
            label = new ChangeContext(navigator, "f:code", new CodeableConcept());
        }

        if (infrequentProperties.Contains(ObservationInfrequentProperties.Value) &&
            !navigator.EvaluateCondition("f:valueSampledData")) // use observation value, except for charts
        {
            return new ResourceSummaryModel
            {
                Label = label,
                Value = new ChangeContext(navigator, new OpenTypeElement(null)),
            };
        }

        if (label != null)
        {
            return new ResourceSummaryModel
            {
                Value = label,
            };
        }

        return null;
    }

    private static bool IsNavigatorNullOrEmpty(XmlDocumentNavigator nav)
    {
        if (nav.Node == null)
        {
            return true;
        }

        if (nav.EvaluateCondition("@value"))
        {
            if (string.IsNullOrEmpty(nav.Evaluate("@value")))
            {
                return true;
            }

            return false;
        }

        if (string.IsNullOrWhiteSpace(nav.Node.InnerXml))
        {
            return true;
        }

        return false;
    }

    public static bool HasBorderedContainer(Widget resourceWidget)
    {
        if (resourceWidget is If ifWidget) // CzLaboratoryObservation is wrapped in an If widget
        {
            return true;
        }

        if (resourceWidget is AlternatingBackgroundColumn
            alternatingBackgroundColumn) // regular observations are wrapped in an AlternatingBackgroundColumn widget, by themselves they have no border, and if there are no broken references, AlternatingBackgroundColumn also has no border
        {
            return false;
        }

        throw new InvalidOperationException(
            $"Expected {nameof(If)} widget or {nameof(AlternatingBackgroundColumn)} widget,  got {resourceWidget.GetType().Name}");
    }

    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        var labObs = new List<XmlDocumentNavigator>();
        var otherObs = new List<XmlDocumentNavigator>();
        foreach (var item in items)
        {
            if (item.EvaluateCondition(CzLaboratoryObservation.XPathCondition))
            {
                labObs.Add(item);
            }
            else
            {
                otherObs.Add(item);
            }
        }

        return
        [
            new If(_ => labObs.Count != 0, new CzLaboratoryObservation(labObs)),
            new If(_ => labObs.Count != 0 && otherObs.Count != 0,
                new Container([], ContainerType.Div, "my-2", renderEmptyContainer: true)),
            new AlternatingBackgroundColumn(
                [
                    ..otherObs.Select(x => new ChangeContext(
                            x,
                            new ObservationCard()
                        )
                    ),
                ]
            )
        ];
    }
}