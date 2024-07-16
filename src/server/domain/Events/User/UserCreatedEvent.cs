using MongoDB.Bson;
using SilverKinetics.w80.Common.Security;

namespace SilverKinetics.w80.Domain.Events.User;

public class UserCreatedEvent
    : EventBase<UserCreatedEvent>
{
    public UserCreatedEvent(ObjectId createdBy, DateTime createdUTC, Entities.User user, RequestSourceInfo requestSourceInfo)
        : base(createdBy, createdUTC, nameof(User), user.Id, requestSourceInfo) {}
}
