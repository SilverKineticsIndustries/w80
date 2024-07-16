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

    public static SystemEventEntry Create(ISystemEvent evnt)
    {
        var entry = new SystemEventEntry();
        entry.Id = ObjectId.GenerateNewId();
        entry.CreatedBy = evnt.CreatedBy;
        entry.CreatedUTC = evnt.CreatedUTC.IsMaxOrMinValue() ? DateTime.UtcNow : evnt.CreatedUTC;

        var type = evnt.GetType();
        entry.EventName = type.Name;
        entry.FullyQualifiedEventType = type.FullName;
        entry.EntityId = evnt.EntityId;
        entry.EntityName = evnt.EntityName;

        if (evnt.KeyProps != null)
            entry.KeyProps = evnt.KeyProps.ToDictionary(x => x.Key, x => x.Value);

        entry.Payload = evnt.ToBsonDocument(nominalType: type);
        return entry;
    }
}