using FelixELabs.OfficeToPdfSdk.Hosting.Servers.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FelixELabs.OfficeToPdfSdk.Hosting.Services;

/// <summary>
/// Hosted service that manages the lifecycle of the conversion server.
/// </summary>
public sealed class ConversionServerHostedService(IConversionServer server, ILogger<ConversionServerHostedService> logger) : BackgroundService
{
    private readonly IConversionServer _server = server ?? throw new ArgumentNullException(nameof(server));
    private readonly ILogger<ConversionServerHostedService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Conversion Server Hosted Service is starting");

        try
        {
            await _server.StartAsync(stoppingToken);
            _logger.LogInformation("Conversion Server started on endpoint: {Endpoint}", _server.GetEndpoint());

            // Keep the service running
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Conversion Server cancellation requested");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Conversion Server");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Conversion Server Hosted Service is stopping");
        await _server.StopAsync();
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        (_server as IDisposable)?.Dispose();
        base.Dispose();
    }
}

