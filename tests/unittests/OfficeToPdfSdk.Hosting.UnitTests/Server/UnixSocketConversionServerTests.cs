using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using FelixELabs.OfficeToPdfSdk.Enums;
using FelixELabs.OfficeToPdfSdk.Hosting.Models;
using FelixELabs.OfficeToPdfSdk.Hosting.Servers;
using FelixELabs.OfficeToPdfSdk.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace FelixELabs.OfficeToPdfSdk.Hosting.UnitTests.Server;

public class UnixSocketConversionServerTests
{
    [Fact]
    public void GetEndpointReturnsSocketPath()
    {
        // Arrange
        var mockConverter = new Mock<IOfficeToPdfConverter>();
        var mockLogger = new Mock<ILogger<UnixSocketConversionServer>>();
        var mockFileSystem = new Mock<IFileSystem>();
        var socketPath = "/tmp/custom-socket.sock";

        var server = new UnixSocketConversionServer(
            mockConverter.Object,
            mockLogger.Object,
            mockFileSystem.Object,
            socketPath);

        // Act
        var endpoint = server.GetEndpoint();

        // Assert
        Assert.Equal(socketPath, endpoint);
    }

    [Fact]
    public void GetEndpointWithDefaultSocketPathReturnsDefaultPath()
    {
        // Arrange
        var mockConverter = new Mock<IOfficeToPdfConverter>();
        var mockLogger = new Mock<ILogger<UnixSocketConversionServer>>();
        var mockFileSystem = new Mock<IFileSystem>();

        var server = new UnixSocketConversionServer(
            mockConverter.Object,
            mockLogger.Object,
            mockFileSystem.Object);

        // Act
        var endpoint = server.GetEndpoint();

        // Assert
        Assert.Equal("/tmp/office-to-pdf.sock", endpoint);
    }

    [Fact]
    public async Task ProcessConversionAsyncWithEmptyFilePathReturnsErrorResponse()
    {
        await TestProcessConversionAsync(request => request.FilePath = "", isFileExists: true, expectedErrorMessage: "No input file path provided");
    }

    [Fact]
    public async Task ProcessConversionAsyncWithNullFilePathReturnsErrorResponse()
    {
        await TestProcessConversionAsync(request => request.FilePath = null, isFileExists: true, expectedErrorMessage: "No input file path provided");
    }

    [Fact]
    public async Task ProcessConversionAsyncWithFileNotFoundReturnsNotFoundError()
    {
        await TestProcessConversionAsync(request => request.FilePath = "nonexistent.docx", isFileExists: false, expectedErrorMessage: "Input file not found");
    }

    [Fact]
    public async Task ProcessConversionAsyncWithValidFileCallsConverterAndReturnsSuccess()
    {
        // Arrange
        var mockConverter = new Mock<IOfficeToPdfConverter>();
        var mockLogger = new Mock<ILogger<UnixSocketConversionServer>>();
        var mockFileSystem = new MockFileSystem();
        mockFileSystem.AddFile("/tmp/test.docx", new MockFileData("Mock content"));

        mockConverter.Setup(x => x.ConvertAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                OfficeDocumentKind.Document,
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var server = new UnixSocketConversionServer(
            mockConverter.Object,
            mockLogger.Object,
            mockFileSystem,
            "/tmp/test.sock");

        var request = new ConversionRequest { FilePath = "/tmp/test.docx", DocumentKind = OfficeDocumentKind.Document };

        // Act
        var response = await InvokeProcessConversionAsync(server, request, CancellationToken.None);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.OutputFilePath);
        mockConverter.Verify(x => x.ConvertAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                OfficeDocumentKind.Document,
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessConversionAsyncOnConverterExceptionReturnsErrorResponse()
    {
        // Arrange
        var mockConverter = new Mock<IOfficeToPdfConverter>();
        var mockLogger = new Mock<ILogger<UnixSocketConversionServer>>();
        var mockFileSystem = new MockFileSystem();
        mockFileSystem.AddFile("/tmp/test.docx", new MockFileData("Mock content"));
        var testException = new InvalidOperationException("Conversion failed");

        mockConverter.Setup(x => x.ConvertAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<OfficeDocumentKind>(),
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(testException);

        var server = new UnixSocketConversionServer(
            mockConverter.Object,
            mockLogger.Object,
            mockFileSystem,
            "/tmp/test.sock");

        var request = new ConversionRequest { FilePath = "/tmp/test.docx", DocumentKind = OfficeDocumentKind.Document };

        // Act
        var response = await InvokeProcessConversionAsync(server, request, CancellationToken.None);

        // Assert
        Assert.False(response.Success);
        Assert.NotNull(response.Error);
        Assert.Contains("Conversion failed", response.Error);
    }

    [Fact]
    public async Task ProcessConversionAsyncOnCancellationReturnsErrorResponse()
    {
        // Arrange
        var mockConverter = new Mock<IOfficeToPdfConverter>();
        var mockLogger = new Mock<ILogger<UnixSocketConversionServer>>();
        var mockFileSystem = new MockFileSystem();
        mockFileSystem.AddFile("/tmp/test.docx", new MockFileData("Mock content"));

        mockConverter.Setup(x => x.ConvertAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<OfficeDocumentKind>(),
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var server = new UnixSocketConversionServer(
            mockConverter.Object,
            mockLogger.Object,
            mockFileSystem,
            "/tmp/test.sock");

        var request = new ConversionRequest { FilePath = "/tmp/test.docx", DocumentKind = OfficeDocumentKind.Document };

        // Act
        var response = await InvokeProcessConversionAsync(server, request, CancellationToken.None);

        // Assert
        Assert.False(response.Success);
        Assert.Equal("Conversion was cancelled", response.Error);
    }
    
    private static async Task TestProcessConversionAsync(Action<ConversionRequest> setupRequest, bool isFileExists, string expectedErrorMessage = "")
    {
        // Arrange
        var mockConverter = new Mock<IOfficeToPdfConverter>();
        var mockLogger = new Mock<ILogger<UnixSocketConversionServer>>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockFile = new Mock<IFile>();
        mockFileSystem.Setup(x => x.File).Returns(mockFile.Object);
        mockFile.Setup(x => x.Exists(It.IsAny<string>())).Returns(isFileExists);

        var request = new ConversionRequest { FilePath = "/tmp/test.docx", DocumentKind = OfficeDocumentKind.Document };
        setupRequest(request);

        var server = new UnixSocketConversionServer(
            mockConverter.Object,
            mockLogger.Object,
            mockFileSystem.Object,
            "/tmp/test.sock");

        // Act
        var response = await InvokeProcessConversionAsync(server, request, CancellationToken.None);

        // Assert
        Assert.False(response.Success);
        if (!string.IsNullOrEmpty(expectedErrorMessage))
        {
            Assert.Contains(expectedErrorMessage, response.Error);
        }
    }

    private static async Task<ConversionResponse> InvokeProcessConversionAsync(
        UnixSocketConversionServer server,
        ConversionRequest request,
        CancellationToken cancellationToken)
    {
        // Use reflection to invoke the private ProcessConversionAsync method
        var method = typeof(UnixSocketConversionServer).GetMethod(
            "ProcessConversionAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (method == null)
            throw new InvalidOperationException("ProcessConversionAsync method not found");

        var task = (Task<ConversionResponse>)method.Invoke(server, new object[] { request, cancellationToken })!;
        return await task;
    }
}

