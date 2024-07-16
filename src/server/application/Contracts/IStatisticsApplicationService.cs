using MongoDB.Bson;
using SilverKinetics.w80.Application.DTOs;

namespace SilverKinetics.w80.Application.Contracts;

public interface IStatisticsApplicationService
{
    Task<StatisticsDto?> GetAsync(ObjectId userId, CancellationToken cancellationToken);
}