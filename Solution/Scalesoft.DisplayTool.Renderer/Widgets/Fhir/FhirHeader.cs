using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;
using Scalesoft.DisplayTool.Shared.Signature;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class FhirHeader : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var compositionProperties =
            InfrequentProperties.Evaluate<CompositionProperties>(navigator);

        var renderDate = DateTimeFormats.GetTimeWidget(DateTime.Now, context.Language,
            DateFormatType.SecondMinuteHourDayMonthYear);

        var encounterNavigator = ReferenceHandler.GetSingleNodeNavigatorFromReference(navigator, "f:encounter",
            ".");
        XmlDocumentNavigator? logoNavigator = null;
        if (encounterNavigator?.Node != null)
        {
            logoNavigator = ReferenceHandler.GetSingleNodeNavigatorFromReference(encounterNavigator,
                "f:serviceProvider",
                "f:extension[@url='https://hl7.cz/fhir/core/StructureDefinition/cz-organization-logo']");
        }

        var containsLogo = logoNavigator?.Node != null;
        var containsQrCode = navigator.EvaluateCondition("f:identifier/f:value/@value");

        var qrCode =
            new Optional("f:identifier/f:value/@value",
                new Container([
                    new Barcode(new Text(), margin: 0, optionalInnerClass: "header-code",
                        optionalOuterClass: "d-flex align-content-center justify-content-center")
                ], optionalClass: "header-code-container align-content-center")
            );

        var optionalClasses = containsQrCode switch
        {
            true when !containsLogo => "pe-5",
            false when containsLogo => "ps-5",
            _ => string.Empty
        };

        var bundle = navigator.SelectSingleNode("ancestor::f:Bundle[1]");

        Widget[] widget =
        [
            new If(_ => containsLogo && containsQrCode,
                new Row([
                    new Row([
                        new ChangeContext(logoNavigator!, new HospitalLogo()),
                        new If(_ => navigator.SignatureValidationResult != null,
                            new SignatureValidationResult(navigator.SignatureValidationResult!)),
                    ]),
                    qrCode,
                ], flexContainerClasses: "justify-content-between mb-2")
            ),
            new Row([
                new If(_ => containsLogo && !containsQrCode,
                    new Row([
                        new ChangeContext(logoNavigator!, new HospitalLogo()),
                        new If(_ => navigator.SignatureValidationResult != null,
                            new SignatureValidationResult(navigator.SignatureValidationResult!)),
                    ])
                ),
                new If(_ => !containsLogo && !containsQrCode && navigator.SignatureValidationResult != null,
                    new SignatureValidationResult(navigator.SignatureValidationResult!)),
                new Container(
                    [
                        new ChangeContext(bundle, new Optional(
                            "f:timestamp",
                            new HideableDetails(new NameValuePair(
                                new LocalizedLabel("bundle.timestamp"),
                                new ShowInstant()
                            ))
                        )),
                        new If(
                            _ => (context.DocumentType is DocumentType.ImagingOrder or DocumentType.LaboratoryOrder) &&
                                 navigator.EvaluateCondition(
                                     "/f:Bundle/f:entry/f:resource/f:Composition/f:author/f:reference"
                                 ),
                            new ConcatBuilder(
                                "/f:Bundle/f:entry/f:resource/f:Composition/f:author",
                                _ =>
                                [
                                    new AnyReferenceNamingWidget(
                                        widgetModel: new ReferenceNamingWidgetModel
                                        {
                                            Type = ReferenceNamingWidgetType.NameValuePair,
                                            LabelOverride = new EhdsiDisplayLabel("general.requester"),
                                        }
                                    ),
                                ]
                            )
                        ),
                        new Condition("f:meta/f:security",
                            new NameValuePair(
                                new LocalizedLabel("composition.meta.security"),
                                new CommaSeparatedBuilder("f:meta/f:security", _ => [new Coding()])
                            )
                        ),
                        compositionProperties.Optional(CompositionProperties.Confidentiality,
                            new NameValuePair(
                                new LocalizedLabel("composition.confidentiality"),
                                new EnumLabel("@value", "http://terminology.hl7.org/CodeSystem/v3-Confidentiality")
                            )
                        ),
                        compositionProperties.Optional(CompositionProperties.VersionNumberExtension, list =>
                        [
                            new NameValuePair(
                                new LocalizedLabel("composition.version"),
                                new ConcatBuilder(list,
                                    (_, _, nav) => [new ChangeContext(nav, new Text("f:valueString/@value"))],
                                    new ConstantText(", "))
                            ),
                        ]),
                        compositionProperties.Optional(CompositionProperties.Identifier,
                            new HideableDetails(new NameValuePair(
                                new LocalizedLabel("composition.identifier"),
                                new ShowIdentifier()
                            ))
                        ),
                        compositionProperties.Optional(CompositionProperties.Type,
                            new HideableDetails(new NameValuePair(
                                new LocalizedLabel("composition.type"),
                                new CodeableConcept()
                            ))
                        ),
                        compositionProperties.Optional(CompositionProperties.Date,
                            new NameValuePair(
                                new EhdsiDisplayLabel(LabelCodes.LastUpdate),
                                new ShowDateTime()
                            )
                        ),
                        new Optional(
                            "f:event[f:code/f:coding/f:system[@value='http://terminology.hl7.org/CodeSystem/v3-ActClass'] and f:code/f:coding/f:code[@value='PCPR'] and f:period]",
                            new HideableDetails(new NameValuePair(
                                new LocalizedLabel("composition.period"),
                                new ShowPeriod("f:period"),
                                new IdentifierSource()
                            ))
                        ),
                        new Condition("f:status",
                            new NameValuePair(
                                new LocalizedLabel("composition.status"),
                                new EnumLabel("f:status", "http://hl7.org/fhir/ValueSet/composition-status")
                            )),
                        new Condition("f:category",
                            new NameValuePair(
                                new LocalizedLabel("composition.category"),
                                new CommaSeparatedBuilder("f:category", _ => new CodeableConcept())
                            )),
                        new HideableDetails(
                            new NameValuePair(
                                new LocalizedLabel("general.display-tool-version"),
                                new ConstantText(VersionProvider.GetVersion())
                            )),
                        new HideableDetails(
                            new NameValuePair(
                                [new LocalizedLabel("general.render-date")],
                                renderDate
                            )),
                        compositionProperties.Optional(CompositionProperties.Encounter,
                            new HideableDetails(ContainerType.Span, children:
                                ShowSingleReference.WithDefaultDisplayHandler(_ => [new OrganizationHierarchy()]
                                )
                            )
                        ),
                    ], ContainerType.Div,
                    "two-col-grid name-value-pair-wrapper w-fit-content " + optionalClasses
                ),
                new If(_ => !containsLogo && containsQrCode,
                    qrCode
                ),
            ], flexContainerClasses: "justify-content-between mb-2 gap-2", flexWrap: false),
            new Row([
                new Heading([
                    new If(_ => context.DocumentType == DocumentType.ImagingOrder,
                        new LocalizedLabel("general.imaging-order-title"),
                        new TextContainer(TextStyle.Bold, new LocalizedLabel("general.imaging-order-title-bold"),
                            optionalClass: "big")
                    ).Else(
                        new Text("f:title/@value")
                    ),
                ], customClass: "mb-auto uppercase"),
                new Row([
                    new NarrativeModal(
                        openButtonContent: new EhdsiDisplayLabel(LabelCodes.OriginalNarrative)
                    ),
                    new Button(style: ButtonStyle.Outline, variant: ButtonVariant.ToggleDetails,
                        icon: SupportedIcons.Eye, altIcon: SupportedIcons.EyeSlash),
                    new Button(onClick: "expandOrCollapseAllSections();",
                        style: ButtonStyle.Primary, variant: ButtonVariant.CollapseSection),
                ], flexContainerClasses: "gap-1 flex-shrink-0 align-items-end", flexWrap: false)
            ], flexContainerClasses: "justify-content-between mb-2 row-gap-1"),
            new ModifierExtensionCheck(),
        ];

        var widgetContainer = new Container(widget, optionalClass: "document-header");

        return widgetContainer.Render(navigator, renderer, context);
    }

    private class SignatureValidationResult(DocumentSignatureValidationOperationResult validationResult)
        : Widget
    {
        public override Task<RenderResult> Render(
            XmlDocumentNavigator navigator,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            var tree = new Container([
                new If(_ => validationResult.OperationSuccess, new Concat([
                    new Column([
                        new Row([
                            new If(_ => validationResult.IsValid!.Value,
                                    new Container([
                                        new TextContainer(TextStyle.Bold,
                                            new LocalizedLabel("signature.validate-valid"),
                                            optionalClass: "text-success-500"),
                                        new Icon(SupportedIcons.Check),
                                    ], ContainerType.Span))
                                .Else(new Container([
                                    new TextContainer(TextStyle.Bold,
                                        new LocalizedLabel("signature.validate-invalid"),
                                        optionalClass: "text-alert-600"),
                                    new Icon(SupportedIcons.TriangleExclamation, optionalClass: "text-alert-600"),
                                ], ContainerType.Span)),
                        ]),

                        new If(_ => validationResult.Signor != null,
                            new Row([
                                new TextContainer(TextStyle.Light,
                                    new ConstantText(validationResult.Signor!))
                            ])),
                        new If(_ => validationResult.SignedAt != null,
                            new Row([
                                new Builder(() =>
                                {
                                    var signedAt = validationResult.SignedAt!.Value;
                                    const DateFormatType format =
                                        DateFormatType.SecondMinuteHourDayMonthYearTimezone;
                                    var timeWidgets =
                                        DateTimeFormats.GetTimeWidget(signedAt, context.Language, format);

                                    return
                                    [
                                        new TextContainer(TextStyle.Light,
                                            new Concat(timeWidgets)),
                                    ];
                                })
                            ])
                        )
                    ], flexContainerClasses: "text-nowrap")
                ])).Else(
                    new Container([
                        new TextContainer(TextStyle.Bold,
                        [
                            new If(_ => validationResult.ErrorMsg != null,
                                    new ConstantText(validationResult.ErrorMsg!))
                                .Else(new LocalizedLabel("signature.validate-error-other")),
                        ], optionalClass: "text-warning-600"),
                        new Icon(SupportedIcons.TriangleExclamation, optionalClass: "text-warning-600"),
                    ], ContainerType.Span)
                ),
            ], ContainerType.Span);

            return tree.Render(navigator, renderer, context);
        }
    }
}

public enum CompositionProperties
{
    [Extension("http://hl7.org/fhir/5.0/StructureDefinition/extension-Composition.version")]
    VersionNumberExtension,
    Confidentiality,
    Identifier,
    Type,
    Date,
    Encounter,
}