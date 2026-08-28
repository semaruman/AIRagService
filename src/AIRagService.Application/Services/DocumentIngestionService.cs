using AIRagService.Application.Common.Exceptions;
using AIRagService.Application.Configuration;
using AIRagService.Application.DTOs;
using AIRagService.Application.Interfaces;
using AIRagService.Domain.Entities;
using AIRagService.Domain.Enums;
using AIRagService.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace AIRagService.Application.Services;

public class DocumentIngestionService : IDocumentIngestionService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPdfValidator _pdfValidator;
    private readonly IFileHashService _fileHashService;
    private readonly IPdfTextExtractor _pdfTextExtractor;
    private readonly ITextChunker _textChunker;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly UploadOptions _uploadOptions;

    public DocumentIngestionService(
        IDocumentRepository documentRepository,
        IUnitOfWork unitOfWork,
        IPdfValidator pdfValidator,
        IFileHashService fileHashService,
        IPdfTextExtractor pdfTextExtractor,
        ITextChunker textChunker,
        IBackgroundTaskQueue backgroundTaskQueue,
        IOptions<UploadOptions> uploadOptions)
    {
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
        _pdfValidator = pdfValidator;
        _fileHashService = fileHashService;
        _pdfTextExtractor = pdfTextExtractor;
        _textChunker = textChunker;
        _backgroundTaskQueue = backgroundTaskQueue;
        _uploadOptions = uploadOptions.Value;
    }

    public async Task<DocumentUploadResultDto> IngestAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ValidationException("File name is required.");
        }

        await using var bufferedStream = await BufferStreamAsync(fileStream, cancellationToken);

        if (bufferedStream.Length > _uploadOptions.MaxFileSizeBytes)
        {
            throw new ValidationException(
                $"File size exceeds the maximum allowed size of {_uploadOptions.MaxFileSizeBytes} bytes.");
        }

        if (bufferedStream.Length == 0)
        {
            throw new ValidationException("File is empty.");
        }

        bufferedStream.Position = 0;
        if (!_pdfValidator.IsPdf(bufferedStream))
        {
            throw new ValidationException("File is not a valid PDF.");
        }

        bufferedStream.Position = 0;
        var contentHash = await _fileHashService.ComputeSha256Async(bufferedStream, cancellationToken);

        var existingDocument = await _documentRepository.GetByContentHashAsync(contentHash, cancellationToken);
        if (existingDocument is not null)
        {
            return new DocumentUploadResultDto
            {
                Document = MapToDto(existingDocument),
                AlreadyExists = true
            };
        }

        bufferedStream.Position = 0;
        IReadOnlyList<PdfPageText> pages;
        try
        {
            pages = _pdfTextExtractor.Extract(bufferedStream);
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            throw new PdfProcessingException("Failed to extract text from PDF.", ex);
        }

        var chunkResults = pages
            .SelectMany(page => _textChunker.Chunk(page.Text, page.PageNumber))
            .ToList();

        if (chunkResults.Count == 0)
        {
            throw new PdfProcessingException("No text could be extracted from the PDF.");
        }

        if (chunkResults.Count > _uploadOptions.MaxChunksPerDocument)
        {
            throw new ValidationException(
                $"Document produces {chunkResults.Count} chunks, which exceeds the maximum of {_uploadOptions.MaxChunksPerDocument}.");
        }

        var now = DateTimeOffset.UtcNow;
        var documentId = Guid.NewGuid();
        var sanitizedFileName = Path.GetFileName(fileName);

        var document = new Document
        {
            Id = documentId,
            FileName = sanitizedFileName,
            OriginalFileName = fileName,
            ContentHash = contentHash,
            FileSize = bufferedStream.Length,
            UploadedAt = now,
            UpdatedAt = now,
            Status = DocumentStatus.Pending,
            ChunkCount = chunkResults.Count,
            IndexedChunkCount = 0
        };

        var chunks = chunkResults.Select((chunk, index) => new DocumentChunk
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            ChunkIndex = index,
            Content = chunk.Content,
            CreatedAt = now,
            PageNumber = chunk.PageNumber,
            StartPage = chunk.PageNumber,
            EndPage = chunk.PageNumber,
            CharacterStart = chunk.CharacterStart,
            CharacterEnd = chunk.CharacterEnd
        }).ToList();

        await _documentRepository.AddAsync(document, cancellationToken);
        await _documentRepository.AddChunksAsync(chunks, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _backgroundTaskQueue.QueueIndexingAsync(documentId, cancellationToken);

        return new DocumentUploadResultDto
        {
            Document = MapToDto(document),
            AlreadyExists = false
        };
    }

    private static async Task<MemoryStream> BufferStreamAsync(
        Stream fileStream,
        CancellationToken cancellationToken)
    {
        var buffered = new MemoryStream();
        await fileStream.CopyToAsync(buffered, cancellationToken);
        buffered.Position = 0;
        return buffered;
    }

    private static DocumentDto MapToDto(Document document) => new()
    {
        Id = document.Id,
        FileName = document.FileName,
        OriginalFileName = document.OriginalFileName,
        FileSize = document.FileSize,
        UploadedAt = document.UploadedAt,
        UpdatedAt = document.UpdatedAt,
        Status = document.Status,
        ChunkCount = document.ChunkCount,
        IndexedChunkCount = document.IndexedChunkCount
    };
}
