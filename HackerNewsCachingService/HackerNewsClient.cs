using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using FireSharp;
using FireSharp.Config;
using Microsoft.Extensions.Options;

namespace HackerNewsCachingService;

public class HackerNewsClient
{
    private readonly FirebaseClient _firebaseClient;
    private readonly CacheServerConfiguration _configuration;

    public HackerNewsClient(
        IOptions<CacheServerConfiguration> configuration)
    {
        _configuration = configuration.Value;
        _firebaseClient = new FirebaseClient(new FirebaseConfig() { BasePath = _configuration.SourceUrl });
    }

    private async Task<BestStory?> GetItem(int id, CancellationToken token)
    {
        var res = await _firebaseClient.GetAsync($"item/{id}").WaitAsync(token);
        if (res.StatusCode == HttpStatusCode.OK)
        {
            var p = JsonSerializer.Deserialize<Dictionary<string, object>>(res.Body);
            return new BestStory(
                p.GetValueOrDefault("title", string.Empty).ToString() ?? string.Empty,
                p.GetValueOrDefault("url", string.Empty).ToString() ?? string.Empty,
                p.GetValueOrDefault("by", string.Empty).ToString() ?? string.Empty,
                DateTime.UnixEpoch.AddSeconds(long.Parse(p.GetValueOrDefault("time", 0).ToString() ?? "0")),
                int.Parse(p.GetValueOrDefault("score", 0).ToString() ?? "0"),
                int.Parse(p.GetValueOrDefault("descendants", 0).ToString() ?? "0")
            );
        }

        return null;
    }

    public async Task<List<int>> GetBestStories(CancellationToken token)
    {
        var res = await _firebaseClient.GetAsync("beststories").WaitAsync(token);

        if (res.StatusCode == HttpStatusCode.OK)
        {
            var payload = JsonSerializer.Deserialize<List<int>>(res.Body);
            if (payload != null)
            {
                return payload;
            }
        }

        return [];
    }

    public async Task<List<(int, BestStory)>> GetItems(IReadOnlyList<int> ids, CancellationToken token)
    {
        var bag = new ConcurrentBag<(int, BestStory)>();
        await Parallel.ForEachAsync(ids, new ParallelOptions()
        {
            MaxDegreeOfParallelism = _configuration.MaxParallelism,
            CancellationToken = token
        }, async (id, t) =>
        {
            var item = await GetItem(id, t);
            if (item != null)
            {
                bag.Add((id, item));
            }
        });
        return bag.ToList();
    }
}