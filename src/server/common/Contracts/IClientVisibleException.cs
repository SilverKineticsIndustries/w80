using System.Net;

namespace SilverKinetics.w80.Common.Contracts;

public interface IClientVisibleException
{
    string? Title { get; }
    string? Type { get; }
    string Message { get; }
    HttpStatusCode StatusCode { get; }
}