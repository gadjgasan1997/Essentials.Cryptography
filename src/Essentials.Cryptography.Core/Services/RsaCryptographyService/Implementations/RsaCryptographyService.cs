using LanguageExt;
using LanguageExt.Common;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Crypto.Generators;
using Essentials.Cryptography.Services.RsaCryptographyService.Extensions;
using Essentials.Cryptography.Services.RsaCryptographyService.Options;
// ReSharper disable ConvertToLambdaExpression

namespace Essentials.Cryptography.Services.RsaCryptographyService.Implementations;

/// <inheritdoc cref="IRsaCryptographyService" />
internal class RsaCryptographyService : IRsaCryptographyService
{
    private readonly KeysPool _keysPool;
    private readonly PoolOptions _options;
    
    public RsaCryptographyService(
        KeysPool keysPool,
        IOptions<RsaCryptographyOptions> options)
    {
        _keysPool = keysPool;
        _options = options.Value.PoolOptions;
    }
    
    /// <inheritdoc cref="IRsaCryptographyService.GenerateKeyPairAsync" />
    public async Task<Validation<Error, (string PrivateKey, string PublicKey)>> GenerateKeyPairAsync(
        int strength)
    {
        if (_options.UsePool)
            return await _keysPool.TakeAsync(strength, GenerateKeysAsync);

        return await Prelude
            .TryAsync(GenerateKeysAsync)
            .ToValidation(
                Fail: exception =>
                {
                    return Error.New(
                        "Во время создания пары из публичного и приватного ключа произошла ошибка",
                        exception);
                });

        async Task<(string, string)> GenerateKeysAsync() =>
            await new RsaKeyPairGenerator().CreateRsaKeyPairAsync(strength);
    }

    /// <inheritdoc cref="IRsaCryptographyService.Decrypt" />
    public Validation<Error, string> Decrypt(
        string privateKey,
        string encryptedString,
        int strength,
        bool usefOAEP = false)
    {
        return Prelude
            .Try(() =>
            {
                using var rsa = new RSACryptoServiceProvider(strength);
                rsa.ImportFromPem(privateKey);
            
                var resultBytes = Convert.FromBase64String(encryptedString);
                var decryptedBytes = rsa.Decrypt(resultBytes, usefOAEP);
                
                return Encoding.UTF8.GetString(decryptedBytes);
            })
            .ToValidation(
                Fail: exception =>
                {
                    return Error.New(
                        $"Во время расшифровки строки '{encryptedString}' произошла ошибка",
                        exception);
                });
    }
}