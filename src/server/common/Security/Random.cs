using System.Security.Cryptography;

namespace SilverKinetics.w80.Common.Security;

public static class Random
{
    public static byte[] GenerateCryptoRandomData(int size)
    {
        var buff = new byte[size];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(buff);
        return buff;
    } 
}