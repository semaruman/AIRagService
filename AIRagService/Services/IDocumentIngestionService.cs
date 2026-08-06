namespace AIRagService.Services;

public sealed record DocumentIngestionResult(string Source, int ChunkCount);

public interface IDocumentIngestionService
{
    Task<DocumentIngestionResult> IngestPdfAsync(
        Stream pdfStream,
        string fileName,
        CancellationToken cancellationToken = default);
}
