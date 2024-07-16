using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Domain.Contracts;

namespace SilverKinetics.w80.Domain.Entities;

public class Statistics
    : IVersionedEntity,
    IAggregateRoot
{
    public ObjectId Id { get; set; }

    [BsonDictionaryOptions(MongoDB.Bson.Serialization.Options.DictionaryRepresentation.ArrayOfArrays)]
    public Dictionary<ObjectId, int> ApplicationRejectionStateCounts = [];

    private Statistics() {}
    public static Statistics Create(ObjectId userId)
    {
        return new Statistics() { Id = userId };
    }

    #region [ IVersionedEntity ]

    int IVersionedEntity.Version { get { return 1; } }

    #endregion
}
