using AIRagService.Application.Common;
using AIRagService.Application.Common.Exceptions;
using AIRagService.Application.DTOs;
using AIRagService.Domain.Entities;
using AIRagService.Domain.Enums;
using AIRagService.Domain.Interfaces;

namespace AIRagService.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;

    public DocumentService(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<PagedResult<DocumentDto>> GetPagedAsync(
        int page,
        int pageSize,
        DocumentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            throw new ValidationException("Page must be greater than or equal to 1.");
        }

        if (pageSize < 1)
        {
            throw new ValidationException("Page size must be greater than or equal to 1.");
        }

        var (items, totalCount) = await _documentRepository.GetPagedAsync(
            page,
            pageSize,
            status,
            cancellationToken);

        return new PagedResult<DocumentDto>
        {
            Items = items.Select(MapToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<DocumentDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(id, cancellationToken);
        if (document is null)
        {
            throw new NotFoundException(nameof(Document), id);
        }

        var chunks = await _documentRepository.GetChunksAsync(id, cancellationToken);

        return MapToDetailDto(document, chunks);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await _documentRepository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException(nameof(Document), id);
        }
    }

    public async Task<DashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var (_, totalDocuments) = await _documentRepository.GetPagedAsync(
            1,
            1,
            cancellationToken: cancellationToken);

        var (_, pendingDocuments) = await _documentRepository.GetPagedAsync(
            1,
            1,
            DocumentStatus.Pending,
            cancellationToken);

        var (_, processingDocuments) = await _documentRepository.GetPagedAsync(
            1,
            1,
            DocumentStatus.Processing,
            cancellationToken);

        var (_, indexedDocuments) = await _documentRepository.GetPagedAsync(
            1,
            1,
            DocumentStatus.Indexed,
            cancellationToken);

        var (_, failedDocuments) = await _documentRepository.GetPagedAsync(
            1,
            1,
            DocumentStatus.Failed,
            cancellationToken);

        var totalChunks = 0;
        var indexedChunks = 0;
        const int pageSize = 100;
        var page = 1;

        while (true)
        {
            var (items, count) = await _documentRepository.GetPagedAsync(
                page,
                pageSize,
                cancellationToken: cancellationToken);

            if (items.Count == 0)
            {
                break;
            }

            totalChunks += items.Sum(d => d.ChunkCount);
            indexedChunks += items.Sum(d => d.IndexedChunkCount);

            if (page * pageSize >= count)
            {
                break;
            }

            page++;
        }

        return new DashboardStatsDto
        {
            TotalDocuments = totalDocuments,
            PendingDocuments = pendingDocuments,
            ProcessingDocuments = processingDocuments,
            IndexedDocuments = indexedDocuments,
            FailedDocuments = failedDocuments,
            TotalChunks = totalChunks,
            IndexedChunks = indexedChunks
        };
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

    private static DocumentDetailDto MapToDetailDto(
        Document document,
        IReadOnlyList<DocumentChunk> chunks) => new()
    {
        Id = document.Id,
        FileName = document.FileName,
        OriginalFileName = document.OriginalFileName,
        ContentHash = document.ContentHash,
        FileSize = document.FileSize,
        UploadedAt = document.UploadedAt,
        UpdatedAt = document.UpdatedAt,
        Status = document.Status,
        ErrorMessage = document.ErrorMessage,
        ChunkCount = document.ChunkCount,
        IndexedChunkCount = document.IndexedChunkCount,
        Chunks = chunks.Select(MapToChunkDto).ToList()
    };

    private static ChunkDto MapToChunkDto(DocumentChunk chunk) => new()
    {
        Id = chunk.Id,
        DocumentId = chunk.DocumentId,
        ChunkIndex = chunk.ChunkIndex,
        Content = chunk.Content,
        CreatedAt = chunk.CreatedAt,
        PageNumber = chunk.PageNumber,
        SectionTitle = chunk.SectionTitle,
        StartPage = chunk.StartPage,
        EndPage = chunk.EndPage,
        CharacterStart = chunk.CharacterStart,
        CharacterEnd = chunk.CharacterEnd
    };
}
