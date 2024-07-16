using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SilverKinetics.w80.Common;
using SilverKinetics.w80.Domain.Shared;
using SilverKinetics.w80.Domain.Entities;
using SilverKinetics.w80.Domain.Contracts;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Domain.Events.Application;

namespace SilverKinetics.w80.Domain.Services;

public class StatisticsGenerationService(
    IDateTimeProvider dateTimeProvider,
    IStatisticsRepository statisticsRepo,
    IUserRepository userRepo,
    ISystemEventEntryRepository systemEventEntryRepo)
    : IStatisticsGenerationService
{
    public async Task<IEnumerable<Statistics>> UpdateStatisticsAsync(SystemState systemState, CancellationToken cancellationToken)
    {
        var toUpdate = new List<Statistics>();
        var lastStatisticsRunUTC = !systemState.LastStatisticsRunUTC.IsMaxOrMinValue() ? systemState.LastStatisticsRunUTC : DateTime.MinValue;

        var now = dateTimeProvider.GetUtcNow();
        if (now < lastStatisticsRunUTC)
            return toUpdate;

        var systemEvents = await QuerySystemEventsForStatisticsAsync(lastStatisticsRunUTC, now, cancellationToken);

        systemState.LastStatisticsRunUTC = dateTimeProvider.GetUtcNow();
        if (!systemEvents.Any())
            return toUpdate;

        var users = await userRepo.GetManyAsync((x) => x.DeactivatedUTC == null, cancellationToken).ConfigureAwait(false);
        var userIds = users.Select(x => x.Id);
        var statistics = await statisticsRepo.GetManyAsync((x) => userIds.Contains(x.Id), cancellationToken).ConfigureAwait(false);

        foreach(var user in users)
        {
            var stats = statistics.FirstOrDefault(x => x.Id == user.Id) ?? Statistics.Create(user.Id);
            cancellationToken.ThrowIfCancellationRequested();
            if (CalculateApplicationRejectionStateCounts(user, stats, systemEvents))
                toUpdate.Add(stats);
        }

        return toUpdate;
    }

    private bool CalculateApplicationRejectionStateCounts(Entities.User user, Statistics statistics, IEnumerable<SystemEventEntry> events)
    {
        var filtered = events.Where(x => x.EventName == nameof(ApplicationRejectedEvent) && x.CreatedBy == user.Id).ToList();
        if (filtered.Count == 0)
            return false;

        var counts = filtered.GroupBy(x => ObjectId.Parse(x.KeyProps[ApplicationRejectedEvent.PropRejectedStateId].ToString()))
                             .ToDictionary(x => x.Key, x => x.Count());

        foreach(var item in counts)
        {
            statistics.ApplicationRejectionStateCounts.TryAdd(item.Key, 0);
            statistics.ApplicationRejectionStateCounts[item.Key] += item.Value;
        }

        return counts.Count > 0;
    }

    private async Task<IEnumerable<SystemEventEntry>> QuerySystemEventsForStatisticsAsync(
        DateTime startUTC, DateTime endUTC, CancellationToken cancellationToken)
    {
        var eventTypesForStatistics = new [] {
            nameof(ApplicationRejectedEvent),
            nameof(ApplicationInsertedEvent),
            nameof(ApplicationAcceptedEvent)
        };

        return await
            systemEventEntryRepo
                .GetManyAsync((x) =>
                    x.CreatedUTC >= startUTC
                    && x.CreatedUTC <= endUTC
                    && eventTypesForStatistics.Contains(x.EventName),
                    cancellationToken
                ).ConfigureAwait(false);
    }
}