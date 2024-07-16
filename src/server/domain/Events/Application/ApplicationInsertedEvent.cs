using MongoDB.Bson;

namespace SilverKinetics.w80.Domain.Events.Application;

public class ApplicationInsertedEvent
    : EventBase<ApplicationInsertedEvent>
{
    public ApplicationInsertedEvent(ObjectId createdBy, DateTime createdUTC, Entities.Application application)
        : base(createdBy, createdUTC, nameof(Application), application.Id)
    {}
}
