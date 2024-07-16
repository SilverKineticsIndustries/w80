using MongoDB.Bson;

namespace SilverKinetics.w80.Domain.Events.Application;

public class ApplicationDeactivatedEvent
    : EventBase<ApplicationDeactivatedEvent>
{
    public ApplicationDeactivatedEvent(ObjectId createdBy, DateTime createdUTC, Entities.Application application)
        : base(createdBy, createdUTC, nameof(Application), application.Id)
    {
        KeyProps.Add(PropDeactivatedState, application.GetCurrentState().Name);
    }

    public const string PropDeactivatedState = "DeactivatedState";
}
