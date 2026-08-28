using AIRagService.Application.DTOs;

namespace AIRagService.Application.Services;

public interface IDocumentIngestionService
{
    Task<DocumentUploadResultDto> IngestAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default);
}
