using System.Globalization;
using SilverKinetics.w80.Common;
using SilverKinetics.w80.Domain.Shared;
using SilverKinetics.w80.Domain.Entities;
using SilverKinetics.w80.Application.DTOs;
using SilverKinetics.w80.Application.Mappers;
using SilverKinetics.w80.Application.Contracts;
using SilverKinetics.w80.Domain.Contracts;

namespace SilverKinetics.w80.Application.Services;

public class OptionsApplicationService(
    ISecurityContext securityContext,
    IGenericReadOnlyRepository<Industry> industryRepo)
    : IOptionsApplicationService
{
    public async Task<IList<GenericNameValueStringDto>> GetIndustriesAsync(CancellationToken cancellationToken)
    {
        var data = await industryRepo.GetManyAsync(x => x.DeactivatedUTC == null, cancellationToken).ConfigureAwait(false);
        return
            data.ToDictionary(x => x.Id.ToString(), x => x)
                .ToNameValueDtoList<Industry>((x) => x.GetString(nameof(Industry.Name), securityContext.Language));
    }

    public IList<GenericNameValueStringDto> GetSupportedCultures()
    {
        return SupportedCultures.Cultures.ToNameValueDtoList<CultureInfo>((x) => x.DisplayName);
    }

    public IList<GenericNameValueStringDto> GetSupportedTimezones()
    {
        return SupportedTimeZones.TimeZones.ToNameValueDtoList<TimeZoneInfo>((x) => x.Id);
    }
}