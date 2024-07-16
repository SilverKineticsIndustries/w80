using MongoDB.Bson;
using SilverKinetics.w80.Domain.Events;
using SilverKinetics.w80.Common.Security;
using SilverKinetics.w80.Domain.Entities;

namespace SilverKinetics.w80.Application.Events;

public class UserLoggedInEvent
    : EventBase<UserLoggedInEvent>
{
    public UserLoggedInEvent(ObjectId createdBy, DateTime createdUTC, RequestSourceInfo requestSourceInfo)
        : base(createdBy, createdUTC, nameof(User), createdBy)
    {
        KeyProps.Add(nameof(RequestSourceInfo.IP), requestSourceInfo.IP);
        KeyProps.Add(nameof(RequestSourceInfo.Host), requestSourceInfo.Host);
        KeyProps.Add(nameof(RequestSourceInfo.Headers), requestSourceInfo.Headers);
    }
}