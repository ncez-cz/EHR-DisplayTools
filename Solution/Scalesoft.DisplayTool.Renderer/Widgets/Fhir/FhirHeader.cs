using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class FhirHeader : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
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

        Widget[] widget =
        [
            new If(_ => containsLogo && containsQrCode,
                new Row([
                    new ChangeContext(logoNavigator!, new HospitalLogo()),
                    qrCode,
                ], flexContainerClasses: "justify-content-between mb-2")
            ),
            new Row([
                new If(_ => containsLogo && !containsQrCode,
                    new ChangeContext(logoNavigator!, new HospitalLogo())
                ),
                new Container(
                    [
                        new If(
                            _ => (context.DocumentType is DocumentType.ImagingOrder or DocumentType.LaboratoryOrder) &&
                                 navigator.EvaluateCondition(
                                     "/f:Bundle/f:entry/f:resource/f:Composition/f:author/f:reference"
                                 ),
                            new ConcatBuilder(
                                "/f:Bundle/f:entry/f:resource/f:Composition/f:author",
                                _ =>
                                [
                                    new NameValuePair(
                                        new ConstantText("Žadatel"),
                                        new AnyReferenceNamingWidget()
                                    ),
                                ]
                            )
                        ),
                        new Optional(
                            "f:identifier",
                            new HideableDetails(new NameValuePair(
                                new ConstantText("Identifikátor dokumentu"),
                                new ShowIdentifier()
                            ))
                        ),
                        new HideableDetails(new NameValuePair(
                            new DisplayLabel(LabelCodes.LastUpdate),
                            new ShowDateTime("f:date")
                        )),
                        new Optional(
                            "f:event[f:code/f:coding/f:system[@value='http://terminology.hl7.org/CodeSystem/v3-ActClass'] and f:code/f:coding/f:code[@value='PCPR'] and f:period]",
                            new HideableDetails(new NameValuePair(
                                new ConstantText("Období zahrnuté v dokumentu"),
                                new ShowPeriod("f:period"),
                                new IdentifierSource()
                            ))
                        ),
                        new NameValuePair(
                            new ConstantText("Stav dokumentu"),
                            new EnumLabel("f:status", "http://hl7.org/fhir/ValueSet/composition-status")
                        ),
                        new HideableDetails(
                        new NameValuePair(
                            new ConstantText("Kategorie"),
                            new ChangeContext("f:type", new CodeableConcept())
                        )),
                        new HideableDetails(
                        new NameValuePair(
                            new ConstantText("DisplayTool verze"),
                            new ConstantText(VersionProvider.GetVersion())
                        )),
                        new HideableDetails(
                        new NameValuePair(
                            [new ConstantText("Vykresleno")],
                            renderDate
                        )),
                        new Optional(
                            "f:encounter",
                            new HideableDetails(
                                ShowSingleReference.WithDefaultDisplayHandler(_ => [new OrganizationHierarchy()]))
                        ),
                    ], ContainerType.Div,
                    "two-col-grid w-100 " + optionalClasses
                ),
                new If(_ => !containsLogo && containsQrCode,
                    qrCode
                ),
            ], flexContainerClasses: "justify-content-between mb-2", flexWrap: false),
            new Row([
                new Heading([
                    new If(_ => context.DocumentType == DocumentType.ImagingOrder,
                        new ConstantText("Poukaz na vyšetření / ošetření "),
                        new TextContainer(TextStyle.Bold, new ConstantText("Z"), optionalClass: "big")
                    ).Else(
                        new Text("f:title/@value")
                    ),
                ], customClass: "mb-auto uppercase"),
                new Row([
                    new NarrativeModal(
                        openButtonContent: new DisplayLabel(LabelCodes.OriginalNarrative)
                    ),
                    new Button(style: ButtonStyle.Outline, variant: ButtonVariant.ToggleDetails,
                        icon: SupportedIcons.Eye, altIcon: SupportedIcons.EyeSlash),
                    new Button(onClick: "expandOrCollapseAllSections();",
                        style: ButtonStyle.Primary, variant: ButtonVariant.CollapseSection),
                ], flexContainerClasses: "gap-1 flex-shrink-0 align-items-end", flexWrap: false)
            ], flexContainerClasses: "justify-content-between mb-2", flexWrap: false),
            new ModifierExtensionCheck(),
        ];

        var widgetContainer = new Container(widget, optionalClass: "document-header");

        return widgetContainer.Render(navigator, renderer, context);
    }
}