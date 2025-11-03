using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Observation;

/// <summary>
///     Automatically selects the best Observation widget for the given resource, based on its profile
/// </summary>
public class ObservationResource : IResourceWidget
{
    public static string ResourceType => "Observation";

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
            new AlternatingBackgroundColumn(
                [
                    ..otherObs.Select(x => new ChangeContext(
                            x,
                            new ObservationCard()
                        )
                    )
                ]
            ),
        ];
    }
}