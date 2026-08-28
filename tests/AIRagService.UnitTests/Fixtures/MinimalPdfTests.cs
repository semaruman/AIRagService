using AIRagService.Infrastructure.Pdf;
using AIRagService.Tests.Fixtures;

namespace AIRagService.UnitTests.Fixtures;

public class MinimalPdfTests
{
    [Fact]
    public void CreateStream_IsValidPdfWithExtractableText()
    {
        var validator = new PdfValidator();
        var extractor = new PdfPigTextExtractor();

        using var stream = MinimalPdf.CreateStream("Integration fixture text.");

        Assert.True(validator.IsPdf(stream));

        stream.Position = 0;
        var pages = extractor.Extract(stream);

        Assert.NotEmpty(pages);
        Assert.Contains("Integration fixture text", pages[0].Text, StringComparison.OrdinalIgnoreCase);
    }
}
