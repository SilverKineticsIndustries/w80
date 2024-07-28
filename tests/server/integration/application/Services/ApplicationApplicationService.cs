using SilverKinetics.w80.Application.Exceptions;

namespace SilverKinetics.w80.Application.IntegrationTests.Services;

[TestFixture(TestOf = typeof(Application.Services.ApplicationApplicationService))]
public class ApplicationApplicationService
{
    [Test]
    public async Task GetAsync_queryExistingApplication_applicationShouldBeFound()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            var app = ctx.CreateApplicationUpdateRequestDto();
            var response = await service.UpsertAsync(app);
            var newApplicationId = response.Result?.Id;
            Assume.That(newApplicationId, Is.Not.Null);

            var ret = await service.GetAsync(ObjectId.Parse(newApplicationId));
            Assert.That(ret, Is.Not.Null);
        }
    }

    [Test]
    public async Task GetAsync_queryNonExistingApplication_applicationShouldNotBeFound()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            var app = ctx.CreateApplicationUpdateRequestDto();
            var response = await service.UpsertAsync(app);
            var newApplicationId = response.Result?.Id;
            Assume.That(newApplicationId, Is.Not.Null);

            var ret = await service.GetAsync(ObjectId.GenerateNewId());
            Assert.That(ret, Is.Null);
        }
    }

    [Test]
    public async Task InitializeAsync_creatingNewRecord_statusesShouldBePresentOnNewApplication()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            var applicationStateSet = ctx.Services.GetRequiredService<IMongoCollection<ApplicationState>>();
            var applicationStatuses = await applicationStateSet.AsQueryable().ToListAsync();

            var response = await service.InitializeAsync();
            Assert.That(response.States.Count, Is.EqualTo(applicationStatuses.Count));
        }
    }

    [Test]
    public async Task UpsertAsync_changeCurrentState_newStateShouldBePersisted()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();

            var app = ctx.CreateApplicationUpdateRequestDto();
            var currenState = app.States.Single(x => x.IsCurrent);
            var response = await service.UpsertAsync(app);
            Assume.That(response?.Result?.States.Where(x => x.IsCurrent == true).Count(), Is.EqualTo(1));

            app.States.First(x => x.IsCurrent == true).IsCurrent = false;
            var newState = app.States.First(x => x.Id != currenState.Id);
            newState.IsCurrent = true;

            var ret = (await service.UpsertAsync(app)).Result;
            Assert.That(ret?.States.Where(x => x.IsCurrent == true && x.Id == newState.Id).Count(), Is.EqualTo(1));
        }
    }

    [Test]
    public async Task UpsertAsync_changeCurrentState_sameNumberOfStatesShouldBeReturned()
    {
        // Had a strange bug appear where EF Core was returning duplicates of the states
        // (db data was fine, input to SaveChanges() was fine, just the loading code was
        // getting confused somewhere). Wrote about this in docs/notes.
        // https://jira.mongodb.org/browse/EF-115

        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            var applicationStateSet = ctx.Services.GetRequiredService<IMongoCollection<ApplicationState>>();
            var applicationStates = await applicationStateSet.AsQueryable().ToListAsync();

            var app = ctx.CreateApplicationUpdateRequestDto();
            var response = (await service.UpsertAsync(app)).Result;
            Assume.That(response?.States.Count(), Is.EqualTo(applicationStates.Count));

            var ret = (await service.UpsertAsync(app)).Result;
            Assert.That(ret?.States.Count(), Is.EqualTo(applicationStates.Count));
        }
    }

    [Test]
    public async Task UpsertAsync_removeOneState_stateShouldBeRemoved()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            var applicationStateSet = ctx.Services.GetRequiredService<IMongoCollection<ApplicationState>>();
            var applicationStates = await applicationStateSet.AsQueryable().ToListAsync();

            var app = ctx.CreateApplicationUpdateRequestDto();
            var response = await service.UpsertAsync(app);

            Assume.That(response.Result?.States.Count, Is.EqualTo(applicationStates.Count));
            var stateToRemove = app.States.Where(x => !x.IsCurrent).First();

            app.States = response.Result?.States.Where(x => x.Id != stateToRemove.Id).ToList() ?? [];

            var ret = (await service.UpsertAsync(app)).Result;
            Assert.Multiple(() =>
            {
                Assert.That(ret?.States.Count(), Is.EqualTo(applicationStates.Count - 1));
                Assert.That(ret?.States.Count(x => x.Id == stateToRemove.Id), Is.EqualTo(0));
            });
        }
    }

    [Test]
    public async Task UpsertAsync_addContact_newContactShouldBeAdded()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();

            var app = ctx.CreateApplicationUpdateRequestDto();
            app.Contacts.Add(ctx.CreateContactDto());

            var res = await service.UpsertAsync(app);
            Assert.That(res?.Result?.Contacts.Count, Is.EqualTo(1));
        }
    }

    [Test]
    public async Task UpsertAsync_removeContact_contactShouldBeRemoved()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();

            var app = ctx.CreateApplicationUpdateRequestDto();
            app.Contacts.Add(ctx.CreateContactDto());

            var res = await service.UpsertAsync(app);
            Assume.That(res?.Result?.Contacts.Count, Is.EqualTo(1));

            app.Contacts.Clear();

            res = await service.UpsertAsync(app);
            Assert.That(res?.Result?.Contacts.Count, Is.EqualTo(0));
        }
    }

    [Test]
    public async Task UpsertAsync_updateContactParameter_contactShouldBeUpdated()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();

            var app = ctx.CreateApplicationUpdateRequestDto();

            app.Contacts.Add(new ContactDto
            {
                SeqNo = 1,
                Type = ContactType.Email,
                ContactName = "name",
                ContactParameter = ctx.GetTestUserEmail(),
                Role = ContactRole.HumanResources
            });
            app.Contacts.Add(new ContactDto
            {
                SeqNo = 2,
                Type = ContactType.Email,
                ContactName = "John Smith",
                ContactParameter = "john.smith@w80.silverkinetics.dev",
                Role = ContactRole.HumanResources
            });
            app.Contacts.Add(new ContactDto
            {
                SeqNo = 3,
                Type = ContactType.Email,
                ContactName = "Mary Smith",
                ContactParameter = "mary.smith@w80.silverkinetics.dev",
                Role = ContactRole.HumanResources
            });

            var res = await service.UpsertAsync(app);
            Assume.That(res?.Result?.Contacts.Count, Is.EqualTo(3));

            var contactToUpdate = app.Contacts.First(x => x.ContactParameter == "john.smith@w80.silverkinetics.dev");
            contactToUpdate.ContactParameter = "john.smith100@w80.silverkinetics.dev";

            res = await service.UpsertAsync(app);

            Assert.Multiple(() =>
            {
                Assert.That(res?.Result?.Contacts.Count, Is.EqualTo(3));
                Assert.That(res?.Result?.Contacts.Count(x => x.ContactParameter == "john.smith@w80.silverkinetics.dev"), Is.EqualTo(0));
                Assert.That(res?.Result?.Contacts.Count(x => x.ContactParameter == "john.smith100@w80.silverkinetics.dev"), Is.EqualTo(1));
            });
        }
    }

    [Test]
    public async Task UpsertAsync_updateContactName_contactShouldBeUpdated()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();

            var app = ctx.CreateApplicationUpdateRequestDto();
            app.Contacts.Add(new ContactDto
            {
                SeqNo = 1,
                Type = ContactType.Email,
                ContactName = "name",
                ContactParameter = ctx.GetTestUserEmail(),
                Role = ContactRole.HumanResources
            });
            app.Contacts.Add(new ContactDto
            {
                SeqNo = 2,
                Type = ContactType.Email,
                ContactName = "John Smith",
                ContactParameter = "john.smith@w80.silverkinetics.dev",
                Role = ContactRole.HumanResources
            });
            app.Contacts.Add(new ContactDto
            {
                SeqNo = 3,
                Type = ContactType.Email,
                ContactName = "Mary Smith",
                ContactParameter = "mary.smith@w80.silverkinetics.dev",
                Role = ContactRole.HumanResources
            });

            var res = await service.UpsertAsync(app);
            Assume.That(res?.Result?.Contacts.Count, Is.EqualTo(3));

            var updatedName = "John Smith Jr";
            app.Contacts.First(x => x.ContactParameter == "john.smith@w80.silverkinetics.dev").ContactName = updatedName;
            res = await service.UpsertAsync(app);

            Assert.Multiple(() =>
            {
                Assert.That(res?.Result?.Contacts.Count, Is.EqualTo(3));
                Assert.That(res?.Result?.Contacts.Count(x => x.ContactName == "John Smith"), Is.EqualTo(0));
                Assert.That(res?.Result?.Contacts.Count(x => x.ContactName == updatedName), Is.EqualTo(1));
            });
        }
    }

    [Test]
    public async Task UpsertAsync_addAppointment_appointmentShouldBeAdded()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();

            var now = DateTime.UtcNow;
            var startEventDateTime = now.AddDays(10);
            var endEventDateTime = now.AddDays(10).AddHours(4);

            var app = ctx.CreateApplicationUpdateRequestDto();
            app.Appointments.Add(ctx.CreateAppointmentDto(startEventDateTime, endEventDateTime));

            var res = await service.UpsertAsync(app);
            Assert.Multiple(() =>
            {
                Assert.That(res?.Result?.Appointments.Count, Is.EqualTo(1));
                Assert.That(res?.Result?.Appointments.Count(x =>
                    x.StartDateTimeUTC.ToString() == startEventDateTime.ToString()
                    && x.EndDateTimeUTC.ToString() == endEventDateTime.ToString()), Is.EqualTo(1));
            });
        }
    }

    [Test]
    public async Task UpsertAsync_addCalendarEvent_calendarStartDateShouldBePersistedAsUTC()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            var applicationSet = ctx.Services.GetRequiredService<IMongoCollection<Domain.Entities.Application>>();

            var now = DateTime.UtcNow;
            var startEventDateTime = now;
            var endEventDateTime = now.AddHours(8);

            var app = ctx.CreateApplicationUpdateRequestDto();
            app.Appointments.Add(ctx.CreateAppointmentDto(startEventDateTime, endEventDateTime));

            await service.UpsertAsync(app);
            var ret = await applicationSet.AsQueryable().FirstAsync((x) => x.Id.ToString() == app.Id);
            Assert.That(ret.Appointments.First().StartDateTimeUTC.Kind, Is.EqualTo(DateTimeKind.Utc));
        }
    }

    [Test]
    public async Task UpsertAsync_addCalendarEvent_calendarEndDateShouldBePersistedAsUTC()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            var applicationSet = ctx.Services.GetRequiredService<IMongoCollection<Domain.Entities.Application>>();

            var now = DateTime.UtcNow;
            var startEventDateTime = now;
            var endEventDateTime = now.AddHours(8);

            var app = ctx.CreateApplicationUpdateRequestDto();
            app.Appointments.Add(ctx.CreateAppointmentDto(startEventDateTime, endEventDateTime));

            await service.UpsertAsync(app);
            var ret = await applicationSet.AsQueryable().FirstAsync((x) => x.Id.ToString() == app.Id);
            Assert.That(ret.Appointments.First().EndDateTimeUTC.Kind, Is.EqualTo(DateTimeKind.Utc));
        }
    }

    [Test]
    public async Task UpsertAsync_removeCalendarEvent_calendarEventShouldBeRemoved()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();

            var now = DateTime.UtcNow;
            var event1StartDateTime = now.AddDays(10);
            var event1EndDateTime = now.AddDays(10).AddHours(4);
            var event2StartDateTime = now.AddDays(20);
            var event2EndDateTime = now.AddDays(20).AddHours(4);

            var app = ctx.CreateApplicationUpdateRequestDto();
            app.Appointments.Add(ctx.CreateAppointmentDto(event1StartDateTime, event1EndDateTime));
            app.Appointments.Add(ctx.CreateAppointmentDto(event2StartDateTime, event2EndDateTime));

            var res = await service.UpsertAsync(app);

            var toRemove = app.Appointments.Find(x =>
                x.StartDateTimeUTC.ToString() == event1StartDateTime.ToString()
                && x.EndDateTimeUTC.ToString() == event1EndDateTime.ToString());

            if (toRemove == null)
                Assert.Fail();
            else
            {
                app.Appointments.Remove(toRemove);
                res = await service.UpsertAsync(app);

                Assert.Multiple(() =>
                {
                    Assert.That(res?.Result?.Appointments.Count, Is.EqualTo(1));
                    Assert.That(res?.Result?.Appointments.Count(x =>
                        x.StartDateTimeUTC.ToString() == event1StartDateTime.ToString()
                        && x.EndDateTimeUTC.ToString() == event1EndDateTime.ToString()), Is.EqualTo(0));
                    Assert.That(res?.Result?.Appointments.Count(x =>
                        x.StartDateTimeUTC.ToString() == event2StartDateTime.ToString()
                        && x.EndDateTimeUTC.ToString() == event2EndDateTime.ToString()), Is.EqualTo(1));
                });
            }
        }
    }

    [Test]
    public async Task UpsertAsync_updateCalendarEventDescription_calendarEventDescriptionShouldBeUpdated()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();

            var now = DateTime.UtcNow;
            var event1StartDateTime = now.AddDays(10);
            var event1EndDateTime = now.AddDays(10).AddHours(4);
            var event2StartDateTime = now.AddDays(20);
            var event2EndDateTime = now.AddDays(20).AddHours(4);

            var app = ctx.CreateApplicationUpdateRequestDto();
            app.Appointments.Add(ctx.CreateAppointmentDto(event1StartDateTime, event1EndDateTime));
            app.Appointments.Add(ctx.CreateAppointmentDto(event2StartDateTime, event2EndDateTime));

            var res = await service.UpsertAsync(app);

            var toUpdate = app.Appointments.Find(x =>
                x.StartDateTimeUTC.ToString() == event1StartDateTime.ToString()
                && x.EndDateTimeUTC.ToString() == event1EndDateTime.ToString());
            if (toUpdate == null)
                Assert.Fail();
            else {
                toUpdate.Description = "Interview with Bill from Company A";
                res = await service.UpsertAsync(app);

                Assert.Multiple(() =>
                {
                    Assert.That(res?.Result?.Appointments.Count, Is.EqualTo(2));
                    Assert.That(res?.Result?.Appointments.Count(
                        x => x.Description == toUpdate.Description
                            && x.StartDateTimeUTC.ToString() == event1StartDateTime.ToString()
                            && x.EndDateTimeUTC.ToString() == event1EndDateTime.ToString()), Is.EqualTo(1));
                });
            }
        }
    }

    [Test]
    public async Task UpsertAsync_updateCalendarEventDateTime_calendarEventDateTimeShouldBeUpdated()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();

            var now = DateTime.UtcNow;
            var event1StartDateTime = now.AddDays(10);
            var event1EndDateTime = now.AddDays(10).AddHours(4);
            var event2StartDateTime = now.AddDays(20);
            var event2EndDateTime = now.AddDays(20).AddHours(4);
            var event3StartDateTime = now.AddDays(30);
            var event3EndDateTime = now.AddDays(30).AddHours(4);

            var app = ctx.CreateApplicationUpdateRequestDto();
            app.Appointments.Add(ctx.CreateAppointmentDto(event1StartDateTime, event1EndDateTime));
            app.Appointments.Add(ctx.CreateAppointmentDto(event2StartDateTime, event2EndDateTime));
            app.Appointments.Add(ctx.CreateAppointmentDto(event3StartDateTime, event3EndDateTime));

            var res = await service.UpsertAsync(app);

            var toUpdate = app.Appointments.Find(x =>
                x.StartDateTimeUTC.ToString() == event3StartDateTime.ToString()
                && x.EndDateTimeUTC.ToString() == event3EndDateTime.ToString());

            if (toUpdate == null)
                Assert.Fail();
            else {
                var newEvent3EndDateTime = event3EndDateTime.AddHours(4);
                toUpdate.EndDateTimeUTC = newEvent3EndDateTime;
                res = await service.UpsertAsync(app);

                Assert.Multiple(() =>
                {
                    Assert.That(res?.Result?.Appointments.Count, Is.EqualTo(3));
                    Assert.That(res?.Result?.Appointments.Count(
                        x => x.Description == toUpdate.Description
                            && x.StartDateTimeUTC.ToString() == event3StartDateTime.ToString()
                            && x.EndDateTimeUTC.ToString() == newEvent3EndDateTime.ToString()), Is.EqualTo(1));
                });
            }
        }
    }


    [Test]
    public async Task UpsertAsync_saveNewApplication_updatedUTCShouldNotBeUpdated()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            var applicationSet = ctx.Services.GetRequiredService<IMongoCollection<Domain.Entities.Application>>();
            var app = ctx.CreateApplicationUpdateRequestDto();
            var res = await service.UpsertAsync(app);
            var appl = await applicationSet.AsQueryable().Where((x) => x.Id.ToString() == app.Id).FirstAsync();

            Assert.Multiple(() =>
            {
                Assert.That(res?.Result?.UpdatedUTC, Is.Null);
                Assert.That(appl.UpdatedUTC, Is.Null);
            });
        }
    }

    [Test]
    public async Task UpsertAsync_updateExistingApplication_updatedUTCshouldBeUpdated()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            var applicationSet = ctx.Services.GetRequiredService<IMongoCollection<Domain.Entities.Application>>();
            var app = ctx.CreateApplicationUpdateRequestDto();
            var res = await service.UpsertAsync(app);

            var now = DateTime.UtcNow;
            app.CompanyName = "Updated company name";
            res = await service.UpsertAsync(app);
            var appl = await applicationSet.AsQueryable().Where((x) => x.Id.ToString() == app.Id).FirstAsync();

            Assert.Multiple(() =>
            {
                Assert.That(res?.Result?.UpdatedUTC, Is.EqualTo(now).Within(10).Seconds);
                Assert.That(appl?.UpdatedUTC, Is.EqualTo(now).Within(10).Seconds);
            });
        }
    }

    [Test]
    public async Task UpsertAsync_updateDeactivatedApplication_deactivatedApplicationCannotBeUpdated()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            var applicationSet = ctx.Services.GetRequiredService<IMongoCollection<Domain.Entities.Application>>();

            var app = ctx.CreateApplicationUpdateRequestDto();
            var res = await service.UpsertAsync(app);

            res = await service.DeactivateAsync(ObjectId.Parse(res?.Result?.Id));
            var appl = await applicationSet.AsQueryable().Where((x) => x.Id.ToString() == app.Id).FirstAsync();
            Assume.That(appl.DeactivatedUTC, Is.Not.Null);

            app.CompanyName = "Updated company name";
            res = await service.UpsertAsync(app);
            Assert.That(res.Errors.Any(x => x.ClientMessage == "Deactivated application cannot be updated."));
        }
    }

    [Test]
    public async Task UpsertAsync_updateApplication_applicationUpdatedSystemEventShouldBePersisted()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            var applicationSet = ctx.Services.GetRequiredService<IMongoCollection<Domain.Entities.Application>>();
            var systemEventSet = ctx.Services.GetRequiredService<IMongoCollection<SystemEventEntry>>();

            var app = ctx.CreateApplicationUpdateRequestDto();
            var res = await service.UpsertAsync(app);

            app.CompanyName = "Updated company name";
            res = await service.UpsertAsync(app);

            var systemEvent = await systemEventSet.AsQueryable().Where((x) => x.EntityId == ObjectId.Parse(app.Id) && x.EntityName == nameof(Domain.Entities.Application)).FirstAsync();
            Assert.Multiple(() =>
            {
                Assert.That(systemEvent, Is.Not.Null);
            });
        }
    }

    [Test]
    public async Task DeactivateAsync_deactivateApplication_applicationShouldBeDeactivated()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            var applicationSet = ctx.Services.GetRequiredService<IMongoCollection<Domain.Entities.Application>>();

            var now = DateTime.UtcNow;
            var app = ctx.CreateApplicationUpdateRequestDto();
            var res = await service.UpsertAsync(app);

            res = await service.DeactivateAsync(ObjectId.Parse(res?.Result?.Id));

            var appl = await applicationSet.AsQueryable().Where((x) => x.Id.ToString() == app.Id).FirstAsync();
            Assert.That(appl.DeactivatedUTC, Is.EqualTo(now).Within(10).Seconds);
        }
    }

    [Test]
    public async Task DeactivateAsync_deactivateApplication_deactivatedByShouldBeSet()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            var applicationSet = ctx.Services.GetRequiredService<IMongoCollection<Domain.Entities.Application>>();

            var now = DateTime.UtcNow;
            var app = ctx.CreateApplicationUpdateRequestDto();
            var res = await service.UpsertAsync(app);

            res = await service.DeactivateAsync(ObjectId.Parse(res?.Result?.Id));

            var appl = await applicationSet.AsQueryable().Where((x) => x.Id.ToString() == app.Id).FirstAsync();
            Assert.That(appl.DeactivatedBy, Is.EqualTo(ctx.GetCurrentUserId()));
        }
    }

    [Test]
    public async Task DeactivateAsync_deactivateApplication_applicationDeactivatedSystemEventIsPersisted()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            var applicationSet = ctx.Services.GetRequiredService<IMongoCollection<Domain.Entities.Application>>();
            var systemEventSet = ctx.Services.GetRequiredService<IMongoCollection<SystemEventEntry>>();

            var now = DateTime.UtcNow;
            var app = ctx.CreateApplicationUpdateRequestDto();
            var res = await service.UpsertAsync(app);

            res = await service.DeactivateAsync(ObjectId.Parse(res?.Result?.Id));

            var appl = await applicationSet.AsQueryable().Where((x) => x.Id.ToString() == app.Id).FirstAsync();
            Assume.That(appl.DeactivatedUTC, Is.Not.Null);

            var systemEvent = await systemEventSet.AsQueryable().Where((x) => x.EntityId == appl.Id && x.EntityName == nameof(Domain.Entities.Application)).FirstAsync();
            Assert.Multiple(() =>
            {
                Assert.That(systemEvent, Is.Not.Null);
            });
        }
    }

    [Test]
    public async Task ReactivateAsync_reactiveApplication_applicationShouldBeReactivated()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            var applicationSet = ctx.Services.GetRequiredService<IMongoCollection<Domain.Entities.Application>>();

            var now = DateTime.UtcNow;
            var app = ctx.CreateApplicationUpdateRequestDto();
            var res = await service.UpsertAsync(app);

            res = await service.DeactivateAsync(ObjectId.Parse(res?.Result?.Id));
            Assume.That(res?.Result?.DeactivatedUTC, Is.Not.Null);

            await service.ReactivateAsync(ObjectId.Parse(res?.Result?.Id));

            var appl = await applicationSet.AsQueryable().Where((x) => x.Id.ToString() == app.Id).FirstAsync();
            Assert.That(appl.DeactivatedUTC, Is.Null);
        }
    }

    [Test]
    public async Task ReactivateAsync_deactivateApplication_deactivatedByShouldBeSetToEmpty()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            var applicationSet = ctx.Services.GetRequiredService<IMongoCollection<Domain.Entities.Application>>();

            var now = DateTime.UtcNow;
            var app = ctx.CreateApplicationUpdateRequestDto();
            var res = await service.UpsertAsync(app);

            await service.ReactivateAsync(ObjectId.Parse(res?.Result?.Id));

            var appl = await applicationSet.AsQueryable().Where((x) => x.Id.ToString() == app.Id).FirstAsync();
            Assert.That(appl.DeactivatedBy, Is.Null);
        }
    }

    [Test]
    public async Task ReactivateAsync_reactivateApplication_applicationReacivatedSystemEventIsPersisted()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            var applicationSet = ctx.Services.GetRequiredService<IMongoCollection<Domain.Entities.Application>>();
            var systemEventSet = ctx.Services.GetRequiredService<IMongoCollection<SystemEventEntry>>();

            var now = DateTime.UtcNow;
            var app = ctx.CreateApplicationUpdateRequestDto();
            var res = await service.UpsertAsync(app);

            res = await service.DeactivateAsync(ObjectId.Parse(res?.Result?.Id));
            Assume.That(res?.Result?.DeactivatedUTC, Is.Not.Null);

            await service.ReactivateAsync(ObjectId.Parse(res?.Result?.Id));

            var appl = await applicationSet.AsQueryable().Where((x) => x.Id.ToString() == app.Id).FirstAsync();
            Assume.That(appl.DeactivatedUTC, Is.Null);

            var systemEvent = await systemEventSet.AsQueryable().Where((x) => x.EntityId == appl.Id && x.EntityName == nameof(Domain.Entities.Application)).FirstAsync();
            Assert.Multiple(() =>
            {
                Assert.That(systemEvent, Is.Not.Null);
            });
        }
    }

    [Test]
    public async Task ArchiveAsync_archiveApplication_applicationShouldBeArchived()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            var applicationSet = ctx.Services.GetRequiredService<IMongoCollection<Domain.Entities.Application>>();

            var now = DateTime.UtcNow;
            var app = ctx.CreateApplication();
            var res = await service.UpsertAsync(ApplicationMapper.ToUpdateDTO(app));

            res = await service.ArchiveAsync(ObjectId.Parse(res?.Result?.Id));

            var appl = await applicationSet.AsQueryable().Where((x) => x.Id == app.Id).FirstAsync();
            Assert.That(appl.ArchivedUTC, Is.EqualTo(now).Within(10).Seconds);
        }
    }

    [Test]
    public async Task ArchiveAsync_archiveApplication_applicationArchiveSystemEventIsPersisted()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            var applicationSet = ctx.Services.GetRequiredService<IMongoCollection<Domain.Entities.Application>>();
            var systemEventSet = ctx.Services.GetRequiredService<IMongoCollection<SystemEventEntry>>();

            var now = DateTime.UtcNow;
            var app = ctx.CreateApplicationUpdateRequestDto();
            var res = await service.UpsertAsync(app);

            res = await service.ArchiveAsync(ObjectId.Parse(res?.Result?.Id));

            var appl = await applicationSet.AsQueryable().Where((x) => x.Id.ToString() == app.Id).FirstOrDefaultAsync();
            Assume.That(appl, Is.Not.Null);
            Assume.That(appl.ArchivedUTC, Is.Not.Null);

            var systemEvent = await systemEventSet.AsQueryable().Where((x) => x.EntityId == appl.Id && x.EntityName == nameof(Domain.Entities.Application)).FirstOrDefaultAsync();

            Assert.Multiple(() =>
            {
                Assert.That(systemEvent, Is.Not.Null);
            });
        }
    }

    [Test]
    public async Task UnarchiveAsync_unarchiveApplication_applicationShouldBeUnarchived()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            var applicationSet = ctx.Services.GetRequiredService<IMongoCollection<Domain.Entities.Application>>();

            var now = DateTime.UtcNow;
            var app = ctx.CreateApplicationUpdateRequestDto();
            var res = await service.UpsertAsync(app);

            res = await service.ArchiveAsync(ObjectId.Parse(res?.Result?.Id));
            Assume.That(res?.Result?.ArchivedUTC, Is.Not.Null);

            await service.UnarchiveAsync(ObjectId.Parse(res?.Result?.Id));

            var appl = await applicationSet.AsQueryable().Where((x) => x.Id.ToString() == app.Id).FirstAsync();
            Assert.That(appl.ArchivedUTC, Is.Null);
        }
    }

    [Test]
    public async Task UnarchiveAsync_unarchiveApplication_applicationUnarchiveSystemEventIsPersisted()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            var applicationSet = ctx.Services.GetRequiredService<IMongoCollection<Domain.Entities.Application>>();
            var systemEventSet = ctx.Services.GetRequiredService<IMongoCollection<SystemEventEntry>>();

            var now = DateTime.UtcNow;
            var app = ctx.CreateApplicationUpdateRequestDto();
            var res = await service.UpsertAsync(app);

            res = await service.ArchiveAsync(ObjectId.Parse(res?.Result?.Id));
            await service.UnarchiveAsync(ObjectId.Parse(res?.Result?.Id));

            var appl = await applicationSet.AsQueryable().Where((x) => x.Id.ToString() == app.Id).FirstAsync();
            Assume.That(appl.ArchivedUTC, Is.Null);

            var systemEvent = await systemEventSet.AsQueryable().Where((x) => x.EntityId == appl.Id && x.EntityName == nameof(Domain.Entities.Application)).FirstAsync();
            Assert.Multiple(() =>
            {
                Assert.That(systemEvent, Is.Not.Null);
            });
        }
    }

    [Test]
    public async Task RejectAsync_rejectApplication_applicationShouldBeRejected()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            var applicationRepo = ctx.Services.GetRequiredService<IApplicationRepository>();

            var now = DateTime.UtcNow;
            var app = ctx.CreateApplicationUpdateRequestDto();
            var res = await service.UpsertAsync(app);

            res = await service.RejectAsync(ObjectId.Parse(res?.Result?.Id), ctx.CreateRejectionDto());

            var appId = ObjectId.Parse(app.Id);
            var appl = await applicationRepo.FirstOrDefaultAsync((x) => x.Id == appId, CancellationToken.None);
            Assert.That(appl?.Rejection?.RejectedUTC, Is.EqualTo(now).Within(10).Seconds);
        }
    }

    [Test]
    public async Task RejectAsync_rejectApplication_applicationRejectedSystemEventIsPersisted()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            var applicationSet = ctx.Services.GetRequiredService<IMongoCollection<Domain.Entities.Application>>();

            var now = DateTime.UtcNow;
            var app = ctx.CreateApplicationUpdateRequestDto();
            var res = await service.UpsertAsync(app);
            res = await service.RejectAsync(ObjectId.Parse(res?.Result?.Id), ctx.CreateRejectionDto());

            var appl = await applicationSet.AsQueryable().Where((x) => x.Id.ToString() == app.Id).FirstAsync();
            Assume.That(appl.Rejection?.RejectedUTC, Is.Not.Null);

            var systemEventSet = ctx.Services.GetRequiredService<IMongoCollection<SystemEventEntry>>();
            var systemEvent = await systemEventSet.AsQueryable().Where((x) => x.EntityId == appl.Id && x.EntityName == nameof(Domain.Entities.Application)).FirstAsync();
            Assert.Multiple(() =>
            {
                Assert.That(systemEvent, Is.Not.Null);
            });
        }
    }

    [Test]
    public async Task UpsertAsync_createApplicationWithDifferentUserId_authorizationExceptionShouldBeThrown()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();

            var now = DateTime.UtcNow;
            var app = ctx.CreateApplicationUpdateRequestDto();
            app.UserId = ObjectId.GenerateNewId().ToString();
            Assert.ThrowsAsync<AuthorizationException>(async () =>
            {
                await service.UpsertAsync(app);
            });
        }
    }

    [Test]
    public async Task UpsertAsync_updatingApplicationWithDifferentUserId_authorizationExceptionShouldBeThrown()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();

            var now = DateTime.UtcNow;
            var app = ctx.CreateApplicationUpdateRequestDto();
            var res = await service.UpsertAsync(app);

            app.UserId = ObjectId.GenerateNewId().ToString();

            Assert.ThrowsAsync<AuthorizationException>(async () =>
            {
                await service.UpsertAsync(app);
            });
        }
    }

    [Test]
    public async Task UpsertAsync_updatingApplicationByChangingIdToApplicationOfDifferentUser_authorizationExceptionShouldBeThrown()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();

            var now = DateTime.UtcNow;
            var appForCurrentUser = ctx.CreateApplicationUpdateRequestDto();
            var appForDifferentUser = ctx.CreateApplicationUpdateRequestDto();

            var currentUser = await service.UpsertAsync(appForCurrentUser);
            await service.UpsertAsync(appForDifferentUser);

            var applicationSet = ctx.Services.GetRequiredService<IMongoCollection<Domain.Entities.Application>>();
            var different = await applicationSet.AsQueryable().FirstAsync(x => x.Id.ToString() == appForDifferentUser.Id);
            var update = Builders<Domain.Entities.Application>.Update
                        .Set(appl => appl.UserId, ctx.GetAdminUserID());
            applicationSet.UpdateOne((x) => x.Id.ToString() == appForDifferentUser.Id, update, new UpdateOptions());

            appForCurrentUser.Id = different.Id.ToString();

            Assert.ThrowsAsync<AuthorizationException>(async () =>
            {
                await service.UpsertAsync(appForCurrentUser);
            });
        }
    }

    [Test]
    public async Task UpsertAsync_appointmentShiftedWithinThresholdAfterEmailAlertWasSend_EmailAndBrowserNotificationSentShouldBeCleared()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();

            var id = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var threshold = TimeSpan.FromMinutes(30);

            var app = ctx.CreateApplicationUpdateRequestDto();
            app.Appointments.Add(new AppointmentDto()
            {
                StartDateTimeUTC = now.AddMinutes(15),
                EndDateTimeUTC = now.AddHours(4),
                Description = "Event 1",
                ApplicationStateId = app.States.Where(x => x.IsCurrent).First().Id,
                Id = id.ToString(),
                BrowserNotificationSent = true,
                EmailNotificationSent = true,
            });
            await service.UpsertAsync(app);
            var appId = ObjectId.Parse(app.Id);

            // shift existing event forward while still keeping it within threshold
            app.Appointments.First().StartDateTimeUTC = now.AddMinutes(25);
            await service.UpsertAsync(app);

            var updatedApp = await ctx.Services.GetRequiredService<IApplicationRepository>().FirstOrDefaultAsync(x => x.Id == appId, CancellationToken.None);
            var evnt = updatedApp?.Appointments.First();
            Assert.Multiple(() =>
            {
                Assert.That(evnt?.EmailNotificationSent, Is.False);
                Assert.That(evnt?.BrowserNotificationSent, Is.False);
            });
        }
    }

    [Test]
    public async Task UpsertAsync_appointmentShiftedOutsideOfThresholdAfterEmailAlertWasSend_EmailAndBrowserNotificationSentShouldBeResetToFalse()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();

            var id = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var threshold = TimeSpan.FromMinutes(30);

            var app = ctx.CreateApplicationUpdateRequestDto();
            app.Appointments.Add(new AppointmentDto()
            {
                StartDateTimeUTC = now.AddMinutes(15),
                EndDateTimeUTC = now.AddHours(4),
                Description = "Event 1",
                ApplicationStateId = app.States.Where(x => x.IsCurrent).First().Id,
                Id = id.ToString(),
                BrowserNotificationSent = true,
                EmailNotificationSent = true,
            });
            await service.UpsertAsync(app);
            var appId = ObjectId.Parse(app.Id);

            // shift existing event forward outside of threshold
            app.Appointments.First().StartDateTimeUTC = now.AddMinutes(45);
            await service.UpsertAsync(app);

            var updatedApp = await ctx.Services.GetRequiredService<IApplicationRepository>().FirstOrDefaultAsync(x => x.Id == appId, CancellationToken.None);
            var evnt = updatedApp?.Appointments.First();
            Assert.Multiple(() =>
            {
                Assert.That(evnt?.EmailNotificationSent, Is.False);
                Assert.That(evnt?.BrowserNotificationSent, Is.False);
            });
        }
    }

    [Test]
    public async Task UpsertAsync_appointmentShiftedIntoThePastAfterEmailAlertWasSend_EmailAndBrowserNotificationSentShouldBeNotBeReset()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();

            var id = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var threshold = TimeSpan.FromMinutes(30);

            var app = ctx.CreateApplicationUpdateRequestDto();
            app.Appointments.Add(new AppointmentDto()
            {
                StartDateTimeUTC = now.AddMinutes(15),
                EndDateTimeUTC = now.AddHours(4),
                Description = "Event 1",
                ApplicationStateId = app.States.Where(x => x.IsCurrent).First().Id,
                Id = id.ToString(),
                BrowserNotificationSent = true,
                EmailNotificationSent = true,
            });
            await service.UpsertAsync(app);
            var appId = ObjectId.Parse(app.Id);

            // shift existing event into the past
            app.Appointments.First().StartDateTimeUTC = now.AddHours(-4);
            app.Appointments.First().EndDateTimeUTC = now.AddHours(-2);
            await service.UpsertAsync(app);

            var updatedApp = await ctx.Services.GetRequiredService<IApplicationRepository>().FirstOrDefaultAsync(x => x.Id == appId, CancellationToken.None);
            var evnt = updatedApp?.Appointments.First();
            Assert.Multiple(() =>
            {
                Assert.That(evnt?.EmailNotificationSent, Is.True);
                Assert.That(evnt?.BrowserNotificationSent, Is.True);
            });
        }
    }

    [Test]
    public async Task UpsertAsync_updateRejectedApplication_rejectionDataShouldNotBeModified()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var service = ctx.Services.GetRequiredService<IApplicationApplicationService>();

            var id = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var threshold = TimeSpan.FromMinutes(30);

            var app = ctx.CreateApplicationUpdateRequestDto();
            await service.UpsertAsync(app);
            var appId = ObjectId.Parse(app.Id);

            var rejection = ctx.CreateRejectionDto();
            await service.RejectAsync(appId, rejection, CancellationToken.None);

        }
    }
}