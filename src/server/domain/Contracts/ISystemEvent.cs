using MongoDB.Bson;

namespace SilverKinetics.w80.Domain.Contracts;

public interface ISystemEvent
    : IHasCreationAudit
{
    ObjectId? EntityId { get; set; }
    string? EntityName { get; set; }
    IDictionary<string, object> KeyProps { get; }
}

public interface ISystemEvent<T>
    : ISystemEvent { }
