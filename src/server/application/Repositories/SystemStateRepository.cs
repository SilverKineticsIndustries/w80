using MongoDB.Driver;
using SilverKinetics.w80.Domain.Shared;
using SilverKinetics.w80.Domain.Entities;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Domain.Contracts;

namespace SilverKinetics.w80.Application.Repositories;

public class SystemStateRepository(
    ISecurityContext securityContext,
    IDateTimeProvider dateTimeProvider,
    IMongoCollection<SystemState> systemStateSet)
        : BaseRepository<SystemState>(securityContext, dateTimeProvider, systemStateSet),
    ISystemStateRepository
{
    public async Task UpdateAsync(SystemState systemState, CancellationToken cancellationToken, IClientSessionHandle? session = null)
    {
        if (session == null)
            await systemStateSet.ReplaceOneAsync(x => x.Id == systemState.Id, systemState, replaceOptions, cancellationToken).ConfigureAwait(false);
        else
            await systemStateSet.ReplaceOneAsync(session, x => x.Id == systemState.Id, systemState, replaceOptions, cancellationToken).ConfigureAwait(false);
    }
}