using LanguageExt;
using LanguageExt.Common;
using System.Text;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Essentials.Utils.Extensions;
using Essentials.Cryptography.Services.RsaCryptographyService.Options;

namespace Essentials.Cryptography.Services.RsaCryptographyService.Implementations;

/// <inheritdoc cref="IRsaCryptographyService" />
internal class RsaCryptographyService : IRsaCryptographyService
{
    private static uint _isExecuted;
    private readonly IOptions<RsaCryptographyOptions> _options;
    private static readonly ConcurrentBag<(string, string)> _keys = new();
    
    public RsaCryptographyService(IOptions<RsaCryptographyOptions> options)
    {
        _options = options;
    }
    
    /// <inheritdoc cref="IRsaCryptographyService.GenerateKeyPairAsync" />
    public async Task<Validation<Error, (string PrivateKey, string PublicKey)>> GenerateKeyPairAsync(
        int strength)
    {
        if (_options.Value.UsePool && _keys.TryTake(out var keys))
        {
            if (_keys.TryGetNonEnumeratedCount(out var count) && count <= 1)
                RunGenerateKeysTask(strength);
            
            return (PrivateKey: keys.Item1, PublicKey: keys.Item2);
        }
        
        return await Prelude
            .TryAsync(async () =>
            {
                var pair = CreateRsaKeyPair(strength);
                return await (RunGetStringFromKeyTask(pair.Private), RunGetStringFromKeyTask(pair.Public));
            })
            .Map(tuple =>
            {
                if (_options.Value.UsePool)
                    RunGenerateKeysTask(strength);
                
                return tuple;
            })
            .ToValidation(
                Fail: exception =>
                {
                    return Error.New(
                        "Во время создания пары из публичного и приватного ключа произошла ошибка",
                        exception);
                });
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
    
    /// <summary>
    /// Запускает задачу по генерации ключей для пула
    /// </summary>
    private void RunGenerateKeysTask(int strength)
    {
        if (!_keys.TryGetNonEnumeratedCount(out var count))
            return;
        
        if (Interlocked.Exchange(ref _isExecuted, 1) == 1)
            return;
        
        Task.Run(async () =>
        {
            await Parallel.ForEachAsync(
                source: Enumerable.Range(1, _options.Value.PoolSize - count),
                body: async (_, _) =>
                {
                    var pair = CreateRsaKeyPair(strength);
                    var tuple = await (RunGetStringFromKeyTask(pair.Private), RunGetStringFromKeyTask(pair.Public));
                    
                    _keys.Add(tuple);
                });
            
            Interlocked.Exchange(ref _isExecuted, 0);
        });
    }
    
    /// <summary>
    /// Создает пару из приватного и публичного ключа
    /// </summary>
    /// <returns></returns>
    private static AsymmetricCipherKeyPair CreateRsaKeyPair(int strength)
    {
        var generator = new RsaKeyPairGenerator();
        generator.Init(new KeyGenerationParameters(new SecureRandom(), strength));
        return generator.GenerateKeyPair();
    }

    /// <summary>
    /// Запускает и возвращает задачу на получение строки из объекта ключа
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private static Task<string> RunGetStringFromKeyTask(AsymmetricKeyParameter key)
    {
        return Task.Run(async () =>
        {
            await using var textWriter = new StringWriter();
            
            using var pemWriter = new PemWriter(textWriter);
            pemWriter.WriteObject(key);
            await pemWriter.Writer.FlushAsync();
            
            return textWriter.ToString();
        });
    }
}