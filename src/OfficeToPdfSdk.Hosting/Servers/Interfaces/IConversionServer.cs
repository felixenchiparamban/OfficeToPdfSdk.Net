namespace FelixELabs.OfficeToPdfSdk.Hosting.Servers.Interfaces;

/// <summary>
/// Interface for a conversion server that listens for conversion requests.
/// Can be implemented using HTTP, Named Pipes, Unix Sockets, or other transport mechanisms.
/// </summary>
public interface IConversionServer
{
    /// <summary>
    /// Starts the server and begins listening for requests.
    /// </summary>
    /// <param name="cancellationToken">Token to signal cancellation.</param>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the server.
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Gets the server endpoint/address information.
    /// </summary>
    string GetEndpoint();
}

