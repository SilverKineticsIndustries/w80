using MongoDB.Driver;
using SilverKinetics.w80.Domain.Shared;
using SilverKinetics.w80.Domain.Entities;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Domain.Contracts;

namespace SilverKinetics.w80.Application.Repositories;

public class SystemEventEntryRepository(
    ISecurityContext securityContext,
    IDateTimeProvider dateTimeProvider,
    IMongoCollection<SystemEventEntry> systemEventEntrySet)
        : BaseRepository<SystemEventEntry>(securityContext, dateTimeProvider, systemEventEntrySet),
    ISystemEventEntryRepository
{
    public async Task InsertAsync(
        IEnumerable<ISystemEvent> systemEvents,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null)
    {
        var entries = new List<SystemEventEntry>();
        foreach(var systemEvent in systemEvents)
        {
            var entry = SystemEventEntry.Create(systemEvent);
            ApplyPreSaveActions(entry);
            entries.Add(entry);
        }

        if (session != null)
            await systemEventEntrySet.InsertManyAsync(session, entries, insertManyOptions, cancellationToken);
        else
            await systemEventEntrySet.InsertManyAsync(entries, insertManyOptions, cancellationToken);
    }

    protected override Task PersistSystemEventsAsync(
        ISystemEventEntryRepository systemEventEntryRepository,
        ISystemEventSink systemEventSink,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null)
    {
        return Task.CompletedTask;
    }
}