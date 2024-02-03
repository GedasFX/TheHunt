using Microsoft.Extensions.Caching.Memory;
using TheHunt.Data.Models;
using TheHunt.Sheets.Models;

namespace TheHunt.Sheets.Services;

public class SpreadsheetQueryService(SpreadsheetService spreadsheetService, IMemoryCache cache)
{
    private static TimeSpan CacheExpiration { get; } = TimeSpan.FromMinutes(1);

    #region Members

    public async Task<IReadOnlyDictionary<ulong, CompetitionUser>> GetCompetitionMembers(SheetsRef sheetRef) =>
        (await UseCache($"__{sheetRef.SpreadsheetId}_members",
            async () => (await spreadsheetService.GetMembers(sheetRef)).ToDictionary(c => c.UserId)))!;

    public async Task<CompetitionUser?> GetCompetitionMember(SheetsRef sheetRef, ulong userId)
    {
        return (await GetCompetitionMembers(sheetRef)).TryGetValue(userId, out var val) ? val : null;
    }

    #endregion

    #region Items

    public async Task<IReadOnlyDictionary<string, CompetitionItem>> GetCompetitionItems(SheetsRef sheetRef) =>
        (await UseCache($"__{sheetRef.SpreadsheetId}_items",
            async () => (await spreadsheetService.GetItems(sheetRef)).ToDictionary(c => c.Name)))!;

    public async Task<CompetitionItem?> GetCompetitionItem(SheetsRef sheetRef, string name)
    {
        return (await GetCompetitionItems(sheetRef)).TryGetValue(name, out var val) ? val : null;
    }

    public async Task<bool> VerifyItemExists(SheetsRef sheetRef, string? itemName)
    {
        return itemName != null && (await GetCompetitionItems(sheetRef)).ContainsKey(itemName);
    }

    #endregion


    public void ResetCache(SheetsRef sheetRef, string type)
    {
        cache.Remove($"__{sheetRef.SpreadsheetId}_{type}");
    }

    private async Task<T?> UseCache<T>(string cacheKey, Func<Task<T?>>? fetcher = null) where T : class
    {
        if (cache.TryGetValue(cacheKey, out var result))
            return result as T;

        if (fetcher == null)
            return null;

        result = await fetcher();

        cache.Set(cacheKey, result, CacheExpiration);
        return result as T;
    }
}