
namespace SilverKinetics.w80.Domain.Entities.UnitTests;

[TestFixture(TestOf = typeof(Entities.Application))]
public class Application
{
    [Test]
    public void InitializeApplicationStates_initStatesOnNewApplication_statesShouldBePreloadedFromGlobalList()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var applStates = ctx.GetApplicationStates();
            var app = new Entities.Application();
            app.Initialize(applStates);
            Assert.That(app.States.Count == applStates.Count());
        }
    }

    [Test]
    public void InitializeApplicationStates_initStatesOnNewApplication_activeStateShouldBeSet()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var applStates = ctx.GetApplicationStates();
            var app = new Entities.Application();
            app.Initialize(applStates);

            var selected = app.States.SingleOrDefault(x => x.IsCurrent);
            Assert.That(selected.Name, Is.EqualTo(applStates.OrderBy(x => x.SeqNo).First().Name));
        }
    }

    [Test]
    public void InitializeApplicationStates_reInitStatesOnExistingApplication_statesShouldBePreloadedFromGlobalList()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var applStates = ctx.GetApplicationStates();

            var app = new Entities.Application();
            app.Initialize(applStates);
            app.States.Remove(app.States.Last());
            app.States.Remove(app.States.First());

            app.Initialize(applStates);
            Assert.That(app.States.Count == applStates.Count());
        }
    }

    [Test]
    public void InitializeApplicationStates_reInitStatesOnExistingApplication_activeStateShouldBeSet()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var applStates = ctx.GetApplicationStates();

            var app = new Entities.Application();
            app.Initialize(applStates);
            app.States.Remove(app.States.Last());
            app.States.Remove(app.States.First());

            app.Initialize(applStates);
            var selected = app.States.SingleOrDefault(x => x.IsCurrent);
            Assert.That(selected.Name, Is.EqualTo(applStates.OrderBy(x => x.SeqNo).First().Name));
        }
    }

    [Test]
    public void InitializeApplicationStates_passInEmptyApplicationStateList_exceptionShouldBeThrown()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var app = new Entities.Application();
            Assert.Throws(
                Is.TypeOf<ArgumentException>()
                .And.Message.EqualTo("Parameter states must contain a list of all application states. (Parameter 'states')"
            ),
            () => app.Initialize(new List<ApplicationState>()));
        }
    }

    [Test]
    public void InitializeApplicationStates_passedInApplicationStateListContainsDuplicateSeqNos_exceptionShouldBeThrown()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var applStates = ctx.GetApplicationStates().ToList();
            applStates.ForEach(x => x.SeqNo = 1);

            var app = new Entities.Application();
            Assert.Throws(
                Is.TypeOf<ArgumentException>()
                .And.Message.EqualTo("Each application state must contain unique sequence number. (Parameter 'states')"
            ),
            () => app.Initialize(applStates));
        }
    }

    [Test]
    public void InitializeApplicationStates_firstItemIsDeactivedInThePassedInApplicationStateList_deactivatedItemShouldNotBeInitialized()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var applStates = ctx.GetApplicationStates().ToList();
            var deactivated = applStates.OrderBy(x => x.SeqNo).First();
            deactivated.DeactivatedUTC = DateTime.UtcNow;

            var app = new Entities.Application();
            app.Initialize(applStates);

            Assert.That(app.States.Any(x => x.Name == deactivated.Name), Is.False);
        }
    }

    [Test]
    public void InitializeApplicationStates_onlyOneActiveApplicationListState_thereShouldBeAtLeastTwoActiveApplicationStates()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var applStates = ctx.GetApplicationStates().ToList();
            applStates = applStates.Take(2).ToList();
            applStates.ForEach(x => x.DeactivatedUTC = DateTime.UtcNow);

            var app = new Entities.Application();
            Assert.Throws(
                Is.TypeOf<ArgumentException>()
                .And.Message.EqualTo("There must be at least two active application states. (Parameter 'states')"
            ),
            () => app.Initialize(applStates));
        }
    }

    [Test]
    public async Task GetScheduleEmailAlertsToSendOut_scheduledEventWhichIsOutsideOfAlertThreshold_shouldNotBeReturned()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var now = DateTime.UtcNow;
            var threshold = TimeSpan.FromMinutes(30);

            var app = ctx.CreateApplication();
            app.Appointments.Add(new Appointment() {
                Id = Guid.NewGuid(),
                Description = "event one",
                StartDateTimeUTC = now.Add(threshold).AddMinutes(10),
                EndDateTimeUTC = now.Add(threshold).AddMinutes(10).AddHours(4),
                ApplicationStateId = app.GetCurrentState().Id.ToString()
            });

            var alerts = app.GetScheduleEmailAlertsToSendOut(now, threshold);
            Assert.That(alerts.Count(), Is.Zero);
        }
    }

    [Test]
    public async Task GetScheduleEmailAlertsToSendOut_scheduledEventWhichIsInsideOfAlertThreshold_shouldNotBeReturned()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var now = DateTime.UtcNow;
            var threshold = TimeSpan.FromMinutes(30);

            var app = ctx.CreateApplication();
            app.Appointments.Add(new Appointment() {
                Id = Guid.NewGuid(),
                Description = "event one",
                StartDateTimeUTC = now.Add(threshold).AddMinutes(-1),
                EndDateTimeUTC = now.Add(threshold).AddMinutes(10).AddHours(4),
                ApplicationStateId = app.GetCurrentState().Id.ToString()
            });

            var alerts = app.GetScheduleEmailAlertsToSendOut(now, threshold);
            Assert.That(alerts.Count(), Is.EqualTo(1));
        }
    }

    [Test]
    public async Task GetScheduleEmailAlertsToSendOut_scheduledEventWhichIsRightOnBorderOfAlertThreshold_shouldNotBeReturned()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var now = DateTime.UtcNow;
            var threshold = TimeSpan.FromMinutes(30);

            var app = ctx.CreateApplication();
            app.Appointments.Add(new Appointment() {
                Id = Guid.NewGuid(),
                Description = "event one",
                StartDateTimeUTC = now.Add(threshold),
                EndDateTimeUTC = now.Add(threshold).AddMinutes(10).AddHours(4),
                ApplicationStateId = app.GetCurrentState().Id.ToString()
            });

            var alerts = app.GetScheduleEmailAlertsToSendOut(now, threshold);
            Assert.That(alerts.Count(), Is.EqualTo(1));
        }
    }

    [Test]
    public async Task GetScheduleEmailAlertsToSendOut_scheduledEventWhichAlreadyStarted_shouldNotBeReturned()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var now = DateTime.UtcNow;
            var threshold = TimeSpan.FromMinutes(30);

            var app = ctx.CreateApplication();
            app.Appointments.Add(new Appointment() {
                Id = Guid.NewGuid(),
                Description = "event one",
                StartDateTimeUTC = now.Subtract(threshold),
                EndDateTimeUTC = now.Subtract(threshold).AddMinutes(10).AddHours(4),
                ApplicationStateId = app.GetCurrentState().Id.ToString()
            });

            var alerts = app.GetScheduleEmailAlertsToSendOut(now, threshold);
            Assert.That(alerts.Count(), Is.EqualTo(0));
        }
    }

    [Test]
    public async Task GetScheduleEmailAlertsToSendOut_scheduledEventStartingAtThisExactMoment_shouldNotBeReturned()
    {
        using(var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var now = DateTime.UtcNow;
            var threshold = TimeSpan.FromMinutes(30);

            var app = ctx.CreateApplication();
            app.Appointments.Add(new Appointment() {
                Id = Guid.NewGuid(),
                Description = "event one",
                StartDateTimeUTC = now,
                EndDateTimeUTC = now.AddHours(4),
                ApplicationStateId = app.GetCurrentState().Id.ToString()
            });

            var alerts = app.GetScheduleEmailAlertsToSendOut(now, threshold);
            Assert.That(alerts.Count(), Is.EqualTo(0));
        }
    }
}
