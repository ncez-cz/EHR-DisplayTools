using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Scalesoft.DisplayTool.Renderer;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Service.DataContracts;

namespace Scalesoft.DisplayTool.Service.Controllers;

[Route("v1/display-tools")]
[ApiController]
public class DisplayToolsController : ControllerBase
{
    private readonly DocumentRenderer m_documentRenderer;

    public DisplayToolsController(DocumentRenderer documentRenderer)
    {
        m_documentRenderer = documentRenderer;
    }

    [HttpPost("patient-summary")]
    public async Task<ActionResult<DisplayToolResponse>> PatientSummary(
        [FromBody] DisplayToolRequest request,
        [FromQuery] LevelOfDetail levelOfDetail = LevelOfDetail.UserDefined
    )
    {
        if (request.InputFormat == null || request.OutputFormat == null)
        {
            return BadRequest(CreateErrorResponse("Input and output formats are required"));
        }

        var options = MapDocumentOptions(request);
        var result = await m_documentRenderer.RenderAsync(
            request.FileContent,
            (InputFormat)request.InputFormat,
            (OutputFormat)request.OutputFormat,
            options,
            DocumentType.PatientSummary,
            levelOfDetail
        );
        var response = MapResponseAsJson(result);

        return response.IsRenderedSuccessfully ? Ok(response) : BadRequest(response);
    }

    [HttpPost("patient-summary/documentation")]
    public async Task<ActionResult<DisplayToolResponse>> PatientSummaryDocumentation(
        [FromBody] DisplayToolDocRequest request
    )
    {
        if (request.InputFormat == null || request.OutputFormat == null)
        {
            return BadRequest(CreateErrorResponse("Input and output formats are required"));
        }

        var result = await m_documentRenderer.RenderDocumentationAsync(
            request.FileContent,
            (InputFormat)request.InputFormat,
            (OutputFormat)request.OutputFormat,
            DocumentType.PatientSummary,
            LanguageOptions.Czech
        );
        var response = MapResponseAsJson(result);

        return response.IsRenderedSuccessfully ? Ok(response) : BadRequest(response);
    }

    [HttpPost("patient-summary/form-api")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DisplayToolResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> PatientSummaryFromForm(
        IFormFile file,
        [FromForm] InputFormatContract? inputFormat = null,
        [FromForm] OutputFormatContract? outputFormat = OutputFormatContract.Html,
        [FromForm] bool validateDocument = false,
        [FromForm] bool validateCodeValues = false,
        [FromForm] bool validateDigitalSignature = false,
        [FromForm] bool generateDocumentationInstead = false,
        [FromForm] bool preferTranslationsFromDocument = false,
        [FromQuery] LevelOfDetail levelOfDetail = LevelOfDetail.UserDefined
    )
    {
        if (inputFormat == null || outputFormat == null)
        {
            return BadRequest(CreateErrorResponse("Input and output formats are required"));
        }

        var fileContent = ReadFormFileToByteArray(file);
        var options = new DocumentOptions
        {
            ValidateDocument = validateDocument,
            ValidateCodeValues = validateCodeValues,
            ValidateDigitalSignature = validateDigitalSignature,
            PreferTranslationsFromDocument = preferTranslationsFromDocument,
        };
        var result = generateDocumentationInstead
            ? await m_documentRenderer.RenderDocumentationAsync(
                fileContent,
                (InputFormat)inputFormat,
                (OutputFormat)outputFormat,
                DocumentType.PatientSummary,
                LanguageOptions.Czech
            )
            : await m_documentRenderer.RenderAsync(
                fileContent,
                (InputFormat)inputFormat,
                (OutputFormat)outputFormat,
                options,
                DocumentType.PatientSummary,
                levelOfDetail
            );

        if (!result.IsRenderedSuccessfully)
        {
            var errorResponse = MapResponseAsJson(result);
            return BadRequest(errorResponse);
        }

        var actionResult = MapResponseAsFile(result, outputFormat.Value);
        return actionResult;
    }

    [HttpPost("hospital-discharge-report")]
    public async Task<ActionResult<DisplayToolResponse>> HospitalDischargeReport(
        [FromBody] DisplayToolRequest request,
        [FromQuery] LevelOfDetail levelOfDetail = LevelOfDetail.UserDefined
    )
    {
        if (request.InputFormat == null || request.OutputFormat == null)
        {
            return BadRequest(CreateErrorResponse("Input and output formats are required"));
        }

        var options = MapDocumentOptions(request);
        var result = await m_documentRenderer.RenderAsync(
            request.FileContent,
            (InputFormat)request.InputFormat,
            (OutputFormat)request.OutputFormat,
            options,
            DocumentType.DischargeReport,
            levelOfDetail
        );
        var response = MapResponseAsJson(result);

        return response.IsRenderedSuccessfully ? Ok(response) : BadRequest(response);
    }

    [HttpPost("hospital-discharge-report/form-api")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DisplayToolResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> HospitalDischargeFromForm(
        IFormFile file,
        [FromForm] InputFormatContract? inputFormat = null,
        [FromForm] OutputFormatContract? outputFormat = OutputFormatContract.Html,
        [FromForm] bool validateDocument = false,
        [FromForm] bool validateCodeValues = false,
        [FromForm] bool validateDigitalSignature = false,
        [FromForm] bool generateDocumentationInstead = false,
        [FromForm] bool preferTranslationsFromDocument = false,
        [FromQuery] LevelOfDetail levelOfDetail = LevelOfDetail.UserDefined
    )
    {
        if (inputFormat == null || outputFormat == null)
        {
            return BadRequest(CreateErrorResponse("Input and output formats are required"));
        }

        var fileContent = ReadFormFileToByteArray(file);
        var options = new DocumentOptions
        {
            ValidateDocument = validateDocument,
            ValidateCodeValues = validateCodeValues,
            ValidateDigitalSignature = validateDigitalSignature,
            PreferTranslationsFromDocument = preferTranslationsFromDocument,
        };
        var result = generateDocumentationInstead
            ? await m_documentRenderer.RenderDocumentationAsync(
                fileContent,
                (InputFormat)inputFormat,
                (OutputFormat)outputFormat,
                DocumentType.DischargeReport,
                LanguageOptions.Czech
            )
            : await m_documentRenderer.RenderAsync(
                fileContent,
                (InputFormat)inputFormat,
                (OutputFormat)outputFormat,
                options,
                DocumentType.DischargeReport,
                levelOfDetail
            );

        if (!result.IsRenderedSuccessfully)
        {
            var errorResponse = MapResponseAsJson(result);
            return BadRequest(errorResponse);
        }

        var actionResult = MapResponseAsFile(result, outputFormat.Value);
        return actionResult;
    }

    [HttpPost("laboratory-report")]
    public async Task<ActionResult<DisplayToolResponse>> LaboratoryReport(
        [FromBody] DisplayToolRequest request,
        [FromQuery] LevelOfDetail levelOfDetail = LevelOfDetail.UserDefined
    )
    {
        if (request.InputFormat == null || request.OutputFormat == null)
        {
            return BadRequest(CreateErrorResponse("Input and output formats are required"));
        }

        var options = MapDocumentOptions(request);
        var result = await m_documentRenderer.RenderAsync(
            request.FileContent,
            (InputFormat)request.InputFormat,
            (OutputFormat)request.OutputFormat,
            options,
            DocumentType.Laboratory,
            levelOfDetail
        );
        var response = MapResponseAsJson(result);

        return response.IsRenderedSuccessfully ? Ok(response) : BadRequest(response);
    }

    [HttpPost("laboratory-report/form-api")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DisplayToolResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> LaboratoryReportFromForm(
        IFormFile file,
        [FromForm] InputFormatContract? inputFormat = null,
        [FromForm] OutputFormatContract? outputFormat = OutputFormatContract.Html,
        [FromForm] bool validateDocument = false,
        [FromForm] bool validateCodeValues = false,
        [FromForm] bool validateDigitalSignature = false,
        [FromForm] bool generateDocumentationInstead = false,
        [FromForm] bool preferTranslationsFromDocument = false,
        [FromQuery] LevelOfDetail levelOfDetail = LevelOfDetail.UserDefined
    )
    {
        if (inputFormat == null || outputFormat == null)
        {
            return BadRequest(CreateErrorResponse("Input and output formats are required"));
        }

        var fileContent = ReadFormFileToByteArray(file);
        var options = new DocumentOptions
        {
            ValidateDocument = validateDocument,
            ValidateCodeValues = validateCodeValues,
            ValidateDigitalSignature = validateDigitalSignature,
            PreferTranslationsFromDocument = preferTranslationsFromDocument,
        };
        var result = generateDocumentationInstead
            ? await m_documentRenderer.RenderDocumentationAsync(
                fileContent,
                (InputFormat)inputFormat,
                (OutputFormat)outputFormat,
                DocumentType.Laboratory,
                LanguageOptions.Czech
            )
            : await m_documentRenderer.RenderAsync(
                fileContent,
                (InputFormat)inputFormat,
                (OutputFormat)outputFormat,
                options,
                DocumentType.Laboratory,
                levelOfDetail
            );

        if (!result.IsRenderedSuccessfully)
        {
            var errorResponse = MapResponseAsJson(result);
            return BadRequest(errorResponse);
        }

        var actionResult = MapResponseAsFile(result, outputFormat.Value);
        return actionResult;
    }

    [HttpPost("laboratory-order")]
    public async Task<ActionResult<DisplayToolResponse>> LaboratoryOrder(
        [FromBody] DisplayToolRequest request,
        [FromQuery] LevelOfDetail levelOfDetail = LevelOfDetail.UserDefined
    )
    {
        if (request.InputFormat == null || request.OutputFormat == null)
        {
            return BadRequest(CreateErrorResponse("Input and output formats are required"));
        }

        var options = MapDocumentOptions(request);
        var result = await m_documentRenderer.RenderAsync(
            request.FileContent,
            (InputFormat)request.InputFormat,
            (OutputFormat)request.OutputFormat,
            options,
            DocumentType.LaboratoryOrder,
            levelOfDetail
        );
        var response = MapResponseAsJson(result);

        return response.IsRenderedSuccessfully ? Ok(response) : BadRequest(response);
    }

    [HttpPost("laboratory-order/form-api")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DisplayToolResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> LaboratoryOrderFromForm(
        IFormFile file,
        [FromForm] InputFormatContract? inputFormat = null,
        [FromForm] OutputFormatContract? outputFormat = OutputFormatContract.Html,
        [FromForm] bool validateDocument = false,
        [FromForm] bool validateCodeValues = false,
        [FromForm] bool validateDigitalSignature = false,
        [FromForm] bool generateDocumentationInstead = false,
        [FromForm] bool preferTranslationsFromDocument = false,
        [FromQuery] LevelOfDetail levelOfDetail = LevelOfDetail.UserDefined
    )
    {
        if (inputFormat == null || outputFormat == null)
        {
            return BadRequest(CreateErrorResponse("Input and output formats are required"));
        }

        var fileContent = ReadFormFileToByteArray(file);
        var options = new DocumentOptions
        {
            ValidateDocument = validateDocument,
            ValidateCodeValues = validateCodeValues,
            ValidateDigitalSignature = validateDigitalSignature,
            PreferTranslationsFromDocument = preferTranslationsFromDocument,
        };
        var result = generateDocumentationInstead
            ? await m_documentRenderer.RenderDocumentationAsync(
                fileContent,
                (InputFormat)inputFormat,
                (OutputFormat)outputFormat,
                DocumentType.LaboratoryOrder,
                LanguageOptions.Czech
            )
            : await m_documentRenderer.RenderAsync(
                fileContent,
                (InputFormat)inputFormat,
                (OutputFormat)outputFormat,
                options,
                DocumentType.LaboratoryOrder,
                levelOfDetail
            );

        if (!result.IsRenderedSuccessfully)
        {
            var errorResponse = MapResponseAsJson(result);
            return BadRequest(errorResponse);
        }

        var actionResult = MapResponseAsFile(result, outputFormat.Value);
        return actionResult;
    }

    [HttpPost("imaging-report")]
    public async Task<ActionResult<DisplayToolResponse>> ImagingReport(
        [FromBody] DisplayToolRequest request,
        [FromQuery] LevelOfDetail levelOfDetail = LevelOfDetail.UserDefined
    )
    {
        if (request.InputFormat == null || request.OutputFormat == null)
        {
            return BadRequest(CreateErrorResponse("Input and output formats are required"));
        }

        var options = MapDocumentOptions(request);
        var result = await m_documentRenderer.RenderAsync(
            request.FileContent,
            (InputFormat)request.InputFormat,
            (OutputFormat)request.OutputFormat,
            options,
            DocumentType.ImagingReport,
            levelOfDetail
        );
        var response = MapResponseAsJson(result);

        return response.IsRenderedSuccessfully ? Ok(response) : BadRequest(response);
    }

    [HttpPost("imaging-report/form-api")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DisplayToolResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ImagingReportFromForm(
        IFormFile file,
        [FromForm] InputFormatContract? inputFormat = null,
        [FromForm] OutputFormatContract? outputFormat = OutputFormatContract.Html,
        [FromForm] bool validateDocument = false,
        [FromForm] bool validateCodeValues = false,
        [FromForm] bool validateDigitalSignature = false,
        [FromForm] bool generateDocumentationInstead = false,
        [FromForm] bool preferTranslationsFromDocument = false,
        [FromQuery] LevelOfDetail levelOfDetail = LevelOfDetail.UserDefined
    )
    {
        if (inputFormat == null || outputFormat == null)
        {
            return BadRequest(CreateErrorResponse("Input and output formats are required"));
        }

        var fileContent = ReadFormFileToByteArray(file);
        var options = new DocumentOptions
        {
            ValidateDocument = validateDocument,
            ValidateCodeValues = validateCodeValues,
            ValidateDigitalSignature = validateDigitalSignature,
            PreferTranslationsFromDocument = preferTranslationsFromDocument,
        };
        var result = generateDocumentationInstead
            ? await m_documentRenderer.RenderDocumentationAsync(
                fileContent,
                (InputFormat)inputFormat,
                (OutputFormat)outputFormat,
                DocumentType.ImagingReport,
                LanguageOptions.Czech
            )
            : await m_documentRenderer.RenderAsync(
                fileContent,
                (InputFormat)inputFormat,
                (OutputFormat)outputFormat,
                options,
                DocumentType.ImagingReport,
                levelOfDetail
            );

        if (!result.IsRenderedSuccessfully)
        {
            var errorResponse = MapResponseAsJson(result);
            return BadRequest(errorResponse);
        }

        var actionResult = MapResponseAsFile(result, outputFormat.Value);
        return actionResult;
    }
    
    [HttpPost("imaging-order")]
    public async Task<ActionResult<DisplayToolResponse>> ImagingOrder(
        [FromBody] DisplayToolRequest request,
        [FromQuery] LevelOfDetail levelOfDetail = LevelOfDetail.UserDefined
    )
    {
        if (request.InputFormat == null || request.OutputFormat == null)
        {
            return BadRequest(CreateErrorResponse("Input and output formats are required"));
        }

        var options = MapDocumentOptions(request);
        var result = await m_documentRenderer.RenderAsync(
            request.FileContent,
            (InputFormat)request.InputFormat,
            (OutputFormat)request.OutputFormat,
            options,
            DocumentType.ImagingOrder,
            levelOfDetail
        );
        var response = MapResponseAsJson(result);

        return response.IsRenderedSuccessfully ? Ok(response) : BadRequest(response);
    }

    [HttpPost("imaging-order/form-api")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DisplayToolResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ImagingOrderFromForm(
        IFormFile file,
        [FromForm] InputFormatContract? inputFormat = null,
        [FromForm] OutputFormatContract? outputFormat = OutputFormatContract.Html,
        [FromForm] bool validateDocument = false,
        [FromForm] bool validateCodeValues = false,
        [FromForm] bool validateDigitalSignature = false,
        [FromForm] bool generateDocumentationInstead = false,
        [FromForm] bool preferTranslationsFromDocument = false,
        [FromQuery] LevelOfDetail levelOfDetail = LevelOfDetail.UserDefined
    )
    {
        if (inputFormat == null || outputFormat == null)
        {
            return BadRequest(CreateErrorResponse("Input and output formats are required"));
        }

        var fileContent = ReadFormFileToByteArray(file);
        var options = new DocumentOptions
        {
            ValidateDocument = validateDocument,
            ValidateCodeValues = validateCodeValues,
            ValidateDigitalSignature = validateDigitalSignature,
            PreferTranslationsFromDocument = preferTranslationsFromDocument,
        };
        var result = generateDocumentationInstead
            ? await m_documentRenderer.RenderDocumentationAsync(
                fileContent,
                (InputFormat)inputFormat,
                (OutputFormat)outputFormat,
                DocumentType.ImagingOrder,
                LanguageOptions.Czech
            )
            : await m_documentRenderer.RenderAsync(
                fileContent,
                (InputFormat)inputFormat,
                (OutputFormat)outputFormat,
                options,
                DocumentType.ImagingOrder,
                levelOfDetail
            );

        if (!result.IsRenderedSuccessfully)
        {
            var errorResponse = MapResponseAsJson(result);
            return BadRequest(errorResponse);
        }

        var actionResult = MapResponseAsFile(result, outputFormat.Value);
        return actionResult;
    }

    private byte[] ReadFormFileToByteArray(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        var fileContent = memoryStream.ToArray();
        return fileContent;
    }

    private DisplayToolResponse CreateErrorResponse(string message)
    {
        return new DisplayToolResponse
        {
            Content = [],
            IsRenderedSuccessfully = false,
            Errors = [message],
        };
    }

    private DocumentOptions MapDocumentOptions(DisplayToolRequest request)
    {
        var options = new DocumentOptions
        {
            ValidateDocument = request.ValidateDocument,
            ValidateCodeValues = request.ValidateCodeValues,
            ValidateDigitalSignature = request.ValidateDigitalSignature,
            PreferTranslationsFromDocument = request.PreferTranslationsFromDocument,
        };
        return options;
    }

    private DisplayToolResponse MapResponseAsJson(DocumentResult result)
    {
        var response = new DisplayToolResponse
        {
            Content = result.Content,
            IsRenderedSuccessfully = result.IsRenderedSuccessfully,
            Errors = result.Errors,
            Warnings = result.Warnings,
        };
        return response;
    }

    private FileContentResult MapResponseAsFile(DocumentResult documentResult, OutputFormatContract outputFormat)
    {
        var fileName = $"Result-{DateTime.Now}.{outputFormat.ToString().ToLowerInvariant()}";
        var contentType = outputFormat switch
        {
            OutputFormatContract.Html => MediaTypeNames.Text.Html,
            OutputFormatContract.Pdf => MediaTypeNames.Application.Pdf,
            _ => MediaTypeNames.Application.Octet,
        };

        foreach (var errorMessage in documentResult.Errors)
        {
            Response.Headers.Append("X-Error", errorMessage);
        }

        foreach (var warningMessage in documentResult.Warnings)
        {
            Response.Headers.Append("X-Warning", warningMessage);
        }

        return File(documentResult.Content, contentType, fileName);
    }
}