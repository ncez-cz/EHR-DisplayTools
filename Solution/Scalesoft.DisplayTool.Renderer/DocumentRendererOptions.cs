using Microsoft.Extensions.Logging;
using Scalesoft.DisplayTool.Shared.Configuration;

namespace Scalesoft.DisplayTool.Renderer;

public class DocumentRendererOptions
{
    public ILoggerFactory? LoggerFactory { get; init; }
    public PdfRendererOptions? PdfRenderer { get; init; }
    public required ExternalServicesConfiguration ExternalServicesConfiguration { get; init; }
    public TranslatorConfiguration? TranslatorConfiguration { get; init; }
}

public class PdfRendererOptions
{
    public string? ChromiumExePath { get; set; }
    public bool DownloadChromium { get; set; }
    public string? DownloadChromiumPath { get; set; }
}