namespace SilverKinetics.w80.Application.Contracts;

public interface ICookieManager
{
    void SetCookie(string name, string value, DateTime? expirationDate = null);
    string GetCookie(string key);
    void RemoveCookie(string name);
}