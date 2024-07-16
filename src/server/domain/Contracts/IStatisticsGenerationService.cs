using SilverKinetics.w80.Domain.Entities;

namespace SilverKinetics.w80.Domain.Contracts;

public interface IStatisticsGenerationService
{
    Task<IEnumerable<Statistics>> UpdateStatisticsAsync(SystemState systemState, CancellationToken cancellationToken);
}