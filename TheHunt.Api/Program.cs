using HashidsNet;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHunt.Api.Extensions;
using TheHunt.Api.Middleware;
using TheHunt.Application.Features.Bounty;
using TheHunt.Application.Helpers;
using TheHunt.Domain;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

IdUtils.HashIds = new Hashids("D98D7101-C409-4D21-8B59-1A6362DF57C9", 11);

// Add services to the container.
builder.Services.AddMediatR(typeof(GetCurrentBountyQueryHandler).Assembly);
builder.Services.AddDbContext<AppDbContext>(o => o
    .UseNpgsql("Server=127.0.0.1;Port=5432;Database=TheHunt;User Id=postgres;Password=example;")
    .EnableSensitiveDataLogging());
builder.Services.AddScoped<IRequestContextAccessor, RequestContextAccessor>();

builder.Services.AddDataProtection();
builder.Services.AddControllers();

builder.Services.AddGrpc(e => { e.Interceptors.Add<ExceptionHandlerInterceptor>(); }).AddJsonTranscoding();
builder.Services.AddAppSwagger();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<AuthorizationMiddleware>();

app.MapGrpcMediatorServices();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"); });

app.Run();