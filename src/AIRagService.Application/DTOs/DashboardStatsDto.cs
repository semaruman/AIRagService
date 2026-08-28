namespace AIRagService.Application.DTOs;

public class DashboardStatsDto
{
    public int TotalDocuments { get; set; }

    public int PendingDocuments { get; set; }

    public int ProcessingDocuments { get; set; }

    public int IndexedDocuments { get; set; }

    public int FailedDocuments { get; set; }

    public int TotalChunks { get; set; }

    public int IndexedChunks { get; set; }
}
