using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using SilverKinetics.w80.Common;
using SilverKinetics.w80.Notifications;
using SilverKinetics.w80.Domain.Shared;
using SilverKinetics.w80.Domain.Entities;
using SilverKinetics.w80.Domain.Services;
using SilverKinetics.w80.Domain.Contracts;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Domain.Services.User;
using SilverKinetics.w80.Common.Configuration;
using SilverKinetics.w80.Application.Services;
using SilverKinetics.w80.Application.Contracts;
using SilverKinetics.w80.Notifications.Services;
using SilverKinetics.w80.Notifications.Contracts;
using SilverKinetics.w80.Application.Repositories;
using SilverKinetics.w80.Domain.Services.Application;

namespace SilverKinetics.w80.DI;

public static class Dependencies
{
    private static IMongoClient _mongoClient = null!;
    private static IMongoDatabase _mongoDatabase = null!;

    public static IServiceCollection AddDependencies(
        this IServiceCollection services,
        IConfiguration config,
        string? databaseName = null) // this is only used for tests since we create a db per test
    {
        _mongoClient = new MongoClient(config[Keys.Secrets.DatabaseConnectionString]);
        _mongoDatabase = _mongoClient.GetDatabase(databaseName ?? config[Keys.DatabaseName]);

        return
            services
                .AddSingleton<IMongoClient>((services) => {
                    return _mongoClient;
                })
                .AddSingleton<IMongoDatabase>((services) => {
                    return _mongoDatabase;
                })
                .AddSingleton<IMongoCollection<Domain.Entities.Application>>((services) => {
                    return _mongoDatabase.GetCollection<Domain.Entities.Application>(nameof(Domain.Entities.Application));
                })
                .AddSingleton<IMongoCollection<User>>((services) => {
                    return _mongoDatabase.GetCollection<User>(nameof(User));
                })
                .AddSingleton<IMongoCollection<UserSecurity>>((services) => {
                    return _mongoDatabase.GetCollection<UserSecurity>(nameof(UserSecurity));
                })
                .AddSingleton<IMongoCollection<Industry>>((services) => {
                    return _mongoDatabase.GetCollection<Industry>(nameof(Industry));
                })
                .AddSingleton<IMongoCollection<ApplicationState>>((services) => {
                    return _mongoDatabase.GetCollection<ApplicationState>(nameof(ApplicationState));
                })
                .AddSingleton<IMongoCollection<SystemEventEntry>>((services) => {
                    return _mongoDatabase.GetCollection<SystemEventEntry>(nameof(SystemEventEntry));
                })
                .AddSingleton<IMongoCollection<SystemState>>((services) => {
                    return _mongoDatabase.GetCollection<SystemState>(nameof(SystemState));
                })
                .AddSingleton<IMongoCollection<Statistics>>((services) => {
                    return _mongoDatabase.GetCollection<Statistics>(nameof(Statistics));
                })

                .AddSingleton<IReCaptchaApplicationService, ReCaptchaApplicationService>()
                .AddSingleton<IDateTimeProvider, DateTimeProvider>()
                .AddScoped<IApplicationAlertsService, ApplicationAlertsService>()
                .AddHostedService<StatisticsRefreshBackgroundService>()
                .AddHostedService<ApplicationAlertsEmailBackgroundService>()
                .AddScoped<IEmailSenderService, EmailSenderService>()
                .AddScoped<ITemplateResolver, TemplateResolver>()
                .AddScoped<IEmailMessageGenerator, EmailMessageGenerator>()
                .AddScoped<IUserUpsertService, UserUpsertService>()
                .AddScoped<IEmailNotificationService, EmailNotificationService>()
                .AddScoped<IUserApplicationService, UserApplicationService>()
                .AddScoped<IAuthenticationApplicationService, AuthenticationApplicationService>()
                .AddScoped<IOptionsApplicationService, OptionsApplicationService>()
                .AddScoped<IApplicationUpsertService, ApplicationUpsertService>()
                .AddScoped<IApplicationApplicationService, ApplicationApplicationService>()
                .AddScoped<IApplicationDeactivationService, ApplicationDeactivationService>()
                .AddScoped<IApplicationArchiveService, ApplicationArchiveService>()
                .AddScoped<IApplicationRejectionService, ApplicationRejectionService>()
                .AddScoped<IApplicationAcceptanceService, ApplicationAcceptanceService>()
                .AddScoped<IApplicationRepository, ApplicationRepository>()
                .AddScoped<IUserRepository, UserRepository>()
                .AddScoped<IUserSecurityRepository, UserSecurityRepository>()
                .AddScoped<ISystemEventEntryRepository, SystemEventEntryRepository>()
                .AddScoped<IStatisticsGenerationService, StatisticsGenerationService>()
                .AddScoped<ISystemStateRepository, SystemStateRepository>()
                .AddScoped<IStatisticsGenerationService, StatisticsGenerationService>()
                .AddScoped<IStatisticsApplicationService, StatisticsApplicationService>()
                .AddScoped<IUserDeactivationService, UserDeactivationService>()
                .AddScoped(typeof(IGenericReadOnlyRepository<>), typeof(GenericReadOnlyRepository<>))
                .AddScoped<IStatisticsRepository, StatisticsRepository>()
                .AddScoped<ISystemEventSink, SystemEventSink>();
    }
}

