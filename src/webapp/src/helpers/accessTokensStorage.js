import { jwtDecode } from 'jwt-decode';

export function getAccessToken()
{
    var val = sessionStorage.getItem("at");
    if (!val || val === "undefined")
        return null;
    else
        return val;
}

export function setAccessToken(token, expirationDate)
{
    sessionStorage.setItem("at", token);
}

export function clearAccessToken()
{
    sessionStorage.removeItem("at");
}

export function getAccessTokenClaimValue(type)
{
    const token = getAccessToken();
    if (token)
    {
        try
        {
            const decoded = jwtDecode(token);
            return decoded[type];
        } catch (err) {
            return;
        }
    }
}

export function getUserFromAccessToken()
{
    const token = getAccessToken();
    if (token)
    {
        try
        {
            const decoded = jwtDecode(token);
            return { email: decoded.Email, id: decoded.ID, browserNotificationsEnabled: decoded.BrowserNotificationsEnabled };
        } catch (err) {
            return;
        }
    }
}
