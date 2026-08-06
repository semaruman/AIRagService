using AIRagService.Data;
using AIRagService.Services;
using AIRagService.Services.Chunking;
using AIRagService.Services.Pdf;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 20 * 1024 * 1024;
});

builder.Services.AddScoped<IDocumentChunkService, DocumentChunkService>();
builder.Services.AddSingleton<IPdfTextExtractor, PdfPigTextExtractor>();
builder.Services.AddSingleton<ITextChunker, TextChunker>();
builder.Services.AddScoped<IDocumentIngestionService, DocumentIngestionService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.UseVector()));

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var canConnect = await db.Database.CanConnectAsync();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Database");
    if (canConnect)
        logger.LogInformation("Database connection OK");
    else
        logger.LogError("Database connection FAILED");
}

app.MapControllers();

app.Run();
