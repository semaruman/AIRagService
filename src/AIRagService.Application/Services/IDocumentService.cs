using AIRagService.Application.Common;
using AIRagService.Application.DTOs;
using AIRagService.Domain.Enums;

namespace AIRagService.Application.Services;

public interface IDocumentService
{
    Task<PagedResult<DocumentDto>> GetPagedAsync(
        int page,
        int pageSize,
        DocumentStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<DocumentDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
}
