using SilverKinetics.w80.Application.DTOs;

namespace SilverKinetics.w80.Application.Contracts;

public interface IOptionsApplicationService
{
    IList<GenericNameValueStringDto> GetSupportedCultures();
    IList<GenericNameValueStringDto> GetSupportedTimezones();
    Task<IList<GenericNameValueStringDto>> GetIndustriesAsync(CancellationToken cancellationToken);
}