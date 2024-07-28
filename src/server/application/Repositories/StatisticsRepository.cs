using MongoDB.Driver;
using SilverKinetics.w80.Domain.Shared;
using SilverKinetics.w80.Domain.Entities;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Domain.Contracts;

namespace SilverKinetics.w80.Application.Repositories;

public class StatisticsRepository(
    ISecurityContext securityContext,
    IDateTimeProvider dateTimeProvider,
    IMongoCollection<Statistics> statisticsSet)
        : BaseRepository<Statistics>(securityContext, dateTimeProvider, statisticsSet),
    IStatisticsRepository
{
    public async Task UpdateAsync(Statistics update, CancellationToken cancellationToken, IClientSessionHandle? session = null)
    {
        if (session == null)
            await Set.ReplaceOneAsync(x => x.Id == update.Id, update, replaceOptions, cancellationToken).ConfigureAwait(false);
        else
            await Set.ReplaceOneAsync(session, x => x.Id == update.Id, update, replaceOptions, cancellationToken).ConfigureAwait(false);
    }
}