using FelixELabs.OfficeToPdfSdk.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace FelixELabs.OfficeToPdfSdk.UnitTests.Services;

public class StorageServiceTests
{
    [Fact]
    public void CreateWorkDirectoryWithoutCustomPathCreatesDirectoryInTempPath()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<StorageService>>();
        var service = new StorageService(mockLogger.Object);

        // Act
        var workDir = service.CreateJobDirectory();

        // Assert
        Assert.NotNull(workDir);
        Assert.True(Directory.Exists(workDir));
        Assert.Contains("OfficeToPdfSdk", workDir);

        // Cleanup
        Directory.Delete(workDir, recursive: true);
    }

    [Fact]
    public void CreateWorkDirectoryWithCustomPathCreatesDirectoryInCustomPath()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<StorageService>>();
        var service = new StorageService(mockLogger.Object);
        var customPath = Path.Combine(Path.GetTempPath(), "TestOfficeToPdf");
        Directory.CreateDirectory(customPath);

        try
        {
            // Act
            var workDir = service.CreateJobDirectory(customPath);

            // Assert
            Assert.NotNull(workDir);
            Assert.True(Directory.Exists(workDir));
            Assert.StartsWith(customPath, workDir);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(customPath))
                Directory.Delete(customPath, recursive: true);
        }
    }

    [Fact]
    public void CreateWorkDirectoryCreatesUniqueDirectories()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<StorageService>>();
        var service = new StorageService(mockLogger.Object);

        // Act
        var dir1 = service.CreateJobDirectory();
        var dir2 = service.CreateJobDirectory();

        // Assert
        Assert.NotEqual(dir1, dir2);
        Assert.True(Directory.Exists(dir1));
        Assert.True(Directory.Exists(dir2));

        // Cleanup
        Directory.Delete(dir1, recursive: true);
        Directory.Delete(dir2, recursive: true);
    }

    [Fact]
    public void DeleteWorkDirectoryDeletesExistingDirectory()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<StorageService>>();
        var service = new StorageService(mockLogger.Object);
        var workDir = service.CreateJobDirectory();
        Assert.True(Directory.Exists(workDir));

        // Act
        service.PurgeJobDirectory(workDir);

        // Assert
        Assert.False(Directory.Exists(workDir));
    }

    [Fact]
    public void DeleteWorkDirectoryWithNonExistentDirectoryDoesNotThrow()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<StorageService>>();
        var service = new StorageService(mockLogger.Object);
        var nonExistentDir = Path.Combine(Path.GetTempPath(), "NonExistent", Guid.NewGuid().ToString());

        // Act & Assert - should not throw
        service.PurgeJobDirectory(nonExistentDir);
    }
}

