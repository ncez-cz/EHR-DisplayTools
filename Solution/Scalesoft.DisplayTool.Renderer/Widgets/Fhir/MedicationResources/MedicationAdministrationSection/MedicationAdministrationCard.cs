using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.MedicationResources.MedicationAdministrationSection;

public class MedicationAdministrationCard : AlternatingBackgroundColumnResourceBase<MedicationAdministrationCard>,
    IResourceWidget
{
    public static string ResourceType => "MedicationAdministration";
    [UsedImplicitly] public static bool RequiresExternalTitle => true;
    
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        Widget[] output =
        [
            new FlexList(
                [
                    new Choose([
                        new When("f:medicationCodeableConcept",
                            new MedicationAdministrationBlock(
                                new Container([
                                    new TextContainer(TextStyle.Bold,
                                        [new ChangeContext("f:medicationCodeableConcept", new CodeableConcept())]),
                                    new HideableDetails(new ChangeContext(navigator, new EnumIconTooltip("f:status",
                                        "http://terminology.hl7.org/CodeSystem/medication-admin-status",
                                        new DisplayLabel(LabelCodes.Status)))),
                                ], optionalClass: "d-flex align-items-center"),
                                navigator
                            )
                        ),
                        new When("f:medicationReference",
                            new ShowSingleReference(x =>
                                {
                                    if (x.ResourceReferencePresent)
                                    {
                                        return
                                        [
                                            new MedicationAdministrationBlock(
                                                new Container([
                                                    new Container(
                                                        new AnyReferenceNamingWidget(
                                                            customFallbackName: new ConstantText("Lék")),
                                                        optionalClass: "fw-bold"),
                                                    new HideableDetails(new ChangeContext(navigator,
                                                        new EnumIconTooltip("f:status",
                                                            "http://terminology.hl7.org/CodeSystem/medication-admin-status",
                                                            new DisplayLabel(LabelCodes.Status)))),
                                                ], optionalClass: "d-flex align-items-center"),
                                                navigator,
                                                x.Navigator
                                            )
                                        ];
                                    }

                                    return
                                    [
                                        new MedicationAdministrationBlock(
                                            new Container([
                                                new TextContainer(TextStyle.Bold,
                                                [
                                                    new ConstantText(x.ReferenceDisplay),
                                                ]),
                                                new HideableDetails(new ChangeContext(navigator,
                                                    new EnumIconTooltip("f:status",
                                                        "http://terminology.hl7.org/CodeSystem/medication-admin-status",
                                                        new DisplayLabel(LabelCodes.Status)))),
                                            ], optionalClass: "d-flex align-items-center"),
                                            navigator
                                        )
                                    ];
                                },
                                "f:medicationReference"
                            )
                        ),
                    ]),
                ], FlexDirection.Column, flexContainerClasses: "gap-1"
            )
        ];

        return output.RenderConcatenatedResult(navigator, renderer, context);
    }

    private class MedicationAdministrationBlock(
        Widget heading,
        XmlDocumentNavigator medicationAdministrationNav,
        XmlDocumentNavigator? medicationNav = null
    ) : Widget
    {
        public override Task<RenderResult> Render(
            XmlDocumentNavigator navigator,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            var infrequentProperties =
                InfrequentProperties.Evaluate<MedicationAdministrationInfrequentProperties>([
                    medicationAdministrationNav
                ]);

            var medicationAdministrationContent = new List<Widget>();

            medicationAdministrationContent.Add(new ChangeContext(medicationAdministrationNav,
                new NameValuePair(
                    [new ConstantText("Datum podání")],
                    [
                        new Chronometry("effective"),
                    ], direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary),
                new If(
                    _ => infrequentProperties.Contains(MedicationAdministrationInfrequentProperties.Request),
                    new HideableDetails(new NameValuePair([new ConstantText("Na základě žádosti")],
                    [
                        ReferenceHandler.BuildAnyReferencesNaming(medicationAdministrationNav, "f:request", context,
                            renderer),
                    ], direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary))),
                new If(_ => infrequentProperties.ContainsAnyOf(MedicationAdministrationInfrequentProperties.Performer,
                        MedicationAdministrationInfrequentProperties.Subject,
                        MedicationAdministrationInfrequentProperties.Device),
                    new NameValuePair([new ConstantText("Zainteresované strany")], 
                    [
                        new Container([
                            new If(
                                _ => infrequentProperties.Contains(MedicationAdministrationInfrequentProperties.Performer),
                                new NameValuePair([new ConstantText("Žadatel")],
                                [
                                    new CommaSeparatedBuilder("f:performer",
                                        (_, _, nav) =>
                                        [
                                            new Container([new AnyReferenceNamingWidget("f:actor")], ContainerType.Span,
                                                idSource: nav)
                                        ])
                                ], style: NameValuePair.NameValuePairStyle.Secondary)),
                            new If(_ => infrequentProperties.Contains(MedicationAdministrationInfrequentProperties.Subject),
                                new HideableDetails(new NameValuePair([new ConstantText("Příjemce")],
                                [
                                    ReferenceHandler.BuildAnyReferencesNaming(medicationAdministrationNav, "f:subject",
                                        context,
                                        renderer)
                                ], style: NameValuePair.NameValuePairStyle.Secondary))),
                            new If(_ => infrequentProperties.Contains(MedicationAdministrationInfrequentProperties.Device),
                                new NameValuePair([new ConstantText("Zařízení")],
                                [
                                    new CommaSeparatedBuilder("f:device", _ => [new AnyReferenceNamingWidget()])
                                ], style: NameValuePair.NameValuePairStyle.Secondary)),
                        ], optionalClass: "name-value-pair-wrapper"),
                    ], direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary)),
                new If(_ => infrequentProperties.Contains(MedicationAdministrationInfrequentProperties.PartOf),
                    new HideableDetails(new NameValuePair([new ConstantText("Související úkony")],
                    [
                        new CommaSeparatedBuilder("f:partOf", _ => [new AnyReferenceNamingWidget()]),
                    ], direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary))),
                new If(
                    _ => infrequentProperties.HasAnyOfGroup("InfoCell"),
                    new HideableDetails(new NameValuePair([new ConstantText("Doplňujíci informace")], 
                    [
                        new Container([
                            new HideableDetails(
                            infrequentProperties.Contains(MedicationAdministrationInfrequentProperties.Identifier)
                                ? new NameValuePair([new ConstantText("Identifikátor podání")],
                                [
                                    new CommaSeparatedBuilder("f:identifier", _ => [new ShowIdentifier()])
                                ], style: NameValuePair.NameValuePairStyle.Secondary)
                                : infrequentProperties.Contains(MedicationAdministrationInfrequentProperties.Id)
                                    ? new NameValuePair([new ConstantText("Technický identifikátor podání")],
                                    [
                                        new TextContainer(TextStyle.Regular,
                                            [new Optional("f:id", new Text("@value"))]),
                                    ], style: NameValuePair.NameValuePairStyle.Secondary)
                                    : new NullWidget()),
                        new HideableDetails(infrequentProperties.Contains(MedicationAdministrationInfrequentProperties.StatusReason)
                            ? new NameValuePair([new ConstantText("Důvody stavu")],
                            [
                                new CommaSeparatedBuilder("f:statusReason", _ => [new CodeableConcept()])
                            ], style: NameValuePair.NameValuePairStyle.Secondary)
                            : new NullWidget()),
                        infrequentProperties.Contains(MedicationAdministrationInfrequentProperties.ReasonCode)
                            ? new NameValuePair([new ConstantText("Důvod podání")],
                            [
                                new CommaSeparatedBuilder("f:reasonCode", _ => [new CodeableConcept()])
                            ], style: NameValuePair.NameValuePairStyle.Secondary)
                            : new NullWidget(),
                        infrequentProperties.Contains(MedicationAdministrationInfrequentProperties.ReasonReference)
                            ? new NameValuePair([new ConstantText("Důvod podání (reference)")],
                            [
                                new Optional("f:reasonReference",
                                    new CommaSeparatedBuilder("f:reasonReference",
                                        _ => [new AnyReferenceNamingWidget()])),
                            ], style: NameValuePair.NameValuePairStyle.Secondary)
                            : new NullWidget(),
                        new HideableDetails(infrequentProperties.Contains(MedicationAdministrationInfrequentProperties.Category)
                            ? new NameValuePair([new ConstantText("Kategorie")],
                            [
                                new Optional("f:category", new CodeableConcept()),
                            ], style: NameValuePair.NameValuePairStyle.Secondary)
                            : new NullWidget()),
                        infrequentProperties.Contains(MedicationAdministrationInfrequentProperties.Note)
                            ? new NameValuePair([new ConstantText("Poznámka")],
                            [
                                new CommaSeparatedBuilder("f:note", _ => [new Optional("f:text", new Text("@value"))])
                            ], style: NameValuePair.NameValuePairStyle.Secondary)
                            : new NullWidget(),
                        ], optionalClass: "name-value-pair-wrapper"),
                    ], direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary))
                )
            ));

            return new Concat([
                new Row([
                    new Container([heading], optionalClass: "h5 m-0 blue-color"),
                    new ChangeContext(medicationAdministrationNav,
                        new NarrativeModal(alignRight: false)
                    ),
                ], flexContainerClasses: "gap-1 align-items-center"),
                new FlexList([
                    new FlexList(
                        medicationAdministrationContent,
                        FlexDirection.Row,
                        flexContainerClasses: "column-gap-6 row-gap-1",
                        idSource: medicationNav
                    ),
                    new ChangeContext(medicationAdministrationNav,
                        new MedicationAdministrationDosageCard()),
                ], FlexDirection.Column, flexContainerClasses: "px-2 gap-1")
            ]).Render(navigator, renderer, context);
        }
    }

    public enum MedicationAdministrationInfrequentProperties
    {
        [Group("MedicationCell")] MedicationCodeableConcept,
        [Group("MedicationCell")] MedicationReference,
        [Group("MedicationCell")] Request,

        [Group("ActorsCell")] Subject,
        [Group("ActorsCell")] Performer,
        [Group("ActorsCell")] PartOf,
        [Group("ActorsCell")] Device,

        [Group("InfoCell")] StatusReason,
        [Group("InfoCell")] Identifier,
        [Group("InfoCell")] Category,
        [Group("InfoCell")] ReasonCode,
        [Group("InfoCell")] ReasonReference,
        [Group("InfoCell")] Note,
        [Group("InfoCell")] Id,
        [Group("InfoCell")] Text,

        [EnumValueSet("http://terminology.hl7.org/CodeSystem/medication-admin-status")]
        Status,
    }
}