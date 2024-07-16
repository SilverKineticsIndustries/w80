import { getUserCulture } from "./common";

export function printLocalizedDate(date)
{
    const locale = getUserCulture();
    return new Intl.DateTimeFormat(locale, {year: 'numeric', month: '2-digit',day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit'}).format(new Date(date));
}

export function printLocalizedShortDate(date)
{
    const locale = getUserCulture();
    return new Intl.DateTimeFormat(locale, {year: '2-digit', month: '2-digit',day: '2-digit', hour: '2-digit', minute: '2-digit'}).format(new Date(date));
}

export function numbOfMinutesFromNowToDate(date)
{
    return (new Date() - date) / (1000 * 60);
}

export function printUtcShortDate(date)
{
    if (date)
    {
        const locale = getUserCulture();
        return new Intl.DateTimeFormat(locale, {year: '2-digit', month: '2-digit',day: '2-digit', hour: '2-digit', minute: '2-digit', timeZone: "GMT"}).format(new Date(date));
    } else
        return '';
}

export function sortDatesAsc(a, b)
{
    // TODO: There are comment on SO which say creating Date() objects in sort method causes perf issues.
    const dateA = (typeof a == 'string') ? new Date(a) : a;
    const dateB = (typeof b == 'string') ? new Date(b) : b;
    return dateA - dateB;
}

export function sortDatesDesc(a, b)
{
    // TODO: There are comment on SO which say creating Date() objects in sort method causes perf issues.
    const dateA = (typeof a == 'string') ? new Date(a) : a;
    const dateB = (typeof b == 'string') ? new Date(b) : b;
    return dateB - dateA;
}

export function dateTimeToInputDateTimeLocalString(date) {
    const now = new Date(date);
    now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
    return now.toISOString().slice(0,16);
}