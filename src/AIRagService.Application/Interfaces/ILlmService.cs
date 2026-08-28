using AIRagService.Application.DTOs;

namespace AIRagService.Application.Interfaces;

public interface ILlmService
{
    bool IsConfigured { get; }

    Task<string> GenerateAnswerAsync(
        string question,
        IReadOnlyList<ContextChunk> context,
        CancellationToken cancellationToken = default);
}
