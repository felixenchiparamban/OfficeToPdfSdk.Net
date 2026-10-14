using FelixELabs.OfficeToPdfSdk.Hosting.Models;
using FelixELabs.OfficeToPdfSdk.Hosting.Servers.Interfaces;
using FelixELabs.OfficeToPdfSdk.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.Net.Sockets;
using System.Text.Json;

namespace FelixELabs.OfficeToPdfSdk.Hosting.Servers;

/// <summary>
/// Unix Domain Socket server for document conversion.
/// Provides secure local IPC on Unix-based systems (Linux, macOS, etc.) using file-based I/O.
/// </summary>
public sealed class UnixSocketConversionServer(IOfficeToPdfConverter converter,
    ILogger<UnixSocketConversionServer> logger,
    IFileSystem fileSystem,
    string socketPath = "/tmp/office-to-pdf.sock")
    : IConversionServer, IDisposable
{
    private readonly IOfficeToPdfConverter _converter = converter ?? throw new ArgumentNullException(nameof(converter));
    private readonly ILogger<UnixSocketConversionServer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFileSystem _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    private readonly string _socketPath = socketPath ?? throw new ArgumentNullException(nameof(socketPath));
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _serverTask;
    private Socket? _listenerSocket;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_cancellationTokenSource is not null)
        {
            _logger.LogWarning("Unix Socket server is already running");
            return Task.CompletedTask;
        }

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _serverTask = RunUnixSocketServerAsync(_cancellationTokenSource.Token);

        _logger.LogInformation("Unix Socket Conversion Server started on {SocketPath}", _socketPath);

        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (_cancellationTokenSource == null)
        {
            _logger.LogWarning("Unix Socket server is not running");
            return;
        }

        _logger.LogInformation("Stopping Unix Socket Conversion Server");
        await _cancellationTokenSource.CancelAsync();

        if (_serverTask != null)
        {
            try
            {
                await _serverTask;
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }

        _listenerSocket?.Dispose();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = null;

        try
        {
            if (_fileSystem.File.Exists(_socketPath))
            {
                _fileSystem.File.Delete(_socketPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete socket file");
        }
    }

    public string GetEndpoint() => _socketPath;

    private async Task RunUnixSocketServerAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_fileSystem.File.Exists(_socketPath))
            {
                _fileSystem.File.Delete(_socketPath);
            }

            var endpoint = new UnixDomainSocketEndPoint(_socketPath);
            _listenerSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            _listenerSocket.Bind(endpoint);
            _listenerSocket.Listen(5);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var clientSocket = await _listenerSocket.AcceptAsync(cancellationToken);
                    _logger.LogDebug("Client connected to Unix Socket");

                    _ = HandleClientAsync(clientSocket, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error accepting connection on Unix Socket server");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in Unix Socket Conversion Server");
        }
    }

    private async Task HandleClientAsync(Socket socket, CancellationToken cancellationToken)
    {
        try
        {
            await using var networkStream = new NetworkStream(socket);
            using var reader = new StreamReader(networkStream);
            await using var writer = new StreamWriter(networkStream);
            writer.AutoFlush = true;

            var requestJson = await reader.ReadLineAsync(cancellationToken);

            if (string.IsNullOrEmpty(requestJson))
            {
                _logger.LogWarning("Received empty request from client");
                await writer.WriteLineAsync(JsonSerializer.Serialize(new ConversionResponse { Success = false, Error = "Empty request" }));
                return;
            }

            ConversionRequest? request;
            try
            {
                request = JsonSerializer.Deserialize<ConversionRequest>(requestJson);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize request JSON");
                await writer.WriteLineAsync(JsonSerializer.Serialize(new ConversionResponse { Success = false, Error = "Failed to deserialize request" }));
                return;
            }

            if (request is null)
            {
                await writer.WriteLineAsync(JsonSerializer.Serialize(new ConversionResponse { Success = false, Error = "Failed to deserialize request" }));
                return;
            }

            var response = await ProcessConversionAsync(request, cancellationToken);
            await writer.WriteLineAsync(JsonSerializer.Serialize(response));

            _logger.LogInformation("Conversion request completed for {FilePath}", request.FilePath);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Client connection cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling client connection");
        }
        finally
        {
            socket.Dispose();
        }
    }

    private async Task<ConversionResponse> ProcessConversionAsync(ConversionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(request.FilePath))
            {
                return new ConversionResponse { Success = false, Error = "No input file path provided" };
            }

            if (!_fileSystem.File.Exists(request.FilePath))
            {
                return new ConversionResponse { Success = false, Error = $"Input file not found: {request.FilePath}" };
            }

            var outputDirectory = _fileSystem.Path.GetDirectoryName(request.FilePath) ?? _fileSystem.Directory.GetCurrentDirectory();
            var outputPath = request.OutputFilePath ?? Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(request.FilePath) + ".pdf");

            _logger.LogInformation("Starting conversion from {InputPath} to {OutputPath}", request.FilePath, outputPath);

            await using var inputStream = _fileSystem.File.OpenRead(request.FilePath);
            await using var outputStream = _fileSystem.File.Create(outputPath);

            await _converter.ConvertAsync(inputStream, Path.GetFileName(request.FilePath), request.DocumentKind, outputStream, cancellationToken);

            _logger.LogInformation("Conversion successful. Output: {OutputPath}", outputPath);

            return new ConversionResponse { Success = true, OutputFilePath = outputPath };
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Conversion was cancelled");
            return new ConversionResponse { Success = false, Error = "Conversion was cancelled" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Conversion failed");
            return new ConversionResponse { Success = false, Error = ex.Message, StackTrace = ex.StackTrace };
        }
    }

    public void Dispose()
    {
        _listenerSocket?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}
