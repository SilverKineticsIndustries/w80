using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;
using SilverKinetics.w80.Domain.ValueObjects;

namespace SilverKinetics.w80.Domain.Shared;

public interface IApplicationRepository
{
    Task<Entities.Application?> GetSingleOrDefaultAsync(Expression<Func<Entities.Application, bool>> predicate, CancellationToken cancellationToken);
    Task<IEnumerable<Entities.Application>> GetManyAsync(Expression<Func<Entities.Application, bool>> predicate, CancellationToken cancellationToken);

    public Task UpsertAsync(
        Entities.Application application,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null);

    public Task DeactivateAsync(
        Entities.Application application,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null);

    public Task ReactivateAsync(
        Entities.Application application,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null);

    public Task ArchiveAsync(
        Entities.Application application,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null);

    public Task UnarchiveAsync(
        Entities.Application application,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null);

    public Task RejectAsync(
        Entities.Application application,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null);

    public Task AcceptAsync(
        Entities.Application application,
        IEnumerable<Entities.Application> applicationsToArchive,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null);

    public Task SetEmailNotificationSentOnAppoinmentsAsync(
        IDictionary<ObjectId, List<Guid>> update,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null);

    public Task SetBrowserNotificationSentOnAppointmentsAsync(
        IDictionary<ObjectId, List<Guid>> update,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null);
}