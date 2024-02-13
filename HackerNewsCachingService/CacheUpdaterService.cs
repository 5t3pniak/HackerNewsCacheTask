using Microsoft.Extensions.Options;

namespace HackerNewsCachingService;

public class CacheUpdaterService(
    BestStoriesReadModel readModel,
    HackerNewsClient client,
    ILogger<CacheUpdaterService> logger,
    IOptions<CacheServerConfiguration> configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Executing Cache updater");

        var bestStories = await client.GetBestStories(stoppingToken);
        await readModel.Create(bestStories, async (stories) => await client.GetItems(stories, stoppingToken));
        logger.LogInformation("Initialized Read model {count} keys", bestStories.Count);

        while (stoppingToken.IsCancellationRequested == false)
        {
            logger.LogInformation("Polling next round");
            var newBestStories = await client.GetBestStories(stoppingToken);
            logger.LogInformation("New story order fetched");
            await readModel.UpdateOrder(newBestStories,
                async addedStories => await client.GetItems(addedStories, stoppingToken));
            logger.LogInformation("Order updated");
            await Task.Delay(TimeSpan.FromSeconds(configuration.Value.PollingInterval), stoppingToken);
        }

        logger.LogInformation("Stopping Cache updater");
    }
}