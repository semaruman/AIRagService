namespace AIRagService.Application.DTOs;

public class DocumentDetailDto : DocumentDto
{
    public string ContentHash { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public IReadOnlyList<ChunkDto> Chunks { get; set; } = [];
}
