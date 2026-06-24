namespace Core;

public interface IBackgroundTaskQueue
{
    void QueuePlatformScrape(Platform platform);
    Task<Platform?> DequeueAsync(CancellationToken cancellationToken);
}