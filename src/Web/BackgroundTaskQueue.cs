using System.Threading.Channels;
using Core;

namespace Web;

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Platform> _queue = Channel.CreateUnbounded<Platform>();

    public void QueuePlatformScrape(Platform platform)
    {
        _queue.Writer.TryWrite(platform);
    }

    public async Task<Platform?> DequeueAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }
}