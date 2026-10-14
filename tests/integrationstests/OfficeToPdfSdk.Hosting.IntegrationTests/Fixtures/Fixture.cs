using FelixELabs.OfficeToPdfSdk.Hosting.Extensions;
using FelixELabs.OfficeToPdfSdk.Hosting.Options;
using FelixELabs.OfficeToPdfSdk.Hosting.Servers.Interfaces;
using FelixELabs.OfficeToPdfSdk.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FelixELabs.OfficeToPdfSdk.Hosting.IntegrationTests.Fixtures;

public sealed class Fixture : IAsyncLifetime
{
    public string SocketPath { get; } = Path.Combine(Path.GetTempPath(), $"office-to-pdf-test-{Guid.NewGuid()}.sock");
    public string TestDirectory { get; } = Path.Combine(Path.GetTempPath(), "OfficeToPdfIntegrationTests");
    public string TestInputFile => Path.Combine(TestDirectory, "test.docx");
    public IServiceProvider ServiceProvider { get; private set; } = null!;
    public IConversionServer Server { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(TestDirectory);
        if (!File.Exists(TestInputFile))
        {
            await File.WriteAllTextAsync(TestInputFile, "Mock Document");
        }

        ServiceProvider = CreateServiceProvider(SocketPath);
        Server = ServiceProvider.GetRequiredService<IConversionServer>();
        await Server.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Server.StopAsync();

        if (File.Exists(SocketPath))
        {
            File.Delete(SocketPath);
        }
        if (Directory.Exists(TestDirectory))
        {
            Directory.Delete(TestDirectory, true);
        }

        await ((IAsyncDisposable)ServiceProvider).DisposeAsync();
    }

    public static IServiceProvider CreateServiceProvider(string? socketPath = null)
    {
        var services = new ServiceCollection();
        services.AddLogging(config => config.SetMinimumLevel(LogLevel.Error));
        services.AddSingleton<IOfficeToPdfConverter, MockOfficeToPdfConverter>();
        services.AddUnixSocketConversionServer(BuildConfiguration(socketPath));
        return services.BuildServiceProvider();
    }

    public static IConfiguration BuildConfiguration(string? socketPath = null) =>
        new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddInMemoryCollection(socketPath is null
                ? []
                : new Dictionary<string, string?> { [$"{nameof(OfficeToPdfSdkHostingModuleOptions)}:{nameof(OfficeToPdfSdkHostingModuleOptions.SocketPath)}"] = socketPath })
            .Build();
}
