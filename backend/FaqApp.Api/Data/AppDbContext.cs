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
            new { Id = 1, Name = "請求処理", DisplayOrder = 1 },
            new { Id = 2, Name = "CSV取込", DisplayOrder = 2 },
            new { Id = 3, Name = "ログイン障害", DisplayOrder = 3 },
            new { Id = 4, Name = "API障害", DisplayOrder = 4 },
            new { Id = 5, Name = "PDF出力", DisplayOrder = 5 },
            new { Id = 6, Name = "月次締め", DisplayOrder = 6 },
            new { Id = 7, Name = "ユーザー権限", DisplayOrder = 7 },
            new { Id = 8, Name = "メール通知", DisplayOrder = 8 },
            new { Id = 9, Name = "システム設定", DisplayOrder = 9 }
        );
        modelBuilder.Entity<Tag>().HasData(
            new { Id = 1, Name = "CSV", DisplayOrder = 1 },
            new { Id = 2, Name = "エラー対応", DisplayOrder = 2 },
            new { Id = 3, Name = "取込", DisplayOrder = 3 },
            new { Id = 4, Name = "文字コード", DisplayOrder = 4 },
            new { Id = 5, Name = "ログイン", DisplayOrder = 5 },
            new { Id = 6, Name = "認証", DisplayOrder = 6 },
            new { Id = 7, Name = "パスワード", DisplayOrder = 7 },
            new { Id = 8, Name = "権限", DisplayOrder = 8 },
            new { Id = 9, Name = "403", DisplayOrder = 9 },
            new { Id = 10, Name = "PDF", DisplayOrder = 10 },
            new { Id = 11, Name = "API", DisplayOrder = 11 },
            new { Id = 12, Name = "タイムアウト", DisplayOrder = 12 },
            new { Id = 13, Name = "月次", DisplayOrder = 13 },
            new { Id = 14, Name = "請求", DisplayOrder = 14 },
            new { Id = 15, Name = "メール", DisplayOrder = 15 },
            new { Id = 16, Name = "SMTP", DisplayOrder = 16 },
            new { Id = 17, Name = "検索", DisplayOrder = 17 },
            new { Id = 18, Name = "公開設定", DisplayOrder = 18 }
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