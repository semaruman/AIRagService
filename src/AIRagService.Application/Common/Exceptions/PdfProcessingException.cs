namespace AIRagService.Application.Common.Exceptions;

public class PdfProcessingException : Exception
{
    public PdfProcessingException(string message)
        : base(message)
    {
    }

    public PdfProcessingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
