using System.Text;
using System.Security.Cryptography;

namespace SilverKinetics.w80.Common.Security;

public static class Hash
{
    public static string Sha256(string input)
    {
        return Sha256(Encoding.Unicode.GetBytes(input));
    }

    public static byte[] Sha256AsBytes(string input)
    {
        return Sha256AsBytes(Encoding.Unicode.GetBytes(input));
    }

    public static string Sha256(byte[] input)
    {
        var inputHash = SHA256.HashData(input);
        return Convert.ToBase64String(inputHash);
    }

    public static byte[] Sha256AsBytes(byte[] input)
    {
        return SHA256.HashData(input);
    }

    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public static bool IsPasswordEqual(string input, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(input, hash);
    }
}