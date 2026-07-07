using CollectManagement.Application.Behaviors;
using CollectManagement.Application.Handlers;
using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

namespace CollectManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddExceptionHandler<ExceptionLoggingHandler>();
        services.AddExceptionHandler<UnAuthorizedExceptionHandler>();
        services.AddExceptionHandler<ForbiddenExceptionHandler>();
        services.AddExceptionHandler<BadRequestExceptionHandler>();
        services.AddExceptionHandler<CustomValidationExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<BadCredentialExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandling>();
        
        var assembly = typeof(DependencyInjection).Assembly;
        
        //mapping with mapster
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(assembly);
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        services.AddValidatorsFromAssembly(assembly);
        
        //this is for mediatR Part
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly)
                .AddOpenBehavior(typeof(LoggingBehavior<,>))
                .AddOpenBehavior(typeof(ValidationBehavior<,>))
                .AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
        });
        
        return services;
    }
}