using System.Text;
using UglyToad.PdfPig;

namespace AIRagService.Services.Pdf;

public class PdfPigTextExtractor : IPdfTextExtractor
{
    public IReadOnlyList<PdfPageText> Extract(Stream pdfStream)
    {
        using var document = PdfDocument.Open(pdfStream);

        var pages = new List<PdfPageText>(document.NumberOfPages);
        foreach (var page in document.GetPages())
        {
            var text = NormalizeWhitespace(page.Text);
            if (string.IsNullOrWhiteSpace(text))
                continue;

            pages.Add(new PdfPageText(page.Number, text));
        }

        return pages;
    }

    private static string NormalizeWhitespace(string text)
    {
        var builder = new StringBuilder(text.Length);
        var previousWasWhitespace = false;

        foreach (var ch in text)
        {
            if (char.IsWhiteSpace(ch))
            {
                if (!previousWasWhitespace)
                {
                    builder.Append(' ');
                    previousWasWhitespace = true;
                }

                continue;
            }

            builder.Append(ch);
            previousWasWhitespace = false;
        }

        return builder.ToString().Trim();
    }
}
