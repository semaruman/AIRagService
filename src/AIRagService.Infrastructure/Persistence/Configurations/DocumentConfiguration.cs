using AIRagService.Domain.Entities;
using AIRagService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIRagService.Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasColumnName("id");

        builder.Property(d => d.FileName)
            .HasColumnName("file_name")
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(d => d.OriginalFileName)
            .HasColumnName("original_file_name")
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(d => d.ContentHash)
            .HasColumnName("content_hash")
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(d => d.FileSize)
            .HasColumnName("file_size");

        builder.Property(d => d.UploadedAt)
            .HasColumnName("uploaded_at");

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(d => d.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(d => d.ErrorMessage)
            .HasColumnName("error_message");

        builder.Property(d => d.ChunkCount)
            .HasColumnName("chunk_count");

        builder.Property(d => d.IndexedChunkCount)
            .HasColumnName("indexed_chunk_count");

        builder.HasIndex(d => d.ContentHash)
            .IsUnique()
            .HasDatabaseName("ix_documents_content_hash");

        builder.HasIndex(d => d.Status)
            .HasDatabaseName("ix_documents_status");

        builder.HasMany(d => d.Chunks)
            .WithOne(c => c.Document)
            .HasForeignKey(c => c.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
