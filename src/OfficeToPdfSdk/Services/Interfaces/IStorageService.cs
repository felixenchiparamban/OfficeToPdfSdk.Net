namespace FelixELabs.OfficeToPdfSdk.Services.Interfaces;

public interface IStorageService
{
    string CreateJobDirectory(string? customTempPath = null);

    void PurgeJobDirectory(string workDir);
}

