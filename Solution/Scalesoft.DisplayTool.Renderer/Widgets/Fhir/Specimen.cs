using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class Specimen : AlternatingBackgroundColumnResourceBase<Specimen>, IResourceWidget
{
    public static string ResourceType => "Specimen";

    public static bool HasBorderedContainer(Widget widget) => true;

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        InfrequentPropertiesData<CollectionInfrequentProperties>? collectionInfrequentProperties = null;
        InfrequentPropertiesData<SpecimenInfrequentProperties>? globalInfrequentProperties = null;

        globalInfrequentProperties = InfrequentProperties.Evaluate<SpecimenInfrequentProperties>(navigator);

        if (navigator.EvaluateCondition("f:collection"))
        {
            collectionInfrequentProperties =
                InfrequentProperties.Evaluate<CollectionInfrequentProperties>(
                    navigator.SelectSingleNode("f:collection"));
        }

        var valueWrapperClass = new NameValuePair.NameValuePairClasses
        {
            ValueClass = "name-value-pair-wrapper",
        };

        var hideableOuterContainerClass = new NameValuePair.NameValuePairClasses
        {
            OuterClass = HideableDetails.HideableDetailsClass,
        };

        var peopleInfo = new NameValuePair([
                new LocalizedLabel("specimen.people"),
            ], [
                globalInfrequentProperties.Optional(SpecimenInfrequentProperties.Subject,
                    new AnyReferenceNamingWidget(
                        showOptionalDetails: false,
                        widgetModel: new ReferenceNamingWidgetModel
                        {
                            Type = ReferenceNamingWidgetType.NameValuePair,
                            LabelOverride = new LocalizedLabel("specimen.subject"),
                            Direction = FlexDirection.Row,
                            Style = NameValuePair.NameValuePairStyle.Secondary,
                        }
                    )
                ),
                new Optional("f:collection",
                    new Optional("f:collector",
                        new NameValuePair(
                            new LocalizedLabel("specimen.collection.collector"),
                            new AnyReferenceNamingWidget(), direction: FlexDirection.Row,
                            style: NameValuePair.NameValuePairStyle.Secondary,
                            optionalClasses: hideableOuterContainerClass
                        )
                    )
                )
            ], direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary,
            optionalClasses: valueWrapperClass);

        var timeInfo = new NameValuePair([
                new LocalizedLabel("specimen.time"),
            ], [
                new Optional("f:receivedTime",
                    new NameValuePair(
                        new LocalizedLabel("specimen.receivedTime"),
                        new ShowDateTime(), direction: FlexDirection.Row,
                        style: NameValuePair.NameValuePairStyle.Secondary
                    )
                ),
                new Optional("f:collection",
                    new If(
                        _ => collectionInfrequentProperties != null &&
                             collectionInfrequentProperties.Contains(CollectionInfrequentProperties.Collected),
                        new NameValuePair(
                            new LocalizedLabel("specimen.collection.collected"),
                            new Chronometry("collected"), direction: FlexDirection.Row,
                            style: NameValuePair.NameValuePairStyle.Secondary
                        )
                    )
                )
            ], direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary,
            optionalClasses: valueWrapperClass);

        var collectionInfo = new Optional("f:collection",
            new NameValuePair([new LocalizedLabel("specimen.collection")], [
                    new Optional("f:duration",
                        new NameValuePair(
                            new LocalizedLabel("specimen.collection.duration"),
                            new ShowDuration(), direction: FlexDirection.Row,
                            style: NameValuePair.NameValuePairStyle.Secondary,
                            optionalClasses: hideableOuterContainerClass
                        )
                    ),
                    new Optional("f:quantity",
                        new NameValuePair(
                            new LocalizedLabel("specimen.collection.quantity"),
                            new ShowQuantity(), direction: FlexDirection.Row,
                            style: NameValuePair.NameValuePairStyle.Secondary,
                            optionalClasses: hideableOuterContainerClass
                        )
                    ),
                    new Optional("f:method",
                        new NameValuePair(
                            new LocalizedLabel("specimen.collection.method"),
                            new CodeableConcept(), direction: FlexDirection.Row,
                            style: NameValuePair.NameValuePairStyle.Secondary
                        )
                    ),
                    new Optional("f:bodySite",
                        new NameValuePair(
                            new LocalizedLabel("specimen.collection.bodySite"),
                            new CodeableConcept(), direction: FlexDirection.Row,
                            style: NameValuePair.NameValuePairStyle.Secondary
                        )
                    ),
                    new If(_ => collectionInfrequentProperties != null &&
                                collectionInfrequentProperties.Contains(
                                    CollectionInfrequentProperties.BodySiteExtension),
                        new NameValuePair(
                            new LocalizedLabel("specimen.collection.bodySite-extension"),
                            new AnyReferenceNamingWidget(
                                "f:extension[@url='http://hl7.org/fhir/StructureDefinition/bodySite']/f:valueReference"),
                            direction: FlexDirection.Row, style: NameValuePair.NameValuePairStyle.Secondary,
                            optionalClasses: hideableOuterContainerClass
                        )
                    ),
                    new If(
                        _ => collectionInfrequentProperties != null &&
                             collectionInfrequentProperties.Contains(CollectionInfrequentProperties.FastingStatus),
                        new NameValuePair(
                            new LocalizedLabel("specimen.collection.fastingStatus"),
                            new OpenTypeElement(null, "fastingStatus"), direction: FlexDirection.Row,
                            style: NameValuePair.NameValuePairStyle.Secondary,
                            optionalClasses: hideableOuterContainerClass // CodeableConcept | Duration
                        )
                    )
                ], direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary,
                optionalClasses: valueWrapperClass)
        );

        var nameValuePairs = new Concat([
            new Optional("f:accessionIdentifier", new HideableDetails(
                new NameValuePair(
                    new LocalizedLabel("specimen.accessionIdentifier"),
                    new ShowIdentifier(), direction: FlexDirection.Column,
                    style: NameValuePair.NameValuePairStyle.Primary
                ))
            ),
            new Optional("f:type",
                new NameValuePair(
                    new LocalizedLabel("specimen.type"),
                    new CodeableConcept(), direction: FlexDirection.Column,
                    style: NameValuePair.NameValuePairStyle.Primary
                )
            ),
            new Condition("f:condition", new HideableDetails(
                new NameValuePair(
                    new LocalizedLabel("specimen.condition"),
                    new CommaSeparatedBuilder("f:condition", _ => [new CodeableConcept()]),
                    direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary
                ))
            ),
            new Condition("f:parent",
                new HideableDetails(new NameValuePair(
                    new LocalizedLabel("specimen.parent"),
                    new CommaSeparatedBuilder("f:parent", _ => [new AnyReferenceNamingWidget()]),
                    direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary
                ))
            ),
            new Condition("f:request",
                new HideableDetails(new NameValuePair(
                    new LocalizedLabel("specimen.request"),
                    new CommaSeparatedBuilder("f:request", _ => [new AnyReferenceNamingWidget()]),
                    direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary
                ))
            ),
        ]);

        var processingInfo =
            new Condition("f:processing",
                new Card(new LocalizedLabel("specimen.processing-details"),
                    new AlternatingBackgroundColumnBuilder("f:processing", (_, _, nav, _) =>
                        {
                            var infrequentProperties =
                                InfrequentProperties.Evaluate<CollectionInfrequentProperties>(nav);

                            var row = new Row(
                            [
                                new Optional("f:description",
                                    new NameValuePair(
                                        new EhdsiDisplayLabel(LabelCodes.Description),
                                        new Text("@value"), direction: FlexDirection.Column,
                                        style: NameValuePair.NameValuePairStyle.Primary
                                    )
                                ),
                                new Optional("f:procedure",
                                    new NameValuePair(
                                        new LocalizedLabel("specimen.processing.procedure"),
                                        new CodeableConcept(), direction: FlexDirection.Column,
                                        style: NameValuePair.NameValuePairStyle.Primary
                                    )
                                ),
                                new Condition("f:additive",
                                    new NameValuePair(
                                        new LocalizedLabel("specimen.processing.additive"),
                                        new CommaSeparatedBuilder("f:additive", _ => [new AnyReferenceNamingWidget()]),
                                        direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary
                                    )
                                ),
                                new If(_ => infrequentProperties.Contains(CollectionInfrequentProperties.Time),
                                    new NameValuePair(
                                        new LocalizedLabel("specimen.processing.time"),
                                        new Chronometry("time"), direction: FlexDirection.Column,
                                        style: NameValuePair.NameValuePairStyle.Primary
                                    )
                                ),
                            ], flexContainerClasses: "column-gap-6 row-gap-2");
                            return [row];
                        }
                    )
                )
            );

        var containerInfo =
            new Condition("f:container",
                new Card(new LocalizedLabel("specimen.container-details"),
                    new AlternatingBackgroundColumnBuilder("f:container", (_, _, nav, _) =>
                    {
                        var infrequentProperties =
                            InfrequentProperties.Evaluate<CollectionInfrequentProperties>(nav);

                        var row = new Row([
                                new Optional("f:description",
                                    new NameValuePair(
                                        new EhdsiDisplayLabel(LabelCodes.Description),
                                        new Text("@value"), direction: FlexDirection.Column,
                                        style: NameValuePair.NameValuePairStyle.Primary
                                    )
                                ),
                                new Optional("f:type",
                                    new NameValuePair(
                                        new LocalizedLabel("specimen.container.type"),
                                        new CodeableConcept(), direction: FlexDirection.Column,
                                        style: NameValuePair.NameValuePairStyle.Primary
                                    )
                                ),
                                new Optional("f:capacity",
                                    new NameValuePair(
                                        new LocalizedLabel("specimen.container.capacity"),
                                        new ShowQuantity(), direction: FlexDirection.Column,
                                        style: NameValuePair.NameValuePairStyle.Primary
                                    )
                                ),
                                new Optional("f:specimenQuantity",
                                    new NameValuePair(
                                        new LocalizedLabel("specimen.container.specimenQuantity"),
                                        new ShowQuantity(), direction: FlexDirection.Column,
                                        style: NameValuePair.NameValuePairStyle.Primary
                                    )
                                ),
                                new If(_ => infrequentProperties.Contains(CollectionInfrequentProperties.Device),
                                    new NameValuePair(
                                        new LocalizedLabel("specimen.container.device"),
                                        new AnyReferenceNamingWidget(
                                            "f:extension[@url='http://hl7.org/fhir/5.0/StructureDefinition/extension-Specimen.container.device']/f:valueReference"),
                                        direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary
                                    )
                                ),
                                new If(_ => infrequentProperties.Contains(CollectionInfrequentProperties.Additive),
                                    new NameValuePair(
                                        new LocalizedLabel("specimen.container.additive"),
                                        new OpenTypeElement(null,
                                            "additive"), direction: FlexDirection.Column,
                                        style: NameValuePair.NameValuePairStyle
                                            .Primary // CodeableConcept | Reference(Substance)
                                    )
                                ),
                            ],
                            flexContainerClasses: "column-gap-6 row-gap-2");

                        return [row];
                    })
                )
            );

        var annotationInfo = new Condition("f:note", new NameValuePair(new LocalizedLabel("specimen.note"),
            new ConcatBuilder("f:note", _ => [new ShowAnnotationCompact()], separator: new LineBreak()),
            direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary
        ));

        var complete =
            new Concat([
                new Row([
                        new Heading([
                            new Container([
                                new LocalizedLabel("specimen"),
                                new HideableDetails(new Optional("f:accessionIdentifier",
                                    new ConstantText("("),
                                    new ShowIdentifier(showSystem: false),
                                    new ConstantText(")")
                                )),
                                new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/specimen-status",
                                    new EhdsiDisplayLabel(LabelCodes.Status))
                            ], optionalClass: "blue-color d-flex align-items-center gap-1"),
                        ], HeadingSize.H5, customClass: "m-0"),
                        new NarrativeModal(alignRight: false),
                    ], flexContainerClasses: "gap-1 align-items-center",
                    idSource: navigator),
                new FlexList([
                    new Row([
                        nameValuePairs,
                        peopleInfo,
                        timeInfo,
                        collectionInfo,
                        annotationInfo,
                    ], flexContainerClasses: "column-gap-6 row-gap-3"),
                    processingInfo,
                    containerInfo,
                    new Condition("f:text", new NarrativeCollapser()),
                ], FlexDirection.Column, flexContainerClasses: "px-2 gap-1")
            ]);

        return complete.Render(navigator, renderer, context);
    }

    private enum CollectionInfrequentProperties
    {
        [OpenType("collected")] Collected,
        [OpenType("fastingStatus")] FastingStatus,
        [OpenType("time")] Time,
        [OpenType("additive")] Additive,

        [Extension("http://hl7.org/fhir/StructureDefinition/bodySite")]
        BodySiteExtension,

        [Extension("http://hl7.org/fhir/5.0/StructureDefinition/extension-Specimen.container.device")]
        Device,
    }

    private enum SpecimenInfrequentProperties
    {
        Subject,
    }
}