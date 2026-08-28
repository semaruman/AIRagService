using AIRagService.Application.Configuration;
using AIRagService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AIRagService.Infrastructure.Embeddings;

public class EmbeddingServiceFactory(IServiceProvider serviceProvider, IOptions<EmbeddingOptions> options)
{
    public IEmbeddingService Create()
    {
        return IsLocalProvider(options.Value.Provider)
            ? serviceProvider.GetRequiredService<LocalHashEmbeddingService>()
            : serviceProvider.GetRequiredService<OpenAiEmbeddingService>();
    }

    public static bool IsLocalProvider(string? provider)
    {
        return string.Equals(provider, "Local", StringComparison.OrdinalIgnoreCase);
    }
}
