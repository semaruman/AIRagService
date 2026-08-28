using AIRagService.Application.Interfaces;

namespace AIRagService.Infrastructure.Pdf;

public class PdfValidator : IPdfValidator
{
    private static ReadOnlySpan<byte> PdfMagicBytes => "%PDF"u8;

    public bool IsPdf(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanSeek)
            return IsPdfHeader(ReadStreamHeader(stream));

        var originalPosition = stream.Position;
        try
        {
            return IsPdfHeader(ReadStreamHeader(stream));
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }

    private static bool IsPdfHeader(byte[] header)
    {
        if (header.Length < PdfMagicBytes.Length)
            return false;

        return header.AsSpan(0, PdfMagicBytes.Length).SequenceEqual(PdfMagicBytes);
    }

    private static byte[] ReadStreamHeader(Stream stream)
    {
        var header = new byte[4];
        _ = stream.Read(header, 0, header.Length);
        return header;
    }
}
