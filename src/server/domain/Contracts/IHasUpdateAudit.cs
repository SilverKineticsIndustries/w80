using MongoDB.Bson;

namespace SilverKinetics.w80.Domain.Contracts;

public interface IHasUpdateAudit
{
    DateTime? UpdatedUTC { get; set; }
    ObjectId? UpdatedBy { get; set; }
}