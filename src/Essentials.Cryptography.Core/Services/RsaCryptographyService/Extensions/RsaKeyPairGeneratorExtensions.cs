using Essentials.Utils.Extensions;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Generators;

namespace Essentials.Cryptography.Services.RsaCryptographyService.Extensions;

/// <summary>
/// Методы расширения для <see cref="RsaKeyPairGenerator" />
/// </summary>
internal static class RsaKeyPairGeneratorExtensions
{
    /// <summary>
    /// Создает пару из приватного и публичного ключа
    /// </summary>
    /// <returns></returns>
    public static async Task<(string, string)> CreateRsaKeyPairAsync(
        this RsaKeyPairGenerator generator,
        int strength)
    {
        generator.Init(new KeyGenerationParameters(new SecureRandom(), strength));
        
        var pair = generator.GenerateKeyPair();
        return await (RunGetStringFromKeyTask(pair.Private), RunGetStringFromKeyTask(pair.Public));
    }
    
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