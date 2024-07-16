using MongoDB.Bson;
using SilverKinetics.w80.Domain.Contracts;

namespace SilverKinetics.w80.Domain.Entities;

public class SystemState
    : IAggregateRoot
{
    public ObjectId Id { get; set; }
    public DateTime LastStatisticsRunUTC { get; set; }
}