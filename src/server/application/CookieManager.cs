using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SilverKinetics.w80.Application.Contracts;

namespace SilverKinetics.w80.Application;

public class CookieManager
    : ICookieManager
{
    public CookieManager(ControllerBase controllerBase)
    {
        _controllerBase = controllerBase;
    }

    public string GetCookie(string key)
    {
        return
            _controllerBase.Request.Cookies.ContainsKey(key)
                ? _controllerBase.Request.Cookies.First(x => x.Key == key).Value
                : string.Empty;
    }

    public void SetCookie(string name, string value, DateTime? expirationDate = null, bool isRefreshToken = false)
    {
        var options = new CookieOptions();
        options.HttpOnly = true;
        if (expirationDate is not null)
            options.Expires = expirationDate;
        if (isRefreshToken)
            options.Path = "/authentication";
        options.SameSite = SameSiteMode.Strict;

        _controllerBase.Response.Cookies.Append(name, value, options);
    }

    public void RemoveCookie(string name)
    {
        if (_controllerBase.Request.Cookies.ContainsKey(name))
            _controllerBase.Response.Cookies.Delete(name);
    }

    private readonly ControllerBase _controllerBase;
}
