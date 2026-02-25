using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.FamilyMemberHistory;

public class FamilyMembersHistory : AlternatingBackgroundColumnResourceBase<FamilyMembersHistory>, IResourceWidget
{
    public static string ResourceType => "FamilyMemberHistory";
    [UsedImplicitly] public static bool RequiresExternalTitle => true;

    public static bool HasBorderedContainer(Widget resourceWidget) => false;

    public static ResourceSummaryModel? RenderSummary(XmlDocumentNavigator item)
    {
        var summaryItems = new List<Widget>();
        if (item.EvaluateCondition("f:relationship and f:condition"))
        {
            summaryItems.Add(new ChangeContext(item, "f:relationship", new CodeableConcept()));
            summaryItems.Add(new ConstantText(" - "));
            summaryItems.Add(new ChangeContext(item, new CommaSeparatedBuilder("f:condition",
                _ => new ChangeContext("f:code", new CodeableConcept()))));
        }

        if (summaryItems.Count == 0)
        {
            return null;
        }

        return new ResourceSummaryModel
        {
            Value = new Container(summaryItems, ContainerType.Span),
        };
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentOptions =
            InfrequentProperties.Evaluate<InfrequentPropertiesPaths>(navigator);

        var complete = new Column([
            new Row([
                new HeadingNoMargin([
                    new TextContainer(TextStyle.Bold | TextStyle.CapitalizeFirst, [
                            infrequentOptions.Optional(InfrequentPropertiesPaths.Name,
                                new Text("@value")
                            ).Else(
                                new ChangeContext("f:relationship", new CodeableConcept())
                            ),
                        ]
                    )
                ], HeadingSize.H5, customClass: "blue-color"),
                infrequentOptions.Condition(InfrequentPropertiesPaths.Text,
                    new NarrativeModal(alignRight: false)
                ),
                new EnumIconTooltip("f:status",
                    "http://hl7.org/fhir/ValueSet/history-status",
                    new EhdsiDisplayLabel(LabelCodes.Status)
                )
            ], flexContainerClasses: "gap-1 align-items-center"),
            new Column([
                new Row([
                    infrequentOptions.Optional(InfrequentPropertiesPaths.Relationship,
                        // if there is no name, then the relationship is already in the title
                        infrequentOptions.Condition(InfrequentPropertiesPaths.Name,
                            new NameValuePair(
                                new LocalizedLabel("family-member-history.relationship"),
                                new CodeableConcept(),
                                style: NameValuePair.NameValuePairStyle.Primary,
                                direction: FlexDirection.Column
                            )
                        )
                    ),
                    infrequentOptions.Condition(InfrequentPropertiesPaths.Born,
                        new NameValuePair(
                            new LocalizedLabel("family-member-history.born"),
                            new Chronometry("born"),
                            style: NameValuePair.NameValuePairStyle.Primary,
                            direction: FlexDirection.Column
                        )
                    ),
                    infrequentOptions.Condition(InfrequentPropertiesPaths.Age,
                        new NameValuePair(
                            new TextContainer(TextStyle.CapitalizeFirst | TextStyle.Lowercase, [
                                new Condition("f:estimatedAge[@value='true']",
                                    new LocalizedLabel("family-member-history.estimated"),
                                    new ConstantText(" ")
                                ),
                                new LocalizedLabel("family-member-history.age")
                            ]),
                            new OpenTypeElement(null, "age"),
                            style: NameValuePair.NameValuePairStyle.Primary,
                            direction: FlexDirection.Column
                        )
                    ),
                    infrequentOptions.Condition(InfrequentPropertiesPaths.Deceased,
                        new NameValuePair(
                            new LocalizedLabel("family-member-history.deceased"),
                            new OpenTypeElement(null, "deceased"),
                            style: NameValuePair.NameValuePairStyle.Primary,
                            direction: FlexDirection.Column
                        )
                    ),
                    infrequentOptions.Optional(InfrequentPropertiesPaths.Sex,
                        new NameValuePair(
                            new LocalizedLabel("family-member-history.sex"),
                            new CodeableConcept(),
                            style: NameValuePair.NameValuePairStyle.Primary,
                            direction: FlexDirection.Column
                        )
                    ),
                    infrequentOptions.Optional(InfrequentPropertiesPaths.Date,
                        new NameValuePair(
                            new LocalizedLabel("family-member-history.date"),
                            new ShowDateTime(),
                            style: NameValuePair.NameValuePairStyle.Primary,
                            direction: FlexDirection.Column
                        )
                    ),
                    new HideableDetails(
                        infrequentOptions.Condition(InfrequentPropertiesPaths.Identifier,
                            new NameValuePair(
                                new LocalizedLabel("family-member-history.identifier"),
                                new CommaSeparatedBuilder("f:identifier", _ => [new ShowIdentifier()]),
                                style: NameValuePair.NameValuePairStyle.Primary,
                                direction: FlexDirection.Column
                            )
                        ).Else(
                            infrequentOptions.Optional(InfrequentPropertiesPaths.Id,
                                new NameValuePair(
                                    new LocalizedLabel("family-member-history.id"),
                                    new Text("@value"),
                                    style: NameValuePair.NameValuePairStyle.Primary,
                                    direction: FlexDirection.Column
                                )
                            )
                        )
                    ),
                    infrequentOptions.Condition(InfrequentPropertiesPaths.Note,
                        new ConcatBuilder("f:note", _ =>
                        [
                            new NameValuePair(
                                new LocalizedLabel("family-member-history.note"),
                                new ShowAnnotationCompact(),
                                style: NameValuePair.NameValuePairStyle.Primary,
                                direction: FlexDirection.Column
                            )
                        ])
                    ),
                ], flexContainerClasses: "row-gap-1 column-gap-6"),
                new Card(
                    new LocalizedLabel("family-member-history.family-member-conditions"),
                    new AlternatingBackgroundColumnBuilder("f:condition", _ =>
                        [
                            new FamilyMemberCondition()
                        ]
                    ), optionalClass: "m-0"
                ),
                new Row([
                    new If(
                        _ => infrequentOptions.ContainsAnyOf(InfrequentPropertiesPaths.ReasonCode,
                            InfrequentPropertiesPaths.ReasonReference),
                        new NameValuePair(
                            new LocalizedLabel("family-member-history.related-patient-conditions"),
                            new CommaSeparatedBuilder("f:reasonReference|f:reasonCode", (_, _, nav) =>
                            {
                                var nodeName = nav.Node?.Name;

                                return nodeName switch
                                {
                                    "reasonReference" => [new AnyReferenceNamingWidget()],
                                    "reasonCode" => [new CodeableConcept()],
                                    _ => [new NullWidget()]
                                };
                            }),
                            style: NameValuePair.NameValuePairStyle.Primary,
                            direction: FlexDirection.Column
                        )
                    ),
                    new If(_ => IsDataAbsent(navigator, "f:dataAbsentReason"),
                        new NameValuePair(
                            new LocalizedLabel("family-member-history.data-absent-reason"),
                            new AbsentData("f:dataAbsentReason"),
                            style: NameValuePair.NameValuePairStyle.Primary,
                            direction: FlexDirection.Column
                        )
                    )
                ])
            ], flexContainerClasses: "px-2 row-gap-1")
        ], flexContainerClasses: "row-gap-1");

        return complete.Render(navigator, renderer, context);
    }

    private class FamilyMemberCondition : Widget
    {
        public override Task<RenderResult> Render(
            XmlDocumentNavigator navigator,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            var infrequentOptions = InfrequentProperties.Evaluate<ConditionsInfrequentPropertiesPaths>(navigator);

            var widget = new Column([
                new HeadingNoMargin([
                    new TextContainer(TextStyle.Bold | TextStyle.CapitalizeFirst, [
                        new ChangeContext("f:code", new CodeableConcept())
                    ], optionalClass: "blue-color"),
                ], HeadingSize.H6),
                new Row([
                    infrequentOptions.Optional(ConditionsInfrequentPropertiesPaths.Outcome,
                        new NameValuePair(
                            new LocalizedLabel("family-member-history.condition.outcome"),
                            new CodeableConcept(),
                            direction: FlexDirection.Column,
                            style: NameValuePair.NameValuePairStyle.Primary
                        )
                    ),
                    infrequentOptions.Optional(ConditionsInfrequentPropertiesPaths.ContributedToDeath,
                        new NameValuePair(
                            new LocalizedLabel("family-member-history.condition.contributedToDeath"),
                            new ShowBoolean(
                                new LocalizedLabel("general.no"),
                                new LocalizedLabel("general.yes")
                            ),
                            direction: FlexDirection.Column,
                            style: NameValuePair.NameValuePairStyle.Primary
                        )
                    ),
                    infrequentOptions.Condition(ConditionsInfrequentPropertiesPaths.Onset,
                        new NameValuePair(
                            new LocalizedLabel("family-member-history.condition.onset"),
                            new Chronometry("onset"),
                            direction: FlexDirection.Column,
                            style: NameValuePair.NameValuePairStyle.Primary
                        )
                    ),
                    infrequentOptions.Condition(ConditionsInfrequentPropertiesPaths.Note,
                        new ConcatBuilder("f:note", _ =>
                        [
                            new NameValuePair(
                                new LocalizedLabel("family-member-history.condition.note"),
                                new ShowAnnotationCompact(),
                                direction: FlexDirection.Column,
                                style: NameValuePair.NameValuePairStyle.Primary
                            )
                        ])
                    )
                ], flexContainerClasses: "px-2 row-gap-2 column-gap-6"),
            ], flexContainerClasses: "row-gap-1");

            return widget.Render(navigator, renderer, context);
        }

        private enum ConditionsInfrequentPropertiesPaths
        {
            Code, // 1..1
            Outcome,
            ContributedToDeath,
            [OpenType("onset")] Onset,
            Note,
        }
    }

    private enum InfrequentPropertiesPaths
    {
        Name, //0..1	string	The family member described

        Relationship, //1..1	CodeableConcept	Relationship to the subject.  http://terminology.hl7.org/CodeSystem/v3-RoleCode

        [OpenType("born")] Born, /*0..1		(approximate) date of birth - Chronometry*/

        [OpenType("age")] Age, /*0..1		(approximate) age - opentype*/

        [OpenType("deceased")] Deceased, /*0..1		Dead? How old/when? - opentype*/
        Sex, //0..1	CodeableConcept	male | female | other | unknown

        /*Participant, //	0..*	BackboneElement	Members of the team*/
        Id,
        Identifier, //	0..*	Identifier	External Id(s) for this record

        Date, //0..1	dateTime	When history was recorded or last updated
        ReasonCode, //	0..*	CodeableConcept	Why was family member history performed?

        ReasonReference, //0..*	Reference(Condition | Observation | AllergyIntolerance | QuestionnaireResponse | DiagnosticReport | DocumentReference) Why was family member history performed?
        Text,

        [EnumValueSet("http://hl7.org/fhir/history-status")]
        Status,
        Note
    }
}