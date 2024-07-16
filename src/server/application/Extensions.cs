using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using SilverKinetics.w80.Common.Security;

namespace SilverKinetics.w80.Application;

public static class Extensions
{
    public static RequestSourceInfo GetRequestSourceInfo(this HttpContext ctx)
    {
        var ri = new RequestSourceInfo();
        ri.IP = ctx.Connection.RemoteIpAddress.ToString();
        ri.Host = ctx.Request.Host.Host;
        ri.Headers = ctx.Request.Headers.GetHeadersForLogging();
        return ri;
    }

    public static IDictionary<string,string> GetHeadersForLogging(this IHeaderDictionary headers)
    {
        // Read info about logginOptions.RequestHeaders ...
        return headers.Where(x => logginOptions.RequestHeaders.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value.ToString());
    }

    public static async Task WrapInTransactionAsync(
        this IMongoClient client,
        Func<IClientSessionHandle, Task> action,
        CancellationToken cancellationToken)
    {
        using (var session = await client.StartSessionAsync(cancellationToken: cancellationToken))
        {
            try
            {
                session.StartTransaction();
                await action(session);
                await session.CommitTransactionAsync(cancellationToken);

            } catch {
                await session.AbortTransactionAsync(cancellationToken);
                throw;
            }
        }
    }

    private static readonly Microsoft.AspNetCore.HttpLogging.HttpLoggingOptions logginOptions = new();
}