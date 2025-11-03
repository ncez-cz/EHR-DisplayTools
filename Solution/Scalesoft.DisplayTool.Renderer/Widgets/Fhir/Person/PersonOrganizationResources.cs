using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
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
}

public class PractitionerRole : SequentialResourceBase<PractitionerRole>, IResourceWidget
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

    public static string ResourceType => "PractitionerRole";
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
}