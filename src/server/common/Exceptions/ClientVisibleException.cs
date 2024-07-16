using System.Net;
using SilverKinetics.w80.Common.Contracts;

namespace SilverKinetics.w80.Common.Exceptions;

public class ClientVisibleException(
    string message,
    HttpStatusCode statusCode,
    string? title = null,
    string? type = null,
    Exception? innerException = null)
        : Exception(message, innerException),
    IClientVisibleException
{
    public string? Title { get; } = title;
    public string? Type { get; } = type;
    public HttpStatusCode StatusCode { get; } = statusCode;
}