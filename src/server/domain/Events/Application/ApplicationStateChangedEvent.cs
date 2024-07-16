using MongoDB.Bson;
using SilverKinetics.w80.Domain.ValueObjects;

namespace SilverKinetics.w80.Domain.Events.Application;

public class ApplicationStateChangedEvent
    : EventBase<ApplicationAcceptedEvent>
{
    public ApplicationStateChangedEvent(ObjectId createdBy, DateTime createdUTC, Entities.Application application, State previousState)
        : base(createdBy, createdUTC, nameof(Application), application.Id)
    {
        KeyProps.Add(PropPreviousState, previousState.Name);
    }

    public const string PropPreviousState = "PreviousState";
}
