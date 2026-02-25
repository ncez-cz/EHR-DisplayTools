using Microsoft.Extensions.Logging;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public abstract class Widget : IEquatable<Widget>
{
    protected uint Id { get; } = GenerateId();

    private static uint m_idCounter;
    private static uint GenerateId() => Interlocked.Increment(ref m_idCounter);

    public bool IsNullWidget { get; protected init; }

    public abstract Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    );

    protected bool IsDataAbsent(XmlDocumentNavigator navigator, string path)
    {
        var absentReasonValue = navigator
            .SelectSingleNode(
                $"{path}/f:extension[@url='http://hl7.org/fhir/StructureDefinition/data-absent-reason']/f:valueCode/@value")
            .Node?.Value;
        var absentReasonCodingValue = navigator
            .SelectSingleNode(
                $"{path}/f:coding[f:system/@value='http://terminology.hl7.org/CodeSystem/data-absent-reason']/f:code/@value")
            .Node?.Value;

        return absentReasonValue != null || absentReasonCodingValue != null;
    }

    protected async Task<InternalRenderResult> RenderInternal(
        XmlDocumentNavigator data,
        IWidgetRenderer renderer,
        RenderContext context,
        params Widget[] widgets
    )
    {
        return await RenderInternal(data, renderer, widgets, context);
    }

    protected async Task<InternalRenderResult> RenderInternal(
        XmlDocumentNavigator data,
        IWidgetRenderer renderer,
        IEnumerable<Widget> widgets,
        RenderContext context
    )
    {
        var results = new Dictionary<uint, string?>();
        List<ParseError> errors = [];

        foreach (var widget in widgets)
        {
            if (widget.IsNullWidget)
            {
                continue;
            }

            var result = await widget.Render(data, renderer, context);


            results[widget.Id] = result.Content;
            errors.AddRange(result.Errors);
        }

        return new InternalRenderResult
        {
            Contents = results,
            Errors = errors,
        };
    }

    protected void HandleIds(
        RenderContext context,
        XmlDocumentNavigator navigator,
        ViewModelBase viewModel,
        IdentifierSource? idSource,
        IdentifierSource? visualIdSource
    )
    {
        if (idSource?.ConstantVal != null)
        {
            viewModel.Id = idSource.ConstantVal;
        }
        else
        {
            var usedNavigator = idSource?.UseContextNav == true ? navigator : idSource?.IdNav;
            if (navigator.IsInHideableContext)
            {
                usedNavigator = usedNavigator?.WithHideableContext();
            }

            ParseAndRegisterId(context, usedNavigator, viewModel);
        }

        if (visualIdSource?.ConstantVal != null)
        {
            viewModel.VisualId = visualIdSource.ConstantVal;
        }
        else
        {
            SetVisualId(visualIdSource?.UseContextNav == true ? navigator : visualIdSource?.IdNav, viewModel);
        }
    }

    /// <summary>
    ///     Parses and registers rendered resource id.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="navigator"></param>
    /// <param name="viewModel"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private void ParseAndRegisterId(RenderContext context, XmlDocumentNavigator? navigator, ViewModelBase viewModel)
    {
        if (navigator == null)
        {
            return;
        }

        RenderedResourceAddFailReason? failReason = null;
        if (ResourceIdentifier.TryFromNavigator(navigator, out var id))
        {
            if (context.AddRenderedResource(navigator, id, out var resourceAddFailReason,
                    navigator.IsInHideableContext))
            {
                viewModel.Id = id.BuildId();
            }
            else
            {
                failReason = resourceAddFailReason;
            }
        }
        else
        {
            failReason = RenderedResourceAddFailReason.MissingId;
        }

        if (failReason == null)
        {
            return;
        }

        var logger = context.LoggerFactory.CreateLogger(nameof(Widget));
        var fullPath = navigator.GetFullPath();
        switch (failReason)
        {
            case RenderedResourceAddFailReason.MissingId:
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug(
                        "Failed to register a rendered widget with path {xpath} - the resource id is missing",
                        fullPath); // decrease the logging level to avoid spamming the log
                }

                break;
            case RenderedResourceAddFailReason.MissingReferencedResource:
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation(
                        "Failed to register a rendered widget with path {xpath} - the referenced resource is missing",
                        fullPath);
                }

                break;
            case RenderedResourceAddFailReason.MissingXmlNode:
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation(
                        "Failed to register a rendered widget with path {xpath} - referenced resource XML node is missing",
                        fullPath);
                }

                break;
            case RenderedResourceAddFailReason.DifferentPositionInDocument:
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation(
                        "Failed to register a rendered widget with path {xpath} - document position of currently rendered resource and referenced resource differs",
                        fullPath);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SetVisualId(XmlDocumentNavigator? navigator, ViewModelBase viewModel)
    {
        if (navigator == null)
        {
            return;
        }

        if (!ResourceIdentifier.TryFromNavigator(navigator, out var id))
        {
            return;
        }

        viewModel.VisualId = id.BuildId();
    }

    protected class InternalRenderResult
    {
        public required Dictionary<uint, string?> Contents { get; init; }

        public List<ParseError> Errors { get; init; } = [];

        public string? GetContent(Widget widget)
        {
            if (widget.IsNullWidget)
            {
                return null;
            }

            return Contents[widget.Id];
        }

        public List<string> GetValidContents(IEnumerable<Widget>? widgets = null)
        {
            if (widgets == null)
            {
                return Contents.Values.OfType<string>().ToList();
            }

            var widgetIds = widgets.Select(x => x.Id).ToList();
            return Contents
                .Where(x => widgetIds.Contains(x.Key))
                .Select(x => x.Value)
                .OfType<string>()
                .ToList();
        }

        public bool IsFatal =>
            Errors.Count > 0 && Errors.Max(e => e.Severity) >= ErrorSeverity.Fatal;

        public bool IsNull => Contents.All(x => x.Value == null) && Errors.Count == 0;
    }

    public bool Equals(Widget? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Widget)obj);
    }

    public override int GetHashCode()
    {
        return (int)Id;
    }
}