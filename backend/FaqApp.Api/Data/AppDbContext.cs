using FaqApp.Api.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace FaqApp.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Faq> Faqs => Set<Faq>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
    }
}