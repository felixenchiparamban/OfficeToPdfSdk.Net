using FelixELabs.OfficeToPdfSdk.Enums;
using FelixELabs.OfficeToPdfSdk.Options;
using FelixELabs.OfficeToPdfSdk.Providers.Interfaces;
using FelixELabs.OfficeToPdfSdk.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FelixELabs.OfficeToPdfSdk.UnitTests.Services;

public class OfficeToPdfConverterTests
{
    [Fact]
    public void ConstructorWithValidProvidersSucceeds()
    {
        // Arrange
        var mockProvider = new Mock<IOfficeToPdfProvider>();
        var mockOptions = new Mock<IOptions<OfficeToPdfModuleOptions>>();
        var mockLogger = new Mock<ILogger<OfficeToPdfConverter>>();

        mockOptions.Setup(o => o.Value).Returns(new OfficeToPdfModuleOptions());

        // Act
        var converter = new OfficeToPdfConverter(
            new[] { mockProvider.Object },
            mockOptions.Object,
            mockLogger.Object);

        // Assert
        Assert.NotNull(converter);
    }

    [Fact]
    public void ConstructorWithoutProvidersThrowsArgumentException()
    {
        // Arrange
        var mockOptions = new Mock<IOptions<OfficeToPdfModuleOptions>>();
        var mockLogger = new Mock<ILogger<OfficeToPdfConverter>>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new OfficeToPdfConverter(
                Array.Empty<IOfficeToPdfProvider>(),
                mockOptions.Object,
                mockLogger.Object));
    }

    [Fact]
    public void ConstructorWithNullProvidersThrowsArgumentNullException()
    {
        // Arrange
        var mockOptions = new Mock<IOptions<OfficeToPdfModuleOptions>>();
        var mockLogger = new Mock<ILogger<OfficeToPdfConverter>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new OfficeToPdfConverter(
                null!,
                mockOptions.Object,
                mockLogger.Object));
    }

    [Fact]
    public async Task ConvertAsyncWithValidStreamCallsProvider()
    {
        // Arrange
        var mockProvider = new Mock<IOfficeToPdfProvider>();
        mockProvider.Setup(p => p.Kind).Returns(OfficeProviderKind.LibreOffice);
        mockProvider.Setup(p => p.IsAvailable).Returns(true);
        mockProvider.Setup(p => p.Supports(OfficeDocumentKind.Document)).Returns(true);
        mockProvider.Setup(p => p.ConvertAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<OfficeDocumentKind>(),
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockOptions = new Mock<IOptions<OfficeToPdfModuleOptions>>();
        mockOptions.Setup(o => o.Value).Returns(new OfficeToPdfModuleOptions());
        var mockLogger = new Mock<ILogger<OfficeToPdfConverter>>();

        var converter = new OfficeToPdfConverter(
            new[] { mockProvider.Object },
            mockOptions.Object,
            mockLogger.Object);

        using var inputStream = new MemoryStream();
        using var outputStream = new MemoryStream();

        // Act
        await converter.ConvertAsync(inputStream, "document.docx", OfficeDocumentKind.Document, outputStream);

        // Assert
        mockProvider.Verify(p => p.ConvertAsync(
            It.IsAny<Stream>(),
            "document.docx",
            OfficeDocumentKind.Document,
            It.IsAny<Stream>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConvertAsyncWithUnknownDocumentKindThrowsArgumentException()
    {
        // Arrange
        var mockProvider = new Mock<IOfficeToPdfProvider>();
        var mockOptions = new Mock<IOptions<OfficeToPdfModuleOptions>>();
        var mockLogger = new Mock<ILogger<OfficeToPdfConverter>>();

        mockOptions.Setup(o => o.Value).Returns(new OfficeToPdfModuleOptions());

        var converter = new OfficeToPdfConverter(
            new[] { mockProvider.Object },
            mockOptions.Object,
            mockLogger.Object);

        using var inputStream = new MemoryStream();
        using var outputStream = new MemoryStream();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            converter.ConvertAsync(inputStream, "document.docx", OfficeDocumentKind.Unknown, outputStream));
    }
}

