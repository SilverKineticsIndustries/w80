using Microsoft.Extensions.Localization;
using Riok.Mapperly.Abstractions;
using SilverKinetics.w80.Application.DTOs;
using SilverKinetics.w80.Domain.Entities;

namespace SilverKinetics.w80.Application.Mappers;

[Mapper(UseDeepCloning = true, RequiredMappingStrategy = RequiredMappingStrategy.Both)]
public partial class StatisticsMapper
{
    public static StatisticsDto ToDTO(Statistics statistics, IEnumerable<ApplicationState> applicationStates, IStringLocalizer stringLocalizer)
    {
        var states = applicationStates.ToDictionary(x => x.Id, x => x);
        StatisticsDto dto = new()
        {
            ApplicationRejectionStateCounts
                = statistics
                    .ApplicationRejectionStateCounts
                    .Select(x =>
                    {
                        var state = states.TryGetValue(x.Key, out ApplicationState? value) ? value : null;
                        return
                            new ApplicationStateCountsDto(
                                state != null ? state.Name : stringLocalizer["Unknown"],
                                state != null ? state.HexColor : "FFFFFF",
                                x.Value
                            );
                    }
                )
                .ToList()
        };
        return dto;
    }
}