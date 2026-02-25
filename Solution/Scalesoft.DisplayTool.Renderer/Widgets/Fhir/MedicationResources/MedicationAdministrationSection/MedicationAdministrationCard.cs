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

    public static bool HasBorderedContainer(Widget widget) => false;

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties
                .Evaluate<MedicationAdministrationInfrequentProperties>(navigator);

        List<Widget> output =
        [
            new Row(
                [
                    new Container(
                        [
                            new Choose(
                                [
                                    new When(
                                        "f:medicationCodeableConcept",
                                        new ChangeContext("f:medicationCodeableConcept", new CodeableConcept())
                                    ),
                                    new When(
                                        "f:medicationReference",
                                        new AnyReferenceNamingWidget(
                                            "f:medicationReference",
                                            customFallbackName: new LocalizedLabel(
                                                "medication-administration.medicationReference"
                                            )
                                        )
                                    ),
                                ]
                            ),
                        ],
                        optionalClass: "h5 m-0 blue-color"
                    ),
                    new HideableDetails(
                        new EnumIconTooltip(
                            "f:status",
                            "http://terminology.hl7.org/CodeSystem/medication-admin-status",
                            new EhdsiDisplayLabel(LabelCodes.Status)
                        )
                    ),
                    new NarrativeModal(alignRight: false),
                ],
                flexContainerClasses: "gap-1 align-items-center",
                flexWrap: false
            ),
            new FlexList(
                [
                    new FlexList(
                        [
                            new NameValuePair(
                                [new LocalizedLabel("medication-administration.effective")],
                                [
                                    new Chronometry("effective"),
                                ],
                                direction: FlexDirection.Column,
                                style: NameValuePair.NameValuePairStyle.Primary
                            ),
                            infrequentProperties.Optional(
                                MedicationAdministrationInfrequentProperties.Request,
                                new HideableDetails(
                                    new AnyReferenceNamingWidget(
                                        customFallbackName: new LocalizedLabel("medication-administration.request"),
                                        widgetModel: new ReferenceNamingWidgetModel
                                        {
                                            Direction = FlexDirection.Column,
                                            Style = NameValuePair.NameValuePairStyle.Primary,
                                            Type = ReferenceNamingWidgetType.NameValuePair,
                                        }
                                    )
                                )
                            ),
                            infrequentProperties.Optional(
                                MedicationAdministrationInfrequentProperties.Performer,
                                new AnyReferenceNamingWidget(
                                    "f:actor",
                                    widgetModel: new ReferenceNamingWidgetModel
                                    {
                                        Direction = FlexDirection.Column,
                                        Style = NameValuePair.NameValuePairStyle.Primary,
                                        Type = ReferenceNamingWidgetType.NameValuePair,
                                        LabelOverride = new Choose(
                                            [
                                                new When(
                                                    "f:function",
                                                    new ChangeContext("f:function", new CodeableConcept())
                                                )
                                            ],
                                            new LocalizedLabel("medication-administration.performer")
                                        ),
                                    }
                                )
                            ),
                            infrequentProperties.Optional(
                                MedicationAdministrationInfrequentProperties.Subject,
                                new AnyReferenceNamingWidget(
                                    widgetModel: new ReferenceNamingWidgetModel
                                    {
                                        Direction = FlexDirection.Column,
                                        Style = NameValuePair.NameValuePairStyle.Primary,
                                        Type = ReferenceNamingWidgetType.NameValuePair,
                                        LabelOverride = new LocalizedLabel("medication-administration.subject"),
                                    }
                                )
                            ),
                            infrequentProperties.Optional(
                                MedicationAdministrationInfrequentProperties.Device,
                                new AnyReferenceNamingWidget(
                                    widgetModel: new ReferenceNamingWidgetModel
                                    {
                                        Direction = FlexDirection.Column,
                                        Style = NameValuePair.NameValuePairStyle.Primary,
                                        Type = ReferenceNamingWidgetType.NameValuePair,
                                        LabelOverride = new LocalizedLabel("medication-administration.device"),
                                    }
                                )
                            ),
                            new If(
                                _ => infrequentProperties.Contains(MedicationAdministrationInfrequentProperties.PartOf),
                                new HideableDetails(
                                    new NameValuePair(
                                        [new LocalizedLabel("medication-administration.partOf")],
                                        [
                                            new CommaSeparatedBuilder(
                                                "f:partOf",
                                                _ => [new AnyReferenceNamingWidget()]
                                            ),
                                        ],
                                        direction: FlexDirection.Column,
                                        style: NameValuePair.NameValuePairStyle.Primary
                                    )
                                )
                            ),
                            new If(
                                _ => infrequentProperties.HasAnyOfGroup("InfoCell"),
                                new HideableDetails(
                                    new NameValuePair(
                                        [new LocalizedLabel("general.additional-info")],
                                        [
                                            new Container(
                                                [
                                                    new HideableDetails(
                                                        infrequentProperties.Contains(
                                                            MedicationAdministrationInfrequentProperties.Identifier
                                                        )
                                                            ? new NameValuePair(
                                                                [
                                                                    new LocalizedLabel(
                                                                        "medication-administration.identifier"
                                                                    )
                                                                ],
                                                                [
                                                                    new CommaSeparatedBuilder(
                                                                        "f:identifier",
                                                                        _ => [new ShowIdentifier()]
                                                                    )
                                                                ],
                                                                style: NameValuePair.NameValuePairStyle.Secondary
                                                            )
                                                            : infrequentProperties.Contains(
                                                                MedicationAdministrationInfrequentProperties.Id
                                                            )
                                                                ? new NameValuePair(
                                                                    [
                                                                        new LocalizedLabel(
                                                                            "medication-administration.id"
                                                                        )
                                                                    ],
                                                                    [
                                                                        new TextContainer(
                                                                            TextStyle.Regular,
                                                                            [new Optional("f:id", new Text("@value"))]
                                                                        ),
                                                                    ],
                                                                    style: NameValuePair.NameValuePairStyle.Secondary
                                                                )
                                                                : new NullWidget()
                                                    ),
                                                    new HideableDetails(
                                                        infrequentProperties.Contains(
                                                            MedicationAdministrationInfrequentProperties.StatusReason
                                                        )
                                                            ? new NameValuePair(
                                                                [
                                                                    new LocalizedLabel(
                                                                        "medication-administration.status-reason"
                                                                    )
                                                                ],
                                                                [
                                                                    new CommaSeparatedBuilder(
                                                                        "f:statusReason",
                                                                        _ => [new CodeableConcept()]
                                                                    )
                                                                ],
                                                                style: NameValuePair.NameValuePairStyle.Secondary
                                                            )
                                                            : new NullWidget()
                                                    ),
                                                    infrequentProperties.Contains(
                                                        MedicationAdministrationInfrequentProperties.ReasonCode
                                                    )
                                                        ? new NameValuePair(
                                                            [
                                                                new LocalizedLabel(
                                                                    "medication-administration.reasonCode"
                                                                )
                                                            ],
                                                            [
                                                                new CommaSeparatedBuilder(
                                                                    "f:reasonCode",
                                                                    _ => [new CodeableConcept()]
                                                                )
                                                            ],
                                                            style: NameValuePair.NameValuePairStyle.Secondary
                                                        )
                                                        : new NullWidget(),
                                                    infrequentProperties.Contains(
                                                        MedicationAdministrationInfrequentProperties.ReasonReference
                                                    )
                                                        ? new NameValuePair(
                                                            [
                                                                new LocalizedLabel(
                                                                    "medication-administration.reasonReference"
                                                                )
                                                            ],
                                                            [
                                                                new Optional(
                                                                    "f:reasonReference",
                                                                    new CommaSeparatedBuilder(
                                                                        "f:reasonReference",
                                                                        _ => [new AnyReferenceNamingWidget()]
                                                                    )
                                                                ),
                                                            ],
                                                            style: NameValuePair.NameValuePairStyle.Secondary
                                                        )
                                                        : new NullWidget(),
                                                    new HideableDetails(
                                                        infrequentProperties.Contains(
                                                            MedicationAdministrationInfrequentProperties.Category
                                                        )
                                                            ? new NameValuePair(
                                                                [
                                                                    new LocalizedLabel(
                                                                        "medication-administration.category"
                                                                    )
                                                                ],
                                                                [
                                                                    new Optional("f:category", new CodeableConcept()),
                                                                ],
                                                                style: NameValuePair.NameValuePairStyle.Secondary
                                                            )
                                                            : new NullWidget()
                                                    ),
                                                    infrequentProperties.Contains(
                                                        MedicationAdministrationInfrequentProperties.Note
                                                    )
                                                        ? new NameValuePair(
                                                            [new LocalizedLabel("medication-administration.note")],
                                                            [
                                                                new CommaSeparatedBuilder(
                                                                    "f:note",
                                                                    _ => [new Optional("f:text", new Text("@value"))]
                                                                )
                                                            ],
                                                            style: NameValuePair.NameValuePairStyle.Secondary
                                                        )
                                                        : new NullWidget(),
                                                ],
                                                optionalClass: "name-value-pair-wrapper"
                                            ),
                                        ],
                                        direction: FlexDirection.Column,
                                        style: NameValuePair.NameValuePairStyle.Primary
                                    )
                                )
                            )
                        ],
                        FlexDirection.Row,
                        flexContainerClasses: "column-gap-6 row-gap-1"
                    ),
                    new MedicationAdministrationDosageCard(),
                ],
                FlexDirection.Column,
                flexContainerClasses: "px-2 gap-1"
            )
        ];


        return output.RenderConcatenatedResult(navigator, renderer, context);
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