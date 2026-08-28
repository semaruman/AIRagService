using AIRagService.Application;
using AIRagService.Application.Configuration;
using AIRagService.Application.Interfaces;
using AIRagService.Application.Services;
using AIRagService.Domain.Entities;
using AIRagService.Domain.Enums;
using AIRagService.Infrastructure;
using AIRagService.Infrastructure.Persistence;
using AIRagService.IntegrationTests.Fixtures;
using AIRagService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AIRagService.IntegrationTests.Persistence;

[Collection("Postgres")]
public class DocumentPersistenceTests(PostgresFixture fixture)
{
    [Fact]
    public async Task SaveDocumentAndChunks_WithEmbedding_PersistsToDatabase()
    {
        await using var context = fixture.CreateDbContext();

        var embeddingService = CreateEmbeddingService();
        var content = "Persistence test chunk about vector databases.";
        var embedding = (await embeddingService.GenerateEmbeddingsAsync([content]))[0];

        var documentId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var document = new Document
        {
            Id = documentId,
            FileName = "persistence.pdf",
            OriginalFileName = "persistence.pdf",
            ContentHash = Guid.NewGuid().ToString("N"),
            FileSize = 1024,
            UploadedAt = now,
            UpdatedAt = now,
            Status = DocumentStatus.Indexed,
            ChunkCount = 1,
            IndexedChunkCount = 1
        };

        var chunk = new DocumentChunk
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            ChunkIndex = 0,
            Content = content,
            Embedding = embedding,
            CreatedAt = now,
            PageNumber = 1,
            StartPage = 1,
            EndPage = 1,
            CharacterStart = 0,
            CharacterEnd = content.Length
        };

        context.Documents.Add(document);
        context.DocumentChunks.Add(chunk);
        await context.SaveChangesAsync();

        await using var verifyContext = fixture.CreateDbContext();
        var savedDocument = await verifyContext.Documents.SingleAsync(d => d.Id == documentId);
        var savedChunk = await verifyContext.DocumentChunks.SingleAsync(c => c.DocumentId == documentId);

        Assert.Equal(DocumentStatus.Indexed, savedDocument.Status);
        Assert.NotNull(savedChunk.Embedding);
        Assert.Equal(1536, savedChunk.Embedding!.Length);
        Assert.Equal(content, savedChunk.Content);
    }

    [Fact]
    public async Task VectorSearch_ReturnsMatchingChunk()
    {
        await using var context = fixture.CreateDbContext();
        var searchService = new Infrastructure.VectorSearch.PgVectorSearchService(context);
        var embeddingService = CreateEmbeddingService();

        const string searchableText = "PostgreSQL pgvector enables semantic search over embeddings.";
        var queryEmbedding = (await embeddingService.GenerateEmbeddingsAsync([searchableText]))[0];
        var storedEmbedding = (await embeddingService.GenerateEmbeddingsAsync([searchableText]))[0];

        var documentId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        context.Documents.Add(new Document
        {
            Id = documentId,
            FileName = "search.pdf",
            OriginalFileName = "search.pdf",
            ContentHash = Guid.NewGuid().ToString("N"),
            FileSize = 2048,
            UploadedAt = now,
            UpdatedAt = now,
            Status = DocumentStatus.Indexed,
            ChunkCount = 1,
            IndexedChunkCount = 1
        });

        context.DocumentChunks.Add(new DocumentChunk
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            ChunkIndex = 0,
            Content = searchableText,
            Embedding = storedEmbedding,
            CreatedAt = now,
            PageNumber = 1
        });

        await context.SaveChangesAsync();

        var results = await searchService.SearchAsync(queryEmbedding, topK: 3, documentIds: null);

        Assert.NotEmpty(results);
        Assert.Contains(results, result =>
            result.DocumentId == documentId &&
            result.Content == searchableText &&
            result.Similarity > 0.9f);
    }

    [Fact]
    public async Task IngestAsync_SamePdfTwice_ReturnsExistingDocument()
    {
        using var scope = CreateServiceScope();
        var ingestionService = scope.ServiceProvider.GetRequiredService<IDocumentIngestionService>();

        await using var firstUpload = MinimalPdf.CreateStream("Deduplication test document content.");
        var firstResult = await ingestionService.IngestAsync(firstUpload, "dedup.pdf");

        await using var secondUpload = MinimalPdf.CreateStream("Deduplication test document content.");
        var secondResult = await ingestionService.IngestAsync(secondUpload, "dedup-copy.pdf");

        Assert.False(firstResult.AlreadyExists);
        Assert.True(secondResult.AlreadyExists);
        Assert.Equal(firstResult.Document.Id, secondResult.Document.Id);
    }

    private IServiceScope CreateServiceScope()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = fixture.ConnectionString,
                ["Embedding:Provider"] = "Local",
                ["Embedding:Dimensions"] = "1536",
                ["Rag:ChunkSize"] = "800",
                ["Rag:ChunkOverlap"] = "120",
                ["Upload:MaxFileSizeBytes"] = "20971520",
                ["Upload:MaxChunksPerDocument"] = "5000",
                ["Upload:MaxQuestionLength"] = "2000",
                ["Upload:MaxTopK"] = "20"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication(configuration);
        services.AddInfrastructure(configuration);

        return services.BuildServiceProvider().CreateScope();
    }

    private static IEmbeddingService CreateEmbeddingService()
    {
        return new Infrastructure.Embeddings.LocalHashEmbeddingService(
            Microsoft.Extensions.Options.Options.Create(new EmbeddingOptions
            {
                Provider = "Local",
                Dimensions = 1536
            }));
    }
}
