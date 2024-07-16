using MongoDB.Bson;

namespace SilverKinetics.w80.Domain.Events.Application;

public class ApplicationUnarchivedEvent
    : EventBase<ApplicationUnarchivedEvent>
{
    public ApplicationUnarchivedEvent(ObjectId createdBy, DateTime createdUTC, Entities.Application application)
        : base(createdBy, createdUTC, nameof(Application), application.Id)
    {}
}
