using System.Text;
using Microsoft.Extensions.Configuration;
using SilverKinetics.w80.Common;
using SilverKinetics.w80.Common.Security;
using SilverKinetics.w80.Common.Configuration;

namespace SilverKinetics.w80.Application.Security;

public static class EmailConfirmations
{
    public static byte[] GenerateNew(IConfiguration config, string email, DateTime utcNow)
    {
        var plainText = string.Concat(email, "|", utcNow.ToISO8610String());
        var plainTextBytes = Encoding.Unicode.GetBytes(plainText);
        return Encryption.Encrypt(plainTextBytes,
            config.GetRequiredValue(Keys.Secrets.EmailConfirmationKey));
    }

    public static void Decrypt(IConfiguration config, string emailConfirmationToken, out string email, out DateTime utcDateTime)
    {
        email = "";
        utcDateTime = DateTime.MinValue;

        var tokenCypherBytes = Convert.FromBase64String(emailConfirmationToken);
        var token = Encoding.Unicode.GetString(Encryption.Decrypt(tokenCypherBytes,
            config.GetRequiredValue(Keys.Secrets.EmailConfirmationKey)));
        if (!token.Contains('|'))
            return;

        var split = token.Split('|');
        email = split[0];
        utcDateTime = split[1].FromISO8610String();
    }

    public static bool IsExpired(IConfiguration config, DateTime utcDateTimeFromToken, DateTime utcNow)
    {
        var lifetimeInHours = Convert.ToDouble(config[Keys.EmailConfirmationLifetimeInHours]);
        return utcNow > utcDateTimeFromToken.AddHours(lifetimeInHours);
    }
}