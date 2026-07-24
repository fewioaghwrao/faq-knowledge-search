using FaqKnowledgeSearch.Application.Faqs.Admin;
using FaqKnowledgeSearch.Domain.Faqs;
using Microsoft.EntityFrameworkCore;

namespace FaqKnowledgeSearch.Infrastructure.Persistence.Repositories;

public sealed class AdminFaqRepository(
    AppDbContext dbContext)
    : IAdminFaqRepository
{
    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Tag>> GetTagsAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Tags
            .AsNoTracking()
            .OrderBy(tag => tag.DisplayOrder)
            .ThenBy(tag => tag.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> CategoryExistsAsync(
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Categories.AnyAsync(
            category => category.Id == categoryId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Tag>> GetTagsByIdsAsync(
        IReadOnlyCollection<int> tagIds,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Tags
            .Where(tag => tagIds.Contains(tag.Id))
            .OrderBy(tag => tag.DisplayOrder)
            .ThenBy(tag => tag.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<Faq?> GetByIdForUpdateAsync(
        int faqId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Faqs
            .Include(faq => faq.Tags)
            .SingleOrDefaultAsync(
                faq =>
                    faq.Id == faqId &&
                    faq.DeletedAt == null,
                cancellationToken);
    }

    public async Task AddAsync(
        Faq faq,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Faqs.AddAsync(
            faq,
            cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(
            cancellationToken);
    }
}