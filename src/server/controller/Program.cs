using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;
using SilverKinetics.w80.DI;
using SilverKinetics.w80.Common;
using SilverKinetics.w80.Domain.Contracts;
using SilverKinetics.w80.Application.Security;
using SilverKinetics.w80.Controller.Configuration;

namespace SilverKinetics.w80.Controller;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddEnvironmentVariables(prefix: Constants.EnvironmentVariablePrefix);
        builder.Configuration.AddEnvFile();

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration.GetSection("Jwt:Issuer").Get<string>(),
                    ValidAudience = builder.Configuration.GetSection("Jwt:Issuer").Get<string>(),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetRequiredValue("Jwt:Key")))
                };
            });

        builder.Services
            .AddAuthorization(options => {
                options.AddPolicy(Policies.User, policyOptions => {
                    policyOptions.RequireClaim("Role", Role.User.ToString());
                });
                options.AddPolicy(Policies.AdministratorOnly, policyOptions => {
                    policyOptions.RequireClaim("Role", Role.Administrator.ToString());
                });
                options.AddPolicy(Policies.UserOrAdministrator, policyOptions => {
                    policyOptions.RequireClaim("Role", [Role.Administrator.ToString(), Role.User.ToString()]);
                });
                #pragma warning disable CS8601 // Possible null reference assignment.
                options.DefaultPolicy = options.GetPolicy(Policies.User);
                #pragma warning restore CS8601 // Possible null reference assignment.
            });

        builder.Services.AddCors();
        builder.Services.AddControllersWithViews()
                        .AddJsonOptions(opt=> {
                                opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                        });

        builder.Services.AddSwaggerGen();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHttpLogging(o => { });
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();
        builder.Services.AddHttpContextAccessor(); // this is needed for SecurityContext
        builder.Services.AddScoped<ISecurityContext, RequestSecurityContext>();

        builder.Services.AddDependencies(builder.Configuration);

        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        builder.Services.AddLocalization();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(
            new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger()
        );

        var app = builder.Build();

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });



        if (app.Environment.IsDevelopment())
            app.UseCors(policy =>
                policy.AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials()
                      .WithOrigins(
                            "http://localhost:15001",
                            "http://localhost:15002",
                            "http://localhost:15003",
                            "http://localhost:5077",
                            "http://localhost:3000"));

        // We use reverse proxy to strip https
        //if (app.Environment.IsProduction())
        //    app.UseHttpsRedirection();

        //TODO: This throws a Unable to resolve service for type
        //      'Serilog.Extensions.Hosting.DiagnosticContext' while attempting to activate 'Serilog.AspNetCore.RequestLoggingMiddleware'
        //app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseExceptionHandler();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRequestLocalization((options) => {

            options.DefaultRequestCulture = new RequestCulture(SupportedCultures.DefaultCulture);
            options.SupportedCultures = SupportedCultures.Cultures.Values.ToList();
            options.SupportedUICultures = SupportedCultures.Cultures.Values.ToList();

            options.RequestCultureProviders.Insert(0,
                new CustomRequestCultureProvider(httpContext =>
                {
                    // We are always returning a culture (either from user or default), so that
                    // means that none of the other culture providers will be called (cookie, etc)

                    var culture = httpContext.User?.FindFirst("Culture");
                    if (culture?.Value == null)
                        return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(SupportedCultures.DefaultCulture));

                    return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(culture.Value));
                })
            );
        });

        app.MapControllers();
        app.UseHttpLogging();
        app.Run();
    }
}

