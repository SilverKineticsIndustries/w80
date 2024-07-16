using MongoDB.Driver;
using SilverKinetics.w80.Domain.Entities;

namespace SilverKinetics.w80.Domain.Shared;

public interface ISystemStateRepository
{
    public Task<SystemState?> GetSingleOrDefaultAsync(CancellationToken cancellationToken);
    public Task UpdateAsync(SystemState systemState, CancellationToken cancellationToken, IClientSessionHandle? session = null);
}