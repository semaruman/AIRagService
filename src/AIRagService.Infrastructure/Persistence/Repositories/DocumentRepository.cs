using AIRagService.Domain.Entities;
using AIRagService.Domain.Enums;
using AIRagService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AIRagService.Infrastructure.Persistence.Repositories;

public class DocumentRepository(AppDbContext context) : IDocumentRepository
{
    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Documents
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<Document?> GetByContentHashAsync(string contentHash, CancellationToken cancellationToken = default)
    {
        return await context.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.ContentHash == contentHash, cancellationToken);
    }

    public async Task<(IReadOnlyList<Document> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        DocumentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Documents.AsNoTracking();

        if (status.HasValue)
            query = query.Where(d => d.Status == status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(d => d.UploadedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        await context.Documents.AddAsync(document, cancellationToken);
        return document;
    }

    public void Update(Document document)
    {
        context.Documents.Update(document);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await context.Documents
            .Where(d => d.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        return deleted > 0;
    }

    public async Task<IReadOnlyList<DocumentChunk>> GetChunksAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        return await context.DocumentChunks
            .AsNoTracking()
            .Where(c => c.DocumentId == documentId)
            .OrderBy(c => c.ChunkIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentChunk>> GetChunksWithoutEmbeddingsAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        return await context.DocumentChunks
            .Where(c => c.DocumentId == documentId && c.Embedding == null)
            .OrderBy(c => c.ChunkIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task<DocumentChunk> AddChunkAsync(DocumentChunk chunk, CancellationToken cancellationToken = default)
    {
        await context.DocumentChunks.AddAsync(chunk, cancellationToken);
        return chunk;
    }

    public async Task AddChunksAsync(IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken = default)
    {
        await context.DocumentChunks.AddRangeAsync(chunks, cancellationToken);
    }

    public void UpdateChunk(DocumentChunk chunk)
    {
        context.DocumentChunks.Update(chunk);
    }

    public async Task<bool> DeleteChunksAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var deleted = await context.DocumentChunks
            .Where(c => c.DocumentId == documentId)
            .ExecuteDeleteAsync(cancellationToken);

        return deleted > 0;
    }
}
