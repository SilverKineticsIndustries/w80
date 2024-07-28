using MongoDB.Bson;
using Microsoft.Extensions.Localization;
using SilverKinetics.w80.Domain.Shared;
using SilverKinetics.w80.Domain.Entities;
using SilverKinetics.w80.Application.DTOs;
using SilverKinetics.w80.Domain.Contracts;
using SilverKinetics.w80.Application.Mappers;
using SilverKinetics.w80.Application.Contracts;
using SilverKinetics.w80.Application.Exceptions;

namespace SilverKinetics.w80.Application.Services;

public class StatisticsApplicationService(
    IStatisticsRepository statisticsRepository,
    IGenericReadOnlyRepository<ApplicationState> applicationStateRepo,
    IStringLocalizer<Common.Resource.Resources> stringLocalizer,
    ISecurityContext securityContext
) : IStatisticsApplicationService
{
    public async Task<StatisticsDto?> GetAsync(ObjectId userId, CancellationToken cancellationToken)
    {
        var stats = await statisticsRepository.FirstOrDefaultAsync((x) => x.Id == userId, cancellationToken).ConfigureAwait(false);
        if (stats is not null)
        {
            if (!securityContext.CanAccess(stats.Id))
                throw new AuthorizationException();

            var states = await applicationStateRepo.GetManyAsync((x) => 1 == 1, cancellationToken);
            return StatisticsMapper.ToDTO(stats, states, stringLocalizer);
        }
        else
            return new StatisticsDto();
    }
}