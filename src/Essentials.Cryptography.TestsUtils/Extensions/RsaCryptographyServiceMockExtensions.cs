using Moq;
using LanguageExt;
using LanguageExt.Common;
using Essentials.Cryptography.Services.RsaCryptographyService;

namespace Essentials.Cryptography.TestsUtils.Extensions;

/// <summary>
/// Методы расширения для мока сервиса <see cref="IRsaCryptographyService" />
/// </summary>
public static class RsaCryptographyServiceMockExtensions
{
    /// <summary>
    /// Устанавливает для мока поведение метода <see cref="IRsaCryptographyService.GenerateKeyPairAsync" />
    /// </summary>
    /// <param name="mock">Мок</param>
    /// <param name="func">Делегат, устанавливающий поведение для метода</param>
    /// <returns>Мок</returns>
    public static Mock<IRsaCryptographyService> Setup_GenerateKeyPairAsync(
        this Mock<IRsaCryptographyService> mock,
        Func<Validation<Error, (string PrivateKey, string PublicKey)>> func)
    {
        mock
            .Setup(service => service.GenerateKeyPairAsync(It.IsAny<int>()))
            .ReturnsAsync((int _) => func());
        
        return mock;
    }
    
    /// <summary>
    /// Устанавливает для мока поведение метода <see cref="IRsaCryptographyService.Decrypt" />
    /// </summary>
    /// <param name="mock">Мок</param>
    /// <param name="func">Делегат, устанавливающий поведение для метода</param>
    /// <returns>Мок</returns>
    public static Mock<IRsaCryptographyService> Setup_Decrypt(
        this Mock<IRsaCryptographyService> mock,
        Func<string, string, Validation<Error, string>> func)
    {
        mock
            .Setup(service =>
                service.Decrypt(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()))
            .Returns(
                (
                        string privateKey,
                        string encryptedCodeWord,
                        int _,
                        bool _) =>
                    func(privateKey, encryptedCodeWord));
        
        return mock;
    }
}