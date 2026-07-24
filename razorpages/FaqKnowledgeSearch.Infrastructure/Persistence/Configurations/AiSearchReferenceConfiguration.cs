using FaqKnowledgeSearch.Domain.AiSearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FaqKnowledgeSearch.Infrastructure.Persistence.Configurations;

public sealed class AiSearchReferenceConfiguration
    : IEntityTypeConfiguration<AiSearchReference>
{
    public void Configure(
        EntityTypeBuilder<AiSearchReference> builder)
    {
        builder.ToTable("AiSearchReferences");

        builder.HasKey(reference => reference.Id);

        builder.Property(reference => reference.FaqId)
            .IsRequired();

        builder.Property(reference => reference.FaqTitle)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(reference => reference.CategoryName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(reference => reference.DisplayOrder)
            .IsRequired();

        builder.Property(reference => reference.Score)
            .IsRequired();

        builder.HasIndex(reference =>
            reference.AiSearchHistoryId);

        builder.HasIndex(reference =>
            reference.FaqId);

        builder.HasIndex(reference => new
        {
            reference.AiSearchHistoryId,
            reference.DisplayOrder
        })
        .IsUnique();
    }
}