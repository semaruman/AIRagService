namespace AIRagService.Application.Interfaces;

public interface IBackgroundTaskQueue
{
    ValueTask QueueIndexingAsync(Guid documentId, CancellationToken cancellationToken = default);

    ValueTask<Guid> DequeueAsync(CancellationToken cancellationToken = default);
}
