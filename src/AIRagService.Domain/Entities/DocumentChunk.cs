namespace AIRagService.Domain.Entities;

public class DocumentChunk
{
    public Guid Id { get; set; }

    public Guid DocumentId { get; set; }

    public int ChunkIndex { get; set; }

    public string Content { get; set; } = string.Empty;

    public float[]? Embedding { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public int? PageNumber { get; set; }

    public string? SectionTitle { get; set; }

    public int? StartPage { get; set; }

    public int? EndPage { get; set; }

    public int? CharacterStart { get; set; }

    public int? CharacterEnd { get; set; }

    public Document Document { get; set; } = null!;
}
