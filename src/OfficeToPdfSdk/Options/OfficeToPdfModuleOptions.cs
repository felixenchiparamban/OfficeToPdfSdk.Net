using FelixELabs.OfficeToPdfSdk.Enums;

namespace FelixELabs.OfficeToPdfSdk.Options;

public sealed class OfficeToPdfModuleOptions
{
    public bool IsEnabled { get; set; } = true;
    
    public string? TempPath { get; set; }

    public OfficeProviderKind OfficeProvider { get; set; } = OfficeProviderKind.None;

    public LibreOfficeOptions LibreOffice { get; set; } = new();
}
