using MongoDB.Bson;

namespace SilverKinetics.w80.Domain.Events.Application;

public class ApplicationReactivatedEvent
    : EventBase<ApplicationReactivatedEvent>
{
    public ApplicationReactivatedEvent(ObjectId createdBy, DateTime createdUTC, Entities.Application application)
        : base(createdBy, createdUTC, nameof(Application), application.Id)
    {}
}
