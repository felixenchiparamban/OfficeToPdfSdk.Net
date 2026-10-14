using FelixELabs.OfficeToPdfSdk.Enums;

namespace FelixELabs.OfficeToPdfSdk.Services.Interfaces;

public interface IOfficeToPdfConverter
{
    Task ConvertAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default);

    Task ConvertAsync(Stream input, string sourceFileName, OfficeDocumentKind documentKind, Stream output, CancellationToken cancellationToken = default);
}
