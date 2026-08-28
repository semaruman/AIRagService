using AIRagService.Application.Configuration;
using AIRagService.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AIRagService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RagOptions>(configuration.GetSection(RagOptions.SectionName));
        services.Configure<UploadOptions>(configuration.GetSection(UploadOptions.SectionName));
        services.Configure<ApiKeyAuthOptions>(configuration.GetSection(ApiKeyAuthOptions.SectionName));

        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IDocumentIngestionService, DocumentIngestionService>();
        services.AddScoped<IRagQueryService, RagQueryService>();
        services.AddScoped<IDocumentIndexingService, DocumentIndexingService>();

        return services;
    }
}
