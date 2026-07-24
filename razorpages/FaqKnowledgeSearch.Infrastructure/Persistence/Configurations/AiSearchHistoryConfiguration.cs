using FaqKnowledgeSearch.Domain.AiSearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FaqKnowledgeSearch.Infrastructure.Persistence.Configurations;

public sealed class AiSearchHistoryConfiguration
    : IEntityTypeConfiguration<AiSearchHistory>
{
    public void Configure(
        EntityTypeBuilder<AiSearchHistory> builder)
    {
        builder.ToTable("AiSearchHistories");

        builder.HasKey(history => history.Id);

        builder.Property(history => history.Question)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(history => history.Answer)
            .HasColumnType("longtext");

        builder.Property(history => history.IsSuccess)
            .IsRequired();

        builder.Property(history => history.ErrorMessage)
            .HasMaxLength(2_000);

        builder.Property(history => history.ModelName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(history => history.UsedExternalAi)
            .IsRequired();

        builder.Property(history => history.WasHelpful);

        builder.Property(history => history.CreatedAt)
            .IsRequired();

        builder.HasMany(history => history.References)
            .WithOne()
            .HasForeignKey(reference =>
                reference.AiSearchHistoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(history => history.References)
            .UsePropertyAccessMode(
                PropertyAccessMode.Field);

        builder.HasIndex(history => history.CreatedAt);

        builder.HasIndex(history => history.IsSuccess);

        builder.HasIndex(history => history.WasHelpful);
    }
}