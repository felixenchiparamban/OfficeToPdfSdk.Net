using FelixELabs.OfficeToPdfSdk.Enums;

namespace FelixELabs.OfficeToPdfSdk.Providers.Interfaces;

public interface IOfficeToPdfProvider
{
    OfficeProviderKind Kind { get; }

    bool IsAvailable { get; }

    bool Supports(OfficeDocumentKind documentKind);

    Task ConvertAsync(Stream input, string sourceFileName, OfficeDocumentKind documentKind, Stream output, CancellationToken cancellationToken = default);
}
