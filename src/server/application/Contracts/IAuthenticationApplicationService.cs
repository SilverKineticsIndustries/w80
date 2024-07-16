using SilverKinetics.w80.Common.Security;
using SilverKinetics.w80.Application.DTOs;

namespace SilverKinetics.w80.Application.Contracts;

public interface IAuthenticationApplicationService
{
    Task<LoginResponseDto> LoginWithCredentialsAsync(
        LoginRequestDto loginRequest, ICookieManager cookieManager, RequestSourceInfo requestSourceInfo, CancellationToken cancellationToken);
    Task<RefreshResponseDto> LoginWithRefreshTokenAsync(
        RefreshRequestDto refreshRequest, ICookieManager cookieManager, RequestSourceInfo requestSourceInfo, CancellationToken cancellationToken);
    Task<LogoutResponseDto> LogoutAsync(
        ICookieManager cookieManager, RequestSourceInfo requestSourceInfo, CancellationToken cancellationToken);
    Task<EmailConfirmationResponseDto> ProcessEmailConfirmationTokenAsync(
        EmailConfirmationRequestDto request, RequestSourceInfo requestSourceInfo, CancellationToken cancellationToken);
    Task<InvitationProcessResponseDto> ProcessInvitationAsync(
        InvitationProcessRequestDto request, CancellationToken cancellationToken);
    Task<EmailConfirmationGenerationResponseDto> ResendEmailConfirmationAsync(
        EmailConfirmationGenerationRequestDto request, CancellationToken cancellationToken);
    InvitationGenerationResponseDto GenerateInvitationCode(
        InvitationGenerationRequestDto request);
}