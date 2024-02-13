using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace HackerNewsCachingService;

public class BestStoriesReadModel
{
    private readonly IMemoryCache _storyCache;
    private readonly ConcurrentBag<int> _expired = new ConcurrentBag<int>();
    private IList<int> _orderedBestStories = new List<int>();
    private readonly MemoryCacheEntryOptions _cacheEntryOpt;

    public BestStoriesReadModel(IMemoryCache storyCache)
    {
        _storyCache = storyCache;
        _cacheEntryOpt = new MemoryCacheEntryOptions();
        _cacheEntryOpt
            .SetPriority(CacheItemPriority.NeverRemove)
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(15))
            .RegisterPostEvictionCallback(EvictionCallback);
    }
    
    public async Task Create(List<int> newOrder,
        Func<List<int>, Task<List<(int, BestStory)>>> getInitialState)
    {
        _orderedBestStories = newOrder;
        var stateCollection = await getInitialState(newOrder);
        foreach (var (id,el) in stateCollection)
        {
            _storyCache.Set(id, el, _cacheEntryOpt);
        }
    }

    private void EvictionCallback(object key, object? value, EvictionReason reason, object? state)
    {
        if (reason == EvictionReason.Expired)
        {
            _expired.Add((int)key);
            _storyCache.Set(key, value, _cacheEntryOpt);
        }
    }

    //Method called periodically every 10s
    public async Task UpdateOrder(
        List<int> newOrder,
        Func<IReadOnlyList<int>, Task<List<(int, BestStory)>>> getNewStories)
    {
        /*
         * 1. Find ids of new elements to add to cache
         * 2. Find ids of elements to delete from cache lathpught they want interfere might lead to old values
         * 3. What about updating elements that should stay in cache till next polling round - ideally event based update but not available
         */
        var added = newOrder.Except(_orderedBestStories).ToList();
        var removed = _orderedBestStories.Except(newOrder).ToArray();
        var toUpdate = _expired.Except(removed).ToList();

        var syncStateTasks = new List<Task<List<(int, BestStory)>>>
        {
            added.Count > 0
                ? getNewStories(added)
                : Task.FromResult(new List<(int, BestStory)>()),
            toUpdate.Count > 0
                ? getNewStories(toUpdate)
                : Task.FromResult(new List<(int, BestStory)>())
        };

        var syncState = await Task.WhenAll(syncStateTasks);
        foreach (var (id, el) in syncState[0])
        {
            _storyCache.Set(id, el, _cacheEntryOpt);
        }

        foreach (var (id, el) in syncState[1])
        {
            _storyCache.Set(id, el, _cacheEntryOpt);
        }

        foreach (var r in removed)
        {
            _storyCache.Remove(r);
        }
    }


    public IEnumerable<BestStory> GetBestStories(int n)
    {
        var takeMax = Math.Min(n, _orderedBestStories.Count);
        var bestStoriesSnapshot = _orderedBestStories.ToList();
        for (int i = 0; i < takeMax; i++)
        {
            if (_storyCache.TryGetValue<BestStory>(bestStoriesSnapshot[i], out var value))
                yield return value!;
        }
    }
}