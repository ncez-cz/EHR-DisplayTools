using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Cda;

public class DisplayAuthorsWidget : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<Widget> widgetTree =
        [
            new ConcatBuilder("/n1:ClinicalDocument/n1:author", i =>
            [
                new Variable("hcpCounter", $"{i + 1}"),
                new Variable("assignedAuthor", "/n1:ClinicalDocument/n1:author[$hcpCounter]/n1:assignedAuthor"),
                new Variable("assignedPerson", "$assignedAuthor/n1:assignedPerson"),
                new Variable("assignedEntity",
                    "/n1:ClinicalDocument/n1:documentationOf[$hcpCounter]/n1:serviceEvent/n1:performer/n1:assignedEntity"),
                new Variable("representedOrganization",
                    "/n1:ClinicalDocument/n1:author[$hcpCounter]/n1:assignedAuthor/n1:representedOrganization"),
                new Variable("functionCode", "/n1:ClinicalDocument/n1:author[$hcpCounter]/n1:functionCode"),
                new Collapser([
                        new Choose([
                            new When("$assignedPerson", new EhdsiDisplayLabel(LabelCodes.Author)),
                        ], new EhdsiDisplayLabel(LabelCodes.AuthorDevice))
                    ], [
                        new Choose([
                            new When("$assignedPerson", new Choose([
                                new When("not($assignedPerson/n1:name/@nullFlavor)", new WidgetWithVariables(
                                    new ShowNameWidget(), [
                                        new Variable("name", "$assignedPerson/n1:name"),
                                    ])),
                            ], new WidgetWithVariables(new ShowNameWidget(), [
                                new Variable("name",
                                    "$assignedEntity/n1:assignedPerson/n1:name"),
                            ]), new Text("$assignedEntity/n1:representedOrganization/n1:name"), new WidgetWithVariables(
                                new ShowContactInfoWidget(), [
                                    new Variable("contact",
                                        "$assignedEntity/n1:representedOrganization"),
                                ]), new ConstantText(@" 
                                            ")), new Condition("$functionCode and not($functionCode/@nullFlavor)",
                                new Container([
                                    new PlainBadge(new ConstantText("Skupina"), Severity.Primary),
                                    new LineBreak(),
                                    new WidgetWithVariables(
                                        new ShowEHdsiHealthcareProfessionalRoleWidget(), [
                                            new Variable("node", "$functionCode"),
                                        ]),
                                ], ContainerType.Span))),
                        ], new Text("$AuthoringDeviceName/n1:manufacturerModelName"), new ConstantText(@" 
                                        "), new ConstantText(@"
                                        ("), new Text("$AuthoringDeviceName/n1:softwareName"), new ConstantText(@")
                                    ")),
                        new WidgetWithVariables(new ShowContactInformationWidget(), [
                            new Variable("contactInfoRoot", "$assignedAuthor"),
                        ]),
                        new LineBreak(),
                        new WidgetWithVariables(new DisplayRepresentedOrganizationWidget(), [
                            new Variable("representedOrganization", "$representedOrganization"),
                        ])
                    ]
                ),
            ]),
        ];
        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}