namespace SilverKinetics.w80.TestHelper.Fakes;

public class CookieManagerFake
    : ICookieManager
{
    public Lazy<IDictionary<string,string>> Cookies = new(() => new Dictionary<string,string>());

    public string GetCookie(string key)
    {
        if (Cookies.Value.TryGetValue(key, out string? val))
            return val;
        return string.Empty;
    }

    public void SetCookie(string name, string value, DateTime? expirationDate = null)
    {
        if (Cookies.Value.ContainsKey(name))
            Cookies.Value[name] = value;
        else
            Cookies.Value.Add(name, value);
    }

    public void RemoveCookie(string name)
    {
        if (Cookies.Value.ContainsKey(name))
            Cookies.Value.Remove(name);
    }
}