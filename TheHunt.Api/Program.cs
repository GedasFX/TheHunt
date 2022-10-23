using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using TheHunt.Api.Extensions;
using TheHunt.Api.Middleware;
using TheHunt.Application.Features.Bounty;
using TheHunt.Application.Helpers;
using TheHunt.Domain;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddMediatR(typeof(GetCurrentBountyQueryHandler).Assembly);
builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql("Server=127.0.0.1;Port=5432;Database=TheHunt;User Id=postgres;Password=example;"));
builder.Services.AddScoped<IRequestContextAccessor, RequestContextAccessor>();

builder.Services.AddGrpc().AddJsonTranscoding();
builder.Services.AddGrpcSwagger();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Basic", new OpenApiSecurityScheme()
    {
        Name = HeaderNames.Authorization,
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Basic",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme() { Reference = new OpenApiReference() { Id = "Basic", Type = ReferenceType.SecurityScheme } },
            Array.Empty<string>()
        },
    });

    c.SwaggerDoc("v1",
        new OpenApiInfo { Title = "gRPC transcoding", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// app.MapGrpcService<GreeterService>();
app.MapGet("/", () =>
    "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.UseMiddleware<AuthorizationMiddleware>();

app.MapGrpcMediatorServices();

app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"); });

app.Run();