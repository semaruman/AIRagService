namespace AIRagService.Application.DTOs;

public class QueryResponseDto
{
    public string Answer { get; set; } = string.Empty;

    public IReadOnlyList<SourceDto> Sources { get; set; } = [];
}
