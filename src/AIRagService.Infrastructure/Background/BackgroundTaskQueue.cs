using System.Threading.Channels;
using AIRagService.Application.Interfaces;

namespace AIRagService.Infrastructure.Background;

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Guid> _queue = Channel.CreateUnbounded<Guid>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public ValueTask QueueIndexingAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return _queue.Writer.WriteAsync(documentId, cancellationToken);
    }

    public ValueTask<Guid> DequeueAsync(CancellationToken cancellationToken = default)
    {
        return _queue.Reader.ReadAsync(cancellationToken);
    }
}
