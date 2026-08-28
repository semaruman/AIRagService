using AIRagService.Domain.Enums;

namespace AIRagService.Application.DTOs;

public class DocumentDto
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTimeOffset UploadedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DocumentStatus Status { get; set; }

    public int ChunkCount { get; set; }

    public int IndexedChunkCount { get; set; }
}
