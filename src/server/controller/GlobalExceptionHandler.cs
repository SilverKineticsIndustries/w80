using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using SilverKinetics.w80.Application;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Application.Exceptions;

namespace SilverKinetics.w80.Controller;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IStringLocalizer<Common.Resource.Resources> stringLocalizer)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is ReCaptchaFailedException)
        {
            await httpContext
                .Response
                .WriteAsJsonAsync(
                    new ProblemDetails() {
                        Status = (int)HttpStatusCode.BadRequest,
                        Type = "CAPTCHA_FAILED",
                        Title = stringLocalizer["Captcha verification failed."],
                        Detail = stringLocalizer["Captcha verification failed."]
                    },
                cancellationToken);

            return true;
        }

        var info = new {
            Path = httpContext.Request.Path.ToString(),
            ContentLength = httpContext.Request.ContentLength.ToString(),
            ContentType = httpContext.Request.ContentType?.ToString(),
            Headers = httpContext.Request.Headers.GetHeadersForLogging(),
            Scheme = httpContext.Request.Scheme,
            Method = httpContext.Request.Method,
            Form = httpContext.Request.HasFormContentType ? httpContext.Request.Form : null,
            Host = httpContext.Request.Host,
            RouteValues = httpContext.Request.RouteValues,
            User = httpContext.User?.Identity?.Name ?? "Unknown",
            Claims = httpContext.User?.Claims?.ToDictionary(x => x.Type, x => x.Value)
        };

        logger.LogError(
            exception,
            exception.Message + " Details: " +
            JsonConvert.SerializeObject(info, Formatting.Indented));

        if (exception is OperationCanceledException)
        {
            // 499 - Client Closed Request Used when the client has closed the
            //       request before the server could send a response.
            httpContext.Response.StatusCode = 499;
            return true;
        }

        // TODO: Handle PersistenceException here (after it is implemented)?

        if (exception is AuthorizationException)
        {
            httpContext.Response.StatusCode = 401;
            return true;
        }

        var response
            = (exception is IClientVisibleException clientVisibleException)
                ?
                    new ProblemDetails
                    {
                        Type = clientVisibleException.Type,
                        Title = clientVisibleException.Title,
                        Detail = clientVisibleException.Message,
                        Status = (int)clientVisibleException.StatusCode
                    }
                :
                    new ProblemDetails
                    {
                        Status = (int)HttpStatusCode.InternalServerError,
                        Type = "GENERAL_ERROR",
                        Title = stringLocalizer["An error has occured and administrator has been notified."],
                        Detail = stringLocalizer["An error has occured and administrator has been notified."]
                    };

        await httpContext
            .Response
            .WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}