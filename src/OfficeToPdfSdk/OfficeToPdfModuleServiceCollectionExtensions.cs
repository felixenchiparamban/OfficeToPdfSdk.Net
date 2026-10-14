using FelixELabs.OfficeToPdfSdk.Options;
using FelixELabs.OfficeToPdfSdk.Providers.Interfaces;
using FelixELabs.OfficeToPdfSdk.Providers.LibreOffice;
using FelixELabs.OfficeToPdfSdk.Services;
using FelixELabs.OfficeToPdfSdk.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;

namespace FelixELabs.OfficeToPdfSdk;

public static class OfficeToPdfModuleServiceCollectionExtensions
{
    public static IServiceCollection AddOfficeToPdfSdk(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var options = configuration.GetSection(nameof(OfficeToPdfModuleOptions)).Get<OfficeToPdfModuleOptions>()
                   ?? new OfficeToPdfModuleOptions();

        if (!options.IsEnabled)
        {
            return services;
        }

        services.Configure<OfficeToPdfModuleOptions>(configuration.GetSection(nameof(OfficeToPdfModuleOptions)));

        // LibreOffice is the default provider — uses a headless CLI process
        services.AddSingleton<IOfficeToPdfProvider, LibreOfficeProvider>();

        services.TryAddSingleton<IStorageService, StorageService>();
        services.TryAddSingleton<IOfficeToPdfConverter, OfficeToPdfConverter>();

        return services;
    }
}

