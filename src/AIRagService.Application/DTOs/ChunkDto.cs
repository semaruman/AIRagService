namespace AIRagService.Application.DTOs;

public class ChunkDto
{
    public Guid Id { get; set; }

    public Guid DocumentId { get; set; }

    public int ChunkIndex { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public int? PageNumber { get; set; }

    public string? SectionTitle { get; set; }

    public int? StartPage { get; set; }

    public int? EndPage { get; set; }

    public int? CharacterStart { get; set; }

    public int? CharacterEnd { get; set; }
}
