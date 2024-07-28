using MongoDB.Bson;
using SilverKinetics.w80.Application.DTOs;
using SilverKinetics.w80.Common.Security;

namespace SilverKinetics.w80.Application.Contracts;

public interface IUserApplicationService
{
    Task<IList<UserViewDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<UserProfileViewDto?> GetProfileAsync(ObjectId id, CancellationToken cancellationToken);
    Task<UserProfileViewDto?> GetProfileFromEmailAsync(string email, CancellationToken cancellationToken);
    Task<ComplexResponseDto<UserProfileViewDto>> UpdateProfileAsync(UserProfileUpdateRequestDto userProfile, RequestSourceInfo requestSourceInfo, CancellationToken cancellationToken );
    Task<ComplexResponseDto<UserViewDto>> UpsertAsync(UserUpsertRequestDto user, RequestSourceInfo requestSourceInfo, CancellationToken cancellationToken);
    Task<ComplexResponseDto<UserViewDto>> DeactivateAsync(ObjectId id, RequestSourceInfo requestSourceInfo, CancellationToken cancellationToken = default);
}