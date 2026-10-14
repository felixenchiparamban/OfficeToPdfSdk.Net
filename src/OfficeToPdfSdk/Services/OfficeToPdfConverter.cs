using FelixELabs.OfficeToPdfSdk.Enums;
using FelixELabs.OfficeToPdfSdk.Exceptions;
using FelixELabs.OfficeToPdfSdk.Extensions;
using FelixELabs.OfficeToPdfSdk.Options;
using FelixELabs.OfficeToPdfSdk.Providers.Interfaces;
using FelixELabs.OfficeToPdfSdk.Services.Interfaces;
using FelixELabs.OfficeToPdfSdk.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FelixELabs.OfficeToPdfSdk.Services;

public sealed class OfficeToPdfConverter : IOfficeToPdfConverter
{
    private readonly IReadOnlyList<IOfficeToPdfProvider> _providers;
    private readonly OfficeToPdfModuleOptions _options;
    private readonly ILogger<OfficeToPdfConverter> _logger;
    private IOfficeToPdfProvider? _cachedProvider;

    public OfficeToPdfConverter(IEnumerable<IOfficeToPdfProvider> providers, IOptions<OfficeToPdfModuleOptions> options, ILogger<OfficeToPdfConverter> logger)
    {
        ArgumentNullException.ThrowIfNull(providers);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _providers = [.. providers];

        if (_providers.Count == 0)
        {
            throw new ArgumentException("At least one IOfficeToPdfProvider must be registered.", nameof(providers));
        }
        
        _options = options.Value;
        _logger = logger;

        OfficeToPdfLogging.ModuleActivating(_logger, null);
    }

    public async Task ConvertAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        var kind = DocumentKindDetector.FromPath(inputPath);
        if (kind == OfficeDocumentKind.Unknown)
        {
            throw new OfficeConversionException($"Unable to detect Office document kind from '{inputPath}'.");
        }

        var sourceFileName = Path.GetFileName(inputPath);
        await using var input = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 1024 * 1024, useAsync: true);
        await using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 1024 * 1024, useAsync: true);
        await ConvertAsync(input, sourceFileName, kind, output, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task ConvertAsync(Stream input, string sourceFileName, OfficeDocumentKind documentKind, Stream output, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFileName);
        ArgumentNullException.ThrowIfNull(output);

        if (documentKind == OfficeDocumentKind.Unknown)
        {
            throw new ArgumentException("Document kind cannot be Unknown.", nameof(documentKind));
        }
        
        var provider = _cachedProvider ??= SelectProvider(_options.OfficeProvider, documentKind);

        OfficeToPdfLogging.ConversionStarted(_logger, documentKind, provider.Kind, null);

        try
        {
            await provider
                .ConvertAsync(input, sourceFileName, documentKind, output, cancellationToken)
                .ConfigureAwait(false);

            OfficeToPdfLogging.ConversionCompleted(_logger, documentKind, provider.Kind, null);
        }
        catch (Exception ex)
        {
            OfficeToPdfLogging.ConversionFailed(_logger, documentKind, provider.Kind, ex);
            throw;
        }
    }

    private IOfficeToPdfProvider SelectProvider(OfficeProviderKind requested, OfficeDocumentKind documentKind)
    {
        OfficeProviderKind target = requested == OfficeProviderKind.None
            ? OfficeProviderKind.LibreOffice
            : requested;

        var match = _providers.FirstOrDefault(p => p.Kind == target);

        if (match is null)
        {
            throw new ProviderUnavailableException($"No provider of kind '{target}' is registered.");
        }

        if (!match.IsAvailable)
        {
            throw new ProviderUnavailableException($"Provider '{target}' is not available on this host.");
        }

        return match.Supports(documentKind) ? match : throw new OfficeConversionException($"Provider '{target}' does not support document kind '{documentKind}'.");
    }
}
