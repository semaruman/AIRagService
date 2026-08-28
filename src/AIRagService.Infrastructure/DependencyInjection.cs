using System.Net;
using AIRagService.Application.Configuration;
using AIRagService.Application.Interfaces;
using AIRagService.Domain.Interfaces;
using AIRagService.Infrastructure.Background;
using AIRagService.Infrastructure.Chunking;
using AIRagService.Infrastructure.Embeddings;
using AIRagService.Infrastructure.Llm;
using AIRagService.Infrastructure.Pdf;
using AIRagService.Infrastructure.Persistence;
using AIRagService.Infrastructure.Persistence.Repositories;
using AIRagService.Infrastructure.VectorSearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace AIRagService.Infrastructure;

public static class DependencyInjection
{
    private const string DefaultOpenAiBaseUrl = "https://api.openai.com/v1";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RagOptions>(configuration.GetSection(RagOptions.SectionName));
        services.Configure<EmbeddingOptions>(configuration.GetSection(EmbeddingOptions.SectionName));
        services.Configure<LlmOptions>(configuration.GetSection(LlmOptions.SectionName));

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.UseVector()));

        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddHostedService<IndexingBackgroundService>();

        services.AddSingleton<IPdfTextExtractor, PdfPigTextExtractor>();
        services.AddSingleton<IPdfValidator, PdfValidator>();
        services.AddSingleton<IFileHashService, FileHashService>();
        services.AddSingleton<ITextChunker, TextChunker>();

        services.AddSingleton<EmbeddingServiceFactory>();
        services.AddSingleton<LocalHashEmbeddingService>();
        services.AddScoped<IEmbeddingService>(sp => sp.GetRequiredService<EmbeddingServiceFactory>().Create());

        services.AddHttpClient<OpenAiEmbeddingService>((sp, client) =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<EmbeddingOptions>>().Value;
                client.BaseAddress = new Uri(ResolveBaseUrl(options.BaseUrl));
                if (!string.IsNullOrWhiteSpace(options.ApiKey))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ApiKey);
                }
            })
            .AddPolicyHandler(GetRetryPolicy());

        services.AddHttpClient<OpenAiLlmService>((sp, client) =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<LlmOptions>>().Value;
                client.BaseAddress = new Uri(ResolveBaseUrl(options.BaseUrl));
                if (!string.IsNullOrWhiteSpace(options.ApiKey))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ApiKey);
                }
            });

        services.AddScoped<ILlmService, OpenAiLlmService>();
        services.AddScoped<IVectorSearchService, PgVectorSearchService>();

        return services;
    }

    private static string ResolveBaseUrl(string? baseUrl)
    {
        return string.IsNullOrWhiteSpace(baseUrl)
            ? DefaultOpenAiBaseUrl
            : baseUrl.TrimEnd('/');
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
