using FelixELabs.OfficeToPdfSdk.Enums;
using FelixELabs.OfficeToPdfSdk.Exceptions;
using FelixELabs.OfficeToPdfSdk.Options;
using FelixELabs.OfficeToPdfSdk.Providers.Interfaces;
using FelixELabs.OfficeToPdfSdk.Services.Interfaces;
using FelixELabs.OfficeToPdfSdk.Utils;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace FelixELabs.OfficeToPdfSdk.Providers.LibreOffice;

public sealed class LibreOfficeProvider : IOfficeToPdfProvider
{
    private readonly OfficeToPdfModuleOptions _officeToPdfModuleOptions;
    private readonly IStorageService _storageService;
    private readonly ILogger<LibreOfficeProvider> _logger;
    private readonly string? _resolvedExecutablePath;

    public LibreOfficeProvider(IStorageService storageService, ILogger<LibreOfficeProvider> logger, IOptions<OfficeToPdfModuleOptions> options)
    {
        ArgumentNullException.ThrowIfNull(storageService);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);

        _officeToPdfModuleOptions = options.Value;
        _storageService = storageService;
        _logger = logger;
        _resolvedExecutablePath = LibreOfficeApplicationDetector.ResolveExecutable(_officeToPdfModuleOptions.LibreOffice.ExecutablePath);
    }

    public OfficeProviderKind Kind => OfficeProviderKind.LibreOffice;

    public bool IsAvailable => _resolvedExecutablePath is not null;

    public bool Supports(OfficeDocumentKind documentKind)
    {
        return documentKind is OfficeDocumentKind.Document or
            OfficeDocumentKind.Spreadsheet or
            OfficeDocumentKind.Presentation;
    }

    public Task ConvertAsync(Stream input, string sourceFileName, OfficeDocumentKind documentKind, Stream output, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);

        if (!Supports(documentKind))
        {
            throw new OfficeConversionException($"LibreOfficeProvider cannot convert {documentKind}.");
        }

        return ConvertInternalAsync(input, sourceFileName, output, cancellationToken);
    }

    private async Task ConvertInternalAsync(Stream input, string sourceFileName, Stream output, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var libreOptions = _officeToPdfModuleOptions.LibreOffice;

        var executablePath = _resolvedExecutablePath
            ?? throw new ProviderUnavailableException($"LibreOffice executable not found at '{libreOptions.ExecutablePath}'. Install LibreOffice or set OfficeToPdfModuleOptions:LibreOffice:ExecutablePath.");

        var jobDir = _storageService.CreateJobDirectory(_officeToPdfModuleOptions.TempPath);
        var extension = Path.GetExtension(sourceFileName);
        var inputPath = Path.Combine(jobDir, "input" + extension);

        using var timeoutCts = new CancellationTokenSource(libreOptions.Timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        var timeoutToken = linkedCts.Token;

        try
        {
            await using (var fs = new FileStream(inputPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 1024 * 1024, useAsync: true))
            {
                await input.CopyToAsync(fs, bufferSize: 1024 * 1024, timeoutToken);
                await fs.FlushAsync(timeoutToken);
            }

            timeoutToken.ThrowIfCancellationRequested();

            var expectedOutputPath = Path.Combine(jobDir, "input.pdf");

            await RunLibreOfficeAsync(executablePath, inputPath, jobDir, timeoutToken);

            if (!File.Exists(expectedOutputPath))
            {
                throw new OfficeConversionException($"LibreOffice completed but the expected output file was not created: {expectedOutputPath}");
            }

            timeoutToken.ThrowIfCancellationRequested();

            await using var pdf = new FileStream(expectedOutputPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 1024 * 1024, useAsync: true);
            await pdf.CopyToAsync(output, bufferSize: 1024 * 1024, timeoutToken);
        }
        catch (OperationCanceledException ex)
        {
            if (timeoutCts.Token.IsCancellationRequested)
            {
                throw new OfficeConversionException($"LibreOffice conversion timed out after {libreOptions.Timeout.TotalSeconds} seconds.", ex);
            }

            throw;
        }
        finally
        {
            try
            {
                _storageService.PurgeJobDirectory(jobDir);
            }
            catch (Exception ex)
            {
                OfficeToPdfLogging.StorageCleanupFailed(_logger, jobDir, ex);
            }
        }
    }

    private async Task RunLibreOfficeAsync(string executablePath, string inputPath, string outputDir, CancellationToken cancellationToken)
    {
        // soffice --headless --convert-to pdf --outdir <dir> <file>
        var arguments = $"--headless --convert-to pdf --outdir \"{outputDir}\" \"{inputPath}\"";

        OfficeToPdfLogging.LibreOfficeStarting(_logger, executablePath, arguments, null);

        var startInfo = new ProcessStartInfo
        {
            FileName        = executablePath,
            Arguments       = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute  = false,
            CreateNoWindow   = true,
        };

        using var process = new Process();
        process.StartInfo = startInfo;

        process.Start();

        // Drain stdout and stderr concurrently to prevent buffer-full deadlocks
        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await Task.WhenAll(process.WaitForExitAsync(cancellationToken), stdoutTask, stderrTask);

        if (process.ExitCode != 0)
        {
            throw new OfficeConversionException($"LibreOffice exited with code {process.ExitCode}. stderr: {stderrTask.Result.Trim()}");
        }

        OfficeToPdfLogging.LibreOfficeCompleted(_logger, process.ExitCode, stdoutTask.Result.Trim(), null);
    }
}

