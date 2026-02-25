using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Cda;

public class PatientDataWidget : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<Widget> idWidgets = navigator.EvaluateCondition("n1:id[2]")
            ?
            [
                new Container([
                    new PlainBadge(new EhdsiDisplayLabel(LabelCodes.SecondaryPatientIdentifier), Severity.Primary),
                    new Choose([
                        new When("n1:id[2]", new Heading(
                            [new WidgetWithVariables(new ShowPatientIdWidget(), [new Variable("id", "n1:id[2]")])],
                            HeadingSize.H5))
                    ], new Heading([new LocalizedLabel("general.unspecified")], HeadingSize.H3, "opacity-50")),
                ]),
            ]
            : [];
        List<Widget> widgetTree =
        [
            new Row([
                new Container([
                    new PlainBadge(new EhdsiDisplayLabel(LabelCodes.Prefix), Severity.Primary),
                    new Choose([
                        new When("n1:patient/n1:name/n1:prefix", new Heading(
                            [new ChangeContext("n1:patient/n1:name/n1:prefix", new Widget50())],
                            HeadingSize.H3)),
                    ], new Heading([new LocalizedLabel("general.unspecified")], HeadingSize.H3, "opacity-50")),
                ]),
                new Container([
                    new PlainBadge(new EhdsiDisplayLabel(LabelCodes.FamilyName), Severity.Primary),
                    new Choose([
                        new When("n1:patient/n1:name/n1:family", new Heading(
                            [new ChangeContext("n1:patient/n1:name/n1:family", new Widget51())],
                            HeadingSize.H3)),
                    ], new Heading([new LocalizedLabel("general.unspecified")], HeadingSize.H3, "opacity-50")),
                ]),
                new Container([
                    new PlainBadge(new EhdsiDisplayLabel(LabelCodes.GivenName), Severity.Primary),
                    new Choose([
                        new When("n1:patient/n1:name/n1:given", new Heading(
                            [new ChangeContext("n1:patient/n1:name/n1:given", new Widget52())],
                            HeadingSize.H3)),
                    ], new Heading([new LocalizedLabel("general.unspecified")], HeadingSize.H3, "opacity-50")),
                ]),
                new Container([
                    new PlainBadge(new EhdsiDisplayLabel(LabelCodes.DateOfBirth), Severity.Primary),
                    new Choose([
                        new When("n1:patient/n1:birthTime", new Heading(
                        [
                            new WidgetWithVariables(new ShowTsWidget(),
                                [new Variable("node", "n1:patient/n1:birthTime")])
                        ], HeadingSize.H3)),
                    ], new Heading([new LocalizedLabel("general.unspecified")], HeadingSize.H3, "opacity-50")),
                ]),
                new Container([
                    new PlainBadge(new EhdsiDisplayLabel(LabelCodes.PrimaryPatientIdentifier), Severity.Primary),
                    new Choose([
                        new When("n1:id[1]", new Heading(
                            [new WidgetWithVariables(new ShowPatientIdWidget(), [new Variable("id", "n1:id[1]")])],
                            HeadingSize.H3)),
                    ], new Heading([new LocalizedLabel("general.unspecified")], HeadingSize.H3, "opacity-50")),
                ]),
            ]),

            new Row([
                ..idWidgets,
                new Container([
                    new PlainBadge(new EhdsiDisplayLabel(LabelCodes.AdministrativeGender), Severity.Primary),
                    new Choose([
                        new When("n1:patient/n1:administrativeGenderCode", new Heading(
                        [
                            new WidgetWithVariables(new ShowEHdsiAdministrativeGenderWidget(),
                                [new Variable("node", "n1:patient/n1:administrativeGenderCode")])
                        ], HeadingSize.H3)),
                    ], new Heading([new LocalizedLabel("general.unspecified")], HeadingSize.H3, "opacity-50"))
                ]),
            ]),
            new Container([
                new DisplayPatientContactInformationWidget()
            ]),
        ];
        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}