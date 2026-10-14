using FelixELabs.OfficeToPdfSdk.Enums;
using FelixELabs.OfficeToPdfSdk.Hosting.Models;
using FelixELabs.OfficeToPdfSdk.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FelixELabs.OfficeToPdfSdk.ConversionService.Controllers;

/// <summary>
/// REST API controller for document conversion operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ConversionController(IOfficeToPdfConverter converter, ILogger<ConversionController> logger) : ControllerBase
{
    private readonly IOfficeToPdfConverter _converter = converter ?? throw new ArgumentNullException(nameof(converter));
    private readonly ILogger<ConversionController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Converts an office document to PDF.
    /// </summary>
    /// <param name="file">The office document file to convert.</param>
    /// <param name="documentKind">The type of office document (Word, Excel, PowerPoint).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The converted PDF file or error details.</returns>
    [HttpPost("convert")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ConversionResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ConversionResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Convert([FromForm] IFormFile? file, [FromForm] OfficeDocumentKind documentKind, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new ConversionResponse
            {
                Success = false,
                Error = "File is required"
            });
        }

        _logger.LogInformation("Processing conversion request for {FileName} (DocumentKind: {DocumentKind})", file.FileName, documentKind);

        try
        {
            await using var inputStream = file.OpenReadStream();
            using var outputStream = new MemoryStream();

            await _converter.ConvertAsync(inputStream, file.FileName, documentKind, outputStream, cancellationToken);

            var pdfBytes = outputStream.ToArray();

            _logger.LogInformation("Conversion successful for {FileName}. Output size: {Size} bytes", file.FileName, pdfBytes.Length);

            var pdfFileName = Path.GetFileNameWithoutExtension(file.FileName) + ".pdf";
            return File(pdfBytes, "application/pdf", pdfFileName);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Conversion operation was cancelled for {FileName}", file.FileName);
            return StatusCode(
                StatusCodes.Status499ClientClosedRequest,
                new ConversionResponse
                {
                    Success = false,
                    Error = "Conversion operation was cancelled"
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Conversion failed for {FileName}", file.FileName);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ConversionResponse
                {
                    Success = false,
                    Error = ex.Message,
                    StackTrace = ex.StackTrace
                });
        }
    }

    /// <summary>
    /// Gets information about supported document types.
    /// </summary>
    [HttpGet("supported-types")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<object>> GetSupportedTypes()
    {
        var types = Enum.GetValues<OfficeDocumentKind>()
            .Select(kind => new
            {
                kind = kind.ToString(),
                value = (int)kind
            })
            .ToList();

        return Ok(types);
    }
}

