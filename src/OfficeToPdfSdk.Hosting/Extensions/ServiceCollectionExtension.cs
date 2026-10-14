using FelixELabs.OfficeToPdfSdk.Hosting.Options;
using FelixELabs.OfficeToPdfSdk.Hosting.Servers;
using FelixELabs.OfficeToPdfSdk.Hosting.Servers.Interfaces;
using FelixELabs.OfficeToPdfSdk.Hosting.Services;
using FelixELabs.OfficeToPdfSdk.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;
// ReSharper disable ConvertToExtensionBlock

namespace FelixELabs.OfficeToPdfSdk.Hosting.Extensions;

public static class ServiceCollectionExtension
{
    /// <summary>
    /// Adds a Unix Domain Socket conversion server to the services.
    /// Unix/Linux/macOS only — uses file-based IPC via Unix domain sockets.
    /// </summary>
    public static IServiceCollection AddUnixSocketConversionServer(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<OfficeToPdfSdkHostingModuleOptions>(configuration.GetSection(nameof(OfficeToPdfSdkHostingModuleOptions)));

        services.AddSingleton<IFileSystem, FileSystem>();

        services.AddSingleton<IConversionServer>(provider =>
            new UnixSocketConversionServer(
                provider.GetRequiredService<IOfficeToPdfConverter>(),
                provider.GetRequiredService<ILogger<UnixSocketConversionServer>>(),
                provider.GetRequiredService<IFileSystem>(),
                provider.GetRequiredService<IOptions<OfficeToPdfSdkHostingModuleOptions>>().Value.SocketPath
            ));

        services.AddConversionServerHostedService();

        return services;
    }

    /// <summary>
    /// Adds the ConversionServerHostedService to enable automatic server startup.
    /// </summary>
    public static IServiceCollection AddConversionServerHostedService(this IServiceCollection services)
    {
        services.AddHostedService<ConversionServerHostedService>();
        return services;
    }
}
