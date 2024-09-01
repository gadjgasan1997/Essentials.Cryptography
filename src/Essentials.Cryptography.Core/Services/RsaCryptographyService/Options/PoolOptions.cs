// ReSharper disable CollectionNeverUpdated.Global
namespace Essentials.Cryptography.Services.RsaCryptographyService.Options;

/// <summary>
/// Опции пула
/// </summary>
internal class PoolOptions
{
    /// <summary>
    /// Признак необходимости использовать пул ключей
    /// </summary>
    public bool UsePool { get; init; } = false;

    /// <summary>
    /// Размер пула
    /// </summary>
    public int PoolSize { get; init; } = 30;
    
    /// <summary>
    /// Список размеров ключей для прогрева
    /// </summary>
    public HashSet<int> KeysSizesToWarm { get; init; } = [];
}