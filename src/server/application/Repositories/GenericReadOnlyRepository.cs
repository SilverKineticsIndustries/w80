using MongoDB.Driver;
using SilverKinetics.w80.Domain.Shared;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Domain.Contracts;

namespace SilverKinetics.w80.Application.Repositories;

public class GenericReadOnlyRepository<T>(
    ISecurityContext securityContext,
    IDateTimeProvider dateTimeProvider,
    IMongoCollection<T> set)
    : BaseRepository<T>(securityContext, dateTimeProvider, set),
        IGenericReadOnlyRepository<T>
    where T : class, IAggregateRoot
{}