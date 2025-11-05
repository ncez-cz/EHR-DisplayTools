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
        //// Observation - Anthropometric Data
            if (navigator.EvaluateCondition(
                    "f:code/f:coding[f:system/@value='http://loinc.org' and (f:code/@value='39156-5' or f:code/@value='56086-2' or f:code/@value='8280-0' or f:code/@value='9843-4' or f:code/@value='8302-2' or f:code/@value='29463-7')]"))
            {
                var valueQuantityNav = navigator.SelectSingleNode("f:valueQuantity");
                if (!IsNavigatorNullOrEmpty(valueQuantityNav))
                {
                    return new ResourceSummaryModel
                    {
                        Value = new NameValuePair(
                            new ChangeContext(
                                navigator.SelectSingleNode("f:code[f:coding/f:system/@value='http://loinc.org']"),
                                new CodeableConcept()),
                            new ChangeContext(valueQuantityNav, new ShowQuantity())),
                    };
                }
            }

            //// Observation - Infectious contact
            if (navigator.EvaluateCondition(
                    "f:code/f:coding[f:system/@value='http://terminology.hl7.org/CodeSystem/v3-ParticipationType' and f:code/@value='EXPAGNT']"))
            {
                var valueNav = navigator.SelectSingleNode("*[starts-with(local-name(), 'value')]");
                if (!IsNavigatorNullOrEmpty(valueNav) &&
                    valueNav.Node?.Name !=
                    "valueSampledData") // ignore if value is missing or is SampleData - cannot create link text out of a chart
                {
                    return new ResourceSummaryModel
                    {
                        Value = new NameValuePair(
                            new ChangeContext(navigator.SelectSingleNode("f:code"), new CodeableConcept()),
                            new ChangeContext(navigator, new OpenTypeElement(null))),
                    };
                        
                }
            }

            //// Observation - SDOH
            if (navigator.EvaluateCondition(
                    "f:category/f:coding[f:system/@value='http://terminology.hl7.org/CodeSystem/observation-category' and f:code/@value='social-history']"))
            {
                var valueNav = navigator.SelectSingleNode("*[starts-with(local-name(), 'value')]");
                if (!IsNavigatorNullOrEmpty(valueNav) &&
                    valueNav.Node?.Name !=
                    "valueSampledData") // ignore if value is missing or is SampleData - cannot create link text out of a chart
                {
                    return new ResourceSummaryModel
                    {
                        Value = new NameValuePair(
                            new ChangeContext(navigator.SelectSingleNode("f:code"), new CodeableConcept()),
                            new ChangeContext(navigator, new OpenTypeElement(null)))
                    };
                }
            }

            //// Observation - travel history
            if (navigator.EvaluateCondition(
                    "f:code/f:coding[f:system/@value='http://loinc.org' and f:code/@value='94651-7']"))
            {
                var valueNav = navigator.SelectSingleNode("f:valueCodeableConcept");
                if (!IsNavigatorNullOrEmpty(valueNav))
                {
                    return new ResourceSummaryModel
                    {
                        Value = new NameValuePair(
                            new ChangeContext(navigator.SelectSingleNode("f:code"), new CodeableConcept()),
                            new ChangeContext(valueNav, new CodeableConcept()))
                    };
                }
            }

            //// Observation - Laboratory
            if (navigator.EvaluateCondition(CzLaboratoryObservation.XPathCondition))
            {
                var valueNav = navigator.SelectSingleNode("*[starts-with(local-name(), 'value')]");
                if (!IsNavigatorNullOrEmpty(valueNav) &&
                    valueNav.Node?.Name !=
                    "valueSampledData") // ignore if value is missing or is SampleData - cannot create link text out of a chart
                {
                    return new ResourceSummaryModel
                    {
                        Value = new NameValuePair(
                            new ChangeContext(navigator.SelectSingleNode("f:code"), new CodeableConcept()),
                            new ChangeContext(navigator, new OpenTypeElement(null))),
                    };
                }
            }

            //// Observation - Imaging Order is skipped - no obvious way to detect it

            //// Observation - Imaging Report
            if (navigator.EvaluateCondition(
                    "f:identifier/f:type/f:coding[f:system/@value='https://hl7.cz/fhir/img/CodeSystem/codesystem-missing-dicom-terminology' and f:code/@value='00080018']"))
            {
                var valueNav = navigator.SelectSingleNode("*[starts-with(local-name(), 'value')]");
                if (!IsNavigatorNullOrEmpty(valueNav) &&
                    valueNav.Node?.Name !=
                    "valueSampledData") // ignore if value is missing or is SampleData - cannot create link text out of a chart
                {
                    return new ResourceSummaryModel
                    {
                        Value = new NameValuePair(
                            new ChangeContext(navigator.SelectSingleNode("f:code"), new CodeableConcept()),
                            new ChangeContext(navigator, new OpenTypeElement(null))),
                    };
                }
            }

            //// Observation - Laboratory Order is skipped - no obvious way to detect it

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