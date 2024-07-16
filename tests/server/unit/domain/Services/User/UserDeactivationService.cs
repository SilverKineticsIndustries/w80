namespace SilverKinetics.w80.Domain.Services.User.UnitTests;

[TestFixture(TestOf = typeof(User.UserDeactivationService))]
public class UserDeactivationService
{
    [Test]
    public async Task ValidateForDeactivationAsync_deactivationServiceWorker_userCannotBeDeactivated()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var user = ctx.CreateUser();
            user.Role = Role.ServiceWorker;

            var service = ctx.Services.GetRequiredService<IUserDeactivationService>();
            var bag = await service.ValidateForDeactivationAsync(user, CancellationToken.None);
            Assert.That(bag.Any(x => x.Message == "User cannot be deactivated."));
        }
    }

    [Test]
    public async Task ValidateForDeactivationAsync_userTryingToDeactivateThemselves_userCannotBeDeactivated()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var user = ctx.CreateUser();
            user.Id = ctx.GetCurrentUserId();

            var service = ctx.Services.GetRequiredService<IUserDeactivationService>();
            var bag = await service.ValidateForDeactivationAsync(user, CancellationToken.None);
            Assert.That(bag.Any(x => x.Message == "User cannot deactivate their own account."));
        }
    }

    [Test]
    public async Task ValidateForDeactivationAsync_deactivatingUserWhoIsAlreadyDeactivated_userCannotBeDeactivated()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var user = ctx.CreateUser();
            user.Id = ObjectId.GenerateNewId();
            user.Email = "testuser1000@silverkinetics.dev";
            user.Deactivate(DateTime.UtcNow);

            var service = ctx.Services.GetRequiredService<IUserDeactivationService>();
            var bag = await service.ValidateForDeactivationAsync(user, CancellationToken.None);
            Assert.That(bag.Any(x => x.Message == "User cannot be deactivated because they are already in a deactivated state."));
        }
    }
}
