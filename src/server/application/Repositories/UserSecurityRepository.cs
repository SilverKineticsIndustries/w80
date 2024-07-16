using MongoDB.Driver;
using SilverKinetics.w80.Domain.Shared;
using SilverKinetics.w80.Domain.Entities;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Domain.Contracts;

namespace SilverKinetics.w80.Application.Repositories;

public class UserSecurityRepository(
    ISecurityContext securityContext,
    IDateTimeProvider dateTimeProvider,
    IMongoCollection<UserSecurity> userSecuritySet,
    ISystemEventEntryRepository systemEventEntryRepository,
    ISystemEventSink systemEventSink)
        : BaseRepository<UserSecurity>(securityContext, dateTimeProvider, userSecuritySet),
    IUserSecurityRepository
{
    public async Task UpsertAsync(UserSecurity userSecurity, CancellationToken cancellationToken, IClientSessionHandle? session = null)
    {
        if (session == null)
            await userSecuritySet.ReplaceOneAsync(x => x.Id == userSecurity.Id, userSecurity, replaceOptions, cancellationToken).ConfigureAwait(false);
        else
            await userSecuritySet.ReplaceOneAsync(session, x => x.Id == userSecurity.Id, userSecurity, replaceOptions, cancellationToken).ConfigureAwait(false);

        await PersistSystemEventsAsync(systemEventEntryRepository, systemEventSink, cancellationToken, session);
    }
}