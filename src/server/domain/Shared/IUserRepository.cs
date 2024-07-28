using System.Linq.Expressions;
using MongoDB.Driver;
using SilverKinetics.w80.Domain.Entities;

namespace SilverKinetics.w80.Domain.Shared;

public interface IUserRepository
{
    public bool QueryFiltersEnabled { set; }

    Task<bool> AnyAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken);
    Task<User> FirstAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken);
    Task<User?> FirstOrDefaultAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken);
    Task<IEnumerable<User>> GetManyAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken);

    public Task UpsertAsync(
        User user,
        User? current,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null);

    public Task DeactivateAsync(
        User user,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null);
}