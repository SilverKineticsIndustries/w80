using System.Security.Claims;
using SilverKinetics.w80.Domain.Entities;

namespace SilverKinetics.w80.Application.Security;

public static class Claims
{
    public static List<Claim> GetClaims(User user)
    {
       var claims = new List<Claim>();
       claims.Add(new Claim("ID", user.Id.ToString()));
       claims.Add(new Claim("Email", user.Email));
       claims.Add(new Claim("Nickname", user.Nickname ?? string.Empty));
       claims.Add(new Claim("Culture", user.Culture));
       claims.Add(new Claim("Timezone", user.TimeZone));
       claims.Add(new Claim("Role", user.Role.ToString()));
       claims.Add(new Claim("BrowserNotificationsEnabled", user.EnableEventBrowserNotifications.ToString()));
       return claims;
    }
}