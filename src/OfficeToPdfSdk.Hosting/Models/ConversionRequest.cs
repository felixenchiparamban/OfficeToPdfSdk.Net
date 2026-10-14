using FelixELabs.OfficeToPdfSdk.Enums;

namespace FelixELabs.OfficeToPdfSdk.Hosting.Models;

/// <summary>
/// Represents a request to convert an office document to PDF.
/// Sent from a client to the conversion server.
/// </summary>
public class ConversionRequest
{
    /// <summary>
    /// The file path to the office document to convert.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// The kind of office document (Word, Excel, PowerPoint).
    /// </summary>
    public OfficeDocumentKind DocumentKind { get; set; }

    /// <summary>
    /// Optional: Output file path for the converted PDF.
    /// If not provided, server determines the output path.
    /// </summary>
    public string? OutputFilePath { get; set; }
}

