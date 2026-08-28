namespace AIRagService.Application.DTOs;

public record SearchResult(
    Guid ChunkId,
    Guid DocumentId,
    string FileName,
    string Content,
    int? PageNumber,
    float Similarity);
