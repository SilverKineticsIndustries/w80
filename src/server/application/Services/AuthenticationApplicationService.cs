using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SilverKinetics.w80.Common;
using SilverKinetics.w80.Domain.Shared;
using SilverKinetics.w80.Common.Security;
using SilverKinetics.w80.Domain.Entities;
using SilverKinetics.w80.Domain.Contracts;
using SilverKinetics.w80.Application.DTOs;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Application.Security;
using SilverKinetics.w80.Application.Contracts;
using SilverKinetics.w80.Common.Configuration;
using SilverKinetics.w80.Application.Events;

namespace SilverKinetics.w80.Application.Services;

public class AuthenticationApplicationService(
    IConfiguration config,
    IMongoClient mongoClient,
    IDateTimeProvider dateTimeProvider,
    IEmailMessageGenerator userEmailGenerator,
    IEmailNotificationService emailNotificationService,
    IUserRepository userRepo,
    IUserSecurityRepository userSecurityRepo,
    ISystemEventSink systemEventSink,
    IEmailMessageGenerator emailMessageGenerator,
    IStringLocalizer<Common.Resource.Resources> stringLocalizer)
    : IAuthenticationApplicationService
{
    public async Task<LoginResponseDto> LoginWithCredentialsAsync(
        LoginRequestDto loginRequest,
        ICookieManager cookieManager,
        RequestSourceInfo requestSourceInfo,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(loginRequest.Email))
            throw new ArgumentNullException(nameof(LoginRequestDto.Email));
        if (string.IsNullOrWhiteSpace(loginRequest.Password))
            throw new ArgumentNullException(nameof(LoginRequestDto.Password));

        var response = new LoginResponseDto() { Success = false };

        var user = await userRepo.FirstOrDefaultAsync(x => x.Email == loginRequest.Email, cancellationToken).ConfigureAwait(false);
        if (user == null)
            return response;
        else if (user.IsDeactivated())
        {
            response.InfoMessage = stringLocalizer["User is deactivated."];
            return response;
        }

        var userSecurity = await userSecurityRepo.FirstAsync((x) => x.Id == user.Id, cancellationToken).ConfigureAwait(false);

        var now = dateTimeProvider.GetUtcNow();

        if (!userSecurity.CanLogin() || !Passwords.ArePasswordEqual(userSecurity, loginRequest.Password))
        {
            return response;
        }
        else if (!userSecurity.IsEmailOwnershipConfirmed)
        {
            if (!loginRequest.ResendEmailConfirmation)
            {
                response.InfoMessage = stringLocalizer["You must confirm email ownership before logging in."];
                return response;
            }
            else
            {
                var token = Convert.ToBase64String(EmailConfirmations.GenerateNew(config, user.Email, now));
                var email = userEmailGenerator.GetEmailAccountOwnershipVerificationEmailMessage(user, token);
                await emailNotificationService.SendAsync([email], cancellationToken);

                response.EmailConfirmationSent = true;
                return response;
            }
        }

        await mongoClient.WrapInTransactionAsync(
            async (session) =>
            {
                systemEventSink.Add(new UserLoggedInEvent(user.Id, now, requestSourceInfo));
                response.Success = true;
                response.AccessToken = GenerateAccessToken(user, now);
                response.RefreshToken = GenerateRefreshToken(userSecurity, now);
                response.RefreshTokenExpirationUTC = userSecurity.RefreshTokenExpirationUTC ?? null;
                await userSecurityRepo.UpsertAsync(userSecurity, cancellationToken, session);
                cookieManager.SetCookie(Tokens.RefreshTokenCookieName, response.RefreshToken, response.RefreshTokenExpirationUTC, includePath: true);
            },
        cancellationToken);

        return response;
    }

    public async Task<LogoutResponseDto> LogoutAsync(
        ICookieManager cookieManager,
        RequestSourceInfo requestSourceInfo,
        CancellationToken cancellationToken)
    {
        var response = new LogoutResponseDto();
        var refreshToken = cookieManager.GetCookie(Tokens.RefreshTokenCookieName);
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            response.InfoMessage = stringLocalizer["Refresh token missing."];
            return response;
        }

        var hashedRefreshTokenEncoded = Tokens.GetRefreshTokenHashInBase64(refreshToken);
        var userSecurity = await userSecurityRepo.FirstOrDefaultAsync((x) => x.RefreshTokenHash == hashedRefreshTokenEncoded, cancellationToken).ConfigureAwait(false);
        if (userSecurity != null)
        {
            userSecurity.InvalidateRefreshToken();
            await userSecurityRepo.UpsertAsync(userSecurity, cancellationToken);
        }
        cookieManager.RemoveCookie(Tokens.RefreshTokenCookieName);

        response.Success = true;
        return response;
    }

    public async Task<RefreshResponseDto> LoginWithRefreshTokenAsync(
        RefreshRequestDto refreshRequest,
        ICookieManager cookieManager,
        RequestSourceInfo requestSourceInfo,
        CancellationToken cancellationToken)
    {
        var response = new RefreshResponseDto() { Success = false };
        if (string.IsNullOrWhiteSpace(refreshRequest.RefreshToken))
        {
            response.InfoMessage = stringLocalizer["Refresh token missing."];
            return response;
        }

        var now = dateTimeProvider.GetUtcNow();
        var refreshTokenHash = Tokens.GetRefreshTokenHashInBase64(refreshRequest.RefreshToken);
        var userSecurity = await userSecurityRepo.FirstOrDefaultAsync((x) => x.RefreshTokenHash == refreshTokenHash, cancellationToken).ConfigureAwait(false);
        if (userSecurity == null || now > userSecurity.RefreshTokenExpirationUTC)
            return response;

        var user = await userRepo.FirstOrDefaultAsync(x => x.Id == userSecurity.Id, cancellationToken).ConfigureAwait(false);
        if (user == null)
            return response;
        else if (user.IsDeactivated())
        {
            response.InfoMessage = stringLocalizer["User is deactivated."];
            return response;
        }

        response.Success = true;
        response.AccessToken = GenerateAccessToken(user, now);
        response.RefreshToken = GenerateRefreshToken(userSecurity, now);
        response.RefreshTokenExpirationUTC = userSecurity.RefreshTokenExpirationUTC ?? null;
        await userSecurityRepo.UpsertAsync(userSecurity, cancellationToken);

        cookieManager.SetCookie(
            Tokens.RefreshTokenCookieName,
            response.RefreshToken,
            expirationDate: response.RefreshTokenExpirationUTC,
            includePath: true);

        return response;
    }

    public async Task<InvitationProcessResponseDto> ProcessInvitationAsync(
        InvitationProcessRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException(nameof(InvitationProcessRequestDto.Email));
        if (string.IsNullOrWhiteSpace(request.InvitationCode))
            throw new ArgumentNullException(nameof(InvitationProcessRequestDto.InvitationCode));
        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentNullException(nameof(InvitationProcessRequestDto.Password));

        string email;
        DateTime utcDateTime;
        var now = dateTimeProvider.GetUtcNow();

        try
        {
            Invitations.Decrypt(config, request.InvitationCode, out email, out utcDateTime);
        }
        catch
        {
            return
                new InvitationProcessResponseDto() {
                    Success = false,
                    InfoMessages = [stringLocalizer["Invalid invitation code."]]
                };
        }

        if (string.IsNullOrWhiteSpace(email) || utcDateTime.IsMaxOrMinValue() || request.Email.ToLowerInvariant() != email.ToLowerInvariant())
            return
                new InvitationProcessResponseDto() {
                    Success = false,
                    InfoMessages = [stringLocalizer["Invalid invitation code."]]
                };

        if (Invitations.IsExpired(config, utcDateTime, now))
            return
                new InvitationProcessResponseDto() {
                    Success = false,
                    InfoMessages = [stringLocalizer["Invitation code has expired."]]
                };

        User? user = await userRepo.FirstOrDefaultAsync((x) => x.Email.ToLower() == email.ToLower(), cancellationToken).ConfigureAwait(false);
        if (user == null)
            return
                new InvitationProcessResponseDto() {
                    Success = false,
                    InfoMessages = [stringLocalizer["Invalid invitation code."]]
                };

        if (user.IsDeactivated())
            return
                new InvitationProcessResponseDto() {
                    Success = false,
                    InfoMessages = [stringLocalizer["User is deactivated."]]
                };

        var userSecurity = await userSecurityRepo.FirstAsync((x) => x.Id == user.Id, cancellationToken).ConfigureAwait(false);
        if (!userSecurity.MustActivateWithInvitationCode)
            return
                new InvitationProcessResponseDto() {
                    Success = false,
                    InfoMessages = [stringLocalizer["Invitation code already used."]]
                };

        var passwordValidationBag = Passwords.Validate(config, stringLocalizer, request.Password, userSecurity);
        if (passwordValidationBag.HasErrors)
        {
            return
                new InvitationProcessResponseDto() {
                    Success = false,
                    InfoMessages = passwordValidationBag.Select(x => x.Message).ToArray()
                };
        }

        userSecurity.SetPassword(request.Password);
        userSecurity.MustActivateWithInvitationCode = false;
        userSecurity.IsEmailOwnershipConfirmed = false;
        await userSecurityRepo.UpsertAsync(userSecurity, cancellationToken);

        var token = Convert.ToBase64String(EmailConfirmations.GenerateNew(config, user.Email, now));
        var emailMessage = emailMessageGenerator.GetEmailAccountOwnershipVerificationEmailMessage(user, token);
        await emailNotificationService.SendAsync([emailMessage], cancellationToken);

        return new InvitationProcessResponseDto() {
            Success = true
        };
    }

    public async Task<EmailConfirmationResponseDto> ProcessEmailConfirmationTokenAsync(
        EmailConfirmationRequestDto request,
        RequestSourceInfo requestSource,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentNullException(nameof(request.Code));

        string email;
        DateTime utcDateTime;
        var now = dateTimeProvider.GetUtcNow();

        try
        {
            EmailConfirmations.Decrypt(config, request.Code, out email, out utcDateTime);
        }
        catch
        {
            return
                new EmailConfirmationResponseDto() {
                    Success = false,
                    InfoMessages = [stringLocalizer["Invalid email confirmation token."]]
                };
        }

        if (string.IsNullOrWhiteSpace(email) || utcDateTime.IsMaxOrMinValue())
            return
                new EmailConfirmationResponseDto() {
                    Success = false,
                    InfoMessages = [stringLocalizer["Invalid email confirmation token."]]
                };

        if (EmailConfirmations.IsExpired(config, utcDateTime, now))
            return
                new EmailConfirmationResponseDto() {
                    Success = false,
                    InfoMessages = [stringLocalizer["Email confirmation token has expired."]]
                };

        var user = await userRepo.FirstOrDefaultAsync((x) => x.Email.ToLower() == email.ToLower(), cancellationToken).ConfigureAwait(false);
        if (user == null)
            return
                new EmailConfirmationResponseDto() {
                    Success = false,
                    InfoMessages = [stringLocalizer["Invalid email confirmation token."]]
                };

        if (user.IsDeactivated())
            return
                new EmailConfirmationResponseDto() {
                    Success = false,
                    InfoMessages = [stringLocalizer["User is deactivated."]]
                };

        var userSecurity = await userSecurityRepo.FirstAsync((x) => x.Id == user.Id, cancellationToken).ConfigureAwait(false);
        if (userSecurity.MustActivateWithInvitationCode)
            return
                new EmailConfirmationResponseDto() {
                    Success = false,
                    InfoMessages = [stringLocalizer["User has not been activated."]]
                };

        if (userSecurity.IsEmailOwnershipConfirmed)
            return
                new EmailConfirmationResponseDto() {
                    Success = false,
                    InfoMessages = [stringLocalizer["User's email has already been confirmed."]]
                };

        userSecurity.IsEmailOwnershipConfirmed = true;
        await userSecurityRepo.UpsertAsync(userSecurity, cancellationToken);

        return new EmailConfirmationResponseDto() { Success = true };
    }

    public InvitationGenerationResponseDto GenerateInvitationCode(
        InvitationGenerationRequestDto request)
    {
        var res = new InvitationGenerationResponseDto();

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentNullException(nameof(request.Email));

        try
        {
            var code = Invitations.GenerateNew(config, request.Email, dateTimeProvider.GetUtcNow());
            res.Success = true;
            res.Code = Convert.ToBase64String(code);
        } catch {
            res.Success = false;
        }

        return res;
    }

    public async Task<EmailConfirmationGenerationResponseDto> ResendEmailConfirmationAsync(
        EmailConfirmationGenerationRequestDto request, CancellationToken cancellationToken)
    {
        var res = new EmailConfirmationGenerationResponseDto();
        try
        {
            var user = await userRepo.FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken).ConfigureAwait(false);
            if (user != null && !user.IsDeactivated())
            {
                var token = Convert.ToBase64String(EmailConfirmations.GenerateNew(config, user.Email, dateTimeProvider.GetUtcNow()));
                var emailMessage = emailMessageGenerator.GetEmailAccountOwnershipVerificationEmailMessage(user, token);
                await emailNotificationService.SendAsync([emailMessage], cancellationToken);
                res.Success = true;
            }

        } catch {
            res.Success = false;
        }
        return res;
    }

    private string GenerateAccessToken(User user, DateTime utcNow)
    {
        return Tokens.EncodeJwtToken(config, utcNow, Claims.GetClaims(user));
    }

    private string GenerateRefreshToken(UserSecurity userSecurity, DateTime utcNow)
    {
        // We return the token and store the hash in the db
        var refreshToken = Tokens.GenerateRefreshToken();

        var hashedRefreshToken = Convert.ToBase64String(Hash.Sha256AsBytes(refreshToken));
        userSecurity.SetRefreshToken(
            hashedRefreshToken,
            utcNow.AddDays(Convert.ToInt32(config[Keys.JwtRefreshLifetimeInDays]))
        );
        return Convert.ToBase64String(refreshToken);
    }
}