namespace AIRagService.Application.DTOs;

public class SourceDto
{
    public Guid DocumentId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public Guid ChunkId { get; set; }

    public int? PageNumber { get; set; }

    public string Content { get; set; } = string.Empty;

    public float Similarity { get; set; }
}
