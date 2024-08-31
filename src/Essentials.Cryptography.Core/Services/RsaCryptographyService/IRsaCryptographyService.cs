using LanguageExt;
using LanguageExt.Common;

namespace Essentials.Cryptography.Services.RsaCryptographyService;

/// <summary>
/// Сервис шифрования RSA
/// </summary>
public interface IRsaCryptographyService
{
    /// <summary>
    /// Генерирует пару открытый/закрытый ключ
    /// </summary>
    /// <param name="strength">Стойкость</param>
    /// <returns></returns>
    Task<Validation<Error, (string PrivateKey, string PublicKey)>> GenerateKeyPairAsync(int strength);

    /// <summary>
    /// Расшифровывает строку
    /// </summary>
    /// <param name="privateKey">Приватный ключ</param>
    /// <param name="encryptedString">Зашифрованная строка</param>
    /// <param name="strength">Стойкость</param>
    /// <param name="usefOAEP">Признак необходимости использовать OAEP</param>
    /// <returns></returns>
    Validation<Error, string> Decrypt(
        string privateKey,
        string encryptedString,
        int strength,
        bool usefOAEP = false);
}