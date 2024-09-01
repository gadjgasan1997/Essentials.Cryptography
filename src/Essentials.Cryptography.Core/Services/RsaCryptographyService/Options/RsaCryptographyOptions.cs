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
    /// Опции пула
    /// </summary>
    public PoolOptions PoolOptions { get; init; } = new();
}