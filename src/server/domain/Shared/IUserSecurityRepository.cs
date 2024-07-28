using System.Linq.Expressions;
using MongoDB.Driver;
using SilverKinetics.w80.Domain.Entities;

namespace SilverKinetics.w80.Domain.Shared;

public interface IUserSecurityRepository
{
    Task<bool> AnyAsync(Expression<Func<UserSecurity, bool>> predicate, CancellationToken cancellationToken);
    Task<UserSecurity> FirstAsync(Expression<Func<UserSecurity, bool>> predicate, CancellationToken cancellationToken);
    Task<UserSecurity?> FirstOrDefaultAsync(Expression<Func<UserSecurity, bool>> predicate, CancellationToken cancellationToken);
    Task<IEnumerable<UserSecurity>> GetManyAsync(Expression<Func<UserSecurity, bool>> predicate, CancellationToken cancellationToken);

    public Task UpsertAsync(
        UserSecurity userSecurity,
        CancellationToken cancellationToken,
        IClientSessionHandle? session = null);
}