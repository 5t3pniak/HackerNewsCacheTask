namespace HackerNewsCachingService;

public class CacheServerConfiguration
{
    public string SourceUrl { get; init; } = null!;
    public int PollingInterval { get; init; }
    public int MaxParallelism { get; init; }
}