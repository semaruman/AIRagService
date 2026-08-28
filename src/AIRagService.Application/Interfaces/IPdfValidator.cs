namespace AIRagService.Application.Interfaces;

public interface IPdfValidator
{
    bool IsPdf(Stream stream);
}
