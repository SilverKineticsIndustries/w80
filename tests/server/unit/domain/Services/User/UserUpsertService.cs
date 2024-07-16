namespace SilverKinetics.w80.Domain.Services.User.UnitTests;

[TestFixture(TestOf = typeof(Services.User.UserUpsertService))]
public class UserProfileUpsertService
{
    [Test]
    public async Task ValidateProfileAsync_updateUserWithEmptyEmail_userEmailCannotBeEmpty()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var user = ctx.CreateUser();
            user.Email = "";
            var service = ctx.Services.GetRequiredService<IUserUpsertService>();
            var bag = await service.ValidateProfileAsync(user, CancellationToken.None);
            Assert.That(bag.Any(x => x.Message == "User email cannot be empty."));
        }
    }

    [Test]
    public async Task ValidateProfileAsync_updateEmailWithEmailFromAnotherUser_twoUsersCannotHaveSameEmail()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var user = ctx.CreateUser();
            user.Email = ctx.GetTestUserEmail();
            user.Id = ObjectId.GenerateNewId();
            var service = ctx.Services.GetRequiredService<IUserUpsertService>();
            var bag = await service.ValidateProfileAsync(user, CancellationToken.None);
            Assert.That(bag.Any(x => x.Message == "An account already exists with same email."));
        }
    }

    [Test]
    public async Task ValidateFullyAsync_updateUserRoleToServiceWorker_serviceWorkerCannotBeUpserted()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var user = ctx.CreateUser();
            user.Id = ObjectId.GenerateNewId();
            user.Role = Role.ServiceWorker;
            user.Email = "testuser1000@silverkinetics.dev";
            var service = ctx.Services.GetRequiredService<IUserUpsertService>();
            var bag = await service.ValidateFullyAsync(user, CancellationToken.None);
            Assert.That(bag.Any(x => x.Message == "Invalid user."));
        }
    }

    [Test]
    public async Task ValidateProfileAsync_insertLongEmailAddress_emailAddressLengthCannotExceedMaximum()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var user = ctx.CreateUser();
            user.Id = ObjectId.GenerateNewId();
            user.Role = Role.User;
            user.Email = string.Join(string.Empty, Enumerable.Repeat('A', Entities.User.EmailMaxLength + 1));
            var service = ctx.Services.GetRequiredService<IUserUpsertService>();
            var bag = await service.ValidateProfileAsync(user, CancellationToken.None);
            Assert.That(bag.Any(x => x.Message == $"User email max length is {Entities.User.EmailMaxLength} characters."), Is.True);
        }
    }

    [Test]
    public async Task ValidateProfileAsync_insertLongNickname_nicknameLengthCannotExceedMaximum()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var user = ctx.CreateUser();
            user.Id = ObjectId.GenerateNewId();
            user.Role = Role.User;
            user.Email = "testuser1000@silverkinetics.dev";
            user.Nickname = string.Join(string.Empty, Enumerable.Repeat('A', Entities.User.NicknameMaxLength+ 1));
            var service = ctx.Services.GetRequiredService<IUserUpsertService>();
            var bag = await service.ValidateProfileAsync(user, CancellationToken.None);
            Assert.That(bag.Any(x => x.Message == $"User nickname max length is {Entities.User.NicknameMaxLength} characters."), Is.True);
        }
    }

    [Test]
    public async Task ValidateFullyAsync_insertUserWithoutRole_roleCannotBeEmpty()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var user = ctx.CreateUser();
            user.Id = ObjectId.GenerateNewId();
            user.Role = Role.None;
            user.Email = "testuser1000@silverkinetics.dev";
            var service = ctx.Services.GetRequiredService<IUserUpsertService>();
            var bag = await service.ValidateFullyAsync(user, CancellationToken.None);
            Assert.That(bag.Any(x => x.Message == "User role cannot be empty."));
        }
    }
}
