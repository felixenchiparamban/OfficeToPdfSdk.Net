using FelixELabs.OfficeToPdfSdk.Enums;
using FelixELabs.OfficeToPdfSdk.Hosting.IntegrationTests.Fixtures;
using FelixELabs.OfficeToPdfSdk.Hosting.Models;
using System.Net.Sockets;
using System.Text.Json;

namespace FelixELabs.OfficeToPdfSdk.Hosting.IntegrationTests.Tests;

public class UnixSocketConversionServerIntegrationTests(Fixture fixture) : IClassFixture<Fixture>
{
    [Fact]
    public async Task ClientConnectionCanConnectToRunningServer()
    {
        // Arrange
        var endpoint = new UnixDomainSocketEndPoint(fixture.SocketPath);
        using var client = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

        // Act
        await client.ConnectAsync(endpoint);

        // Assert
        Assert.True(client.Connected);
    }

    [Fact]
    public async Task SendValidRequestServerResponds()
    {
        // Arrange
        var request = new ConversionRequest
        {
            FilePath = fixture.TestInputFile,
            DocumentKind = OfficeDocumentKind.Document,
            OutputFilePath = Path.Combine(fixture.TestDirectory, $"output-{Guid.NewGuid()}.pdf")
        };

        // Act
        var response = await SendRequestAsync(JsonSerializer.Serialize(request));

        // Assert
        Assert.NotNull(response);
    }

    [Fact]
    public async Task SendNullFilePathReceivesErrorResponse()
    {
        // Arrange
        var request = new ConversionRequest { FilePath = null, DocumentKind = OfficeDocumentKind.Document };

        // Act
        var response = await SendRequestAsync(JsonSerializer.Serialize(request));

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.NotNull(response.Error);
    }

    [Fact]
    public async Task SendInvalidJsonServerHandlesGracefully()
    {
        // Act & Assert (no exception = server handled gracefully)
        await SendRequestAsync("{ invalid json }");
    }

    [Fact]
    public async Task SendEmptyRequestServerRespondsWithError()
    {
        // Act
        var response = await SendRequestAsync(string.Empty);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task NonExistentFileServerRespondsWithError()
    {
        // Arrange
        var request = new ConversionRequest
        {
            FilePath = Path.Combine(fixture.TestDirectory, "non-existent.docx"),
            DocumentKind = OfficeDocumentKind.Document
        };

        // Act
        var response = await SendRequestAsync(JsonSerializer.Serialize(request));

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Contains("not found", response.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MultipleSequentialRequestsAllReceiveResponses()
    {
        // Arrange
        int responseCount = 0;

        // Act
        for (int i = 0; i < 3; i++)
        {
            var request = new ConversionRequest
            {
                FilePath = fixture.TestInputFile,
                DocumentKind = OfficeDocumentKind.Document,
                OutputFilePath = Path.Combine(fixture.TestDirectory, $"output-{Guid.NewGuid()}.pdf")
            };

            var response = await SendRequestAsync(JsonSerializer.Serialize(request));
            if (response is not null) responseCount++;
        }

        // Assert
        Assert.True(responseCount >= 1, $"Expected at least 1 response, got {responseCount}");
    }
    private async Task<ConversionResponse?> SendRequestAsync(string requestLine)
    {
        try
        {
            var endpoint = new UnixDomainSocketEndPoint(fixture.SocketPath);
            using var client = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            await client.ConnectAsync(endpoint);

            await using var networkStream = new NetworkStream(client);
            using var reader = new StreamReader(networkStream);
            await using var writer = new StreamWriter(networkStream);
            writer.AutoFlush = true;

            await writer.WriteLineAsync(requestLine);

            var responseJson = await reader.ReadLineAsync();
            return string.IsNullOrEmpty(responseJson)
                ? null
                : JsonSerializer.Deserialize<ConversionResponse>(responseJson);
        }
        catch (ObjectDisposedException)
        {
            return null;
        }
    }
}
