using System.Text;

namespace AIRagService.Tests.Fixtures;

public static class MinimalPdf
{
    /// <summary>
    /// Creates a minimal valid PDF with extractable text content.
    /// </summary>
    public static byte[] CreateBytes(string text = "Sample document text for testing.")
    {
        var escaped = EscapePdfString(text);
        var streamContent = $"BT /F1 12 Tf 50 700 Td ({escaped}) Tj ET";
        var streamLength = Encoding.ASCII.GetByteCount(streamContent);

        var pdf = $"""
            %PDF-1.4
            1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj
            2 0 obj<</Type/Pages/Kids[3 0 R]/Count 1>>endobj
            3 0 obj<</Type/Page/MediaBox[0 0 612 792]/Parent 2 0 R/Resources<</Font<</F1 4 0 R>>>>/Contents 5 0 R>>endobj
            4 0 obj<</Type/Font/Subtype/Type1/BaseFont/Helvetica>>endobj
            5 0 obj<</Length {streamLength}>>stream
            {streamContent}
            endstream
            endobj
            xref
            0 6
            0000000000 65535 f
            0000000009 00000 n
            0000000058 00000 n
            0000000115 00000 n
            0000000244 00000 n
            0000000320 00000 n
            trailer<</Size 6/Root 1 0 R>>
            startxref
            413
            %%EOF
            """;

        return Encoding.ASCII.GetBytes(pdf);
    }

    public static MemoryStream CreateStream(string text = "Sample document text for testing.")
    {
        var bytes = CreateBytes(text);
        return new MemoryStream(bytes);
    }

    private static string EscapePdfString(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);
    }
}
