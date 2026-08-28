using AIRagService.Application.Common.Exceptions;
using AIRagService.Application.Interfaces;
using AIRagService.Domain.Enums;
using AIRagService.Domain.Interfaces;

namespace AIRagService.Application.Services;

public class DocumentIndexingService(
    IDocumentRepository documentRepository,
    IUnitOfWork unitOfWork,
    IBackgroundTaskQueue backgroundTaskQueue) : IDocumentIndexingService
{
    public async Task QueueIndexingAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document is null)
            throw new NotFoundException("Document", documentId);

        document.Status = DocumentStatus.Pending;
        document.ErrorMessage = null;
        document.UpdatedAt = DateTimeOffset.UtcNow;
        documentRepository.Update(document);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await backgroundTaskQueue.QueueIndexingAsync(documentId, cancellationToken);
    }
}
