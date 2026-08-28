namespace AIRagService.Application.Interfaces;

public interface IFileHashService
{
    Task<string> ComputeSha256Async(Stream stream, CancellationToken cancellationToken = default);
}
