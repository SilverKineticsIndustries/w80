using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SilverKinetics.w80.Application.Contracts;
using Microsoft.Extensions.Configuration;
using SilverKinetics.w80.Common;
using SilverKinetics.w80.Common.Configuration;
using SilverKinetics.w80.Application.Security;

namespace SilverKinetics.w80.Application;

public class CookieManager
    : ICookieManager
{
    public CookieManager(ControllerBase controllerBase, IConfiguration configuration)
    {
        _controllerBase = controllerBase;
        _configuration = configuration;
    }

    public string GetCookie(string key)
    {
        return
            _controllerBase.Request.Cookies.ContainsKey(key)
                ? _controllerBase.Request.Cookies.First(x => x.Key == key).Value
                : string.Empty;
    }

    public void SetCookie(string name, string value, DateTime? expirationDate = null)
    {
        var options = new CookieOptions();
        options.HttpOnly = true;
        if (expirationDate is not null)
            options.Expires = expirationDate;
        if (name == Tokens.RefreshTokenCookieName)
            options.Path = _configuration.GetOptionalValue(Keys.RefreshCookiePath, "/");
        options.SameSite = SameSiteMode.Strict;

        _controllerBase.Response.Cookies.Append(name, value, options);
    }

    public void RemoveCookie(string name)
    {
        if (_controllerBase.Request.Cookies.ContainsKey(name))
            _controllerBase.Response.Cookies.Delete(name);
    }

    private readonly ControllerBase _controllerBase;
    private readonly IConfiguration _configuration;
}
