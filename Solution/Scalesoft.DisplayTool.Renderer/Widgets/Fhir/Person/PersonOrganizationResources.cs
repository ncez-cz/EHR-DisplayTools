using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Person;

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

    public static bool HasBorderedContainer(Widget resourceWidget) => true;

    public static ResourceSummaryModel? RenderSummary(XmlDocumentNavigator item)
    {
        if (item.EvaluateCondition("f:name/@value"))
        {
            return new ResourceSummaryModel
            {
                Value = new ChangeContext(item, new Text("f:name/@value")),
            };
        }

        return null;
    }
}

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

    public static bool HasBorderedContainer(Widget resourceWidget) => true;

    public static ResourceSummaryModel? RenderSummary(XmlDocumentNavigator item)
    {
        return ResourceSummaryUtils.SummaryByHumanName(item);
    }
}

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

    public static bool HasBorderedContainer(Widget resourceWidget) => true;

    public static ResourceSummaryModel? RenderSummary(XmlDocumentNavigator item)
    {
        return ResourceSummaryUtils.SummaryByHumanName(item);
    }
}

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
            var summary = ReferenceHandler.GetResourceSummary(practitionerResource);
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
                if (practitionerNav.EvaluateCondition("f:display/@value"))
                {
                    content.Add(new Text("f:display/@value"));
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
                content.AddRange(
                    [
                        new ConstantText(" ("),
                        new ChangeContext(
                            navigator,
                            new CommaSeparatedBuilder("f:specialty", _ => [new CodeableConcept()])
                        ),
                        new ConstantText(")"),
                    ]
                );
            }

            if (navigator.EvaluateCondition("f:organization"))
            {
                var organizationDefined = false;
                var organizationNavs = ReferenceHandler.GetReferencesWithContent(navigator, "f:organization");
                if (organizationNavs.Count == 1)
                {
                    var organizationResource = organizationNavs.First().Value;
                    var summary = ReferenceHandler.GetResourceSummary(organizationResource);
                    if (summary != null)
                    {
                        content.AddRange(
                            [
                                new ConstantText(" - "),
                                summary.Value,
                            ]
                        );
                        organizationDefined = true;
                    }
                }

                if (!organizationDefined)
                {
                    var organizationsWithDisplay =
                        ReferenceHandler.GetReferencesWithDisplayValue(navigator, "f:organization");
                    if (organizationsWithDisplay.Count == 1)
                    {
                        var organizationWithDisplay = organizationsWithDisplay.First();
                        if (organizationWithDisplay.EvaluateCondition("f:display/@value"))
                        {
                            content.Add(new ChangeContext(organizationWithDisplay, new Text("f:display/@value")));
                        }
                    }
                }
                //ignore no reference content
            }
        }

        Widget? label = null;
        if (navigator.EvaluateCondition("f:code"))
        {
            label = new ChangeContext(navigator, "f:code", new CodeableConcept());
        }


        if (practitionerNav != null && content.Count != 0)
        {
            return new ResourceSummaryModel
            {
                Label = label,
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

    public static bool HasBorderedContainer(Widget resourceWidget) => true;
}

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

    public static bool HasBorderedContainer(Widget resourceWidget) => true;

    public static ResourceSummaryModel? RenderSummary(XmlDocumentNavigator item)
    {
        var humanNameSummary = ResourceSummaryUtils.SummaryByHumanName(item);
        if (humanNameSummary != null)
        {
            return humanNameSummary;
        }

        if (item.EvaluateCondition("f:relationship"))
        {
            return new ResourceSummaryModel
            {
                Value =
                    new ChangeContext(item, new CommaSeparatedBuilder("f:relationship", _ => new CodeableConcept())),
            };
        }

        return null;
    }
}