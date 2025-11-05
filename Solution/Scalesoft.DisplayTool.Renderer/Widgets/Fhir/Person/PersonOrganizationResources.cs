using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Person;

[UsedImplicitly]
public class Organization : SequentialResourceBase<Organization>, IResourceWidget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        return new PersonOrOrganization(navigator, collapserTitle: new LocalNodeName())
            .Render(navigator, renderer, context);
    }

    public static string ResourceType => "Organization";

    public static ResourceSummaryModel? RenderSummary(XmlDocumentNavigator item)
    {
        if (item.EvaluateCondition("f:name/@value"))
        {
            var name = item.SelectSingleNode("f:name/@value").Node?.Value;
            if (name != null)
            {
                return new ResourceSummaryModel
                {
                    Value = new ConstantText(name),
                };
            }
        }

        return null;
    }
}

[UsedImplicitly]
public class Patient : SequentialResourceBase<Patient>, IResourceWidget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        return new PersonOrOrganization(navigator, collapserTitle: new LocalNodeName())
            .Render(navigator, renderer, context);
    }

    public static string ResourceType => "Patient";

    public static ResourceSummaryModel? RenderSummary(XmlDocumentNavigator item)
    {
        return ResourceSummaryUtils.SummaryByHumanName(item);
    }
}

[UsedImplicitly]
public class Practitioner : SequentialResourceBase<Practitioner>, IResourceWidget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        return new PersonOrOrganization(navigator, collapserTitle: new LocalNodeName())
            .Render(navigator, renderer, context);
    }

    public static string ResourceType => "Practitioner";

    public static ResourceSummaryModel? RenderSummary(XmlDocumentNavigator item)
    {
        return ResourceSummaryUtils.SummaryByHumanName(item);
    }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class PractitionerRole : SequentialResourceBase<PractitionerRole>, IResourceWidget
{
    public static ResourceSummaryModel? RenderSummary(XmlDocumentNavigator navigator)
    {
        XmlDocumentNavigator? practitionerNav = null;
        var content = new List<Widget>();
        var practitionerPresent = false;
        var practitionerNavs = ReferenceHandler.GetReferencesWithContent(navigator, "f:practitioner");

        if (practitionerNavs.Count == 1)
        {
            practitionerNav = practitionerNavs.First().Key;
            practitionerPresent = true;
            var practitionerResource = practitionerNavs.First().Value;
            var summary = Practitioner.RenderSummary(practitionerResource);
            if (summary != null)
            {
                content.Add(summary.Value);
            }
        }

        if (practitionerNav == null)
        {
            var practitionerDisplay = ReferenceHandler.GetReferencesWithDisplayValue(navigator, "f:practitioner");
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

        var practitionerNoContent = ReferenceHandler.GetReferencesWithoutContent(navigator, "f:practitioner");
        if (practitionerNoContent.Count != 0)
        {
            practitionerPresent = false;
        }

        if (practitionerPresent)
        {
            if (navigator.EvaluateCondition("f:specialty"))
            {
                content.AddRange([
                    new ConstantText(" ("),
                    new ChangeContext(navigator,
                        new CommaSeparatedBuilder("f:specialty", _ => [new CodeableConcept()])
                    ),
                    new ConstantText(")"),
                ]);
            }

            if (navigator.EvaluateCondition("f:organization"))
            {
                var organizationDefined = false;
                var organizationNavs = ReferenceHandler.GetReferencesWithContent(navigator, "f:organization");
                if (organizationNavs.Count == 1)
                {
                    var organizationResource = organizationNavs.First().Value;
                    var summary = Organization.RenderSummary(organizationResource);
                    if (summary != null)
                    {
                        content.AddRange([
                            new ConstantText(" - "),
                            summary.Value,
                        ]);
                        organizationDefined = true;
                    }
                }

                if (!organizationDefined)
                {
                    var organizationDisplays =
                        ReferenceHandler.GetReferencesWithDisplayValue(navigator, "f:organization");
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
            return new ResourceSummaryModel
            {
                Value = new Container(content, ContainerType.Span),
            };
        }

        return null;
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        return new PersonOrOrganization(navigator, collapserTitle: new LocalNodeName())
            .Render(navigator, renderer, context);
    }

    public static string ResourceType => "PractitionerRole";
}

[UsedImplicitly]
public class RelatedPerson : SequentialResourceBase<RelatedPerson>, IResourceWidget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        return new PersonOrOrganization(navigator, collapserTitle: new LocalNodeName())
            .Render(navigator, renderer, context);
    }

    public static string ResourceType => "RelatedPerson";
}