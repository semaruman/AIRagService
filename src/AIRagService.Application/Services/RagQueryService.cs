using AIRagService.Application.Common.Exceptions;
using AIRagService.Application.Configuration;
using AIRagService.Application.DTOs;
using AIRagService.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace AIRagService.Application.Services;

public class RagQueryService : IRagQueryService
{
    private const string LlmNotConfiguredMessage =
        "LLM is not configured. Showing relevant excerpts from your documents.";

    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorSearchService _vectorSearchService;
    private readonly ILlmService _llmService;
    private readonly RagOptions _ragOptions;
    private readonly UploadOptions _uploadOptions;

    public RagQueryService(
        IEmbeddingService embeddingService,
        IVectorSearchService vectorSearchService,
        ILlmService llmService,
        IOptions<RagOptions> ragOptions,
        IOptions<UploadOptions> uploadOptions)
    {
        _embeddingService = embeddingService;
        _vectorSearchService = vectorSearchService;
        _llmService = llmService;
        _ragOptions = ragOptions.Value;
        _uploadOptions = uploadOptions.Value;
    }

    public async Task<QueryResponseDto> QueryAsync(
        QueryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Question))
        {
            throw new ValidationException("Question is required.");
        }

        if (request.Question.Length > _uploadOptions.MaxQuestionLength)
        {
            throw new ValidationException(
                $"Question exceeds the maximum length of {_uploadOptions.MaxQuestionLength} characters.");
        }

        var topK = request.TopK ?? _ragOptions.TopK;
        if (topK < 1)
        {
            throw new ValidationException("TopK must be greater than or equal to 1.");
        }

        topK = Math.Min(topK, _uploadOptions.MaxTopK);

        var embeddings = await _embeddingService.GenerateEmbeddingsAsync(
            [request.Question],
            cancellationToken);

        if (embeddings.Count == 0 || embeddings[0].Length == 0)
        {
            throw new ExternalServiceException("Embedding", "Failed to generate embedding for the question.");
        }

        var searchResults = await _vectorSearchService.SearchAsync(
            embeddings[0],
            topK,
            request.DocumentIds,
            cancellationToken);

        var sources = searchResults
            .Select(result => new SourceDto
            {
                DocumentId = result.DocumentId,
                FileName = result.FileName,
                ChunkId = result.ChunkId,
                PageNumber = result.PageNumber,
                Content = result.Content,
                Similarity = result.Similarity
            })
            .ToList();

        var context = searchResults
            .Select(result => new ContextChunk(
                result.DocumentId,
                result.FileName,
                result.Content,
                result.PageNumber))
            .ToList();

        var answer = _llmService.IsConfigured
            ? await _llmService.GenerateAnswerAsync(request.Question, context, cancellationToken)
            : LlmNotConfiguredMessage;

        return new QueryResponseDto
        {
            Answer = answer,
            Sources = sources
        };
    }
}
