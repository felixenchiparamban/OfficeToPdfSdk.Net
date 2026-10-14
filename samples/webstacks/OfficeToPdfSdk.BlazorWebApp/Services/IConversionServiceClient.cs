using FelixELabs.OfficeToPdfSdk.Enums;
using FelixELabs.OfficeToPdfSdk.Hosting.Models;

namespace FelixELabs.OfficeToPdfSdk.BlazorWebApp.Services;

/// <summary>
/// Interface for converting office documents to PDF via a conversion service.
/// </summary>
public interface IConversionServiceClient
{
    /// <summary>
    /// Converts an office document to PDF.
    /// </summary>
    /// <param name="fileBytes">The office document file bytes.</param>
    /// <param name="documentKind">The kind of document (Word, Excel, PowerPoint).</param>
    /// <param name="fileName">Optional file name for logging purposes.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The converted PDF file bytes.</returns>
    /// <exception cref="HttpRequestException">If the service request fails.</exception>
    /// <exception cref="InvalidOperationException">If the conversion fails.</exception>
    Task<byte[]> ConvertAsync(
        byte[] fileBytes,
        OfficeDocumentKind documentKind,
        string? fileName = null,
        CancellationToken cancellationToken = default);
}

