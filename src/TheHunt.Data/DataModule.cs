using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheHunt.Core;
using TheHunt.Data.Services;

namespace TheHunt.Data;

public class DataModule : Module
{
    public override void Configure(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<CompetitionsQueryService>();
    }
}