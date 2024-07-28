using System.Linq.Expressions;

namespace SilverKinetics.w80.Domain.Shared;

public interface IGenericReadOnlyRepository<T>
    where T : class
{
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
    Task<IEnumerable<T>> GetManyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
}