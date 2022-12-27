using FluentValidation;
using HashidsNet;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheHunt.Application.Behaviors;
using TheHunt.Application.Helpers;
using TheHunt.Domain;

namespace TheHunt.Application;

public static class ApplicationModule
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        IdUtils.HashIds = new Hashids("D98D7101-C409-4D21-8B59-1A6362DF57C9", 11);

        services.AddDbContext<AppDbContext>(o => o
            .UseNpgsql("Server=127.0.0.1;Port=5432;Database=TheHunt;User Id=postgres;Password=example;Include Error Detail=true")
            .EnableSensitiveDataLogging());

        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddMediatR(typeof(ValidationBehavior<,>).Assembly);

        services.AddValidatorsFromAssembly(typeof(ValidationBehavior<,>).Assembly);

        services.AddScoped<IRequestContextAccessor, RequestContextAccessor>();

        return services;
    }
}