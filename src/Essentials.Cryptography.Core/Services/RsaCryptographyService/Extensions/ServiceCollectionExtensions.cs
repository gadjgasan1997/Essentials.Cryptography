using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Essentials.Cryptography.Dictionaries;
using Essentials.Cryptography.Services.RsaCryptographyService.Options;

namespace Essentials.Cryptography.Services.RsaCryptographyService.Extensions;

/// <summary>
/// Методы расширения для <see cref="IServiceCollection" />
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Настраивает сервиса шифрования RSA
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static IServiceCollection ConfigureRsaCryptographyService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration
            .GetSection(KnownSections.CRYPTOGRAPHY)
            .GetSection(RsaCryptographyOptions.Section);
        
        services.Configure<RsaCryptographyOptions>(section);
        services.AddSingleton<IRsaCryptographyService, Implementations.RsaCryptographyService>();
        
        return services;
    }
}