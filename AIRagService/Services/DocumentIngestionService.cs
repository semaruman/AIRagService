using AIRagService.Data.Entities;
using AIRagService.Services.Chunking;
using AIRagService.Services.Pdf;

namespace AIRagService.Services;

public class DocumentIngestionService(
    IPdfTextExtractor pdfTextExtractor,
    ITextChunker textChunker,
    IDocumentChunkService documentChunkService) : IDocumentIngestionService
{
    public async Task<DocumentIngestionResult> IngestPdfAsync(
        Stream pdfStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var source = string.IsNullOrWhiteSpace(fileName) ? "uploaded.pdf" : Path.GetFileName(fileName);
        var pages = pdfTextExtractor.Extract(pdfStream);

        var chunks = new List<DocumentChunk>();
        foreach (var page in pages)
        {
            foreach (var content in textChunker.Chunk(page.Text))
            {
                chunks.Add(new DocumentChunk
                {
                    Id = Guid.NewGuid(),
                    Content = content,
                    Embedding = null,
                    Source = source,
                    PageNumber = page.PageNumber
                });
            }
        }

        if (chunks.Count == 0)
            throw new InvalidOperationException("PDF contains no extractable text.");

        await documentChunkService.AddRangeAsync(chunks, cancellationToken);
        return new DocumentIngestionResult(source, chunks.Count);
    }
}
