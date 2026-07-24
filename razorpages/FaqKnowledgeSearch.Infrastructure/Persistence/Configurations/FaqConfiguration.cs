using FaqKnowledgeSearch.Domain.Faqs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FaqKnowledgeSearch.Infrastructure.Persistence.Configurations;

public sealed class FaqConfiguration
    : IEntityTypeConfiguration<Faq>
{
    public void Configure(
        EntityTypeBuilder<Faq> builder)
    {
        builder.ToTable("Faqs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Body)
            .IsRequired()
            .HasColumnType("longtext");

        builder.Property(x => x.IsPublished)
            .IsRequired();

        builder.Property(x => x.ViewCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Tags)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "FaqTag",
                right => right
                    .HasOne<Tag>()
                    .WithMany()
                    .HasForeignKey("TagId")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<Faq>()
                    .WithMany()
                    .HasForeignKey("FaqId")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("FaqTags");
                    join.HasKey("FaqId", "TagId");

                    join.HasIndex("TagId");
                });

        // 論理削除済みFAQを通常の検索対象から除外
        builder.HasQueryFilter(x => x.DeletedAt == null);

        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.IsPublished);
        builder.HasIndex(x => x.UpdatedAt);
    }
}