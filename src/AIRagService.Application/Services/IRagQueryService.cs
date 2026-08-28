using AIRagService.Application.DTOs;

namespace AIRagService.Application.Services;

public interface IRagQueryService
{
    Task<QueryResponseDto> QueryAsync(
        QueryRequestDto request,
        CancellationToken cancellationToken = default);
}
