using SilverKinetics.w80.Application.Exceptions;

namespace SilverKinetics.w80.Application.Services.IntegrationTests;

[TestFixture(TestOf = typeof(Services.UserApplicationService))]
public class UserApplicationService
{
    [Test]
    public async Task UpdateAsync_asNormalUserCreateNewUser_authorizationExceptionShouldBeThrownBecauseOnlyAdminCanAddUsers()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            var user = ctx.CreateUser("john.smith@dev.silverkinetics.com");

            Assert.ThrowsAsync<AuthorizationException>(async () => {
                await service.UpdateAsync(UserMapper.ToUpdateDTO(user), RequestSourceInfo.Empty, CancellationToken.None);
            });
        }
    }

    [Test]
    public async Task UpdateAsync_updateOwnProfile_roleShouldNotChange()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var set = ctx.Services.GetRequiredService<IMongoCollection<User>>();
            var beforeUpdate = await set.AsQueryable().FirstAsync(x => x.Id == ctx.GetTestUserID());

            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            var user = await service.GetProfileAsync(ctx.GetTestUserID(), CancellationToken.None);
            user.Nickname = user.Nickname + " updated";
            await service.UpdateAsync(UserMapper.ToUpdateDTO(user), RequestSourceInfo.Empty, CancellationToken.None);

            var afterUpdate = await set.AsQueryable().FirstAsync(x => x.Id == ctx.GetTestUserID());

            Assert.That(beforeUpdate.Role, Is.EqualTo(afterUpdate.Role));
        }
    }

    [Test]
    public async Task UpdateAsync_asNormalUserUpdateOwnPassword_passworkShouldBeUpdated()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            var user = await service.GetProfileAsync(ctx.GetTestUserID(), CancellationToken.None);
            var updateRequest = UserMapper.ToUpdateDTO(user);
            updateRequest.Password = "longpassword456";
            await service.UpdateAsync(updateRequest, RequestSourceInfo.Empty, CancellationToken.None);

            var authenticationService = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var res = await authenticationService.LoginWithCredentialsAsync(
                new LoginRequestDto() { Email = ctx.GetTestUserEmail(), Password = "longpassword456"},
                new CookieManagerFake(),
                RequestSourceInfo.Empty,
                CancellationToken.None
            );

            Assert.That(res.Success, Is.True);
        }
    }

    [Test]
    public async Task UpdateAsync_asAdminUserUpdateOwnPassword_passworkShouldBeUpdated()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            ctx.SetUserToAdmin();

            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            var user = await service.GetProfileAsync(ctx.GetAdminUserID(), CancellationToken.None);
            var updateRequest = UserMapper.ToUpdateDTO(user);
            updateRequest.Password = "longpassword456";
            await service.UpdateAsync(updateRequest, RequestSourceInfo.Empty, CancellationToken.None);

            var authenticationService = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
            var res = await authenticationService.LoginWithCredentialsAsync(
                new LoginRequestDto() { Email = ctx.GetAdminUserEmail(), Password = "longpassword456"},
                new CookieManagerFake(),
                RequestSourceInfo.Empty,
                CancellationToken.None
            );

            Assert.That(res.Success, Is.True);
        }
    }

    [Test]
    public async Task UpdateAsync_asNormalUserUpdateOwnPasswordWithSmallPassword_passwordLengthCannotBeLessThanMinimum()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var config = ctx.Services.GetRequiredService<IConfiguration>();
            var minLength = Convert.ToInt32(config[Keys.PasswordMinimumLength]);
            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            var user = await service.GetProfileAsync(ctx.GetTestUserID(), CancellationToken.None);
            var updateRequest = UserMapper.ToUpdateDTO(user);
            updateRequest.Password =  string.Join(string.Empty, Enumerable.Repeat('A', minLength - 1));
            var res = await service.UpdateAsync(updateRequest, RequestSourceInfo.Empty, CancellationToken.None);
            Assert.That(res.Errors.Any(y => y.ClientMessage == $"Password must be at least {minLength} characters long."), Is.True);
        }
    }

    [Test]
    public async Task UpdateAsync_asNormalUserUpdateOwnPasswordToSamePasswordAsBefore_validationErrorShouldBeThrown()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            var user = await service.GetProfileAsync(ctx.GetTestUserID(), CancellationToken.None);
            var updateRequest = UserMapper.ToUpdateDTO(user);
            updateRequest.Password = "longpassword123";
            var res = await service.UpdateAsync(updateRequest, RequestSourceInfo.Empty, CancellationToken.None);
            Assert.That(res.Errors.Any(y => y.ClientMessage == "New password is same as current password."), Is.True);
        }
    }

    [Test]
    public async Task UpdateAsync_asAdminCreateNewUser_newUserShouldBeCreated()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            ctx.SetUserToAdmin();

            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            var user = ctx.CreateUser("john.smith@dev.silverkinetics.com");

            await service.UpdateAsync(UserMapper.ToUpdateDTO(user), RequestSourceInfo.Empty, CancellationToken.None);
            var ret = service.GetProfileAsync(user.Id, CancellationToken.None);
            Assert.That(ret, Is.Not.Null);
        }
    }

    [Test]
    public async Task UpdateAsync_asAdminChangeEmailOfAnotherUser_anotherUserEmailShouldBeChanged()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            ctx.SetUserToAdmin();

            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            var user = ctx.CreateUser("john.smith@dev.silverkinetics.com");
            var ret = await service.UpdateAsync(UserMapper.ToUpdateDTO(user), RequestSourceInfo.Empty, CancellationToken.None);

            var updatedEmailAddress = "updated@silverkinetics.dev";
            ret.Result.Email = updatedEmailAddress;
            await service.UpdateAsync(UserMapper.ToUpdateDTO(ret.Result), RequestSourceInfo.Empty, CancellationToken.None);

            var updated = await service.GetProfileAsync(user.Id, CancellationToken.None);
            Assert.That(updated.Email, Is.EqualTo(updatedEmailAddress));
        }
    }

    [Test]
    public async Task UpdateAsync_asAdminCreateNewUserWithSameEmail_validationErrorShouldBeReturned()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            ctx.SetUserToAdmin();

            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            var userRepo = ctx.Services.GetRequiredService<IUserRepository>();

            var user1 = ctx.CreateUser("john.smith@dev.silverkinetics.com");
            await service.UpdateAsync(UserMapper.ToUpdateDTO(user1), RequestSourceInfo.Empty, CancellationToken.None);
            Assume.That(await userRepo.GetSingleOrDefaultAsync(x => x.Id == user1.Id, CancellationToken.None), Is.Not.Null);

            var user2 = ctx.CreateUser("john.smith@dev.silverkinetics.com");
            var ret = await service.UpdateAsync(UserMapper.ToUpdateDTO(user2), RequestSourceInfo.Empty, CancellationToken.None);
            Assert.That(ret.Errors.Any(x => x.ClientMessage == "An account already exists with same email."));
        }
    }

    [Test]
    public async Task UpdateAsync_asNormalUserUpdateEmailOnOwnProfile_emailShouldBeUpdated()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var securityContext = ctx.Services.GetRequiredService<ISecurityContext>();
            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            var userRepo = ctx.Services.GetRequiredService<IUserRepository>();

            var updatedEmail = "john.smith_updated@dev.silverkinetics.com";

            var user = await userRepo.GetSingleOrDefaultAsync(x => x.Id == securityContext.UserId, CancellationToken.None);
            user.Email = updatedEmail;
            await service.UpdateAsync(UserMapper.ToUpdateDTO(user), RequestSourceInfo.Empty, CancellationToken.None);

            var updated = await service.GetProfileAsync(user.Id, CancellationToken.None);
            Assert.That(updated.Email, Is.EqualTo(updatedEmail));
        }
    }

    [Test]
    public async Task UpdateAsync_asNormalUserUpdateOwnProfile_makeSureUpdatedUTCisChanged()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var securityContext = ctx.Services.GetRequiredService<ISecurityContext>();
            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            var userRepo = ctx.Services.GetRequiredService<IUserRepository>();

            var user = await userRepo.GetSingleOrDefaultAsync(x => x.Id == securityContext.UserId, CancellationToken.None);
            var currentUpdatedUTC = user.UpdatedUTC;
            user.Nickname = "Updated";
            await service.UpdateAsync(UserMapper.ToUpdateDTO(user), RequestSourceInfo.Empty, CancellationToken.None);

            user = await userRepo.GetSingleOrDefaultAsync(x => x.Id == securityContext.UserId, CancellationToken.None);
            Assert.Multiple(() => {
                Assert.That(user.UpdatedUTC, Is.Not.EqualTo(currentUpdatedUTC));
                Assert.That(user.UpdatedUTC, Is.EqualTo(DateTime.UtcNow).Within(10).Seconds);
            });
        }
    }

    [Test]
    public async Task UpdateAsync_changeUserIdOnExistingUser_authorizationExceptionShouldBeThrown()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var securityContext = ctx.Services.GetRequiredService<ISecurityContext>();
            var userRepo = ctx.Services.GetRequiredService<IUserRepository>();
            var service = ctx.Services.GetRequiredService<IUserApplicationService>();

            var user = await userRepo.GetSingleOrDefaultAsync(x => x.Id == securityContext.UserId, CancellationToken.None);
            user.Id = ObjectId.GenerateNewId();
            var userDto = UserMapper.ToUpdateDTO(user);

            Assert.ThrowsAsync<AuthorizationException>(async() => {
                await service.UpdateAsync(userDto, RequestSourceInfo.Empty, CancellationToken.None);
            });
        }
    }

    [Test]
    public async Task DeactivateAsync_deactivateRegularUser_userShouldBeDeactivated()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            ctx.SetUserToAdmin();

            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            await service.DeactivateAsync(ctx.GetTestUserID(), RequestSourceInfo.Empty, CancellationToken.None);
            var user = await ctx.Services.GetRequiredService<IMongoCollection<User>>().AsQueryable().FirstAsync(x => x.Id == ctx.GetTestUserID());
            Assert.That(user.IsDeactivated(), Is.True);
        }
    }

    [Test]
    public async Task DeactivateAsync_deactivateRegularUser_deactivatedByShouldBeSetCorrectly()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            ctx.SetUserToAdmin();
            var adminUserId = ctx.GetCurrentUserId();

            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            await service.DeactivateAsync(ctx.GetTestUserID(), RequestSourceInfo.Empty, CancellationToken.None);
            var user = await ctx.Services.GetRequiredService<IMongoCollection<User>>().AsQueryable().FirstAsync(x => x.Id == ctx.GetTestUserID());
            Assert.That(user.DeactivatedBy, Is.EqualTo(adminUserId));
        }
    }

    [Test]
    public async Task GetAllAsync_getAllUsers_allNormalUsersShouldBeReturned()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var count = await ctx.Services.GetRequiredService<IMongoCollection<User>>().AsQueryable().CountAsync();
            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            var users = await service.GetAllAsync(CancellationToken.None);

            Assert.That(users.Count, Is.EqualTo(count - 1));  // minus 1 for service worker which we dont return
        }
    }

    [Test]
    public async Task GetAllAsync_getAllUsers_serviceWorkerShouldNotBeReturned()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            var users = await service.GetAllAsync(CancellationToken.None);

            Assert.That(users.Any(x => x.Role == Role.ServiceWorker), Is.EqualTo(false));
        }
    }

    [Test]
    public async Task GetAllAsync_getAllUsers_deactivatedUsersShouldAlsoBeReturned()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var userId = ctx.GetCurrentUserId();
            ctx.SetUserToAdmin();

            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            await service.DeactivateAsync(userId, RequestSourceInfo.Empty, CancellationToken.None);

            var user = await ctx.Services.GetRequiredService<IMongoCollection<User>>().AsQueryable().FirstAsync(x => x.Id == userId);
            Assume.That(user.IsDeactivated(), Is.True);

            var users = await service.GetAllAsync(CancellationToken.None);
            Assert.That(users.Any(x => x.Id == userId.ObjectIdToStringId()), Is.EqualTo(true));
        }
    }


    [Test]
    public async Task UpsertAsync_asRegularUserCreateNewNotActiveUser_authorizationExceptionShouldBeThrown()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var userId = ObjectId.GenerateNewId().ObjectIdToStringId();
            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            Assert.ThrowsAsync<AuthorizationException>(async () => {
                await service.UpsertAsync(
                    new UserUpsertRequestDto() {
                        Nickname = "testuser1000",
                        Email = "testuser1000@silverkinetics.dev",
                        Culture = SupportedCultures.DefaultCulture,
                        TimeZone = SupportedTimeZones.DefaultTimezone,
                        Role = Role.User,
                        Id = userId
                    },
                    RequestSourceInfo.Empty,
                    CancellationToken.None
                );
            });
        }
    }

    [Test]
    public async Task UpsertAsync_asAdminCreateNewNotActiveUser_newUserShouldBeCreated()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            ctx.SetUserToAdmin();

            var userId = ObjectId.GenerateNewId().ObjectIdToStringId();
            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            await service.UpsertAsync(
                new UserUpsertRequestDto() {
                    Nickname = "testuser1000",
                    Email = "testuser1000@silverkinetics.dev",
                    Culture = SupportedCultures.DefaultCulture,
                    TimeZone = SupportedTimeZones.DefaultTimezone,
                    Role = Role.User,
                    Id = userId
                },
                RequestSourceInfo.Empty,
                CancellationToken.None
            );

            var userSet = ctx.Services.GetRequiredService<IMongoCollection<User>>();
            var user = await userSet.AsQueryable().SingleAsync(x => x.Id == ObjectId.Parse(userId));
            Assert.That(user, Is.Not.Null);
        }
    }

    [Test]
    public async Task UpsertAsync_asAdminCreateNewNotActiveUser_correspondingUserSecurityRecordShouldBeCreated()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            ctx.SetUserToAdmin();

            var userId = ObjectId.GenerateNewId().ObjectIdToStringId();
            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            await service.UpsertAsync(
                new UserUpsertRequestDto() {
                    Nickname = "testuser1000",
                    Email = "testuser1000@silverkinetics.dev",
                    Culture = SupportedCultures.DefaultCulture,
                    TimeZone = SupportedTimeZones.DefaultTimezone,
                    Role = Role.User,
                    Id = userId
                },
                RequestSourceInfo.Empty,
                CancellationToken.None
            );

            var userSecuritySet = ctx.Services.GetRequiredService<IMongoCollection<UserSecurity>>();
            var user = await userSecuritySet.AsQueryable().SingleAsync(x => x.Id == ObjectId.Parse(userId));
            Assert.That(user, Is.Not.Null);
        }
    }

    [Test]
    public async Task UpsertAsync_asAdminCreateNewNotActiveUser_userShouldBeMarkedAsRequiringUsageOfInvitationCode()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            ctx.SetUserToAdmin();

            var userId = ObjectId.GenerateNewId().ObjectIdToStringId();
            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            await service.UpsertAsync(
                new UserUpsertRequestDto() {
                    Nickname = "testuser1000",
                    Email = "testuser1000@silverkinetics.dev",
                    Culture = SupportedCultures.DefaultCulture,
                    TimeZone = SupportedTimeZones.DefaultTimezone,
                    Role = Role.User,
                    Id = userId
                },
                RequestSourceInfo.Empty,
                CancellationToken.None
            );

            var userSecuritySet = ctx.Services.GetRequiredService<IMongoCollection<UserSecurity>>();
            var userSec = await userSecuritySet.AsQueryable().SingleAsync(x => x.Id == ObjectId.Parse(userId));
            Assert.That(userSec.MustActivateWithInvitationCode, Is.True);
        }
    }

    [Test]
    public async Task UpsertAsync_asAdminCreateNewNotActiveUser_userShouldBeMarkedAsNotHavingEmailConfirmated()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            ctx.SetUserToAdmin();

            var userId = ObjectId.GenerateNewId().ObjectIdToStringId();
            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            await service.UpsertAsync(
                new UserUpsertRequestDto() {
                    Nickname = "testuser1000",
                    Email = "testuser1000@silverkinetics.dev",
                    Culture = SupportedCultures.DefaultCulture,
                    TimeZone = SupportedTimeZones.DefaultTimezone,
                    Role = Role.User,
                    Id = userId
                },
                RequestSourceInfo.Empty,
                CancellationToken.None
            );

            var userSecuritySet = ctx.Services.GetRequiredService<IMongoCollection<UserSecurity>>();
            var userSec = await userSecuritySet.AsQueryable().SingleAsync(x => x.Id == ObjectId.Parse(userId));
            Assert.That(userSec.IsEmailOwnershipConfirmed, Is.False);
        }
    }

    [Test]
    public async Task UpsertAsync_asAdminCreateNewNotActiveUser_userShouldNotHavePassword()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            ctx.SetUserToAdmin();

            var userId = ObjectId.GenerateNewId().ObjectIdToStringId();
            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            await service.UpsertAsync(
                new UserUpsertRequestDto() {
                    Nickname = "testuser1000",
                    Email = "testuser1000@silverkinetics.dev",
                    Culture = SupportedCultures.DefaultCulture,
                    TimeZone = SupportedTimeZones.DefaultTimezone,
                    Role = Role.User,
                    Id = userId
                },
                RequestSourceInfo.Empty,
                CancellationToken.None
            );

            var userSecuritySet = ctx.Services.GetRequiredService<IMongoCollection<UserSecurity>>();
            var userSec = await userSecuritySet.AsQueryable().SingleAsync(x => x.Id == ObjectId.Parse(userId));
            Assert.That(userSec.PasswordHash, Is.Null);
        }
    }

    [Test]
    public async Task UpsertAsync_asAdminCreateNewNotActiveUser_newUserShouldBeMarkedAsNeedingInvitationCode()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            ctx.SetUserToAdmin();

            var userId = ObjectId.GenerateNewId().ObjectIdToStringId();
            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            await service.UpsertAsync(
                new UserUpsertRequestDto() {
                    Nickname = "testuser1000",
                    Email = "testuser1000@silverkinetics.dev",
                    Culture = SupportedCultures.DefaultCulture,
                    TimeZone = SupportedTimeZones.DefaultTimezone,
                    Role = Role.User,
                    Id = userId
                },
                RequestSourceInfo.Empty,
                CancellationToken.None
            );

            var userSecuritySet = ctx.Services.GetRequiredService<IMongoCollection<UserSecurity>>();
            var userSecurity = await userSecuritySet.AsQueryable().SingleAsync(x => x.Id == ObjectId.Parse(userId));
            Assert.That(userSecurity.MustActivateWithInvitationCode, Is.EqualTo(true));
        }
    }

    [Test]
    public async Task UpsertAsync_asAdminCreateNewNotActiveUser_newUserShouldHaveInvitationCode()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            ctx.SetUserToAdmin();

            var userId = ObjectId.GenerateNewId().ObjectIdToStringId();
            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            var ret = await service.UpsertAsync(
                new UserUpsertRequestDto() {
                    Nickname = "testuser1000",
                    Email = "testuser1000@silverkinetics.dev",
                    Culture = SupportedCultures.DefaultCulture,
                    TimeZone = SupportedTimeZones.DefaultTimezone,
                    Role = Role.User,
                    Id = userId
                },
                RequestSourceInfo.Empty,
                CancellationToken.None
            );
            Assert.That(ret.Result.InvitationCode, Is.Not.Empty);
        }
    }

    [Test]
    public async Task UpsertAsync_asAdminCreateNewNotActiveUserWithProvidingId_newIdShouldBeCreated()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            ctx.SetUserToAdmin();

            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            var ret = await service.UpsertAsync(
                new UserUpsertRequestDto() {
                    Nickname = "testuser1000",
                    Email = "testuser1000@silverkinetics.dev",
                    Culture = SupportedCultures.DefaultCulture,
                    TimeZone = SupportedTimeZones.DefaultTimezone,
                    Role = Role.User,
                },
                RequestSourceInfo.Empty,
                CancellationToken.None
            );
            Assert.That(ret.Result.Id, Is.Not.Empty);
        }
    }

    [Test]
    public async Task UpsertAsync_asAdminCreateNewNotActiveUser_newUserShouldHaveValidInvitationCode()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            ctx.SetUserToAdmin();

            var userEmail = "testuser1000@silverkinetics.dev";
            var userId = ObjectId.GenerateNewId().ObjectIdToStringId();
            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            var ret = await service.UpsertAsync(
                new UserUpsertRequestDto() {
                    Nickname = "testuser1000",
                    Email = userEmail,
                    Culture = SupportedCultures.DefaultCulture,
                    TimeZone = SupportedTimeZones.DefaultTimezone,
                    Role = Role.User,
                    Id = userId
                },
                RequestSourceInfo.Empty,
                CancellationToken.None
            );

            var code = ret.Result.InvitationCode;
            var config = ctx.Services.GetRequiredService<IConfiguration>();
            Invitations.Decrypt(config, code, out string email, out DateTime utcDateTime);

            Assert.Multiple(() => {
                Assert.That(userEmail, Is.EqualTo(email));
                Assert.That(utcDateTime, Is.EqualTo(DateTime.UtcNow).Within(10).Seconds);
            });
        }
    }

    [Test]
    public async Task UpsertAsync_asAdminCreateNewServiceWorkerUser_validationErrorShouldReturn()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            ctx.SetUserToAdmin();

            var userId = ObjectId.GenerateNewId().ObjectIdToStringId();
            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            var ret = await service.UpsertAsync(
                new UserUpsertRequestDto() {
                    Nickname = "testuser1000",
                    Email = "testuser1000@silverkinetics.dev",
                    Culture = SupportedCultures.DefaultCulture,
                    TimeZone = SupportedTimeZones.DefaultTimezone,
                    Role = Role.ServiceWorker,
                    Id = userId
                },
                RequestSourceInfo.Empty,
                CancellationToken.None
            );
            Assert.That(ret.Errors.Any(x => x.ClientMessage == "Invalid user."), Is.True);
        }
    }

    [Test]
    public async Task DeactivateAsync_userTriesToDeactivateThemselves_validationErrorShouldReturn()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            ctx.SetUserToAdmin();
            var service = ctx.Services.GetRequiredService<IUserApplicationService>();
            await service.DeactivateAsync(ctx.GetAdminUserID(), RequestSourceInfo.Empty, CancellationToken.None);
            var user = await ctx.Services.GetRequiredService<IMongoCollection<User>>().AsQueryable().FirstAsync(x => x.Id == ctx.GetAdminUserID());
            Assert.That(user.IsDeactivated(), Is.False);
        }
    }
}
