using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using SilverKinetics.w80.Common.Security;
using SilverKinetics.w80.Common.Configuration;
using SilverKinetics.w80.Common;

namespace SilverKinetics.w80.Application.Security;

public static class Tokens
{
    public const string RefreshTokenCookieName = "_rt";
    public const string LogoutTokenEndPointPath = "/authentication/logout";
    public const string RefreshTokenEndPointPath = "/authentication/refresh";

    public static byte[] GenerateRefreshToken()
    {
        return
            Common.Security.Random.GenerateCryptoRandomData(64);
    }

    public static string GetRefreshTokenHashInBase64(string refreshToken)
    {
        return Convert.ToBase64String(Hash.Sha256AsBytes(Convert.FromBase64String(refreshToken)));
    }

    public static JwtSecurityToken DecodeJwtToken(string encodedToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(encodedToken);
        return (JwtSecurityToken)jsonToken;
    }

    public static string EncodeJwtToken(IConfiguration config, DateTime utcNow, IEnumerable<Claim> claims)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetRequiredValue(Keys.Secrets.JwtKey)));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            config[Keys.JwtIssuer],
            config[Keys.JwtIssuer],
            claims,
            expires: utcNow.AddMinutes(Convert.ToInt32(config[Keys.JwtAccessLifetimeInMinutes])),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}