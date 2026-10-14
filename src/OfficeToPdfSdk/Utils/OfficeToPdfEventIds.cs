using Microsoft.Extensions.Logging;

namespace FelixELabs.OfficeToPdfSdk.Utils;

/// <summary>
/// Centralized EventId definitions for OfficeToPdf logging.
/// EventIds are organized by category with reserved ranges to prevent collisions.
/// </summary>
public static class OfficeToPdfEventIds
{
    private const int ModuleEventIdBase = 1000;
    private const int ProviderEventIdBase = 2000;
    private const int ConversionEventIdBase = 3000;
    private const int StorageEventIdBase = 4000;

    // Module lifecycle events (1000-1099)
    public static readonly EventId ModuleActivating = new(ModuleEventIdBase + 1, nameof(ModuleActivating));
    public static readonly EventId ModuleDisabled = new(ModuleEventIdBase + 2, nameof(ModuleDisabled));

    // Provider detection events (2000-2099)
    public static readonly EventId WindowsDetected = new(ProviderEventIdBase + 1, nameof(WindowsDetected));
    public static readonly EventId WindowsNotDetected = new(ProviderEventIdBase + 2, nameof(WindowsNotDetected));
    public static readonly EventId ProviderQuitFailed = new(ProviderEventIdBase + 3, nameof(ProviderQuitFailed));
    public static readonly EventId ProviderComReleaseFailed = new(ProviderEventIdBase + 4, nameof(ProviderComReleaseFailed));

    // Conversion events (3000-3099)
    public static readonly EventId ConversionStarted = new(ConversionEventIdBase + 1, nameof(ConversionStarted));
    public static readonly EventId ConversionCompleted = new(ConversionEventIdBase + 2, nameof(ConversionCompleted));
    public static readonly EventId ConversionFailed = new(ConversionEventIdBase + 3, nameof(ConversionFailed));

    // Storage events (4000-4099)
    public static readonly EventId StorageCreated = new(StorageEventIdBase + 1, nameof(StorageCreated));
    public static readonly EventId StorageCleanup = new(StorageEventIdBase + 2, nameof(StorageCleanup));

    // LibreOffice process events (5000-5099)
    private const int LibreOfficeEventIdBase = 5000;
    public static readonly EventId LibreOfficeStarting = new(LibreOfficeEventIdBase + 1, nameof(LibreOfficeStarting));
    public static readonly EventId LibreOfficeCompleted = new(LibreOfficeEventIdBase + 2, nameof(LibreOfficeCompleted));




}

