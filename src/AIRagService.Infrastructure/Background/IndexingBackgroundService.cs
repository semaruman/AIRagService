using AIRagService.Application.Configuration;
using AIRagService.Application.Interfaces;
using AIRagService.Domain.Enums;
using AIRagService.Domain.Interfaces;
using AIRagService.Infrastructure.Embeddings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIRagService.Infrastructure.Background;

public class IndexingBackgroundService(
    IBackgroundTaskQueue taskQueue,
    IServiceScopeFactory scopeFactory,
    EmbeddingServiceFactory embeddingServiceFactory,
    IOptions<EmbeddingOptions> embeddingOptions,
    ILogger<IndexingBackgroundService> logger) : BackgroundService
{
    private const int MaxAttempts = 3;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var documentId = await taskQueue.DequeueAsync(stoppingToken);

            try
            {
                await IndexDocumentAsync(documentId, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected failure while indexing document {DocumentId}", documentId);
                await MarkDocumentFailedAsync(documentId, ex.Message, stoppingToken);
            }
        }
    }

    private async Task IndexDocumentAsync(Guid documentId, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var documentRepository = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var embeddingService = embeddingServiceFactory.Create();
        var batchSize = Math.Max(1, embeddingOptions.Value.BatchSize);

        var document = await documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document is null)
        {
            logger.LogWarning("Document {DocumentId} was not found for indexing", documentId);
            return;
        }

        document.Status = DocumentStatus.Processing;
        document.ErrorMessage = null;
        document.UpdatedAt = DateTimeOffset.UtcNow;
        documentRepository.Update(document);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var chunks = await documentRepository.GetChunksWithoutEmbeddingsAsync(documentId, cancellationToken);
        if (chunks.Count == 0)
        {
            document.Status = DocumentStatus.Indexed;
            document.IndexedChunkCount = document.ChunkCount;
            document.UpdatedAt = DateTimeOffset.UtcNow;
            documentRepository.Update(document);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var indexedCount = document.IndexedChunkCount;

        for (var offset = 0; offset < chunks.Count; offset += batchSize)
        {
            var batch = chunks.Skip(offset).Take(batchSize).ToList();
            var texts = batch.Select(chunk => chunk.Content).ToArray();
            var embeddings = await EmbedWithRetryAsync(embeddingService, texts, documentId, cancellationToken);

            for (var i = 0; i < batch.Count; i++)
            {
                batch[i].Embedding = embeddings[i];
                documentRepository.UpdateChunk(batch[i]);
            }

            indexedCount += batch.Count;
            document.IndexedChunkCount = indexedCount;
            document.UpdatedAt = DateTimeOffset.UtcNow;
            documentRepository.Update(document);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        document.Status = DocumentStatus.Indexed;
        document.IndexedChunkCount = document.ChunkCount;
        document.UpdatedAt = DateTimeOffset.UtcNow;
        documentRepository.Update(document);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Indexed document {DocumentId} with {ChunkCount} chunks", documentId, document.ChunkCount);
    }

    private async Task<IReadOnlyList<float[]>> EmbedWithRetryAsync(
        IEmbeddingService embeddingService,
        IReadOnlyList<string> texts,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        Exception? lastException = null;

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                return await embeddingService.GenerateEmbeddingsAsync(texts, cancellationToken);
            }
            catch (Exception ex) when (IsTransientEmbeddingFailure(ex) && attempt < MaxAttempts)
            {
                lastException = ex;
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                logger.LogWarning(
                    ex,
                    "Transient embedding failure for document {DocumentId} on attempt {Attempt}/{MaxAttempts}. Retrying in {Delay}.",
                    documentId,
                    attempt,
                    MaxAttempts,
                    delay);

                await Task.Delay(delay, cancellationToken);
            }
        }

        throw lastException ?? new InvalidOperationException("Embedding failed after retries.");
    }

    private async Task MarkDocumentFailedAsync(Guid documentId, string errorMessage, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var documentRepository = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var document = await documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document is null)
            return;

        document.Status = DocumentStatus.Failed;
        document.ErrorMessage = errorMessage;
        document.UpdatedAt = DateTimeOffset.UtcNow;
        documentRepository.Update(document);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static bool IsTransientEmbeddingFailure(Exception exception)
    {
        return exception is HttpRequestException httpRequestException
            && httpRequestException.StatusCode is
                System.Net.HttpStatusCode.TooManyRequests
                or >= System.Net.HttpStatusCode.InternalServerError;
    }
}
