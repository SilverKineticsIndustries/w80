using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SilverKinetics.w80.Domain.Shared;
using SilverKinetics.w80.Domain.Entities;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Domain.Contracts;

namespace SilverKinetics.w80.Application.Repositories;

public class UserRepository(
    ISecurityContext securityContext,
    IDateTimeProvider dateTimeProvider,
    IMongoCollection<User> userSet,
    ISystemEventEntryRepository systemEventEntryRepository,
    ISystemEventSink systemEventSink)
        : BaseRepository<User>(securityContext, dateTimeProvider, userSet),
    IUserRepository
{
    public async Task UpsertAsync(User update, User? current, CancellationToken cancellationToken, IClientSessionHandle? session = null)
    {
        update.CopyFrom(current);
        ApplyPreSaveActions(update);
        if (session == null)
            await Set.ReplaceOneAsync(x => x.Id == update.Id, update, replaceOptions, cancellationToken).ConfigureAwait(false);
        else
            await Set.ReplaceOneAsync(session, x => x.Id == update.Id, update, replaceOptions, cancellationToken).ConfigureAwait(false);

        await PersistSystemEventsAsync(systemEventEntryRepository, systemEventSink, cancellationToken, session);
    }

    public async Task DeactivateAsync(User user, CancellationToken cancellationToken, IClientSessionHandle? session = null)
    {
        ApplyPreDeleteActions(user);
        var update = Builders<User>.Update
                        .Set(appl => appl.DeactivatedUTC, user.DeactivatedUTC)
                        .Set(appl => appl.DeactivatedBy, user.DeactivatedBy);

        if (session == null)
            await Set.UpdateOneAsync(x => x.Id == user.Id, update, updateOptions, cancellationToken).ConfigureAwait(false);
        else
            await Set.UpdateOneAsync(session, x => x.Id == user.Id, update, updateOptions, cancellationToken).ConfigureAwait(false);

        await PersistSystemEventsAsync(systemEventEntryRepository, systemEventSink, cancellationToken);
    }

    protected override IMongoQueryable<User> AddRepositoryQueryFilters(IMongoQueryable<User> users)
    {
        return
            users.Where(x => x.Role != Common.Role.ServiceWorker);
    }
}