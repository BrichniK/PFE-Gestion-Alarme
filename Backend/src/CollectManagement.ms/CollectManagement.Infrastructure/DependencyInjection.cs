using System.IO.Abstractions;
using System.Text.Json;
using CollectManagement.Application.Common;
using CollectManagement.Application.Exceptions;
using CollectManagement.Infrastructure.AI;
using CollectManagement.Application.Interfaces.Authentification;
using CollectManagement.Application.Interfaces.Employees;
using CollectManagement.Application.Interfaces.Groupes;
using CollectManagement.Application.Interfaces.Repositories;
using CollectManagement.Application.Interfaces.Repositories.Alertes;
using CollectManagement.Application.Interfaces.Repositories.ConfigurationGenerales;
using CollectManagement.Application.Interfaces.Repositories.Devices;
using CollectManagement.Application.Interfaces.Repositories.JoursFeries;
using CollectManagement.Application.Interfaces.Repositories.Maintenances;
using CollectManagement.Application.Interfaces.Repositories.Plannings;
using CollectManagement.Application.Interfaces.Repositories.SensorMeasurements;
using CollectManagement.Application.Interfaces.Repositories.Shifts;
using CollectManagement.Application.Interfaces.Repositories.SMS;
using CollectManagement.Application.Interfaces.Repositories.SMSConfigurations;
using CollectManagement.Application.Interfaces.Repositories.Types;
using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Application.Interfaces.Societes;
using CollectManagement.Application.Shared;
using CollectManagement.Infrastructure.Authentification;
using CollectManagement.Infrastructure.Common;
using CollectManagement.Infrastructure.Interceptors;
using CollectManagement.Infrastructure.Persistence;
using CollectManagement.Infrastructure.Persistence.Context;
using CollectManagement.Infrastructure.Persistence.Repositories;
using CollectManagement.Infrastructure.Persistence.Repositories.AlerteRepositories;
using CollectManagement.Infrastructure.Persistence.Repositories.ConfigurationGeneraleRepositories;
using CollectManagement.Infrastructure.Persistence.Repositories.DeviceRepositories;
using CollectManagement.Infrastructure.Persistence.Repositories.EmployeeRepositories;
using CollectManagement.Infrastructure.Persistence.Repositories.GroupeRepositories;
using CollectManagement.Infrastructure.Persistence.Repositories.JourFerieRepositories;
using CollectManagement.Infrastructure.Persistence.Repositories.MaintenanceRepositories;
using CollectManagement.Infrastructure.Persistence.Repositories.PlanningRepositories;
using CollectManagement.Infrastructure.Persistence.Repositories.SensorMeasurementRepositories;
using CollectManagement.Infrastructure.Persistence.Repositories.ShiftRepositories;
using CollectManagement.Infrastructure.Persistence.Repositories.SocieteRepositories;
using CollectManagement.Infrastructure.Persistence.Repositories.SMSRepositories;
using CollectManagement.Infrastructure.Persistence.Repositories.SMSConfigurationRepositories;
using CollectManagement.Infrastructure.Persistence.Repositories.TypeRepositories;
using CollectManagement.Infrastructure.Persistence.Repositories.UtilisateurRepositories;
using CollectManagement.Infrastructure.PuppeteerConfig;
using CollectManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CollectManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<ILoggedInUserService, LoggedInUserService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.Configure<PuppeteerOptions>(configuration.GetSection(PuppeteerOptions.SectionName));
        services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.Converters.Add(new NullableUlidJsonConverter());
        });
        services.Configure<GeminiOptions>(
            configuration.GetSection(GeminiOptions.SectionName));

        services.AddScoped<IAiService, GeminiAiService>();

        services.AddScoped<AuditableInterceptor>();
        
        services
            .AddAuth(configuration)
            .AddAuthorization()
            .AddDbContext(configuration)
            .AddPersistance();

        
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IFileSystem, FileSystem>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IPdfGeneratorService, PdfGeneratorService>();
        services.AddScoped<IBrowserProvider, BrowserProvider>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IMaintenanceRfidService, MaintenanceRfidService>();
        services.AddScoped<IMqttPublisher, MqttPublisher>();
        services.AddScoped<ISignalService, SignalService>();
        services.AddHttpContextAccessor();
        services.AddHttpClient<ISMSService, SMSService>(client =>
        {
            // Still keep MQTT responsive, but allow configuration per environment.
            var timeoutSeconds = configuration.GetValue<int?>("SMS:TimeoutSeconds") ?? 30;
            timeoutSeconds = Math.Clamp(timeoutSeconds, 1, 120);
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        });

        services.AddScoped<IEmailService, EmailService>();
        
        
        return services;
    }
    
    public static IServiceCollection AddDbContext(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlServerConnection");

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
            options
                //.UseLazyLoadingProxies()
                .UseSqlServer(
                    connectionString,
                    o =>
                        o.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "F3SManagement")
                            .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .AddInterceptors(sp.GetRequiredService<AuditableInterceptor>())
                .EnableSensitiveDataLogging()
            );
        
        return services;
    }
    
  

    public static IServiceCollection AddPersistance(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUtilisateurRepository, UtilisateurRepository>();
        services.AddScoped<ISocieteRepository, SocieteRepository>();
        services.AddScoped<IRoleUtilisateurRepository, RoleUtilisateurRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IGroupeRepository, GroupeRepository>();
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<IShiftRepository, ShiftRepository>();
        services.AddScoped<ISensorMeasurementRepository, SensorMeasurementRepository>();
        services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
        services.AddScoped<IMaintenanceCaptureHistoryRepository, MaintenanceCaptureHistoryRepository>();
        services.AddScoped<IPlanningRepository, PlanningRepository>();

        services.AddScoped<ITypeRepository, TypeRepository>();
        services.AddScoped<IAlerteRepository, AlerteRepository>();
        services.AddScoped<IJourFerieRepository, JourFerieRepository>();
        services.AddScoped<ISMSRepository, SMSRepository>();
        services.AddScoped<ISMSConfigurationRepository, SMSConfigurationRepository>();
        services.AddScoped<IConfigurationGeneraleRepository, ConfigurationGeneraleRepository>();

        return services;
    }
    
    public static IServiceCollection AddAuth(
        this IServiceCollection services,
        IConfiguration configration)
    {
        var jwtOptions = new JwtOptions();
        configration.Bind(JwtOptions.SectionName, jwtOptions);

        services.AddSingleton(Options.Create(jwtOptions));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.Secret)),

                };
                
                options.Events = new JwtBearerEvents()
                {
                    OnChallenge = context =>
                    {
                        throw new UnAuthorizedException("UnAuthorized User.");
                    },
                    OnForbidden = _ =>
                    {
                        throw new ForbiddenException("Forbidden User.");
                    },
                };
            });

        return services;
    }
    
}
