using MongoDB.Bson;

namespace SilverKinetics.w80.Domain.Contracts;

public interface IApplicationAlertsService
{
    Task<IDictionary<ObjectId, List<Guid>>> SendScheduleEmailAlertsAsync(CancellationToken cancellationToken = default);
}