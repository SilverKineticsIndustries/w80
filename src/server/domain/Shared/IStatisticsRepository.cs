using System.Linq.Expressions;
using MongoDB.Driver;
using SilverKinetics.w80.Domain.Entities;

namespace SilverKinetics.w80.Domain.Shared;

public interface IStatisticsRepository
{
    Task<Statistics?> GetSingleOrDefaultAsync(Expression<Func<Statistics, bool>> predicate, CancellationToken cancellationToken);
    Task<IEnumerable<Statistics>> GetManyAsync(Expression<Func<Statistics, bool>> predicate, CancellationToken cancellationToken);

    public Task UpdateAsync(
        Statistics statistics,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null);
}