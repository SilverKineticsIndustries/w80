using System.Text;
using Microsoft.Extensions.Configuration;
using SilverKinetics.w80.Common.Security;
using SilverKinetics.w80.Common.Configuration;
using SilverKinetics.w80.Common;

namespace SilverKinetics.w80.Application.Security;

public static class Invitations
{
    public static byte[] GenerateNew(IConfiguration config, string email, DateTime utcNow)
    {
        var plainText = string.Concat(email, "|", utcNow.ToISO8610String());
        var plainTextBytes = Encoding.Unicode.GetBytes(plainText);
        return Encryption.Encrypt(plainTextBytes, config[Keys.Secrets.InvitationKey]);
    }

    public static void Decrypt(IConfiguration config, string invitationCodeBase64, out string email, out DateTime utcDateTime)
    {
        email = "";
        utcDateTime = DateTime.MinValue;

        var invitationCodeCypherBytes = Convert.FromBase64String(invitationCodeBase64);
        var invitationCode = Encoding.Unicode.GetString(Encryption.Decrypt(invitationCodeCypherBytes, config[Keys.Secrets.InvitationKey]));
        if (!invitationCode.Contains('|'))
            return;

        var split = invitationCode.Split('|');
        email = split[0];
        utcDateTime = split[1].FromISO8610String();
    }

    public static bool IsExpired(IConfiguration config, DateTime utcDateTimeFromInvitation, DateTime utcNow)
    {
        var invitationCodeLifeTimeInHours = Convert.ToDouble(config[Keys.InvitationCodeLifetimeInHours]);
        return utcNow > utcDateTimeFromInvitation.AddHours(invitationCodeLifeTimeInHours);
    }
}