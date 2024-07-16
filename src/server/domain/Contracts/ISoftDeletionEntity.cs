using MongoDB.Bson;

namespace SilverKinetics.w80.Domain.Contracts;

public interface ISoftDeletionEntity
{
    DateTime? DeactivatedUTC { set; get; }
    ObjectId? DeactivatedBy { set; get; }
}