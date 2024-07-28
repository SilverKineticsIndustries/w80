using MongoDB.Bson;
using MongoDB.Driver;
using SilverKinetics.w80.Domain.Shared;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Domain.Contracts;
using SilverKinetics.w80.Domain.ValueObjects;

namespace SilverKinetics.w80.Application.Repositories;

public class ApplicationRepository(
    ISecurityContext securityContext,
    IDateTimeProvider dateTimeProvider,
    IMongoCollection<Domain.Entities.Application> applicationSet,
    ISystemEventSink systemEventSink,
    ISystemEventEntryRepository systemEventEntryRepository)
        : BaseRepository<Domain.Entities.Application>(securityContext, dateTimeProvider, applicationSet),
    IApplicationRepository
{
    public async Task AcceptAsync(
        Domain.Entities.Application application,
        IEnumerable<Domain.Entities.Application> applicationsToArchive,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null)
    {
        ApplyPreSaveActions(application);
        var update = Builders<Domain.Entities.Application>.Update
                        .Set(appl => appl.Acceptance, application.Acceptance);

        var ret = await Set.UpdateOneAsync((x) => x.Id == application.Id, update, updateOptions, cancellationToken).ConfigureAwait(false);
        foreach(var appl in applicationsToArchive)
        {
            ApplyPreSaveActions(appl);
            var archive = Builders<Domain.Entities.Application>.Update
                    .Set(appl => appl.ArchivedUTC, appl.ArchivedUTC);

            if (session == null)
                await Set.UpdateOneAsync(x => x.Id == appl.Id, archive, updateOptions, cancellationToken).ConfigureAwait(false);
            else
                await Set.UpdateOneAsync(session, x => x.Id == appl.Id, archive, updateOptions, cancellationToken).ConfigureAwait(false);
        }
        await PersistSystemEventsAsync(systemEventEntryRepository, systemEventSink, cancellationToken, session);
    }

    public async Task ArchiveAsync(
        Domain.Entities.Application application,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null)
    {
        ApplyPreSaveActions(application);
        var update = Builders<Domain.Entities.Application>.Update
                        .Set(appl => appl.ArchivedUTC, application.ArchivedUTC);

        if (session == null)
            await Set.UpdateOneAsync(x => x.Id == application.Id, update, updateOptions, cancellationToken).ConfigureAwait(false);
        else
            await Set.UpdateOneAsync(session, x => x.Id == application.Id, update, updateOptions, cancellationToken).ConfigureAwait(false);

        await PersistSystemEventsAsync(systemEventEntryRepository, systemEventSink, cancellationToken, session);
    }

    public async Task DeactivateAsync(
        Domain.Entities.Application application,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null)
    {
        ApplyPreDeleteActions(application);
        var update = Builders<Domain.Entities.Application>.Update
                        .Set(appl => appl.DeactivatedUTC, application.DeactivatedUTC)
                        .Set(appl => appl.DeactivatedBy, application.DeactivatedBy);

        if (session == null)
            await Set.UpdateOneAsync(x => x.Id == application.Id, update, updateOptions, cancellationToken).ConfigureAwait(false);
        else
            await Set.UpdateOneAsync(session, x => x.Id == application.Id, update, updateOptions, cancellationToken).ConfigureAwait(false);

        await PersistSystemEventsAsync(systemEventEntryRepository, systemEventSink, cancellationToken, session);
    }

    public async Task ReactivateAsync(
        Domain.Entities.Application application,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null)
    {
        ApplyPreSaveActions(application);
        var update = Builders<Domain.Entities.Application>.Update
                        .Set(appl => appl.DeactivatedUTC, application.DeactivatedUTC)
                        .Set(appl => appl.DeactivatedBy, application.DeactivatedBy);

        if (session == null)
            await Set.UpdateOneAsync(x => x.Id == application.Id, update, updateOptions, cancellationToken).ConfigureAwait(false);
        else
            await Set.UpdateOneAsync(session, x => x.Id == application.Id, update, updateOptions, cancellationToken).ConfigureAwait(false);

        await PersistSystemEventsAsync(systemEventEntryRepository, systemEventSink, cancellationToken, session);
    }

    public async Task RejectAsync(
        Domain.Entities.Application application,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null)
    {
        ApplyPreSaveActions(application);
         var update = Builders<Domain.Entities.Application>.Update
                        .Set(appl => appl.Rejection, application.Rejection);

        if (session == null)
            await Set.UpdateOneAsync(x => x.Id == application.Id, update, updateOptions, cancellationToken).ConfigureAwait(false);
        else
            await Set.UpdateOneAsync(session, x => x.Id == application.Id, update, updateOptions, cancellationToken).ConfigureAwait(false);

        await PersistSystemEventsAsync(systemEventEntryRepository, systemEventSink, cancellationToken, session);
    }

    public async Task UnarchiveAsync(
        Domain.Entities.Application application,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null)
    {
        ApplyPreSaveActions(application);
        var update = Builders<Domain.Entities.Application>.Update
                        .Set(appl => appl.ArchivedUTC, application.ArchivedUTC);

        if (session == null)
            await Set.UpdateOneAsync(x => x.Id == application.Id, update, updateOptions, cancellationToken).ConfigureAwait(false);
        else
            await Set.UpdateOneAsync(session, x => x.Id == application.Id, update, updateOptions, cancellationToken).ConfigureAwait(false);

        await PersistSystemEventsAsync(systemEventEntryRepository, systemEventSink, cancellationToken, session);
    }

    public async Task UpsertAsync(
        Domain.Entities.Application update,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null)
    {
        ApplyPreSaveActions(update);

        if (session == null)
            await Set.ReplaceOneAsync(x => x.Id == update.Id, update, replaceOptions, cancellationToken).ConfigureAwait(false);
        else
            await Set.ReplaceOneAsync(session, x => x.Id == update.Id, update, replaceOptions, cancellationToken).ConfigureAwait(false);

        await PersistSystemEventsAsync(systemEventEntryRepository, systemEventSink, cancellationToken, session);
    }

    public async Task SetEmailNotificationSentOnAppoinmentsAsync(
        IDictionary<ObjectId, List<Guid>> update,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null)
    {
        var eventIds = update.SelectMany(x => x.Value).ToList();
        var appFilter = Builders<Domain.Entities.Application>.Filter.In(x => x.Id, update.Keys);

        var updatePath = nameof(Appointment) + "s.$[evt]." + nameof(Appointment.EmailNotificationSent);
        var appUpdate = Builders<Domain.Entities.Application>.Update
                            .Set(updatePath, true);

        var updateOptions = new UpdateOptions
        {
            ArrayFilters =
            [
                new BsonDocumentArrayFilterDefinition<BsonDocument>(
                    new BsonDocument("evt._id", new BsonDocument("$in", new BsonArray(eventIds)))
                ),
            ],

        };

        if (session == null)
            await Set.UpdateManyAsync(appFilter, appUpdate, updateOptions, cancellationToken);
        else
            await Set.UpdateManyAsync(session, appFilter, appUpdate, updateOptions, cancellationToken);
    }

    public async Task SetBrowserNotificationSentOnAppointmentsAsync(
        IDictionary<ObjectId, List<Guid>> update,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null)
    {
        var eventIds = update.SelectMany(x => x.Value).ToList();
        var appFilter = Builders<Domain.Entities.Application>.Filter.In(x => x.Id, update.Keys);

        var updatePath = nameof(Appointment) + "s.$[evt]." + nameof(Appointment.BrowserNotificationSent);
        var appUpdate = Builders<Domain.Entities.Application>.Update
                            .Set(updatePath, true);

        var updateOptions = new UpdateOptions
        {
            ArrayFilters =
            [
                new BsonDocumentArrayFilterDefinition<BsonDocument>(
                    new BsonDocument("evt._id", new BsonDocument("$in", new BsonArray(eventIds)))
                ),
            ],

        };

        if (session == null)
            await Set.UpdateManyAsync(appFilter, appUpdate, updateOptions, cancellationToken);
        else
            await Set.UpdateManyAsync(session, appFilter, appUpdate, updateOptions, cancellationToken);
    }
}