using FelixELabs.OfficeToPdfSdk.Services.Interfaces;
using FelixELabs.OfficeToPdfSdk.Utils;
using Microsoft.Extensions.Logging;

namespace FelixELabs.OfficeToPdfSdk.Services;

public class StorageService(ILogger<StorageService> logger) : IStorageService
{
    private readonly ILogger<StorageService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public string CreateJobDirectory(string? customTempPath = null)
    {
        var basePath = string.IsNullOrWhiteSpace(customTempPath) ? Path.GetTempPath() : customTempPath;
        
        var workDir = Path.Combine(basePath, Constants.AppName, Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(workDir);
        OfficeToPdfLogging.StorageCreated(_logger, workDir, null);
        
        return workDir;
    }

    public void PurgeJobDirectory(string workDir)
    {
        if (Directory.Exists(workDir))
        {
            Directory.Delete(workDir, recursive: true);
        }
    }
}

