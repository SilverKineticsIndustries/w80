using MongoDB.Bson;

namespace SilverKinetics.w80.Domain.Events.Application;

public class ApplicationUpdatedEvent
    : EventBase<ApplicationUpdatedEvent>
{
    public ApplicationUpdatedEvent(ObjectId createdBy, DateTime createdUTC, Entities.Application application)
        : base(createdBy, createdUTC, nameof(Application), application.Id)
    {}
}
