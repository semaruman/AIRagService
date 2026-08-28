namespace AIRagService.Application.Services;

public interface IDocumentIndexingService
{
    Task QueueIndexingAsync(Guid documentId, CancellationToken cancellationToken = default);
}
