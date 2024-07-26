namespace SilverKinetics.w80.Domain.UnitTests.Services.Application;

[TestFixture(TestOf = typeof(Domain.Services.Application.ApplicationAcceptanceService))]
public class ApplicationAcceptanceService
{
    [Test]
    public async Task ValidateAsync_noAcceptanceMethodProvided_acceptanceMethodMustBeProvided()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();

            var service = ctx.Services.GetRequiredService<IApplicationAcceptanceService>();
            var acceptance = new Acceptance();

            var bag = await service.ValidateAsync(app, acceptance);
            Assert.That(bag.Any(x => x.Message == "Acceptance method must be provided."));
        }
    }

    [Test]
    public async Task ValidateAsync_acceptanceTextIsLargerThanMaxLength_acceptanceTextMustBeSmallerThanMaxLength()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();

            var service = ctx.Services.GetRequiredService<IApplicationAcceptanceService>();
            var acceptance = new Acceptance();
            acceptance.Method = AcceptanceMethod.Email;
            acceptance.ReponseText = string.Join("", Enumerable.Repeat("A", Acceptance.ResponseTextMaxSize + 1));

            var bag = await service.ValidateAsync(app, acceptance);
            Assert.That(bag.Any(x => x.Message == $"Acceptance text max length is {Acceptance.ResponseTextMaxSize} characters."), Is.True);
        }
    }

    [Test]
    public async Task ValidateAsync_deactivatedApplicationBeingAccepted_cannotAcceptDeactivatedApplication()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.Deactivate(ctx.Services.GetRequiredService<IDateTimeProvider>());

            var service = ctx.Services.GetRequiredService<IApplicationAcceptanceService>();
            var acceptance = new Acceptance();
            acceptance.Method = AcceptanceMethod.Email;

            var bag = await service.ValidateAsync(app, acceptance);
            Assert.That(bag.Any(x => x.Message == "Deactivated applications cannot be accepted."));
        }
    }

    [Test]
    public async Task ValidateAsync_archivedApplicationBeingAccepted_cannotAcceptArchivedApplication()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.Archive(ctx.Services.GetRequiredService<IDateTimeProvider>());

            var service = ctx.Services.GetRequiredService<IApplicationAcceptanceService>();
            var acceptance = new Acceptance();
            acceptance.Method = AcceptanceMethod.Email;

            var bag = await service.ValidateAsync(app, acceptance);
            Assert.That(bag.Any(x => x.Message == "Archived applications cannot be accepted."));
        }
    }

    [Test]
    public async Task Accept_acceptApplication_calendarEventsForApplicationShouldBeCleared()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.Appointments.Add(new Appointment()
            {
                StartDateTimeUTC = DateTime.UtcNow.AddDays(2),
                EndDateTimeUTC = DateTime.UtcNow.AddDays(2),
                Description = "Interview"
            });

            var service = ctx.Services.GetRequiredService<IApplicationAcceptanceService>();
            var acceptance = new Acceptance();
            acceptance.Method = AcceptanceMethod.Email;

            service.Accept(app, acceptance);
            Assert.That(app.Appointments.Count, Is.EqualTo(0));
        }
    }

    [Test]
    public async Task Accept_acceptApplication_acceptanceDateShouldBeSet()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();

            var service = ctx.Services.GetRequiredService<IApplicationAcceptanceService>();
            var acceptance = new Acceptance();
            acceptance.Method = AcceptanceMethod.Email;

            service.Accept(app, acceptance);
            Assert.That(app.Acceptance.AcceptedUTC, Is.EqualTo(DateTime.UtcNow).Within(10).Seconds);
        }
    }

    [Test]
    public async Task ArchiveAllOpenNotAcceptedApplications_archiveOpenApplications_otherOpenApplicationsShouldBeArchived()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var repo = ctx.Services.GetRequiredService<IApplicationRepository>();
            var set = ctx.Services.GetRequiredService<IMongoCollection<Domain.Entities.Application>>();

            var app1 = ctx.CreateApplication();
            app1.CompanyName = "Company 1";
            var app2 = ctx.CreateApplication();
            app2.CompanyName = "Company 2";
            var app3 = ctx.CreateApplication();
            app3.CompanyName = "Company 3";
            var app4 = ctx.CreateApplication();
            app4.CompanyName = "Company 4";

            var upsertService = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            await upsertService.UpsertAsync(app1);
            await repo.UpsertAsync(app1, CancellationToken.None);

            await upsertService.UpsertAsync(app2);
            await repo.UpsertAsync(app2, CancellationToken.None);

            await upsertService.UpsertAsync(app3);
            await repo.UpsertAsync(app3, CancellationToken.None);

            await upsertService.UpsertAsync(app4);
            await repo.UpsertAsync(app4, CancellationToken.None);

            var acceptance = new Acceptance();
            acceptance.Method = AcceptanceMethod.Email;
            var acceptanceService = ctx.Services.GetRequiredService<IApplicationAcceptanceService>();
            acceptanceService.Accept(app1, acceptance);
            var applicationsToArchive = await acceptanceService.ArchiveAllOpenNotAcceptedApplications(app1);
            await repo.AcceptAsync(app1, applicationsToArchive, CancellationToken.None);

            var apps = await set.AsQueryable().Where(x => x.Id == app2.Id || x.Id == app3.Id || x.Id == app4.Id).ToListAsync();
            Assert.That(apps.All(x => x.ArchivedUTC is not null), Is.True);
        }
    }

    [Test]
    public async Task ArchiveAllOpenNotAcceptedApplications_archiveOpenApplications_deactivatedApplicationShouldNotBeArchived()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var repo = ctx.Services.GetRequiredService<IApplicationRepository>();
            var set = ctx.Services.GetRequiredService<IMongoCollection<Domain.Entities.Application>>();

            var app1 = ctx.CreateApplication();
            app1.CompanyName = "Company 1";
            var app2 = ctx.CreateApplication();
            app2.CompanyName = "Company 2";
            var app3 = ctx.CreateApplication();
            app3.CompanyName = "Company 3";
            var app4 = ctx.CreateApplication();
            app4.CompanyName = "Company 4";

            var upsertService = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            await upsertService.UpsertAsync(app1);
            await repo.UpsertAsync(app1, CancellationToken.None);
            await upsertService.UpsertAsync(app2);
            await repo.UpsertAsync(app2, CancellationToken.None);
            await upsertService.UpsertAsync(app3);
            await repo.UpsertAsync(app3, CancellationToken.None);
            await upsertService.UpsertAsync(app4);
            await repo.UpsertAsync(app4, CancellationToken.None);

            var deactivateService = ctx.Services.GetRequiredService<IApplicationDeactivationService>();
            deactivateService.Deactivate(app4);
            await repo.DeactivateAsync(app4, CancellationToken.None);

            var acceptance = new Acceptance();
            acceptance.Method = AcceptanceMethod.Email;
            var acceptanceService = ctx.Services.GetRequiredService<IApplicationAcceptanceService>();
            acceptanceService.Accept(app1, acceptance);
            var applicationsToArchive = await acceptanceService.ArchiveAllOpenNotAcceptedApplications(app1);
            await repo.AcceptAsync(app1, applicationsToArchive, CancellationToken.None);

            var apps = await set.AsQueryable().Where(x => x.Id != app1.Id).ToListAsync();
            Assert.That(apps.Count(x => x.ArchivedUTC is not null && x.Id == app4.Id), Is.EqualTo(0));
        }
    }

    [Test]
    public async Task ArchiveAllOpenNotAcceptedApplications_archiveOpenApplications_rejectedApplicationShouldNotBeArchived()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var repo = ctx.Services.GetRequiredService<IApplicationRepository>();
            var set = ctx.Services.GetRequiredService<IMongoCollection<Domain.Entities.Application>>();

            var app1 = ctx.CreateApplication();
            app1.CompanyName = "Company 1";
            var app2 = ctx.CreateApplication();
            app2.CompanyName = "Company 2";
            var app3 = ctx.CreateApplication();
            app3.CompanyName = "Company 3";
            var app4 = ctx.CreateApplication();
            app4.CompanyName = "Company 4";

            var upsertService = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            await upsertService.UpsertAsync(app1);
            await repo.UpsertAsync(app1, CancellationToken.None);
            await upsertService.UpsertAsync(app2);
            await repo.UpsertAsync(app2, CancellationToken.None);
            await upsertService.UpsertAsync(app3);
            await repo.UpsertAsync(app3, CancellationToken.None);
            await upsertService.UpsertAsync(app4);
            await repo.UpsertAsync(app4, CancellationToken.None);

            var rejectionService = ctx.Services.GetRequiredService<IApplicationRejectionService>();
            rejectionService.Reject(app4, new Rejection() { Method = RejectionMethod.Email, Reason = "Some reason" });
            await repo.RejectAsync(app4, CancellationToken.None);

            var acceptance = new Acceptance();
            acceptance.Method = AcceptanceMethod.Email;
            var acceptanceService = ctx.Services.GetRequiredService<IApplicationAcceptanceService>();
            acceptanceService.Accept(app1, acceptance);
            var applicationsToArchive = await acceptanceService.ArchiveAllOpenNotAcceptedApplications(app1);
            await repo.AcceptAsync(app1, applicationsToArchive, CancellationToken.None);

            var apps = await set.AsQueryable().Where(x => x.Id != app1.Id).ToListAsync();
            Assert.That(apps.Count(x => x.ArchivedUTC is not null && x.Id == app4.Id), Is.EqualTo(0));
        }
    }

    [Test]
    public async Task ArchiveAllOpenNotAcceptedApplications_archiveOpenApplications_acceptedApplicationShouldNotBeArchived()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var repo = ctx.Services.GetRequiredService<IApplicationRepository>();
            var set = ctx.Services.GetRequiredService<IMongoCollection<Domain.Entities.Application>>();

            var app1 = ctx.CreateApplication();
            app1.CompanyName = "Company 1";
            var app2 = ctx.CreateApplication();
            app2.CompanyName = "Company 2";
            var app3 = ctx.CreateApplication();
            app3.CompanyName = "Company 3";
            var app4 = ctx.CreateApplication();
            app4.CompanyName = "Company 4";

            var upsertService = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            await upsertService.UpsertAsync(app1);
            await repo.UpsertAsync(app1, CancellationToken.None);
            await upsertService.UpsertAsync(app2);
            await repo.UpsertAsync(app2, CancellationToken.None);
            await upsertService.UpsertAsync(app3);
            await repo.UpsertAsync(app3, CancellationToken.None);
            await upsertService.UpsertAsync(app4);
            await repo.UpsertAsync(app4, CancellationToken.None);

            var rejectionService = ctx.Services.GetRequiredService<IApplicationRejectionService>();
            rejectionService.Reject(app4, new Rejection() { Method = RejectionMethod.Email, Reason = "Some reason" });
            await repo.RejectAsync(app4, CancellationToken.None);

            var acceptance = new Acceptance();
            acceptance.Method = AcceptanceMethod.Email;
            var acceptanceService = ctx.Services.GetRequiredService<IApplicationAcceptanceService>();
            acceptanceService.Accept(app1, acceptance);
            await repo.AcceptAsync(app1, new List<Domain.Entities.Application>(), CancellationToken.None);

            acceptanceService.Accept(app2, acceptance);
            var applicationsToArchive = await acceptanceService.ArchiveAllOpenNotAcceptedApplications(app2);
            await repo.AcceptAsync(app1, applicationsToArchive, CancellationToken.None);

            var apps = await set.AsQueryable().Where(x => x.Id == app1.Id || x.Id == app2.Id || x.Id == app3.Id || x.Id == app3.Id).ToListAsync();
            Assert.That(apps.Count(x => x.ArchivedUTC is not null), Is.EqualTo(1));
        }
    }
}