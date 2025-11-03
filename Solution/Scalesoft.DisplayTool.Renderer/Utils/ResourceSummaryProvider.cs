using Scalesoft.DisplayTool.Renderer.Widgets;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Observation;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Utils;

public static class ResourceSummaryProvider
{
    public static ResourceSummaryInfo? GetSummary(XmlDocumentNavigator resource, bool showKind = false)
    {
        //PractitionerRole 
        if (resource.Node?.Name == "PractitionerRole" && resource.EvaluateCondition("f:practitioner"))
        {
            XmlDocumentNavigator? practitionerNav = null;
            var content = new List<Widget>();
            var practitionerPresent = false;
            var practitionerNavs = ReferenceHandler.GetReferencesWithContent(resource, "f:practitioner");
            if (practitionerNavs.Count == 1)
            {
                practitionerNav = practitionerNavs.First().Key;
                practitionerPresent = true;
                var practitionerResource = practitionerNavs.First().Value;
                var widget = ReferenceHandler
                    .GetFallbackDisplayName(practitionerResource, practitionerResource.Node?.Name, showKind).Widget;
                if (widget != null)
                {
                    content.Add(widget);
                }
            }

            if (practitionerNav == null)
            {
                var practitionerDisplay = ReferenceHandler.GetReferencesWithDisplayValue(resource, "f:practitioner");
                if (practitionerDisplay.Count == 1)
                {
                    practitionerNav = practitionerDisplay.First();
                    practitionerPresent = true;
                    var display = practitionerNav.SelectSingleNode("f:display/@value").Node?.Value;
                    if (!string.IsNullOrEmpty(display))
                    {
                        content.Add(new ConstantText(display));
                    }
                }
            }

            var practitionerNoContent = ReferenceHandler.GetReferencesWithoutContent(resource, "f:practitioner");
            if (practitionerNoContent.Count != 0)
            {
                practitionerPresent = false;
            }

            if (practitionerPresent)
            {
                if (resource.EvaluateCondition("f:specialty"))
                {
                    content.AddRange([
                        new ConstantText(" ("),
                        new ChangeContext(resource,
                            new CommaSeparatedBuilder("f:specialty", _ => [new CodeableConcept()])),
                        new ConstantText(")"),
                    ]);
                }

                if (resource.EvaluateCondition("f:organization"))
                {
                    var organizationDefined = false;
                    var organizationNavs = ReferenceHandler.GetReferencesWithContent(resource, "f:organization");
                    if (organizationNavs.Count == 1)
                    {
                        var organizationResource = organizationNavs.First().Value;
                        var widget = ReferenceHandler
                            .GetFallbackDisplayName(organizationResource, organizationResource.Node?.Name, showKind)
                            .Widget;
                        if (widget != null)
                        {
                            content.AddRange([
                                new ConstantText(" - "),
                                widget,
                            ]);
                            organizationDefined = true;
                        }
                    }

                    if (!organizationDefined)
                    {
                        var organizationDisplays =
                            ReferenceHandler.GetReferencesWithDisplayValue(resource, "f:organization");
                        if (organizationDisplays.Count == 1)
                        {
                            var organizationDisplay = organizationDisplays.First();
                            var display = organizationDisplay.SelectSingleNode("f:display/@value").Node?.Value;
                            if (!string.IsNullOrEmpty(display))
                            {
                                content.Add(new ConstantText(display));
                            }
                        }
                    }
                    //ignore no reference content
                }
            }

            if (practitionerNav != null && content.Count != 0)
            {
                return new ResourceSummaryInfo(practitionerNav, new Container(content));
            }
        }

        //MedicationStatement
        if (resource.Node?.Name == "MedicationStatement" &&
            resource.EvaluateCondition("f:medicationCodeableConcept or f:medicationReference"))
        {
            var medicationCodeableConcept = resource.SelectSingleNode("f:medicationCodeableConcept");
            if (medicationCodeableConcept.Node != null)
            {
                return new ResourceSummaryInfo(medicationCodeableConcept,
                    new ChangeContext(medicationCodeableConcept, new CodeableConcept()));
            }

            if (resource.EvaluateCondition("f:medicationReference"))
            {
                var medicationNavs = ReferenceHandler.GetReferencesWithContent(resource, "f:medicationReference");
                if (medicationNavs.Count == 1)
                {
                    var medicationResource = medicationNavs.First().Value;
                    if (medicationResource.EvaluateCondition("f:code"))
                    {
                        var widget = new ChangeContext(medicationResource.SelectSingleNode("f:code"),
                            new CodeableConcept());

                        return new ResourceSummaryInfo(medicationNavs.First().Key, widget);
                    }
                }

                var medicationDisplays =
                    ReferenceHandler.GetReferencesWithDisplayValue(resource, "f:medicationReference");
                if (medicationDisplays.Count == 1)
                {
                    var medicationDisplay = medicationDisplays.First();
                    var display = medicationDisplay.SelectSingleNode("f:display/@value").Node?.Value;
                    if (!string.IsNullOrEmpty(display))
                    {
                        return new ResourceSummaryInfo(medicationNavs.First().Key, new ConstantText(display));
                    }
                }
            }
        }

        // Observation
        if (resource.Node?.Name == "Observation")
        {
            //// Observation - Anthropometric Data
            if (resource.EvaluateCondition(
                    "f:code/f:coding[f:system/@value='http://loinc.org' and (f:code/@value='39156-5' or f:code/@value='56086-2' or f:code/@value='8280-0' or f:code/@value='9843-4' or f:code/@value='8302-2' or f:code/@value='29463-7')]"))
            {
                var valueQuantityNav = resource.SelectSingleNode("f:valueQuantity");
                if (!IsNavigatorNullOrEmpty(valueQuantityNav))
                {
                    return new ResourceSummaryInfo(valueQuantityNav,
                        new NameValuePair(
                            new ChangeContext(
                                resource.SelectSingleNode("f:code[f:coding/f:system/@value='http://loinc.org']"),
                                new CodeableConcept()),
                            new ChangeContext(valueQuantityNav, new ShowQuantity())));
                }
            }

            //// Observation - Infectious contact
            if (resource.EvaluateCondition(
                    "f:code/f:coding[f:system/@value='http://terminology.hl7.org/CodeSystem/v3-ParticipationType' and f:code/@value='EXPAGNT']"))
            {
                var valueNav = resource.SelectSingleNode("*[starts-with(local-name(), 'value')]");
                if (!IsNavigatorNullOrEmpty(valueNav) &&
                    valueNav.Node?.Name !=
                    "valueSampledData") // ignore if value is missing or is SampleData - cannot create link text out of a chart
                {
                    return new ResourceSummaryInfo(valueNav,
                        new NameValuePair(
                            new ChangeContext(resource.SelectSingleNode("f:code"), new CodeableConcept()),
                            new ChangeContext(resource, new OpenTypeElement(null))));
                }
            }

            //// Observation - SDOH
            if (resource.EvaluateCondition(
                    "f:category/f:coding[f:system/@value='http://terminology.hl7.org/CodeSystem/observation-category' and f:code/@value='social-history']"))
            {
                var valueNav = resource.SelectSingleNode("*[starts-with(local-name(), 'value')]");
                if (!IsNavigatorNullOrEmpty(valueNav) &&
                    valueNav.Node?.Name !=
                    "valueSampledData") // ignore if value is missing or is SampleData - cannot create link text out of a chart
                {
                    return new ResourceSummaryInfo(valueNav,
                        new NameValuePair(
                            new ChangeContext(resource.SelectSingleNode("f:code"), new CodeableConcept()),
                            new ChangeContext(resource, new OpenTypeElement(null))));
                }
            }

            //// Observation - travel history
            if (resource.EvaluateCondition(
                    "f:code/f:coding[f:system/@value='http://loinc.org' and f:code/@value='94651-7']"))
            {
                var valueNav = resource.SelectSingleNode("f:valueCodeableConcept");
                if (!IsNavigatorNullOrEmpty(valueNav))
                {
                    return new ResourceSummaryInfo(valueNav,
                        new NameValuePair(
                            new ChangeContext(resource.SelectSingleNode("f:code"), new CodeableConcept()),
                            new ChangeContext(valueNav, new CodeableConcept())));
                }
            }

            //// Observation - Laboratory
            if (resource.EvaluateCondition(CzLaboratoryObservation.XPathCondition))
            {
                var valueNav = resource.SelectSingleNode("*[starts-with(local-name(), 'value')]");
                if (!IsNavigatorNullOrEmpty(valueNav) &&
                    valueNav.Node?.Name !=
                    "valueSampledData") // ignore if value is missing or is SampleData - cannot create link text out of a chart
                {
                    return new ResourceSummaryInfo(valueNav,
                        new NameValuePair(
                            new ChangeContext(resource.SelectSingleNode("f:code"), new CodeableConcept()),
                            new ChangeContext(resource, new OpenTypeElement(null))));
                }
            }

            //// Observation - Imaging Order is skipped - no obvious way to detect it

            //// Observation - Imaging Report
            if (resource.EvaluateCondition(
                    "f:identifier/f:type/f:coding[f:system/@value='https://hl7.cz/fhir/img/CodeSystem/codesystem-missing-dicom-terminology' and f:code/@value='00080018']"))
            {
                var valueNav = resource.SelectSingleNode("*[starts-with(local-name(), 'value')]");
                if (!IsNavigatorNullOrEmpty(valueNav) &&
                    valueNav.Node?.Name !=
                    "valueSampledData") // ignore if value is missing or is SampleData - cannot create link text out of a chart
                {
                    return new ResourceSummaryInfo(valueNav,
                        new NameValuePair(
                            new ChangeContext(resource.SelectSingleNode("f:code"), new CodeableConcept()),
                            new ChangeContext(resource, new OpenTypeElement(null))));
                }
            }

            //// Observation - Laboratory Order is skipped - no obvious way to detect it
        }

        // Device
        if (resource.Node?.Name == "Device" &&
            resource.EvaluateCondition("f:manufacturer or f:deviceName or f:modelNumber or f:serialNumber"))
        {
            if (resource.EvaluateCondition("f:deviceName"))
            {
                var deviceNames = resource.SelectAllNodes("f:deviceName/@value")
                    .Where(x => !string.IsNullOrEmpty(x.Node?.Value)).Select(x => x.Node!.Value).ToArray();
                if (deviceNames.Length != 0)
                {
                    return new ResourceSummaryInfo(resource, new ConstantText(string.Join(", ", deviceNames)));
                }
            }

            var display = string.Empty;
            if (resource.EvaluateCondition("f:manufacturer"))
            {
                if (display != string.Empty)
                {
                    display += " ";
                }

                display += resource.SelectSingleNode("f:manufacturer/@value").Node?.Value;
            }

            if (resource.EvaluateCondition("f:modelNumber"))
            {
                if (display != string.Empty)
                {
                    display += " ";
                }

                display += resource.SelectSingleNode("f:modelNumber/@value").Node?.Value;
            }

            if (resource.EvaluateCondition("f:serialNumber"))
            {
                if (display != string.Empty)
                {
                    display += " ";
                }

                display += $"({resource.SelectSingleNode("f:serialNumber/@value").Node?.Value})";
            }

            return new ResourceSummaryInfo(resource, new ConstantText(display));
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
}

public class ResourceSummaryInfo
{
    public ResourceSummaryInfo(XmlDocumentNavigator navigator, Widget widget)
    {
        Navigator = navigator;
        Widget = widget;
    }

    public XmlDocumentNavigator Navigator { get; }
    public Widget Widget { get; }
}