using MongoDB.Bson;

namespace SilverKinetics.w80.Domain.Events.Application;

public class ApplicationArchivedEvent
    : EventBase<ApplicationArchivedEvent>
{
    public ApplicationArchivedEvent(ObjectId createdBy, DateTime createdUTC, Entities.Application application)
        : base(createdBy, createdUTC, nameof(Application), application.Id)
    {
        KeyProps.Add(PropArchivedState, application.GetCurrentState().Name);
    }

    public const string PropArchivedState = "ArchivedState";
}
