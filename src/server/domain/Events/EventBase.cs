using MongoDB.Bson;
using SilverKinetics.w80.Common.Security;
using SilverKinetics.w80.Domain.Contracts;

namespace SilverKinetics.w80.Domain.Events;

public abstract class EventBase<T>
    : ISystemEvent<T>
{
    public ObjectId CreatedBy { get; set; }
    public DateTime CreatedUTC { get; set; }
    public ObjectId? EntityId { get; set; }
    public string? EntityName { get; set; }
    public IDictionary<string, object> KeyProps { get; } = new Dictionary<string, object>();

    public EventBase(ObjectId createdBy, DateTime createdUTC)
    {
        CreatedBy = createdBy;
        CreatedUTC = createdUTC;
    }

    public EventBase(ObjectId createdBy, DateTime createdUTC, RequestSourceInfo requestSourceInfo)
        : this(createdBy, createdUTC)
    {
        KeyProps.Add(PropSourceIP, requestSourceInfo.IP);
        KeyProps.Add(PropSourceHost, requestSourceInfo.Host);
        KeyProps.Add(PropSourceHeaders, requestSourceInfo.Headers);
    }

    public EventBase(ObjectId createdBy, DateTime createdUTC, string entityName, ObjectId entityId)
        : this(createdBy, createdUTC)
    {
        EntityId = entityId;
        EntityName = entityName;
    }

    public EventBase(ObjectId createdBy, DateTime createdUTC, string entityName, ObjectId entityId, RequestSourceInfo requestSourceInfo)
        : this(createdBy, createdUTC, requestSourceInfo)
    {
        EntityId = entityId;
        EntityName = entityName;
    }

    public const string PropSourceIP = "SourceIP";
    public const string PropSourceHost = "SourceHost";
    public const string PropSourceHeaders = "SourceHeaders";
}
