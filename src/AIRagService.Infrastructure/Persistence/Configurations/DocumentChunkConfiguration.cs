using AIRagService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pgvector;

namespace AIRagService.Infrastructure.Persistence.Configurations;

public class DocumentChunkConfiguration : IEntityTypeConfiguration<DocumentChunk>
{
    public void Configure(EntityTypeBuilder<DocumentChunk> builder)
    {
        builder.ToTable("document_chunks");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.DocumentId)
            .HasColumnName("document_id");

        builder.Property(c => c.ChunkIndex)
            .HasColumnName("chunk_index");

        builder.Property(c => c.Content)
            .HasColumnName("content")
            .IsRequired();

        builder.Property(c => c.Embedding)
            .HasColumnName("embedding")
            .HasColumnType("vector(1536)")
            .HasConversion(
                v => v == null ? null : new Vector(v),
                v => v == null ? null : v.ToArray());

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(c => c.PageNumber)
            .HasColumnName("page_number");

        builder.Property(c => c.SectionTitle)
            .HasColumnName("section_title")
            .HasMaxLength(512);

        builder.Property(c => c.StartPage)
            .HasColumnName("start_page");

        builder.Property(c => c.EndPage)
            .HasColumnName("end_page");

        builder.Property(c => c.CharacterStart)
            .HasColumnName("character_start");

        builder.Property(c => c.CharacterEnd)
            .HasColumnName("character_end");

        builder.HasIndex(c => new { c.DocumentId, c.ChunkIndex })
            .IsUnique()
            .HasDatabaseName("ix_document_chunks_document_id_chunk_index");

        builder.HasIndex(c => c.DocumentId)
            .HasDatabaseName("ix_document_chunks_document_id");
    }
}
