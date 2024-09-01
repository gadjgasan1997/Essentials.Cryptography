using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Essentials.Utils.Extensions;
using Essentials.Cryptography.Services.RsaCryptographyService.Options;
using Essentials.Cryptography.Services.RsaCryptographyService.Extensions;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto.Generators;

namespace Essentials.Cryptography.Services.RsaCryptographyService;

/// <summary>
/// Сервис прогрева пула ключей
/// </summary>
internal class WarmKeysPoolService : IHostedService
{
    private readonly KeysPool _keysPool;
    private readonly PoolOptions _options;
    private readonly ILogger<WarmKeysPoolService> _logger;
    
    public WarmKeysPoolService(
        KeysPool keysPool,
        IOptions<RsaCryptographyOptions> options,
        ILogger<WarmKeysPoolService> logger)
    {
        _keysPool = keysPool.CheckNotNull();
        _options = options.Value.PoolOptions;
        _logger = logger;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.UsePool || _options.KeysSizesToWarm.Count == 0)
            return;
        
        _logger.LogInformation("Начинается прогрев пула приватных ключей...");
        
        foreach (var size in _options.KeysSizesToWarm)
        {
            await _keysPool.WarmPoolAsync(
                size,
                async () => await new RsaKeyPairGenerator().CreateRsaKeyPairAsync(size));
        }
        
        _logger.LogInformation("Прогрев пула приватных ключей завершен");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}