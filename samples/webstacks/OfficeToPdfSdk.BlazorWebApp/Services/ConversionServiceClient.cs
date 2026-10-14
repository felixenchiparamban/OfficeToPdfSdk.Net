using FelixELabs.OfficeToPdfSdk.Enums;

namespace FelixELabs.OfficeToPdfSdk.BlazorWebApp.Services;

/// <summary>
/// HTTP client for communicating with the OfficeToPdfSdk.ConversionService REST API.
/// </summary>
public sealed class ConversionServiceClient(HttpClient httpClient, ILogger<ConversionServiceClient> logger) : IConversionServiceClient
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ILogger<ConversionServiceClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Converts an office document to PDF by calling the conversion service.
    /// </summary>
    public async Task<byte[]> ConvertAsync(
        byte[] fileBytes,
        OfficeDocumentKind documentKind,
        string? fileName = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileBytes);

        try
        {
            _logger.LogInformation("Sending conversion request for {FileName} to service", fileName ?? "document");

            // Create multipart form data
            using var content = new MultipartFormDataContent();
            using var fileStream = new MemoryStream(fileBytes);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/octet-stream");
            content.Add(fileContent, "file", fileName ?? "document");
            content.Add(new StringContent(((int)documentKind).ToString(System.Globalization.CultureInfo.InvariantCulture)), "documentKind");

            var response = await _httpClient.PostAsync(
                "/api/conversion/convert",
                content,
                cancellationToken);

            _logger.LogDebug("Service responded with status: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Service returned error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Conversion service returned {response.StatusCode}");
            }

            // Read the PDF file directly from response
            var pdfBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                throw new InvalidOperationException("Service returned no PDF data");
            }

            _logger.LogInformation("Conversion completed successfully. PDF size: {Size} bytes", pdfBytes.Length);

            return pdfBytes;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request to conversion service failed");
            throw new InvalidOperationException(
                $"Failed to connect to conversion service. Make sure the service is running at {_httpClient.BaseAddress}",
                ex);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Conversion request was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during conversion");
            throw;
        }
    }
}

