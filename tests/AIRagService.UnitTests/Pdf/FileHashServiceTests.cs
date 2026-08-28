using AIRagService.Infrastructure.Pdf;
using System.Text;

namespace AIRagService.UnitTests.Pdf;

public class FileHashServiceTests
{
    private readonly FileHashService _service = new();

    [Fact]
    public async Task ComputeSha256_SameStreamContent_ReturnsSameHash()
    {
        var content = Encoding.UTF8.GetBytes("repeatable content for hashing");
        using var first = new MemoryStream(content);
        using var second = new MemoryStream(content);

        var firstHash = await _service.ComputeSha256Async(first);
        var secondHash = await _service.ComputeSha256Async(second);

        Assert.Equal(firstHash, secondHash);
        Assert.Equal(64, firstHash.Length);
    }

    [Fact]
    public async Task ComputeSha256_DifferentContent_ReturnsDifferentHash()
    {
        using var first = new MemoryStream(Encoding.UTF8.GetBytes("content-a"));
        using var second = new MemoryStream(Encoding.UTF8.GetBytes("content-b"));

        var firstHash = await _service.ComputeSha256Async(first);
        var secondHash = await _service.ComputeSha256Async(second);

        Assert.NotEqual(firstHash, secondHash);
    }
}
