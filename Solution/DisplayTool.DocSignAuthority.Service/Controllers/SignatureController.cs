using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using DisplayTool.DocSignAuthority.Service.Exceptions;
using DisplayTool.DocSignAuthority.Service.Managers;
using DisplayTool.DocSignAuthority.Service.Models;
using DisplayTool.DocSignAuthority.Service.Models.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace DisplayTool.DocSignAuthority.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(IgnoreApi = false)]
public class SignatureController : Controller
{
    private readonly DocumentSignManager m_documentSignManager;
    private readonly IMapper m_mapper;

    public SignatureController(DocumentSignManager documentSignManager, IMapper mapper)
    {
        m_documentSignManager = documentSignManager;
        m_mapper = mapper;
    }

    [HttpPost]
    [Route("encapsulated/sign")]
    [ProducesResponseType(typeof(DocumentSignResponseContract), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DocumentSignatureVerificationErrorContract), StatusCodes.Status400BadRequest)]
    public IActionResult SignEncapsulated([Required] DocumentSignContract contract)
    {
        if (!IsAcceptableFormat(contract.DocumentType, out var errorResult))
        {
            return errorResult;
        }

        var documentType = m_mapper.Map<DocumentType>(contract.DocumentType);
        var documentBytes = Convert.FromBase64String(contract.Base64Document);
        try
        {
            var signedDocument = m_documentSignManager.SignEncapsulated(documentBytes, documentType);
            var b64SignedDoc = Convert.ToBase64String(signedDocument);
            var response = new DocumentSignResponseContract { Base64Document = b64SignedDoc };

            return Ok(response);
        }
        catch (DocumentSignatureException e)
        {
            if (ContainsErrorCode(e, out var result))
            {
                return result;
            }

            throw;
        }
    }

    [HttpPost]
    [Route("file/encapsulated/sign")]
    public IActionResult SignEncapsulated(
        [Required] [FromForm] FileDocumentSignContract contract
    )
    {
        if (!IsAcceptableFormat(contract.DocumentType, out var errorResult))
        {
            return errorResult;
        }

        var file = contract.File;
        using var ms = new MemoryStream();
        file.CopyTo(ms);
        var documentBytes = ms.ToArray();
        var documentType = m_mapper.Map<DocumentType>(contract.DocumentType);
        try
        {
            var signedDocument = m_documentSignManager.SignEncapsulated(documentBytes, documentType);
            var resultFileName =
                $"{Path.GetFileNameWithoutExtension(file.FileName)}.signedEncapsulated{Path.GetExtension(file.FileName)}";

            return File(signedDocument, file.ContentType, resultFileName);
        }
        catch (DocumentSignatureException e)
        {
            if (ContainsErrorCode(e, out var result))
            {
                return result;
            }

            throw;
        }
    }

    [HttpPost]
    [Route("encapsulated/verify")]
    [ProducesResponseType(typeof(DocumentSignatureVerificationResponseContract), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DocumentSignatureVerificationErrorContract), StatusCodes.Status400BadRequest)]
    public IActionResult VerifyEncapsulated([Required] DocumentSignContract contract)
    {
        if (!IsAcceptableFormat(contract.DocumentType, out var errorResult))
        {
            return errorResult;
        }

        var documentType = m_mapper.Map<DocumentType>(contract.DocumentType);
        var documentBytes = Convert.FromBase64String(contract.Base64Document);
        try
        {
            var validationResult = m_documentSignManager.ValidateEncapsulated(documentBytes, documentType);
            var resultContract = m_mapper.Map<DocumentSignatureVerificationResponseContract>(validationResult);

            return Ok(resultContract);
        }
        catch (DocumentSignatureException e)
        {
            if (ContainsErrorCode(e, out var result))
            {
                return result;
            }

            throw;
        }
    }

    [HttpPost]
    [Route("file/encapsulated/verify")]
    [ProducesResponseType(typeof(DocumentSignatureVerificationResponseContract), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DocumentSignatureVerificationErrorContract), StatusCodes.Status400BadRequest)]
    public IActionResult VerifyEncapsulated(
        [Required] [FromForm] FileDocumentSignContract contract
    )
    {
        if (!IsAcceptableFormat(contract.DocumentType, out var errorResult))
        {
            return errorResult;
        }

        var documentType = m_mapper.Map<DocumentType>(contract.DocumentType);
        using var ms = new MemoryStream();
        contract.File.CopyTo(ms);
        var documentBytes = ms.ToArray();
        try
        {
            var validationResult = m_documentSignManager.ValidateEncapsulated(documentBytes, documentType);
            var resultContract = m_mapper.Map<DocumentSignatureVerificationResponseContract>(validationResult);

            return Ok(resultContract);
        }
        catch (DocumentSignatureException e)
        {
            if (ContainsErrorCode(e, out var result))
            {
                return result;
            }

            throw;
        }
    }

    [HttpPost]
    [Route("integrated/sign")]
    [ProducesResponseType(typeof(DocumentSignResponseContract), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DocumentSignatureVerificationErrorContract), StatusCodes.Status400BadRequest)]
    public IActionResult SignIntegrated([Required] DocumentSignContract contract)
    {
        if (!IsAcceptableFormat(contract.DocumentType, out var errorResult))
        {
            return errorResult;
        }

        var documentType = m_mapper.Map<DocumentType>(contract.DocumentType);
        var documentBytes = Convert.FromBase64String(contract.Base64Document);
        try
        {
            var signedDocument = m_documentSignManager.SignIntegrated(documentBytes, documentType);
            var b64SignedDoc = Convert.ToBase64String(signedDocument);
            var response = new DocumentSignResponseContract { Base64Document = b64SignedDoc };

            return Ok(response);
        }
        catch (DocumentSignatureException e)
        {
            if (ContainsErrorCode(e, out var result))
            {
                return result;
            }

            throw;
        }
    }

    [HttpPost]
    [Route("file/integrated/sign")]
    public IActionResult SignIntegrated(
        [Required] [FromForm] FileDocumentSignContract contract
    )
    {
        if (!IsAcceptableFormat(contract.DocumentType, out var errorResult))
        {
            return errorResult;
        }

        var file = contract.File;
        var documentType = m_mapper.Map<DocumentType>(contract.DocumentType);
        using var ms = new MemoryStream();
        file.CopyTo(ms);
        var documentBytes = ms.ToArray();
        try
        {
            var signedDocument = m_documentSignManager.SignIntegrated(documentBytes, documentType);

            var resultFileName =
                $"{Path.GetFileNameWithoutExtension(file.FileName)}.signedEmbedded{Path.GetExtension(file.FileName)}";

            return File(signedDocument, file.ContentType, resultFileName);
        }
        catch (DocumentSignatureException e)
        {
            if (ContainsErrorCode(e, out var result))
            {
                return result;
            }

            throw;
        }
    }

    [HttpPost]
    [Route("integrated/verify")]
    [ProducesResponseType(typeof(DocumentSignatureVerificationResponseContract), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DocumentSignatureVerificationErrorContract), StatusCodes.Status400BadRequest)]
    public IActionResult VerifyIntegrated([Required] DocumentSignContract contract)
    {
        if (!IsAcceptableFormat(contract.DocumentType, out var errorResult))
        {
            return errorResult;
        }

        var documentType = m_mapper.Map<DocumentType>(contract.DocumentType);
        var documentBytes = Convert.FromBase64String(contract.Base64Document);
        try
        {
            var validationResult = m_documentSignManager.ValidateIntegrated(documentBytes, documentType);

            var resultContract = m_mapper.Map<DocumentSignatureVerificationResponseContract>(validationResult);

            return Ok(resultContract);
        }
        catch (DocumentSignatureException e)
        {
            if (ContainsErrorCode(e, out var result))
            {
                return result;
            }

            throw;
        }
    }

    [HttpPost]
    [Route("file/integrated/verify")]
    public IActionResult VerifyIntegrated(
        [Required] [FromForm] FileDocumentSignContract contract
    )
    {
        if (!IsAcceptableFormat(contract.DocumentType, out var errorResult))
        {
            return errorResult;
        }

        var documentType = m_mapper.Map<DocumentType>(contract.DocumentType);
        using var ms = new MemoryStream();
        contract.File.CopyTo(ms);
        var documentBytes = ms.ToArray();
        try
        {
            var validationResult = m_documentSignManager.ValidateIntegrated(documentBytes, documentType);
            var resultContract = m_mapper.Map<DocumentSignatureVerificationResponseContract>(validationResult);

            return Ok(resultContract);
        }
        catch (DocumentSignatureException e)
        {
            if (ContainsErrorCode(e, out var result))
            {
                return result;
            }

            throw;
        }
    }

    private bool IsAcceptableFormat(
        DocumentTypeContract documentType,
        [NotNullWhen(false)] out IActionResult? errorResult
    )
    {
        if (documentType != DocumentTypeContract.FhirJson)
        {
            errorResult = new ObjectResult(new DocumentSignatureVerificationErrorContract
                    { ErrorCode = ErrorCodeContract.UnsupportedDocumentType })
                { StatusCode = StatusCodes.Status400BadRequest };
            return false;
        }

        errorResult = null;
        return true;
    }

    private bool ContainsErrorCode(
        DocumentSignatureException exception,
        [NotNullWhen(true)] out IActionResult? errorResult
    )
    {
        if (exception.ErrorCode != null)
        {
            var errorCodeContract = m_mapper.Map<ErrorCodeContract>(exception.ErrorCode);

            errorResult = new ObjectResult(new DocumentSignatureVerificationErrorContract
                    { ErrorCode = errorCodeContract })
                { StatusCode = StatusCodes.Status400BadRequest };
            return true;
        }

        errorResult = null;
        return false;
    }
}