using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using Essentials.Cryptography.Services.RsaCryptographyService.Options;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace Essentials.Cryptography.Services.RsaCryptographyService;

/// <summary>
/// Пул ключей
/// </summary>
internal class KeysPool
{
    private static uint _isExecuted;
    private readonly PoolOptions _options;
    private static readonly ConcurrentDictionary<int, ConcurrentBag<(string, string)>> _keys = [];
    
    public KeysPool(IOptions<RsaCryptographyOptions> options)
    {
        _options = options.Value.PoolOptions;
    }
    
    /// <summary>
    /// Возвращает пару ключей из пула. Если пул пустой, выполняет делегат создания пары <paramref name="keysFactory" />
    /// </summary>
    /// <param name="strength">Стойкость</param>
    /// <param name="keysFactory">Фабрика создания пары ключей в случае их отсутствия в пуле</param>
    /// <returns></returns>
    public async ValueTask<(string, string)> TakeAsync(int strength, Func<Task<(string, string)>> keysFactory)
    {
        var bag = _keys.GetOrAdd(strength, _ => []);
        if (!bag.TryTake(out var keys))
        {
            RunGenerateKeysTask(strength, keysFactory);
            return await keysFactory();
        }
        
        if (_keys.TryGetNonEnumeratedCount(out var count) && count <= 1)
            RunGenerateKeysTask(strength, keysFactory);

        return keys;
    }
    
    /// <summary>
    /// Прогревает пул
    /// </summary>
    /// <param name="strength">Стойкость</param>
    /// <param name="keysFactory">Фабрика создания пары ключей</param>
    public async Task WarmPoolAsync(int strength, Func<Task<(string, string)>> keysFactory) =>
        await RunGenerateKeysTask(strength, keysFactory);
    
    /// <summary>
    /// Запускает задачу по генерации ключей для пула
    /// </summary>
    /// <param name="strength">Стойкость</param>
    /// <param name="keysFactory">Фабрика создания пары ключей</param>
    private Task RunGenerateKeysTask(int strength, Func<Task<(string, string)>> keysFactory)
    {
        var bag = _keys.GetOrAdd(strength, _ => []);
        if (!bag.TryGetNonEnumeratedCount(out var count))
            return Task.CompletedTask;
        
        if (Interlocked.Exchange(ref _isExecuted, 1) == 1)
            return Task.CompletedTask;
        
        return Task.Run(async () =>
        {
            await Parallel.ForEachAsync(
                source: Enumerable.Range(1, _options.PoolSize - count),
                body: async (_, _) =>
                {
                    var tuple = await keysFactory();
                    bag.Add(tuple);
                });
            
            Interlocked.Exchange(ref _isExecuted, 0);
        });
    }
}