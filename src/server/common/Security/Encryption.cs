using System.Text;
using System.Security.Cryptography;

namespace SilverKinetics.w80.Common.Security;

public static class Encryption
{
    public static byte[] Encrypt(byte[] plainText, string key)
    {
        if (plainText.Length == 0)
            throw new ArgumentNullException(nameof(plainText));

        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        using (var aes = Aes.Create())
        {
            aes.GenerateIV();
            aes.KeySize = 256;
            aes.Key = Hash.Sha256AsBytes(Encoding.Unicode.GetBytes(key));

            using (var output = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(output, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainText);
                    cryptoStream.FlushFinalBlock();

                    return [.. aes.IV, .. output.ToArray()];
                }
            }
        }
    }

    public static byte[] Decrypt(byte[] cypherText, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        if (cypherText.Length < (128 / 8))
            throw new ArgumentException("Minimum AES cypher text length is 128 bites.");

        var iv = cypherText.Take(16).ToArray();
        cypherText = cypherText.Skip(16).ToArray();

        using (var aes = Aes.Create())
        {
            aes.IV = iv;
            aes.KeySize = 256;
            aes.Key = Hash.Sha256AsBytes(Encoding.Unicode.GetBytes(key));

            using (var input = new MemoryStream(cypherText))
            {
                using (var cryptoStream = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (var output = new MemoryStream())
                    {
                        cryptoStream.CopyTo(output);
                        return output.ToArray();
                    }
                }
            }
        }
    }
}