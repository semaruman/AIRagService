namespace AIRagService.Application.Common.Exceptions;

public class DuplicateDocumentException : Exception
{
    public DuplicateDocumentException(string contentHash)
        : base($"A document with content hash '{contentHash}' already exists.")
    {
        ContentHash = contentHash;
    }

    public string ContentHash { get; }
}
