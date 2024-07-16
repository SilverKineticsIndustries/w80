function setBrowserNotificationsEnabledFlag(val)
{
    sessionStorage.setItem("w80_browserNotificationsEnabled", val);
}

function getBrowserNotificationsEnabledFlag()
{
    return sessionStorage.getItem("w80_browserNotificationsEnabled") || false;
}

export { setBrowserNotificationsEnabledFlag, getBrowserNotificationsEnabledFlag }