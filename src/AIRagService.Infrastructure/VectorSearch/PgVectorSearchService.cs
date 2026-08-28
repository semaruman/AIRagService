using AIRagService.Application.DTOs;
using AIRagService.Application.Interfaces;
using AIRagService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pgvector;

namespace AIRagService.Infrastructure.VectorSearch;

public class PgVectorSearchService(AppDbContext context) : IVectorSearchService
{
    public async Task<IReadOnlyList<SearchResult>> SearchAsync(
        float[] embedding,
        int topK,
        IReadOnlyList<Guid>? documentIds,
        CancellationToken cancellationToken = default)
    {
        if (embedding.Length == 0)
            return [];

        var limit = Math.Max(1, topK);
        var queryVector = new Vector(embedding);

        var sql = """
            SELECT
                dc.id AS "ChunkId",
                dc.document_id AS "DocumentId",
                d.file_name AS "FileName",
                dc.content AS "Content",
                dc.page_number AS "PageNumber",
                (1 - (dc.embedding <=> @query))::real AS "Similarity"
            FROM document_chunks dc
            INNER JOIN documents d ON d.id = dc.document_id
            WHERE dc.embedding IS NOT NULL
            """;

        var parameters = new List<NpgsqlParameter>
        {
            new("query", queryVector),
            new("limit", limit)
        };

        if (documentIds is { Count: > 0 })
        {
            sql += " AND dc.document_id = ANY(@documentIds)";
            parameters.Add(new NpgsqlParameter("documentIds", documentIds.ToArray()));
        }

        sql += " ORDER BY dc.embedding <=> @query LIMIT @limit";

        var rows = await context.Database
            .SqlQueryRaw<SearchRow>(sql, parameters.ToArray())
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new SearchResult(
                row.ChunkId,
                row.DocumentId,
                row.FileName,
                row.Content,
                row.PageNumber,
                row.Similarity))
            .ToList();
    }

    private sealed class SearchRow
    {
        public Guid ChunkId { get; set; }

        public Guid DocumentId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public int? PageNumber { get; set; }

        public float Similarity { get; set; }
    }
}
