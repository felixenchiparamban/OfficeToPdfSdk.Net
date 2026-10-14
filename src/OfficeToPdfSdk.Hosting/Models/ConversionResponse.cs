namespace FelixELabs.OfficeToPdfSdk.Hosting.Models;

/// <summary>
/// Represents the response from the conversion server after processing a conversion request.
/// </summary>
public class ConversionResponse
{
    /// <summary>
    /// Whether the conversion was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The file path to the converted PDF (if successful).
    /// </summary>
    public string? OutputFilePath { get; set; }

    /// <summary>
    /// Error message if conversion failed.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Stack trace if conversion failed (for debugging).
    /// </summary>
    public string? StackTrace { get; set; }
}

