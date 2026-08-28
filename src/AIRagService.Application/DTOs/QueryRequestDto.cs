namespace AIRagService.Application.DTOs;

public class QueryRequestDto
{
    public string Question { get; set; } = string.Empty;

    public int? TopK { get; set; }

    public IReadOnlyList<Guid>? DocumentIds { get; set; }
}
