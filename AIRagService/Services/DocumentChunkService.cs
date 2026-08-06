using AIRagService.Data;
using AIRagService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIRagService.Services;

public class DocumentChunkService(AppDbContext db) : IDocumentChunkService
{
    public async Task<IReadOnlyList<DocumentChunk>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await db.DocumentChunks
            .AsNoTracking()
            .OrderBy(c => c.Source)
            .ThenBy(c => c.PageNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<DocumentChunk?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await db.DocumentChunks
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken = default)
    {
        await db.DocumentChunks.AddRangeAsync(chunks, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }
}
