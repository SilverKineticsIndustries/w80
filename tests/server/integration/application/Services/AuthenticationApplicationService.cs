using System.Text;
using SilverKinetics.w80.Application.Events;
using SilverKinetics.w80.Notifications.Contracts;

namespace SilverKinetics.w80.Application.IntegrationTests.Services;

[TestFixture(TestOf = typeof(Application.Services.AuthenticationApplicationService))]
public class AuthenticationApplicationService
{
    [Test]
    public async Task LoginWithCredentialsAsync_emptyUserName_shouldNotAllowLogin()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var request = new LoginRequestDto();
            request.Email = string.Empty;
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await service.LoginWithCredentialsAsync(request, new CookieManagerFake(), RequestSourceInfo.Empty, CancellationToken.None);
            });
        }
    }

    [Test]
    public async Task LoginWithCredentialsAsync_filledUserNameAndEmptyPassword_shouldNotAllowLogin()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var request = new LoginRequestDto();
            request.Email = ctx.GetTestUserEmail();
            request.Password = string.Empty;
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await service.LoginWithCredentialsAsync(request, new CookieManagerFake(), RequestSourceInfo.Empty, CancellationToken.None);
            });
        }
    }

    [Test]
    public async Task LoginWithCredentialsAsync_validEmailAndInvalidPassword_shouldNotAllowLogin()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var request = new LoginRequestDto();
            request.Email = ctx.GetTestUserEmail();
            request.Password = "wrongpassword";
            var response = await service.LoginWithCredentialsAsync(request, new CookieManagerFake(), RequestSourceInfo.Empty, CancellationToken.None);
            Assert.That(response.Success, Is.False);
        }
    }

    [Test]
    public async Task LoginWithCredentialsAsync_invalidEmailAndValidPassword_shouldNotAllowLogin()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var request = new LoginRequestDto();
            request.Email = "invalidemail@silverkinetics.dev";
            request.Password = ctx.GetTestUserPassword();
            var response = await service.LoginWithCredentialsAsync(request, new CookieManagerFake(), RequestSourceInfo.Empty, CancellationToken.None);
            Assert.That(response.Success, Is.False);
        }
    }

    [Test]
    public async Task LoginWithCredentialsAsync_validEmailAndValidPassword_shouldAllowLogin()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var request = new LoginRequestDto();
            request.Email = ctx.GetTestUserEmail();
            request.Password = ctx.GetTestUserPassword();
            var response = await service.LoginWithCredentialsAsync(request, new CookieManagerFake(), RequestSourceInfo.Empty, CancellationToken.None);
            Assert.That(response.Success, Is.True);
        }
    }

    [Test]
    public async Task LoginWithCredentialsAsync_validEmailAndValidPassword_shouldReturnAccessToken()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var request = new LoginRequestDto();
            request.Email = ctx.GetTestUserEmail();
            request.Password = ctx.GetTestUserPassword();
            var response = await service.LoginWithCredentialsAsync(request, new CookieManagerFake(), RequestSourceInfo.Empty, CancellationToken.None);
            Assert.That(response.AccessToken, Is.Not.Empty);
        }
    }

    [Test]
    public async Task LoginWithCredentialsAsync_validEmailAndValidPassword_shouldReturnRefreshToken()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var request = new LoginRequestDto();
            request.Email = ctx.GetTestUserEmail();
            request.Password = ctx.GetTestUserPassword();
            var response = await service.LoginWithCredentialsAsync(request, new CookieManagerFake(), RequestSourceInfo.Empty, CancellationToken.None);
            Assert.That(response.RefreshToken, Is.Not.Empty);
        }
    }

    [Test]
    public async Task LoginWithCredentialsAsync_validEmailAndValidPassword_refreshTokenShouldBeInCookie()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var cookieManager = new CookieManagerFake();
            var request = new LoginRequestDto();
            request.Email = ctx.GetTestUserEmail();
            request.Password = ctx.GetTestUserPassword();
            await service.LoginWithCredentialsAsync(request, cookieManager, RequestSourceInfo.Empty, CancellationToken.None);
            Assert.That(cookieManager.GetCookie(
                Tokens.RefreshTokenCookieName),
                Is.Not.Empty);
        }
    }

    [Test]
    public async Task LoginWithCredentialsAsync_validEmailAndValidPassword_refreshTokenExpirationDateShouldBeAccordingToConfiguration()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var cookieManager = new CookieManagerFake();
            var request = new LoginRequestDto();
            request.Email = ctx.GetTestUserEmail();
            request.Password = ctx.GetTestUserPassword();
            var response = await service.LoginWithCredentialsAsync(request, cookieManager, RequestSourceInfo.Empty, CancellationToken.None);

            var config = ctx.Services.GetRequiredService<IConfiguration>();
            var dateTimeProvider = ctx.Services.GetRequiredService<IDateTimeProvider>();
            var expirationDate = dateTimeProvider.GetUtcNow().AddDays(Convert.ToInt32(config[Keys.JwtRefreshLifetimeInDays]));
            Assert.That(response.RefreshTokenExpirationUTC, Is.EqualTo(expirationDate).Within(1).Minutes);
        }
    }

    [Test]
    public async Task LoginWithCredentialsAsync_validEmailAndValidPassword_accessTokenExpirationDateShouldBeAccordingToConfiguration()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var cookieManager = new CookieManagerFake();
            var request = new LoginRequestDto();
            request.Email = ctx.GetTestUserEmail();
            request.Password = ctx.GetTestUserPassword();
            var response = await service.LoginWithCredentialsAsync(request, cookieManager, RequestSourceInfo.Empty, CancellationToken.None);
            var accessTokenDecoded = Tokens.DecodeJwtToken(response?.AccessToken ?? string.Empty);

            var config = ctx.Services.GetRequiredService<IConfiguration>();
            var dateTimeProvider = ctx.Services.GetRequiredService<IDateTimeProvider>();
            var expirationDate = dateTimeProvider.GetUtcNow().AddMinutes(Convert.ToInt32(config[Keys.JwtAccessLifetimeInMinutes]));

            Assert.That(accessTokenDecoded.ValidTo, Is.EqualTo(expirationDate).Within(1).Minutes);
        }
    }

    [Test]
    public async Task LoginWithCredentialsAsync_validEmailAndValidPassword_shouldReturnProperClaims()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var request = new LoginRequestDto();
            request.Email = ctx.GetTestUserEmail();
            request.Password = ctx.GetTestUserPassword();
            var response = await service.LoginWithCredentialsAsync(request, new CookieManagerFake(), RequestSourceInfo.Empty, CancellationToken.None);
            var tokenEncoded = response.AccessToken;
            if (string.IsNullOrWhiteSpace(tokenEncoded))
            {
                Assert.Fail();
                return;
            }

            var token = Tokens.DecodeJwtToken(tokenEncoded);
            var roleClaim = token.Claims.FirstOrDefault(x => x.Type == "Role");
            var emailClaim = token.Claims.FirstOrDefault(x => x.Type == "Email");
            var nicknameClaim = token.Claims.FirstOrDefault(x => x.Type == "Nickname");

            Assert.Multiple(() =>
            {
                    Assert.That(nicknameClaim?.Value, Is.EqualTo("testuser"));
                    Assert.That(roleClaim?.Value, Is.EqualTo(Role.User.ToString()));
                    Assert.That(emailClaim?.Value, Is.EqualTo(ctx.GetTestUserEmail()));
            });
        }
    }

    [Test]
    public async Task LoginWithCredentialsAsync_validEmailAndValidPassword_userLoggedInEventShouldBeCreated()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var request = new LoginRequestDto();
            request.Email = ctx.GetTestUserEmail();
            request.Password = ctx.GetTestUserPassword();
            var response = await service.LoginWithCredentialsAsync(request, new CookieManagerFake(), RequestSourceInfo.Empty, CancellationToken.None);
            Assume.That(response.AccessToken, Is.Not.Null);

            var repo = ctx.Services.GetRequiredService<ISystemEventEntryRepository>();
            var loggedInEvent = await repo.FirstOrDefaultAsync((x) => x.EventName == nameof(UserLoggedInEvent) && x.CreatedBy == ctx.GetTestUserID(), CancellationToken.None);
            Assert.That(loggedInEvent, Is.Not.Null);
        }
    }

    [Test]
    public async Task LoginWithCredentialsAsync_validCredentialsButDeactivatedUser_shouldNotAllowLogin()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var userProfileSet = ctx.Services.GetRequiredService<IMongoCollection<User>>();
            var userProfileRepo = ctx.Services.GetRequiredService<IUserRepository>();

            var user = await userProfileSet.AsQueryable().FirstAsync(x => x.Email == ctx.GetTestUserEmail());
            user.Deactivate(ctx.Services.GetRequiredService<IDateTimeProvider>());
            await userProfileRepo.UpsertAsync(user, user, CancellationToken.None);

            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var request = new LoginRequestDto();
            request.Email = ctx.GetTestUserEmail();
            request.Password = ctx.GetTestUserPassword();
            var response = await service.LoginWithCredentialsAsync(request, new CookieManagerFake(), RequestSourceInfo.Empty, CancellationToken.None);

            Assert.That(response.Success, Is.False);
        }
    }

    [Test]
    public async Task LoginWithCredentialsAsync_validCredentialsButUserRequiesEmailVerification_shouldNotAllowLogin()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var userProfileSet = ctx.Services.GetRequiredService<IMongoCollection<User>>();
            var userSecuritySet = ctx.Services.GetRequiredService<IMongoCollection<UserSecurity>>();
            var userSecurityRepo = ctx.Services.GetRequiredService<IUserSecurityRepository>();

            var user = await userProfileSet.AsQueryable().SingleAsync(x => x.Email == ctx.GetTestUserEmail());
            var userSecurity = await userSecuritySet.AsQueryable().SingleAsync(x => x.Id == user.Id);
            userSecurity.IsEmailOwnershipConfirmed = false;
            await userSecurityRepo.UpsertAsync(userSecurity, CancellationToken.None);

            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var request = new LoginRequestDto();
            request.Email = ctx.GetTestUserEmail();
            request.Password = ctx.GetTestUserPassword();
            var response = await service.LoginWithCredentialsAsync(request, new CookieManagerFake(), RequestSourceInfo.Empty, CancellationToken.None);

            Assert.That(response.Success, Is.False);
        }
    }

    [Test]
    public async Task LoginWithCredentialsAsync_validCredentialsAndUserRequiesEmailVerificationAndUserAskedForVerificationEmail_shouldNotAllowLogin()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var userProfileSet = ctx.Services.GetRequiredService<IMongoCollection<User>>();
            var userSecuritySet = ctx.Services.GetRequiredService<IMongoCollection<UserSecurity>>();
            var userSecurityRepo = ctx.Services.GetRequiredService<IUserSecurityRepository>();

            var user = await userProfileSet.AsQueryable().SingleAsync(x => x.Email == ctx.GetTestUserEmail());
            var userSecurity = await userSecuritySet.AsQueryable().SingleAsync(x => x.Id == user.Id);
            userSecurity.IsEmailOwnershipConfirmed = false;
            await userSecurityRepo.UpsertAsync(userSecurity, CancellationToken.None);

            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var request = new LoginRequestDto();
            request.Email = ctx.GetTestUserEmail();
            request.Password = ctx.GetTestUserPassword();
            request.ResendEmailConfirmation = true;
            var response = await service.LoginWithCredentialsAsync(request, new CookieManagerFake(), RequestSourceInfo.Empty, CancellationToken.None);

            Assert.That(response.Success, Is.False);
        }
    }

    [Test]
    public async Task LoginWithCredentialsAsync_validCredentialsAndUserRequiesEmailVerificationAndUserAskedForVerificationEmail_verificationEmailShouldBeSent()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var userProfileSet = ctx.Services.GetRequiredService<IMongoCollection<User>>();
            var userSecuritySet = ctx.Services.GetRequiredService<IMongoCollection<UserSecurity>>();
            var userSecurityRepo = ctx.Services.GetRequiredService<IUserSecurityRepository>();

            var user = await userProfileSet.AsQueryable().FirstAsync((x) => x.Email == ctx.GetTestUserEmail(), CancellationToken.None);
            var userSecurity = await userSecuritySet.AsQueryable().SingleAsync(x => x.Id == user.Id);
            userSecurity.IsEmailOwnershipConfirmed = false;
            await userSecurityRepo.UpsertAsync(userSecurity, CancellationToken.None);

            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var stringLocalizer = ctx.Services.GetRequiredService<IStringLocalizer<Common.Resource.Resources>>();
            var request = new LoginRequestDto();
            request.Email = ctx.GetTestUserEmail();
            request.Password = ctx.GetTestUserPassword();
            request.ResendEmailConfirmation = true;
            var response = await service.LoginWithCredentialsAsync(request, new CookieManagerFake(), RequestSourceInfo.Empty, CancellationToken.None);

            var emailSenderService = (EmailSenderServiceFake)ctx.Services.GetRequiredService<IEmailSenderService>();

            Assert.Multiple(() =>
            {
                Assert.That(response.EmailConfirmationSent, Is.True);
                Assert.That(emailSenderService.Emails.Value.Any(x => x.Subject == stringLocalizer["Email account ownership confirmation"]));
                Assert.That(emailSenderService.Emails.Value.Any(x => x.Addresses.Contains(request.Email)));
            });
        }
    }

    [Test]
    public async Task LoginWithRefreshTokenAsync_validRefreshTokenSupplied_newRefreshTokenShouldBeProvided()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var cookieManager = new CookieManagerFake();
            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();

            // login through L/P to get refresh token ...
            var credentialLoginRequest = new LoginRequestDto();
            credentialLoginRequest.Email = ctx.GetTestUserEmail();
            credentialLoginRequest.Password = ctx.GetTestUserPassword();
            var response = await service.LoginWithCredentialsAsync(credentialLoginRequest, cookieManager, RequestSourceInfo.Empty, CancellationToken.None);
            var firstRefreshToken = cookieManager.GetCookie(Tokens.RefreshTokenCookieName);

            var request = new RefreshRequestDto();
            request.RefreshToken = firstRefreshToken;
            var refreshLoginResponse = await service.LoginWithRefreshTokenAsync(request, cookieManager, RequestSourceInfo.Empty, CancellationToken.None);
            var newRefreshToken = cookieManager.GetCookie(Tokens.RefreshTokenCookieName);

            Assert.Multiple(() =>
            {
                Assert.That(firstRefreshToken, Is.Not.Empty);
                Assert.That(newRefreshToken, Is.Not.Empty);
                Assert.That(firstRefreshToken, Is.Not.EqualTo(newRefreshToken));
            });
        }
    }

    [Test]
    public async Task LoginWithRefreshTokenAsync_validRefreshTokenSupplied_newRefreshTokenExpirationDateShouldBeAccordingToConfiguration()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var cookieManager = new CookieManagerFake();
            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();

            // login through L/P to get refresh token ...
            var credentialLoginRequest = new LoginRequestDto();
            credentialLoginRequest.Email = ctx.GetTestUserEmail();
            credentialLoginRequest.Password = ctx.GetTestUserPassword();
            var response = await service.LoginWithCredentialsAsync(credentialLoginRequest, cookieManager, RequestSourceInfo.Empty, CancellationToken.None);
            var firstRefreshToken = cookieManager.GetCookie(Tokens.RefreshTokenCookieName);

            var request = new RefreshRequestDto();
            request.RefreshToken = firstRefreshToken;
            var refreshLoginResponse = await service.LoginWithRefreshTokenAsync(request, cookieManager, RequestSourceInfo.Empty, CancellationToken.None);

            var config = ctx.Services.GetRequiredService<IConfiguration>();
            var dateTimeProvider = ctx.Services.GetRequiredService<IDateTimeProvider>();
            var expirationDate = dateTimeProvider.GetUtcNow().AddDays(Convert.ToInt32(config[Keys.JwtRefreshLifetimeInDays]));
            Assert.That(refreshLoginResponse.RefreshTokenExpirationUTC, Is.EqualTo(expirationDate).Within(1).Minutes);
        }
    }

    [Test]
    public async Task LoginWithRefreshTokenAsync_validRefreshTokenSupplied_newAccessTokenExpirationDateShouldBeAccordingToConfiguration()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var cookieManager = new CookieManagerFake();
            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();

            // login through L/P to get refresh token ...
            var credentialLoginRequest = new LoginRequestDto();
            credentialLoginRequest.Email = ctx.GetTestUserEmail();
            credentialLoginRequest.Password = ctx.GetTestUserPassword();
            var response = await service.LoginWithCredentialsAsync(credentialLoginRequest, cookieManager, RequestSourceInfo.Empty, CancellationToken.None);
            var firstRefreshToken = cookieManager.GetCookie(Tokens.RefreshTokenCookieName);

            var request = new RefreshRequestDto();
            request.RefreshToken = firstRefreshToken;
            var refreshLoginResponse = await service.LoginWithRefreshTokenAsync(request, cookieManager, RequestSourceInfo.Empty, CancellationToken.None);
            if (string.IsNullOrWhiteSpace(refreshLoginResponse.AccessToken))
            {
                Assert.Fail();
                return;
            }

            var tokenDecoded = Tokens.DecodeJwtToken(refreshLoginResponse.AccessToken);

            var config = ctx.Services.GetRequiredService<IConfiguration>();
            var dateTimeProvider = ctx.Services.GetRequiredService<IDateTimeProvider>();
            var expirationDate = dateTimeProvider.GetUtcNow().AddMinutes(Convert.ToInt32(config[Keys.JwtAccessLifetimeInMinutes]));
            Assert.That(tokenDecoded.ValidTo, Is.EqualTo(expirationDate).Within(1).Minutes);
        }
    }

    [Test]
    public async Task LoginWithRefreshTokenAsync_validRefreshTokenSupplied_newAccessTokenShouldBeProvided()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var cookieManager = new CookieManagerFake();
            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();

            // login through L/P to get refresh token ...
            var credentialLoginRequest = new LoginRequestDto();
            credentialLoginRequest.Email = ctx.GetTestUserEmail();
            credentialLoginRequest.Password = ctx.GetTestUserPassword();
            var response = await service.LoginWithCredentialsAsync(credentialLoginRequest, cookieManager, RequestSourceInfo.Empty, CancellationToken.None);
            var tokenEncoded = response.AccessToken;
            var refreshToken = cookieManager.GetCookie(Tokens.RefreshTokenCookieName);

            // We need this so that prev access token is different from new one.
            // Otherwise the expiration date on prev token is same as new one and this make them
            // exactly the same
            Thread.Sleep(1 * 1000);

            var request = new RefreshRequestDto();
            request.RefreshToken = refreshToken;
            var refreshLoginResponse = await service.LoginWithRefreshTokenAsync(request, cookieManager, RequestSourceInfo.Empty, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(refreshLoginResponse.AccessToken, Is.Not.Empty);
                Assert.That(refreshLoginResponse.AccessToken, Is.Not.EqualTo(tokenEncoded));
            });
        }
    }

    [Test]
    public async Task LoginWithRefreshTokenAsync_validCredentialsButDeactivatedUser_shouldNotAllowLogin()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var userProfileSet = ctx.Services.GetRequiredService<IMongoCollection<User>>();
            var userProfileRepo = ctx.Services.GetRequiredService<IUserRepository>();
            var cookieManager = new CookieManagerFake();
            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();

            // login through L/P to get refresh token ...
            var credentialLoginRequest = new LoginRequestDto();
            credentialLoginRequest.Email = ctx.GetTestUserEmail();
            credentialLoginRequest.Password = ctx.GetTestUserPassword();
            var response = await service.LoginWithCredentialsAsync(credentialLoginRequest, cookieManager, RequestSourceInfo.Empty, CancellationToken.None);
            var tokenEncoded = response.AccessToken;
            var refreshToken = cookieManager.GetCookie(Tokens.RefreshTokenCookieName);

            // We need this so that prev access token is different from new one.
            // Otherwise the expiration date on prev token is same as new one and this make them
            // exactly the same
            Thread.Sleep(1 * 1000);

            var user = await userProfileSet.AsQueryable().SingleAsync(x => x.Email == ctx.GetTestUserEmail());
            user.Deactivate(ctx.Services.GetRequiredService<IDateTimeProvider>());
            await userProfileRepo.UpsertAsync(user, user, CancellationToken.None);

            var request = new RefreshRequestDto();
            request.RefreshToken = refreshToken;
            var refreshLoginResponse = await service.LoginWithRefreshTokenAsync(request, cookieManager, RequestSourceInfo.Empty, CancellationToken.None);

            Assert.That(refreshLoginResponse.Success, Is.False);
        }
    }

    [Test]
    public async Task LogoutAsync_validRefreshTokenCookie_shouldDeleteRefreshCookieProperly()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var cookieManager = new CookieManagerFake();
            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var request = new LoginRequestDto();
            request.Email = ctx.GetTestUserEmail();
            request.Password = ctx.GetTestUserPassword();
            var response = await service.LoginWithCredentialsAsync(request, cookieManager, RequestSourceInfo.Empty, CancellationToken.None);
            Assume.That(response.Success, Is.True);

            var logOutResponse = await service.LogoutAsync(cookieManager, RequestSourceInfo.Empty, CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.That(logOutResponse.Success, Is.True);
                Assert.That(!cookieManager.Cookies.Value.ContainsKey(Tokens.RefreshTokenCookieName));
            });
        }
    }

    [Test]
    public async Task LogoutAsync_validRefreshTokenCookie_shouldDeleteRefreshTokenProperly()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var cookieManager = new CookieManagerFake();
            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var request = new LoginRequestDto();
            request.Email = ctx.GetTestUserEmail();
            request.Password = ctx.GetTestUserPassword();
            var response = await service.LoginWithCredentialsAsync(request, cookieManager, RequestSourceInfo.Empty, CancellationToken.None);
            Assume.That(response.Success, Is.True);

            var refreshToken = Convert.ToBase64String(Hash.Sha256AsBytes(cookieManager.GetCookie(Tokens.RefreshTokenCookieName)));
            var logOutResponse = await service.LogoutAsync(cookieManager, RequestSourceInfo.Empty, CancellationToken.None);

            var userProfileSet = ctx.Services.GetRequiredService<IMongoCollection<User>>();
            var userSecuritySet = ctx.Services.GetRequiredService<IMongoCollection<UserSecurity>>();

            var userProfile = await userProfileSet.AsQueryable().SingleAsync(x => x.Email == request.Email);
            var userStorage = await userSecuritySet.AsQueryable().SingleAsync(x => x.Id == userProfile.Id);
            Assert.Multiple(() =>
            {
                Assert.That(userStorage.RefreshTokenHash, Is.Null);
                Assert.That(userStorage.RefreshTokenExpirationUTC, Is.Null);
            });
        }
    }

    [Test]
    public async Task ProcessInvitationAsync_invitationCodeForNonExistingUser_shouldReturnNotValid()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var request = ctx.CreateInvitationProcessRequestDto(
                "testuser1000@silverkinetics.dev",
                Convert.ToBase64String(Encoding.Unicode.GetBytes("randomdata")));

            var response = await service.ProcessInvitationAsync(request, CancellationToken.None);
            Assert.That(response.Success, Is.False);
        }
    }

    [Test]
    public async Task ProcessInvitationAsync_processInvitationCodeForValidUserWhoHasAlreadyUsedTheirInvitationCode_shouldReturnNotValid()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var request = new InvitationGenerationRequestDto();
            request.Email = ctx.GetTestUserEmail();
            var response = service.GenerateInvitationCode(request);
            Assume.That(response.Success, Is.True);

            var userSet = ctx.Services.GetRequiredService<IMongoCollection<User>>();
            var userSecuritySet = ctx.Services.GetRequiredService<IMongoCollection<UserSecurity>>();
            var userSecurityRepo = ctx.Services.GetRequiredService<IUserSecurityRepository>();

            var userProfile = await userSet.AsQueryable().SingleAsync(x => x.Email == request.Email);
            var userSecurity = await userSecuritySet.AsQueryable().SingleAsync(x => x.Id == userProfile.Id);

            userSecurity.MustActivateWithInvitationCode = false;
            await userSecurityRepo.UpsertAsync(userSecurity, CancellationToken.None);

            var invitationProcessRequest = ctx.CreateInvitationProcessRequestDto(request.Email, response.Code ?? string.Empty);
            var verificationResponse = await service.ProcessInvitationAsync(invitationProcessRequest, CancellationToken.None);
            Assert.That(verificationResponse.Success, Is.False);
        }
    }

    [Test]
    public async Task ProcessInvitationAsync_processInvitationCodeForValidInactiveUser_shouldBeSuccess()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var userEmail = "testuser1000@silverkinetics.dev";
            var ret = await ctx.UpsertNewUserAsync(userEmail);

            var response = await ctx.LoginUserWithInvitationCodeAsync(userEmail, ret?.Result?.InvitationCode ?? string.Empty);
            Assert.That(response.Success, Is.True);
        }
    }

    [Test]
    public async Task ProcessInvitationAsync_processInvitationCodeForValidInactiveUser_userShouldBeMarkedThatTheyActivatedWithInvitationCodeAnymore()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var userEmail = "testuser1000@silverkinetics.dev";
            var ret = await ctx.UpsertNewUserAsync(userEmail);
            if (ret.Result == null)
            {
                Assert.Fail();
                return;
            }

            await ctx.LoginUserWithInvitationCodeAsync(userEmail, ret.Result?.InvitationCode ?? string.Empty);

            var id = ret.Result?.Id;
            var userSecurityRepo = ctx.Services.GetRequiredService<IMongoCollection<UserSecurity>>();
            var userSecurity = await userSecurityRepo.AsQueryable().FirstAsync(x => x.Id == ObjectId.Parse(id));
            Assert.That(userSecurity.MustActivateWithInvitationCode, Is.False);
        }
    }

    [Test]
    public async Task ProcessInvitationAsync_processInvitationCodeForValidInactiveUser_userShouldBePutIntoEmailConfirmationState()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var userEmail = "testuser1000@silverkinetics.dev";
            var ret = await ctx.UpsertNewUserAsync(userEmail);
            if (ret.Result == null)
            {
                Assert.Fail();
                return;
            }

            await ctx.LoginUserWithInvitationCodeAsync(userEmail, ret.Result?.InvitationCode ?? string.Empty);

            var id = ret.Result?.Id;
            var userSecurityRepo = ctx.Services.GetRequiredService<IMongoCollection<UserSecurity>>();
            var userSecurity = await userSecurityRepo.AsQueryable().FirstAsync(x => x.Id == ObjectId.Parse(id));

            Assert.Multiple(() =>
            {
                Assert.That(userSecurity.MustActivateWithInvitationCode, Is.False);
                Assert.That(userSecurity.IsEmailOwnershipConfirmed, Is.False);
            });
        }
    }

    [Test]
    public async Task ProcessInvitationAsync_invitationCodeForDifferentEmailAddress_shouldReturnNotValid()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var request = new InvitationGenerationRequestDto();
            request.Email = ctx.GetTestUserEmail();
            var response = service.GenerateInvitationCode(request);
            Assume.That(response.Success, Is.True);

            var invitationProcessRequest = ctx.CreateInvitationProcessRequestDto("testuser1000@silverkinetics.dev", response.Code ?? string.Empty);
            var verificationResponse = await service.ProcessInvitationAsync(invitationProcessRequest, CancellationToken.None);
            Assert.That(verificationResponse.Success, Is.False);
        }
    }

    [Test]
    public async Task ProcessInvitationAsync_invitationCodeForDeactivatedUser_shouldReturnNotValid()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var request = new InvitationGenerationRequestDto();
            request.Email = ctx.GetTestUserEmail();
            var response = service.GenerateInvitationCode(request);
            Assume.That(response.Success, Is.True);

            var userSet = ctx.Services.GetRequiredService<IMongoCollection<User>>();
            var userRepo = ctx.Services.GetRequiredService<IUserRepository>();

            var user = await userSet.AsQueryable().SingleAsync(x => x.Email == request.Email);
            user.Deactivate(ctx.Services.GetRequiredService<IDateTimeProvider>());
            await userRepo.UpsertAsync(user, user, CancellationToken.None);

            var invitationProcessRequest = ctx.CreateInvitationProcessRequestDto(request.Email, response.Code ?? string.Empty);
            var verificationResponse = await service.ProcessInvitationAsync(invitationProcessRequest, CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.That(verificationResponse.Success, Is.False);
                Assert.That(verificationResponse?.InfoMessages?.Any(x => x == "User is deactivated."), Is.True);
            });
        }
    }

    [Test]
    public async Task ProcessInvitationAsync_validInvitationCodeForNewUser_emailConfirmationNotificationShouldBeSent()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var email = "testuser1000@silverkinetics.dev";
            var ret = await ctx.UpsertNewUserAsync(email);
            var res = await ctx.LoginUserWithInvitationCodeAsync(email, ret.Result?.InvitationCode ?? string.Empty);

            var emailSenderService = ctx.Services.GetRequiredService<IEmailSenderService>() as EmailSenderServiceFake;
            var emailSent = emailSenderService?.Emails.Value.Any(x => x.Template == TemplateType.EmailConfirmation && x.Addresses.Contains(email));
            Assert.That(emailSent, Is.True);
        }
    }

    [Test]
    public async Task ProcessEmailConfirmationTokenAsync_validEmailConfirmationCodeForNewUser_shouldBeSuccess()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var email = "testuser1000@silverkinetics.dev";
            var ret = await ctx.UpsertNewUserAsync(email);

            var verificationResponse = await ctx.LoginUserWithInvitationCodeAsync(email, ret.Result?.InvitationCode ?? string.Empty);
            Assume.That(verificationResponse.Success, Is.True);

            var config = ctx.Services.GetRequiredService<IConfiguration>();
            var req = new EmailConfirmationRequestDto();
            req.Code = Convert.ToBase64String(EmailConfirmations.GenerateNew(config, email, DateTime.UtcNow));

            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var response = await service.ProcessEmailConfirmationTokenAsync(req, RequestSourceInfo.Empty, CancellationToken.None);
            Assert.That(response.Success, Is.True);
        }
    }

    [Test]
    public async Task ProcessEmailConfirmationTokenAsync_invalidEmailConfirmationCodeForNewUser_shouldReturnFail()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var email = "testuser1000@silverkinetics.dev";
            var ret = await ctx.UpsertNewUserAsync(email);

            var verificationResponse = await ctx.LoginUserWithInvitationCodeAsync(email, ret.Result?.InvitationCode ?? string.Empty);
            Assume.That(verificationResponse.Success, Is.True);

            var config = ctx.Services.GetRequiredService<IConfiguration>();
            var req = new EmailConfirmationRequestDto();
            req.Code = "Random" + Convert.ToBase64String(EmailConfirmations.GenerateNew(config, email, DateTime.UtcNow));

            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var response = await service.ProcessEmailConfirmationTokenAsync(req, RequestSourceInfo.Empty, CancellationToken.None);
            Assert.That(response.Success, Is.False);
        }
    }

    [Test]
    public async Task ProcessEmailConfirmationTokenAsync_validEmailConfirmationCodeForDeactivatedUser_shouldFailAndReturnDeactivatedUserMessage()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var email = "testuser1000@silverkinetics.dev";
            var ret = await ctx.UpsertNewUserAsync(email);

            var verificationResponse = await ctx.LoginUserWithInvitationCodeAsync(email, ret.Result?.InvitationCode ?? string.Empty);
            Assume.That(verificationResponse.Success, Is.True);

            await ctx.DeactivateUserAsync(ObjectId.Parse(ret?.Result?.Id));

            var config = ctx.Services.GetRequiredService<IConfiguration>();
            var req = new EmailConfirmationRequestDto();
            req.Code = Convert.ToBase64String(EmailConfirmations.GenerateNew(config, email, DateTime.UtcNow));

            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var response = await service.ProcessEmailConfirmationTokenAsync(req, RequestSourceInfo.Empty, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(response.Success, Is.False);
                Assert.That(response?.InfoMessages?.Contains("User is deactivated."), Is.True);
            });
        }
    }

    [Test]
    public async Task ProcessEmailConfirmationTokenAsync_validEmailConfirmationCodeForUserWhoDidNotActivateWithInvitationCode_shouldReturnFail()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var email = "testuser1000@silverkinetics.dev";
            var ret = await ctx.UpsertNewUserAsync(email);

            var config = ctx.Services.GetRequiredService<IConfiguration>();
            var req = new EmailConfirmationRequestDto();
            req.Code = Convert.ToBase64String(EmailConfirmations.GenerateNew(config, email, DateTime.UtcNow));

            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var response = await service.ProcessEmailConfirmationTokenAsync(req, RequestSourceInfo.Empty, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(response.Success, Is.False);
                Assert.That(response?.InfoMessages?.Contains("User has not been activated."), Is.True);
            });
        }
    }

    [Test]
    public async Task ProcessEmailConfirmationTokenAsync_validEmailConfirmationCodeForUserWhoAlreadyConfirmedEmail_shouldReturnFailAndReturnUserAlreadyConfirmatedMessage()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var email = "testuser1000@silverkinetics.dev";
            var ret = await ctx.UpsertNewUserAsync(email);

            var verificationResponse = await ctx.LoginUserWithInvitationCodeAsync(email, ret.Result?.InvitationCode ?? string.Empty);
            Assume.That(verificationResponse.Success, Is.True);

            var config = ctx.Services.GetRequiredService<IConfiguration>();
            var req = new EmailConfirmationRequestDto();
            req.Code = Convert.ToBase64String(EmailConfirmations.GenerateNew(config, email, DateTime.UtcNow));

            var service = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            await service.ProcessEmailConfirmationTokenAsync(req, RequestSourceInfo.Empty, CancellationToken.None);
            var response = await service.ProcessEmailConfirmationTokenAsync(req, RequestSourceInfo.Empty, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(response.Success, Is.False);
                Assert.That(response?.InfoMessages?.Contains("User's email has already been confirmed."), Is.True);
            });
        }
    }
}
