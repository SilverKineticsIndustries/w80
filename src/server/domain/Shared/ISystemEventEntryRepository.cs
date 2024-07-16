using System.Linq.Expressions;
using MongoDB.Driver;
using SilverKinetics.w80.Domain.Contracts;
using SilverKinetics.w80.Domain.Entities;

namespace SilverKinetics.w80.Domain.Shared;

public interface ISystemEventEntryRepository
{
    Task<SystemEventEntry?> GetSingleOrDefaultAsync(Expression<Func<SystemEventEntry, bool>> predicate, CancellationToken cancellationToken);
    Task<IEnumerable<SystemEventEntry>> GetManyAsync(Expression<Func<SystemEventEntry, bool>> predicate, CancellationToken cancellationToken);

    public Task InsertAsync(
        IEnumerable<ISystemEvent> systemEvents,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null);
}