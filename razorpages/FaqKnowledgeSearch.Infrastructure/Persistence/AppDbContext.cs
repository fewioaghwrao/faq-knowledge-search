using FaqKnowledgeSearch.Domain.AiSearch;
using FaqKnowledgeSearch.Domain.Faqs;
using FaqKnowledgeSearch.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FaqKnowledgeSearch.Infrastructure.Persistence;

public sealed class AppDbContext
    : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Faq> Faqs => Set<Faq>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<AiSearchHistory> AiSearchHistories =>
        Set<AiSearchHistory>();

    public DbSet<AiSearchReference> AiSearchReferences =>
        Set<AiSearchReference>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}