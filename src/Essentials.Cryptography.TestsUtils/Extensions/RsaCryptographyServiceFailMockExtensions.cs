using Moq;
using LanguageExt.Common;
using Essentials.Cryptography.Services.RsaCryptographyService;

namespace Essentials.Cryptography.TestsUtils.Extensions;

/// <summary>
/// Методы расширения для мока сервиса <see cref="IRsaCryptographyService" />, возвращающие ошибочный результат
/// </summary>
public static class RsaCryptographyServiceFailMockExtensions
{
    /// <summary>
    /// Устанавливает для мока поведение метода <see cref="IRsaCryptographyService.GenerateKeyPairAsync" />
    /// </summary>
    /// <param name="mock">Мок</param>
    /// <param name="error">Требуемая ошибка</param>
    /// <returns>Мок</returns>
    public static Mock<IRsaCryptographyService> SetupFail_GenerateKeyPairAsync(
        this Mock<IRsaCryptographyService> mock,
        Error? error = null)
    {
        error ??= Error.New("Unknown Generate Key Pair Error");
        return mock.Setup_GenerateKeyPairAsync(() => error);
    }
    
    /// <summary>
    /// Устанавливает для мока поведение метода <see cref="IRsaCryptographyService.Decrypt" />
    /// </summary>
    /// <param name="mock">Мок</param>
    /// <param name="error">Требуемая ошибка</param>
    /// <returns>Мок</returns>
    public static Mock<IRsaCryptographyService> SetupFail_Decrypt(
        this Mock<IRsaCryptographyService> mock,
        Error? error = null)
    {
        error ??= Error.New("Unknown Decrypt Error");
        return mock.Setup_Decrypt((_, _) => error);
    }
}