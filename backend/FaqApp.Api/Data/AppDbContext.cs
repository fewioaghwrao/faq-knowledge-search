using FaqApp.Api.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace FaqApp.Api.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Faq> Faqs => Set<Faq>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<AiSearchHistory> AiSearchHistories => Set<AiSearchHistory>();

    public DbSet<AiSearchHistorySource> AiSearchHistorySources => Set<AiSearchHistorySource>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(x => x.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });

        modelBuilder.Entity<Faq>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Body)
                .IsRequired();

            entity.Property(x => x.IsPublished)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.Property(x => x.UpdatedAt)
                .IsRequired();

            entity.HasOne(x => x.Category)
                .WithMany(x => x.Faqs)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Tags)
                .WithMany(x => x.Faqs);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<Category>().HasData(
            new { Id = 1, Name = "ログイン", DisplayOrder = 1 },
            new { Id = 2, Name = "請求", DisplayOrder = 2 },
            new { Id = 3, Name = "エラー対応", DisplayOrder = 3 }
        );

        modelBuilder.Entity<Tag>().HasData(
            new { Id = 1, Name = "初期対応", DisplayOrder = 1 },
            new { Id = 2, Name = "FAQ", DisplayOrder = 2 },
            new { Id = 3, Name = "障害対応", DisplayOrder = 3 }
        );

        modelBuilder.Entity<AiSearchHistory>(entity =>
        {
            entity.Property(x => x.Question)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(x => x.SearchKeywords)
                .HasMaxLength(500);

            entity.Property(x => x.AiAnswer);

            entity.Property(x => x.ErrorMessage);

            entity.Property(x => x.ExecutedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasMany(x => x.Sources)
                .WithOne(x => x.AiSearchHistory)
                .HasForeignKey(x => x.AiSearchHistoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AiSearchHistorySource>(entity =>
        {
            entity.Property(x => x.FaqTitle)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasOne(x => x.Faq)
                .WithMany()
                .HasForeignKey(x => x.FaqId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}