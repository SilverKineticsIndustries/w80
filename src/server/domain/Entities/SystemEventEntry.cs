using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SilverKinetics.w80.Common;
using SilverKinetics.w80.Domain.Contracts;

namespace SilverKinetics.w80.Domain.Entities;

public class SystemEventEntry
    : IHasCreationAudit,
    IAggregateRoot
{
    public ObjectId Id { get; private set; }
    public string? EntityName { get; private set; }
    public ObjectId? EntityId { get; private set; }
    public string EventName { get; private set; }
    public string FullyQualifiedEventType { get; private set; }
    public ObjectId CreatedBy { get; set; }
    public DateTime CreatedUTC { get; set; }
    public BsonDocument Payload { get; private set; }

    [BsonDictionaryOptions(MongoDB.Bson.Serialization.Options.DictionaryRepresentation.Document)]
    public Dictionary<string,object> KeyProps { get; private set; } = [];

    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public SystemEventEntry(ISystemEvent evnt)
    #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        var type = evnt.GetType();

        Id = ObjectId.GenerateNewId();
        CreatedBy = evnt.CreatedBy;
        CreatedUTC = evnt.CreatedUTC.IsMaxOrMinValue() ? DateTime.UtcNow : evnt.CreatedUTC;
        EventName = type.Name;

        #pragma warning disable CS8601 // Possible null reference assignment.
        FullyQualifiedEventType = type.FullName;
        #pragma warning restore CS8601 // Possible null reference assignment.

        EntityId = evnt.EntityId;
        EntityName = evnt.EntityName;

        if (evnt.KeyProps != null)
            KeyProps = evnt.KeyProps.ToDictionary(x => x.Key, x => x.Value);

        Payload = evnt.ToBsonDocument(nominalType: type);
    }
}