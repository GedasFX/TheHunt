using System.Text.Json;
using Discord;
using NReJSON;
using StackExchange.Redis;
using TheHunt.Core.Exceptions;
using TheHunt.Data.Models;
using TheHunt.Data.Services;
using TheHunt.Sheets.Models;

namespace TheHunt.Sheets.Services;

public class SpreadsheetQueryService
{
    private readonly SpreadsheetService _spreadsheetService;
    private readonly CompetitionsQueryService _competitionsQueryService;
    private readonly IDatabase _cache;

    private static TimeSpan CacheExpiry { get; } = TimeSpan.FromMinutes(1);

    public SpreadsheetQueryService(SpreadsheetService spreadsheetService, CompetitionsQueryService competitionsQueryService, IConnectionMultiplexer cache)
    {
        NReJSONSerializer.SerializerProxy = new SystemTextJsonSerializerProxy();

        _spreadsheetService = spreadsheetService;
        _competitionsQueryService = competitionsQueryService;
        _cache = cache.GetDatabase();
    }

    public async Task<IReadOnlyDictionary<ulong, CompetitionUser>> GetCompetitionMembers(ulong competitionId)
    {
        return (await UseCache($"__{competitionId}_members", "$", async () =>
        {
            var sheetRef = await _competitionsQueryService.GetSpreadsheetRef(competitionId)
                           ?? throw EntityNotFoundException.CompetitionNotFound;
            return (await _spreadsheetService.GetMembers(sheetRef)).ToDictionary(c => c.UserId, c => c);
        }))!;
    }

    public async Task<CompetitionUser?> GetCompetitionMember(ulong competitionId, ulong userId)
    {
        var result = await _cache.JsonGetAsync<CompetitionUser>($"__{competitionId}_members", $"""$["{userId}"]""");

        if (!result.InnerResult.IsNull)
            return result.FirstOrDefault();

        return (await GetCompetitionMembers(competitionId)).TryGetValue(userId, out var val) ? val : null;
    }

    public void InvalidateCache(ulong competitionId, string cache)
    {
        _cache.KeyExpire($"__{competitionId}_{cache}", TimeSpan.Zero, CommandFlags.FireAndForget);
    }

    private async Task<T?> UseCache<T>(string cacheKey, string path, Func<Task<T?>>? fetcher = null)
    {
        var result = await _cache.JsonGetAsync<T>(cacheKey, "$");
        if (result.Any())
            return result.First();

        if (fetcher == null)
            return default;

        var item = await fetcher();

        await _cache.JsonSetAsync(cacheKey, item, path);
        _cache.KeyExpire(cacheKey, CacheExpiry, CommandFlags.FireAndForget);

        return item;
    }


    public sealed class SystemTextJsonSerializerProxy : ISerializerProxy
    {
        public TResult? Deserialize<TResult>(RedisResult serializedValue)
        {
            return !serializedValue.IsNull ? JsonSerializer.Deserialize<TResult>((string)serializedValue!) : default;
        }

        public string Serialize<TObjectType>(TObjectType obj)
        {
            return JsonSerializer.Serialize(obj);
        }
    }
}