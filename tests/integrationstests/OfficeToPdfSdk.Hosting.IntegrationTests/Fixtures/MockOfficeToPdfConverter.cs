using FelixELabs.OfficeToPdfSdk.Enums;
using FelixELabs.OfficeToPdfSdk.Services.Interfaces;

namespace FelixELabs.OfficeToPdfSdk.Hosting.IntegrationTests.Fixtures;

internal sealed class MockOfficeToPdfConverter : IOfficeToPdfConverter
{
    public async Task ConvertAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        var pdfHeader = "%PDF-1.4\n"u8.ToArray();
        await File.WriteAllBytesAsync(outputPath, pdfHeader, cancellationToken);
    }

    public async Task ConvertAsync(Stream inputStream, string sourceFileName, OfficeDocumentKind kind, Stream outputStream, CancellationToken cancellationToken = default)
    {
        var pdfHeader = "%PDF-1.4\n"u8.ToArray();
        await outputStream.WriteAsync(pdfHeader, cancellationToken);
    }
}
