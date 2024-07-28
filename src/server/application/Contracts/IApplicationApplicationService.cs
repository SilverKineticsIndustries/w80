using MongoDB.Bson;
using SilverKinetics.w80.Application.DTOs;

namespace SilverKinetics.w80.Application.Contracts;

public interface IApplicationApplicationService
{
    Task<ApplicationViewDto> InitializeAsync(CancellationToken cancellationToken = default);
    Task<ApplicationViewDto?> GetAsync(ObjectId id, CancellationToken cancellationToken = default);
    Task<ComplexResponseDto<ApplicationViewDto>> DeactivateAsync(ObjectId id, CancellationToken cancellationToken = default);
    Task<ComplexResponseDto<ApplicationViewDto>> ReactivateAsync(ObjectId id, CancellationToken cancellationToken = default);
    Task<ComplexResponseDto<ApplicationViewDto>> ArchiveAsync(ObjectId id, CancellationToken cancellationToken = default);
    Task<ComplexResponseDto<ApplicationViewDto>> RejectAsync(ObjectId id, RejectionDto rejection, CancellationToken cancellationToken = default);
    Task<ComplexResponseDto<ApplicationViewDto>> AcceptAsync(ObjectId id, AcceptanceDto acceptance, CancellationToken cancellationToken = default);
    Task<ComplexResponseDto<ApplicationViewDto>> UnarchiveAsync(ObjectId id, CancellationToken cancellationToken = default);
    Task<IList<ApplicationViewDto>> GetForUser(ObjectId userId, bool includeDeactivated = false, CancellationToken cancellationToken = default);
    Task<IList<ValidationItemDto>> ValidateAsync(ApplicationUpdateRequestDto application, CancellationToken cancellationToken = default);
    Task<ComplexResponseDto<ApplicationViewDto>> UpsertAsync(ApplicationUpdateRequestDto application, CancellationToken cancellationToken = default);
    Task SetBrowserNotificationSentAsync(IDictionary<string, List<Guid>> evnts, CancellationToken cancellationToken = default);
}
