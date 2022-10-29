using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;

namespace TheHunt.Api.Extensions;

public static class StartupExtensions
{
    public static IServiceCollection AddAppSwagger(this IServiceCollection services)
    {
        services.AddGrpcSwagger();
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Token", new OpenApiSecurityScheme()
            {
                Name = HeaderNames.Authorization,
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = "Token", Type = ReferenceType.SecurityScheme } },
                    Array.Empty<string>()
                },
            });

            c.SwaggerDoc("v1", new OpenApiInfo { Title = "gRPC transcoding", Version = "v1" });
        });

        return services;
    }
}