namespace TheHunt.Bot.Internal;

public class CdnHttpClient
{
    private readonly HttpClient _client;

    public CdnHttpClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<Stream> GetImageStream(string uri)
    {
        var resp = await _client.GetAsync(uri);
        var stream = await resp.Content.ReadAsStreamAsync();

        return stream;
    }
}