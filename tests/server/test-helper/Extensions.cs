using Microsoft.Extensions.Hosting;

namespace SilverKinetics.w80.TestHelper;

public static class Extensions
{
    public static User CreateUser(this TestContext cxt, string? email = null)
    {
        return new User()
        {
            Id = ObjectId.GenerateNewId(),
            Role = Role.User,
            Email = email ?? "testuser@silverkinetics.dev",
            Culture = SupportedCultures.DefaultCulture,
            Nickname = "testuser@silverkientics.dev",
            TimeZone = SupportedTimeZones.DefaultTimezone
        };
    }

    public static Domain.Entities.Application CreateApplication(this TestContext ctx)
    {
        var app = new Domain.Entities.Application()
        {
            Id = ObjectId.GenerateNewId(),
            UserId = ctx.GetTestUserID(),
            CompanyName = "Company 1",
            Role = "Senior Java Developer",
            RoleDescription = "Senior Java Developer. Must know Java.",
            CompensationMax = 140000,
            CompensationMin = 120000,
            CompensationType = CompensationType.Salary,
            PositionType = PositionType.Fulltime,
            WorkSetting = WorkSetting.OnSite,
            Industry = "Healthcare",
            HQLocation = "Miami",
            PositionLocation = "Miami",
            TravelRequirements = "No travel is required",
        };

        app.Initialize(_GetApplicationStates());
        return app;
    }

    public static ApplicationUpdateRequestDto CreateApplicationUpdateRequestDto(this TestContext ctx)
    {
        return ApplicationMapper.ToUpdateDTO(CreateApplication(ctx));
    }

    public static InvitationProcessRequestDto CreateInvitationProcessRequestDto(this TestContext ctx, string email, string code, string? password = null)
    {
        if (string.IsNullOrWhiteSpace(password))
            password = ctx.GetTestUserPassword();

        return new InvitationProcessRequestDto() {
            Email = email,
            InvitationCode = code,
            Password = password
        };
    }

    public static RejectionDto CreateRejectionDto(this TestContext ctx)
    {
        var rejection = new RejectionDto();
        rejection.Reason = "Some reason";
        rejection.Method = RejectionMethod.Email;
        return rejection;
    }
    public static Rejection CreateRejection(this TestContext ctx)
    {
        var rejection = new Rejection();
        rejection.Reason = "Some reason";
        rejection.Method = RejectionMethod.Email;
        return rejection;
    }

    public static AppointmentDto CreateAppointmentDto(this TestContext ctx, DateTime startEventDateTime, DateTime endEventDateTime)
    {
        var appointment = new AppointmentDto();
        appointment.Id = Guid.NewGuid().ToString();
        appointment.StartDateTimeUTC = startEventDateTime;
        appointment.EndDateTimeUTC = endEventDateTime;
        appointment.Description = Guid.NewGuid().ToString();
        return appointment;
    }

    public static ContactDto CreateContactDto(this TestContext ctx)
    {
        var contact = new ContactDto();
        contact.SeqNo = 1;
        contact.Type = ContactType.Email;
        contact.ContactName = "name";
        contact.ContactParameter = ctx.GetTestUserEmail();
        contact.Role = ContactRole.HumanResources;
        return contact;
    }

    public static IList<ApplicationState> GetApplicationStates(this TestContext ctx)
    {
        return _GetApplicationStates();
    }

    public async static Task<ComplexResponseDto<UserViewDto>> UpsertNewUserAsync(this TestContext ctx,
        string email, string id = null, bool enableEventEmailNotifications = false)
    {
        ctx.SetUserToAdmin();
        var userId = id ?? ObjectId.GenerateNewId().ObjectIdToStringId();
        var service = ctx.Services.GetRequiredService<IUserApplicationService>();
        var ret =
            await service.UpsertAsync(
                new UserUpsertRequestDto() {
                    EnableEventEmailNotifications = enableEventEmailNotifications,
                    Nickname = email,
                    Email = email,
                    Culture = SupportedCultures.DefaultCulture,
                    TimeZone = SupportedTimeZones.DefaultTimezone,
                    Role = Role.User,
                    Id = userId
                },
                RequestSourceInfo.Empty,
                CancellationToken.None
            );
        ctx.SetUserToUser();
        return ret;
    }

    public async static Task<InvitationProcessResponseDto> LoginUserWithInvitationCodeAsync(this TestContext ctx, string email, string invitationCode)
    {
        var invitationProcessRequest = ctx.CreateInvitationProcessRequestDto(email, invitationCode);
        var authenticationService = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
        return await authenticationService.ProcessInvitationAsync(invitationProcessRequest, CancellationToken.None);
    }

    public async static Task DeactivateUserAsync(this TestContext ctx, ObjectId userId)
    {
        ctx.SetUserToAdmin();
        var userService = ctx.Services.GetRequiredService<IUserApplicationService>();
        await userService.DeactivateAsync(userId, RequestSourceInfo.Empty, CancellationToken.None);
    }

    public static T GetHostedService<T>(this IServiceProvider serviceProvider)
        where T : class, IHostedService
    {
        return
            serviceProvider.GetServices<IHostedService>().OfType<T>().Single();
    }

    private static IList<ApplicationState> _GetApplicationStates()
    {
        return
            new List<ApplicationState>()
            {
                {
                    new ApplicationState()
                    {
                        Id = ObjectId.Parse("661ecad42dc928266934b5d5"),
                        Name = "Applied",
                        HexColor = "007BFF",
                        SeqNo = 0
                    }
                },
                {
                    new ApplicationState()
                    {
                        Id = ObjectId.Parse("661ecb1d2dc928266934b5d6"),
                        Name = "Client Responded",
                        HexColor = "00A3FF",
                        SeqNo = 10
                    }
                },
                {
                    new ApplicationState()
                    {
                        Id = ObjectId.Parse("661ecb412dc928266934b5d7"),
                        Name = "Screening",
                        HexColor = "00D2FF",
                        SeqNo = 20
                    }
                },
                {
                    new ApplicationState()
                    {
                        Id = ObjectId.Parse("661ecb592dc928266934b5d8"),
                        Name = "Assessments",
                        HexColor = "00FFD1",
                        SeqNo = 30
                    }
                },
                {
                    new ApplicationState()
                    {
                        Id = ObjectId.Parse("661ecb902dc928266934b5d9"),
                        Name = "Hiring Manager Screening",
                        HexColor = "00FF85",
                        SeqNo = 40
                    }
                },
                {
                    new ApplicationState()
                    {
                        Id = ObjectId.Parse("661ecba72dc928266934b5da"),
                        Name = "Decision / Job Offer",
                        HexColor = "9EFF00",
                        SeqNo = 50
                    }
                }
            };
    }
}