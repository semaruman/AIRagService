using Pgvector;

namespace AIRagService.Data.Entities;

public class DocumentChunk
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public Vector? Embedding { get; set; }
    public string Source { get; set; } = string.Empty;
    public int? PageNumber { get; set; }
}
