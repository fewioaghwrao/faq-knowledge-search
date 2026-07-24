using FaqKnowledgeSearch.Domain.Faqs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FaqKnowledgeSearch.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration
    : IEntityTypeConfiguration<Category>
{
    public void Configure(
        EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.HasIndex(x => x.DisplayOrder);
    }
}