namespace AIRagService.Services.Pdf;

public sealed record PdfPageText(int PageNumber, string Text);

public interface IPdfTextExtractor
{
    IReadOnlyList<PdfPageText> Extract(Stream pdfStream);
}
