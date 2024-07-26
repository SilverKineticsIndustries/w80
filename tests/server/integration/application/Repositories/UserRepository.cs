namespace SilverKinetics.w80.Application.IntegrationTests.Repositories;

[TestFixture(TestOf = typeof(Application.Repositories.UserRepository))]
public class UserRepository
{
    [Test]
    public async Task AnyAsync_queryForServiceWorkerWithQueryFiltersEnabled_serviceWorkerShouldNotBeFound()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var repo = ctx.Services.GetRequiredService<IUserRepository>();
            var ret = await repo.AnyAsync(x => x.Role == Role.ServiceWorker, CancellationToken.None);
            Assert.That(ret, Is.False);
        }
    }

    [Test]
    public async Task AnyAsync_queryForServiceWorkerWithQueryFiltersDisabled_serviceWorkerShouldBeFound()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var repo = ctx.Services.GetRequiredService<IUserRepository>();
            repo.QueryFiltersEnabled = false;

            var ret = await repo.AnyAsync(x => x.Role == Role.ServiceWorker, CancellationToken.None);
            Assert.That(ret, Is.True);
        }
    }
}