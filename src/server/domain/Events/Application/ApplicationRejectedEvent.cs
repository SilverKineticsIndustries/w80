using MongoDB.Bson;

namespace SilverKinetics.w80.Domain.Events.Application;

public class ApplicationRejectedEvent
    : EventBase<ApplicationRejectedEvent>
{
    public ApplicationRejectedEvent(ObjectId createdBy, DateTime createdUTC, Entities.Application application)
        : base(createdBy, createdUTC, nameof(Application), application.Id)
    {
        var state = application.GetCurrentState();

        KeyProps.Add(PropRejectedStateId, state.Id);
        KeyProps.Add(PropRejectedStateName, state.Name);
    }

    public const string PropRejectedStateId = "RejectedStateId";
    public const string PropRejectedStateName = "RejectedStateName";
}
