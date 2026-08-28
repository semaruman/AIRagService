using AIRagService.Application.DTOs;

namespace AIRagService.Application.Interfaces;

public interface IVectorSearchService
{
    Task<IReadOnlyList<SearchResult>> SearchAsync(
        float[] embedding,
        int topK,
        IReadOnlyList<Guid>? documentIds,
        CancellationToken cancellationToken = default);
}
