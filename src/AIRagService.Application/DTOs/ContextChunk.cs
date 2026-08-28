namespace AIRagService.Application.DTOs;

public record ContextChunk(
    Guid DocumentId,
    string FileName,
    string Content,
    int? PageNumber);
