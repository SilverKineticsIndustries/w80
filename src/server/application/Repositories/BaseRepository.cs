using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SilverKinetics.w80.Common;
using SilverKinetics.w80.Domain.Shared;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Domain.Contracts;

namespace SilverKinetics.w80.Application.Repositories;

public abstract class BaseRepository<T>
    where T : class, IAggregateRoot
{
    protected BaseRepository(
        ISecurityContext securityContext,
        IDateTimeProvider dateTimeProvider,
        IMongoCollection<T> set)
    {
        Set = set;
        _securityContext = securityContext;
        _dateTimeProvider = dateTimeProvider;
    }

    protected IMongoCollection<T> Set { private set; get; }
    public bool QueryFiltersEnabled { private get; set; } = true;

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        return await ApplyRepositoryQueryFilters(Set.AsQueryable())
                     .AnyAsync(predicate, cancellationToken)
                     .ConfigureAwait(false);
    }

    public async Task<T?> FirstOrDefaultAsync(CancellationToken cancellationToken)
    {
        return await ApplyRepositoryQueryFilters(Set.AsQueryable())
                        .FirstOrDefaultAsync(cancellationToken)
                        .ConfigureAwait(false);
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        return await ApplyRepositoryQueryFilters(Set.AsQueryable())
                        .Where(predicate)
                        .FirstOrDefaultAsync(cancellationToken)
                        .ConfigureAwait(false);
    }

    public async Task<T> FirstAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        return await ApplyRepositoryQueryFilters(Set.AsQueryable())
                        .Where(predicate)
                        .FirstAsync(cancellationToken)
                        .ConfigureAwait(false);
    }

    public async Task<T> FirstAsync(CancellationToken cancellationToken)
    {
        return await ApplyRepositoryQueryFilters(Set.AsQueryable())
                        .FirstAsync(cancellationToken)
                        .ConfigureAwait(false);
    }

    public async Task<IEnumerable<T>> GetManyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        return await ApplyRepositoryQueryFilters(Set.AsQueryable())
                        .Where(predicate)
                        .ToListAsync(cancellationToken)
                        .ConfigureAwait(false);
    }

    protected virtual void ApplyPreSaveActions(T obj)
    {
        bool creationUpdated = false;
        if (obj is IHasCreationAudit hasCreationAudit)
        {
            if (hasCreationAudit.CreatedUTC.IsMaxOrMinValue())
            {
                hasCreationAudit.CreatedUTC = _dateTimeProvider.GetUtcNow();
                hasCreationAudit.CreatedBy = _securityContext.UserId;
                creationUpdated = true;
            }
        }

        if (!creationUpdated)
        {
            if (obj is IHasUpdateAudit hasUpdateAudit)
            {
                hasUpdateAudit.UpdatedUTC = _dateTimeProvider.GetUtcNow();
                hasUpdateAudit.UpdatedBy = _securityContext.UserId;
            }
        }
    }
    protected virtual void ApplyPreDeleteActions(T obj)
    {
        if (obj is ISoftDeletionEntity softDeletionEntity)
        {
            // Deactivation date is set in the domain most of the time and we dont want to change it.
            if (softDeletionEntity.DeactivatedUTC is null || softDeletionEntity.DeactivatedUTC.Value.IsMaxOrMinValue())
                softDeletionEntity.DeactivatedUTC = _dateTimeProvider.GetUtcNow();

            softDeletionEntity.DeactivatedBy = _securityContext.UserId;
        }
    }

    protected IMongoQueryable<T> ApplyRepositoryQueryFilters(IMongoQueryable<T> query)
    {
        if (QueryFiltersEnabled)
            return AddRepositoryQueryFilters(query);
        else
            return query;
    }
    protected virtual IMongoQueryable<T> AddRepositoryQueryFilters(IMongoQueryable<T> query) { return query; }

    protected virtual async Task PersistSystemEventsAsync(
        ISystemEventEntryRepository systemEventEntryRepository,
        ISystemEventSink systemEventSink,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null)
    {
        var systemEvents = systemEventSink.All();
        if (systemEvents.Any())
        {
            await systemEventEntryRepository.InsertAsync(systemEvents, cancellationToken, session);
            systemEventSink.Clear();
        }
    }

    protected static T SuccessOrPersistenceException(T obj, UpdateResult result)
    {
        return obj;
        /* TODO: Figure out what we need to do here
        if (result.ModifiedCount == 1)
            return obj;
        else
            throw new PersistenceException();
        */
    }
    protected static T SuccessOrPersistenceException(T obj, ReplaceOneResult result)
    {
        return obj;
        /* TODO: Figure out what we need to do here
        if (result.ModifiedCount == 1)
            return obj;
        else
            throw new PersistenceException();
        */
    }

    private readonly ISecurityContext _securityContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    internal static readonly UpdateOptions updateOptions = new() { IsUpsert = false };
    internal static readonly ReplaceOptions replaceOptions = new() { IsUpsert = true };
    internal static readonly InsertOneOptions insertOptions = new() { BypassDocumentValidation = true };
    internal static readonly InsertManyOptions insertManyOptions = new () { BypassDocumentValidation = true };
}