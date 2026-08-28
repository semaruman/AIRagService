using AIRagService.Domain.Entities;
using AIRagService.Domain.Enums;

namespace AIRagService.Domain.Interfaces;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Document?> GetByContentHashAsync(string contentHash, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Document> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        DocumentStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default);

    void Update(Document document);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocumentChunk>> GetChunksAsync(
        Guid documentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocumentChunk>> GetChunksWithoutEmbeddingsAsync(
        Guid documentId,
        CancellationToken cancellationToken = default);

    Task<DocumentChunk> AddChunkAsync(DocumentChunk chunk, CancellationToken cancellationToken = default);

    Task AddChunksAsync(IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken = default);

    void UpdateChunk(DocumentChunk chunk);

    Task<bool> DeleteChunksAsync(Guid documentId, CancellationToken cancellationToken = default);
}
