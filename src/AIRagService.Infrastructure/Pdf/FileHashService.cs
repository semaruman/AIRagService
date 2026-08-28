using System.Security.Cryptography;
using AIRagService.Application.Interfaces;

namespace AIRagService.Infrastructure.Pdf;

public class FileHashService : IFileHashService
{
    public async Task<string> ComputeSha256Async(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var sha256 = SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);
        return Convert.ToHexStringLower(hashBytes);
    }
}
