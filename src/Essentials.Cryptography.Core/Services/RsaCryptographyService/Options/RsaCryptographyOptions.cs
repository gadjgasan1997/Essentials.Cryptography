namespace Essentials.Cryptography.Services.RsaCryptographyService.Options;

/// <summary>
/// Опции по работе с криптографией
/// </summary>
internal class RsaCryptographyOptions
{
    /// <summary>
    /// Название секции в конфигурации
    /// </summary>
    public static string Section => "Rsa";

    /// <summary>
    /// Признак необходимости использовать пул ключей
    /// </summary>
    public bool UsePool { get; init; } = true;

    /// <summary>
    /// Размер пула
    /// </summary>
    public int PoolSize { get; init; } = 100;
}