namespace FelixELabs.OfficeToPdfSdk.Options;

public sealed class LibreOfficeOptions
{
    /// <summary>
    /// Path to the LibreOffice executable (soffice / soffice.exe).
    /// Defaults to "soffice" (bare name) which searches your system PATH.
    /// For non-standard installations, provide the full path:
    ///  - Windows: "C:\Program Files\LibreOffice\program\soffice.exe"
    ///  - Linux/Mac: "/opt/libreoffice/soffice"
    /// </summary>
    public string ExecutablePath { get; set; } = "soffice";

    /// <summary>Maximum time allowed for a single conversion.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(2);
}
