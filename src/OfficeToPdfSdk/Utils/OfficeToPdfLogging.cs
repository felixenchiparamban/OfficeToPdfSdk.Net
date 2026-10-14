using Microsoft.Extensions.Logging;
using FelixELabs.OfficeToPdfSdk.Enums;

namespace FelixELabs.OfficeToPdfSdk.Utils;

public static class OfficeToPdfLogging
{
    // Module lifecycle events (1000-1099)
    public static readonly Action<ILogger, Exception?> ModuleActivating =
        LoggerMessage.Define(LogLevel.Information, OfficeToPdfEventIds.ModuleActivating,
            "OfficeToPdf module is being activated");
    
    public static readonly Action<ILogger, Exception?> ModuleDisabled =
        LoggerMessage.Define(LogLevel.Warning, OfficeToPdfEventIds.ModuleDisabled,
            "OfficeToPdf module is disabled in configuration");

    // Provider detection events (2000-2099)
    public static readonly Action<ILogger, Exception?> WindowsDetected =
        LoggerMessage.Define(LogLevel.Information, OfficeToPdfEventIds.WindowsDetected,
            "Windows detected - registering Windows Office (COM) providers");

    public static readonly Action<ILogger, Exception?> WindowsNotDetected =
        LoggerMessage.Define(LogLevel.Warning, OfficeToPdfEventIds.WindowsNotDetected,
            "Not running on Windows - Windows Office providers will not be registered");

    public static readonly Action<ILogger, Exception?> ProviderQuitFailed =
        LoggerMessage.Define(LogLevel.Warning, OfficeToPdfEventIds.ProviderQuitFailed,
            "Failed to quit Office application gracefully");

    public static readonly Action<ILogger, Exception?> ProviderComReleaseFailed =
        LoggerMessage.Define(LogLevel.Warning, OfficeToPdfEventIds.ProviderComReleaseFailed,
            "Failed to release COM object reference");

    // Conversion events (3000-3099)
    public static readonly Action<ILogger, OfficeDocumentKind, OfficeProviderKind, Exception?> ConversionStarted =
        LoggerMessage.Define<OfficeDocumentKind, OfficeProviderKind>(LogLevel.Information, OfficeToPdfEventIds.ConversionStarted,
            "Converting {Kind} document with provider {Provider}.");

    public static readonly Action<ILogger, OfficeDocumentKind, OfficeProviderKind, Exception?> ConversionCompleted =
        LoggerMessage.Define<OfficeDocumentKind, OfficeProviderKind>(LogLevel.Information, OfficeToPdfEventIds.ConversionCompleted,
            "Conversion completed for {Kind} document with provider {Provider}.");

    public static readonly Action<ILogger, OfficeDocumentKind, OfficeProviderKind, Exception?> ConversionFailed =
        LoggerMessage.Define<OfficeDocumentKind, OfficeProviderKind>(LogLevel.Error, OfficeToPdfEventIds.ConversionFailed,
            "Conversion failed for {Kind} document with provider {Provider}.");

    // Storage events (4000-4099)
    public static readonly Action<ILogger, string, Exception?> StorageCreated =
        LoggerMessage.Define<string>(LogLevel.Debug, OfficeToPdfEventIds.StorageCreated,
            "Created work directory {Dir}");

    public static readonly Action<ILogger, string, Exception?> StorageCleanupFailed =
        LoggerMessage.Define<string>(LogLevel.Error, OfficeToPdfEventIds.StorageCleanup,
            "Failed to clean work directory {Dir}");

    // LibreOffice process events (5000-5099)
    public static readonly Action<ILogger, string, string, Exception?> LibreOfficeStarting =
        LoggerMessage.Define<string, string>(LogLevel.Debug, OfficeToPdfEventIds.LibreOfficeStarting,
            "Starting LibreOffice: {Executable} {Arguments}");

    public static readonly Action<ILogger, int, string, Exception?> LibreOfficeCompleted =
        LoggerMessage.Define<int, string>(LogLevel.Debug, OfficeToPdfEventIds.LibreOfficeCompleted,
            "LibreOffice exited with code {ExitCode}. stdout: {Stdout}");
}

