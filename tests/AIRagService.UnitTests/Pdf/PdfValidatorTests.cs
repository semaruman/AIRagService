using AIRagService.Infrastructure.Pdf;
using AIRagService.Tests.Fixtures;

namespace AIRagService.UnitTests.Pdf;

public class PdfValidatorTests
{
    private readonly PdfValidator _validator = new();

    [Fact]
    public void IsPdf_ValidHeader_ReturnsTrue()
    {
        using var stream = MinimalPdf.CreateStream();

        var result = _validator.IsPdf(stream);

        Assert.True(result);
        Assert.Equal(0, stream.Position);
    }

    [Fact]
    public void IsPdf_InvalidFile_ReturnsFalse()
    {
        using var stream = new MemoryStream("not-a-pdf-file"u8.ToArray());

        var result = _validator.IsPdf(stream);

        Assert.False(result);
    }

    [Fact]
    public void IsPdf_NonSeekableStream_ValidHeader_ReturnsTrue()
    {
        var bytes = MinimalPdf.CreateBytes();
        using var stream = new NonSeekableMemoryStream(bytes);

        var result = _validator.IsPdf(stream);

        Assert.True(result);
    }

    private sealed class NonSeekableMemoryStream(byte[] buffer) : MemoryStream(buffer)
    {
        public override bool CanSeek => false;
    }
}
