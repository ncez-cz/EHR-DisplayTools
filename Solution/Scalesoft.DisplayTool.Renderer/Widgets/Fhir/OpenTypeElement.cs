using System.Web;
using Microsoft.Extensions.Logging;
using Scalesoft.DisplayTool.Renderer.DocumentNavigation;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;
using PrimitiveType = Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Observation.PrimitiveType;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

/// <summary>
/// </summary>
/// <param name="collapsibleContent">
///     Required nullable parameter for collapsible row content. Used to populate SampledData rendered chart. If
///     SampledData data type cannot appear at the given path, set to null.
/// </param>
/// <param name="prefix">Node name prefix</param>
/// <param name="path">Rendered XPath</param>
public class OpenTypeElement(
    StructuredDetails? collapsibleContent,
    string prefix = "value",
    string path = ".",
    OpenTypeElementRenderingHints? hints = null,
    bool useChartInDetailsPlaceholder = true
) : Widget
{
    // Actual url is http://hl7.org/fhir/5.0/StructureDefinition/extension-Observation.value[x] but this might occur outside Observations too.
    public const string ValueR5ExtensionPath =
        "f:extension[starts-with(@url,'http://hl7.org/fhir/5.0/StructureDefinition') and contains(@url, '.value[x]')]/f:valueAttachment";

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var parentNode = navigator.SelectSingleNode(path);

        var valueNav = parentNode.SelectSingleNode($"*[starts-with(local-name(), '{prefix}')]");

        if (context.RenderMode == RenderMode.Documentation)
        {
            return valueNav.GetFullPath();
        }

        if (valueNav.Node == null)
        {
            var valueExtension = parentNode.SelectSingleNode(ValueR5ExtensionPath);
            if (valueExtension.Node != null)
            {
                return await new ChangeContext(valueExtension, new Attachment()).Render(parentNode, renderer, context);
            }

            return await new Optional("f:dataAbsentReason", new CodeableConcept()).Render(
                parentNode,
                renderer,
                context
            );
        }

        var absentReasonExtension =
            valueNav.SelectSingleNode("f:extension[@url='http://hl7.org/fhir/StructureDefinition/data-absent-reason']");
        if (absentReasonExtension.Node != null)
        {
            return await
                new ChangeContext(
                    absentReasonExtension,
                    new EnumLabel("f:valueCode", "http://hl7.org/fhir/ValueSet/data-absent-reason")
                ).Render(navigator, renderer, context);
        }

        var valueNodeName = valueNav.Node.Name;
        var suffix = valueNodeName[prefix.Length..];

        /*Primitive types*/
        if (Enum.TryParse<WidgetUtils.PrimitiveType>(suffix, out var type))
        {
            return await new PrimitiveType(prefix, suffix).Render(navigator, renderer, context);
        }

        Widget unsupportedContent =
            new TextContainer(
                TextStyle.Regular,
                [
                    new ConstantText("Nepodporovaný kódovaný obsah"),
                    new ConstantText(": "),
                    new TextContainer(
                        TextStyle.Small,
                        [
                            new LineBreak(),
                            new Container(
                                [new ConstantText(valueNav.Node.InnerXml)],
                                ContainerType.Span,
                                idSource: valueNav
                            ),
                        ]
                    ),
                    new LineBreak(),
                ]
            );

        Widget? result;
        switch (suffix)
        {
            case DataTypes.Period or DataTypes.Age or DataTypes.Range or DataTypes.Timing:
                result = new ChangeContext(parentNode, new Chronometry(prefix));
                break;
            case DataTypes.CodeableConcept:
                result = new CodeableConcept();
                break;
            case DataTypes.Annotation:
                result = new ShowAnnotationCompact();
                break;
            case DataTypes.Attachment:
                result = new Attachment();
                break;
            case DataTypes.Coding:
                result = new Coding();
                break;
            case DataTypes.HumanName or DataTypes.Reference:
                result = new AnyReferenceNamingWidget();
                break;
            case DataTypes.Identifier:
                result = new ShowIdentifier();
                break;
            case DataTypes.Quantity:
                result = new ShowQuantity(
                    showUnit: hints?.HasFlag(OpenTypeElementRenderingHints.HideQuantityUnit) != true
                );
                break;
            case DataTypes.Ratio:
                result = new ShowRatio();
                break;
            case DataTypes.SampledData:
                if (collapsibleContent == null)
                {
                    // sampleData occurred at an unexpected point
                    var logger = context.LoggerFactory.CreateLogger(nameof(OpenTypeElement));
                    if (logger.IsEnabled(LogLevel.Error))
                    {
                        logger.LogError(
                            "SampledData container not provided at Xpath {xpath}",
                            parentNode.GetFullPath()
                        );
                    }

                    result = unsupportedContent;
                    break;
                }

                var sampledDataId = valueNav.Node.UniqueId();
                context.IdentifierDisplayTitleMapping.Add(sampledDataId);
                var id = HttpUtility.UrlEncode(sampledDataId);
                var idTitle = context.IdentifierDisplayTitleMapping.GetTitle(sampledDataId);
                var header = new ConstantText($"Graf č. {idTitle} - reprezentace dat");
                var chartWidget = new ShowSampledData($"f:{valueNodeName}", idSource: new IdentifierSource(id));
                var link = new Link(
                    new ConstantText(useChartInDetailsPlaceholder
                        ? $"Viz přiložený graf č. {idTitle} s hodnotami měření v detailu"
                        : $"Viz přiložený graf č. {idTitle} s hodnotami měření"),
                    $"#{id}"
                );
                collapsibleContent.Add(new CollapsibleDetail(header, chartWidget, IsHideable: false));
                result = new TextContainer(TextStyle.Muted, [link]);
                break;
            case DataTypes.Duration:
                result = new ShowDuration();
                break;
            case DataTypes.Address:
                result = new Address(showLabel: hints?.HasFlag(OpenTypeElementRenderingHints.HideAddressLabel) != true);
                break;
            case DataTypes.Money:
                result = new ShowMoney();
                break;
            case DataTypes.Signature:
                result = new ShowSignature();
                break;
            default:
                result = unsupportedContent;
                break;
        }

        return await result.Render(valueNav, renderer, context);
    }
}

[Flags]
public enum OpenTypeElementRenderingHints
{
    None = 0,
    HideQuantityUnit = 1 << 0,
    HideAddressLabel = 1 << 1,
}