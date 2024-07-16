import { getAccessTokenClaimValue } from "./accessTokensStorage";

const sortByName = (a, b) =>
{
    if (a.name < b.name)
        return -1;
    else if (a.name > b.name)
        return 1;
    else
        return 0;
}

const sortById = (a, b) =>
{
    if (a.id < b.id)
        return -1;
    else if (a.id > b.id)
        return 1;
    else
        return 0;
}

const isValidHttpUrl = (val) => {
    let url;

    try {
      url = new URL(val);
    } catch (_) {
      return false;
    }

    return url.protocol === "http:" || url.protocol === "https:";
}

const getUserCulture = () => {
    var culture = getAccessTokenClaimValue('Culture');
    if (!culture || culture.indexOf('-') === -1)
        return 'en';
    else
        return culture.split('-')[0];
}

const createdValidationError = (msg) => {
    return { 'clientMessage': msg };
}

const onUpdateField = (e, setFunc) => {
    const isDate = e.target.type === "datetime-local";
    if (isDate && e.target.validationMessage)
        return;

    if (isDate)
    {
        setFunc({
            type: "update",
            name: e.target.name,
            payload: new Date(e.target.value).toISOString()
        });
    }
    else
    {
        setFunc({
            type: "update",
            name: e.target.name,
            payload: e.target.type === "checkbox" ? e.target.checked : e.target.value
        });
    }
}

export { sortByName, sortById, isValidHttpUrl, getUserCulture, createdValidationError, onUpdateField }