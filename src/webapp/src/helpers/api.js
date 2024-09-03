import axios from 'axios';
import { jwtDecode } from "jwt-decode";
import { getAccessToken, setAccessToken } from './accessTokensStorage';
import { isRefreshTokenInvalid, isValidationErrors } from './badResponses';

const instance = axios.create({
    headers: {'Content-Type': 'application/json'},
    timeout: process.env.REACT_APP_TIMEOUT_IN_MILLISECONDS,
    withCredentials: true
});

instance.interceptors.request.use(
    async (config) => {
        if (!config.unprotected)
        {
            var res = await getWorkingAccessToken();
            if (res?.data?.accessToken)
            {
                setAccessToken(res.data.accessToken);
                config.headers['Authorization'] = `Bearer ${res.data.accessToken}`;
            }
            else if (res)
                config.headers['Authorization'] = `Bearer ${res}`;
            else
                returnToLogin();
        }
        return config;
    },
    error => {
        return Promise.reject(error);
    }
);

async function get(url, unprotectedEndpoint = false)
{
    return instance.request({
        method: 'GET',
        url: process.env.REACT_APP_BASE_API + url,
        unprotected: unprotectedEndpoint
    });
}

function post(url, payload, unprotectedEndpoint = false, sendCookies = false)
{
    return instance.request({
        method: 'POST',
        url: process.env.REACT_APP_BASE_API + url,
        data: payload,
        unprotected: unprotectedEndpoint
    });
}

function returnToLogin() {
    window.location.href = '/';
}

async function getWorkingAccessToken(forceNewToken=false) {
    const token = getAccessToken();
    let tokenObj, expirationDate;
    try // In case tokens is corrupted
    {
        tokenObj = !token || jwtDecode(token);
        expirationDate = !tokenObj || tokenObj.exp;
    }
    catch (err) {}
    if (forceNewToken || (!token || !expirationDate || (Date.now() > expirationDate * 1000)))
    {
        // Post refresh token (in cookie) to get another access+refresh tokens.
        const path = process.env.REACT_APP_BASE_API + "/authentication/refresh";
        try
        {
            return await axios.post(path, { timeout: process.env.REACT_APP_TIMEOUT_IN_MILLISECONDS}, { withCredentials: true });
        }
        catch(err)
        {
            if (err.code === "ERR_NETWORK")
                throw err;

            if (isRefreshTokenInvalid(err))
                setAccessToken();
        }
    }
    return token;
}

function apiDecoratorOptions(statusContext, onOk, onValidationError, target, onFinally)
{
    return {
        statusContext: statusContext || { setLoading: () => {}, setServerErrorMessage: () => {} },
        onOk:  onOk || function () {},
        onValidationError: onValidationError || function () {},
        target: target || {},
        onFinally: onFinally || function () {}
    }
};

function apiDispatchDecorator(func, options)
{
    return async (dispatch, getState) =>
    {
        try
        {
            options.onValidationError([]);
            options.target.disabled = true;
            options.statusContext.setLoading(true);
            var res = await func(dispatch, getState);
            options.onOk(res?.data);
            options.statusContext.setServerErrorMessage();
        }
        catch (err)
        {
            if (isValidationErrors(err))
                options.onValidationError(err.response.data.validationErrors);
            else
                options.statusContext.setServerErrorMessage(err.code);
        }
        finally {
            options.statusContext.setLoading(false);
            options.target.disabled = false;
            options.onFinally();
        }
    }
}

function apiDirectDecorator(func, options)
{
    return async () =>
    {
        try
        {
            options.onValidationError([]);
            options.target.disabled = true;
            options.statusContext.setLoading(true);
            var res = await func();
            options.onOk(res?.data);
        }
        catch (err)
        {
            if (isValidationErrors(err))
                options.onValidationError(err.response.data.validationErrors);
            else
                options.statusContext.setServerErrorMessage(err.code);
        }
        finally {
            options.statusContext.setLoading(false);
            options.target.disabled = false;
            options.onFinally();
        }
    }
}

export { get, post, apiDispatchDecorator, apiDirectDecorator, apiDecoratorOptions }