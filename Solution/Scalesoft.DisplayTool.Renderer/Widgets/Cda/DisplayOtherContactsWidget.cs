using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Cda;

public class DisplayOtherContactsWidget : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<Widget> widgetTree =
        [
            new Collapser([
                new EhdsiDisplayLabel(LabelCodes.OtherContacts)
            ], [
                new ConcatBuilder(
                    "/n1:ClinicalDocument/n1:participant/n1:templateId[@root='1.3.6.1.4.1.19376.1.5.3.1.2.4']/../n1:associatedEntity",
                    i =>
                    [
                        new Condition("not(../n1:functionCode) or not(../n1:functionCode/@code='PCP')", new Condition(
                            "n1:associatedPerson/n1:name/* or n1:scopingOrganization", new Container([
                                new Condition("not(n1:associatedPerson/n1:name/@nullFlavor)", new WidgetWithVariables(
                                    new ShowNameWidget(), [
                                        new Variable("name", "n1:associatedPerson/n1:name"),
                                    ])),
                                new Text("n1:scopingOrganization/n1:name"),
                                new ConstantText(@" 
                                        "),
                                new Condition("@classCode", new Container([
                                    new WidgetWithVariables(new ShowEHdsiRoleClassWidget(), [
                                        new Variable("code", "@classCode"),
                                    ]),
                                ], ContainerType.Span), new ConstantText(@" 
                                        ")),
                                new Condition(
                                    "../n1:functionCode and not(../n1:functionCode/@nullFlavor)", new Container([
                                        new WidgetWithVariables(
                                            new ShowEHdsiPersonalRelationshipWidget(), [
                                                new Variable("node", "../n1:functionCode"),
                                            ]),
                                    ], ContainerType.Span)),
                                new Condition(
                                    "../n1:associatedEntity/n1:code and not(../n1:associatedEntity/n1:code/@nullFlavor)",
                                    new Container([
                                        new WidgetWithVariables(
                                            new ShowEHdsiPersonalRelationshipWidget(), [
                                                new Variable("node",
                                                    "../n1:associatedEntity/n1:code"),
                                            ]),
                                    ], ContainerType.Span)),
                            ]), new Container([
                                new PlainBadge(
                                    new EhdsiDisplayLabel(LabelCodes.ContactInformation), Severity.Primary),
                                new WidgetWithVariables(new ShowContactInfoWidget(), [
                                    new Variable("contact", "."),
                                ]),
                            ]))),
                    ]),
            ]),
        ];
        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}