using AIRagService.Data.Entities;

namespace AIRagService.Services;

public interface IDocumentChunkService
{
    Task<IReadOnlyList<DocumentChunk>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DocumentChunk?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
