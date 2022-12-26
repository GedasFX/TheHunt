using Discord;
using Discord.WebSocket;
using FluentValidation;
using HashidsNet;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHunt.Api.Extensions;
using TheHunt.Api.Middleware;
using TheHunt.Application.Behaviors;
// using TheHunt.Application.Features.Bounty;
using TheHunt.Application.Features.Competition;
using TheHunt.Application.Helpers;
using TheHunt.Bot;
using TheHunt.Domain;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

IdUtils.HashIds = new Hashids("D98D7101-C409-4D21-8B59-1A6362DF57C9", 11);

// Add services to the container.
builder.Services.AddValidatorsFromAssembly(typeof(ValidationBehavior<,>).Assembly);
builder.Services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddMediatR(typeof(ValidationBehavior<,>).Assembly);
builder.Services.AddDbContext<AppDbContext>(o => o
    .UseNpgsql("Server=127.0.0.1;Port=5432;Database=TheHunt;User Id=postgres;Password=example;")
    .EnableSensitiveDataLogging());
builder.Services.AddScoped<IRequestContextAccessor, RequestContextAccessor>();

builder.Services.AddDataProtection();
builder.Services.AddControllers();

builder.Services.AddGraphQLServer()
    .AddProjections()
    .RegisterDbContext<AppDbContext>()
    .AddQueryType<Query>();

builder.Services.AddGrpc(e => { e.Interceptors.Add<ExceptionHandlerInterceptor>(); }).AddJsonTranscoding();
builder.Services.AddAppSwagger();

builder.Services.AddDiscord(builder.Configuration["Discord:Token"]!);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<AuthorizationMiddleware>();

app.MapGrpcMediatorServices();
app.MapGraphQL();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"); });

var discord = app.Services.GetRequiredService<DiscordSocketClient>();
discord.Ready += async () =>
{
    var guild = discord.GetGuild(609728856211062785);
    await guild.CreateApplicationCommandAsync(new SlashCommandBuilder()
        .WithName("create")
        .WithDescription("Creates a new competition and binds it to the channel")
        .AddOption("name", ApplicationCommandOptionType.String, "Name of the competition", true)
        .AddOption("description", ApplicationCommandOptionType.String, "Short description", false)
        .AddOption("start_date", ApplicationCommandOptionType.String,
            "Start date for the competition. Example: '2022-12-26 21:30:00' (without quotes).", false)
        .AddOption("end_date", ApplicationCommandOptionType.String,
            "End date for the competition. Example: '2022-12-26 21:30:00' (without quotes).", true)
        .Build());

    await guild.CreateApplicationCommandAsync(new SlashCommandBuilder().WithName("help").WithDescription("Shows help.").Build());
};


app.Run();