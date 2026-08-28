namespace AIRagService.Application.Interfaces;

public record PdfPageText(int PageNumber, string Text);

public interface IPdfTextExtractor
{
    IReadOnlyList<PdfPageText> Extract(Stream stream);
}
