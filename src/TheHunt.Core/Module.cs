using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TheHunt.Core;

public abstract class Module
{
    public abstract void Configure(IServiceCollection services, IConfiguration configuration);
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddModule<T>(this IServiceCollection services, IConfiguration configuration) where T : Module
    {
        Activator.CreateInstance<T>().Configure(services, configuration);
        return services;
    }
}