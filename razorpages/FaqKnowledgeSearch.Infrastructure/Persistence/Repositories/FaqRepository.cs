using FaqKnowledgeSearch.Application.Faqs.Abstractions;
using FaqKnowledgeSearch.Domain.Faqs;
using Microsoft.EntityFrameworkCore;

namespace FaqKnowledgeSearch.Infrastructure.Persistence.Repositories;

public sealed class FaqRepository(
    AppDbContext dbContext)
    : IFaqRepository
{
    public async Task<IReadOnlyList<Faq>> GetPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Faqs
            .AsNoTracking()
            .Include(faq => faq.Category)
            .Include(faq => faq.Tags)
            .Where(faq =>
                faq.IsPublished &&
                faq.DeletedAt == null)
            .ToListAsync(cancellationToken);
    }

    public async Task<Faq?> GetPublishedByIdAsync(
        int faqId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Faqs
            .Include(faq => faq.Category)
            .Include(faq => faq.Tags)
            .SingleOrDefaultAsync(
                faq =>
                    faq.Id == faqId &&
                    faq.IsPublished &&
                    faq.DeletedAt == null,
                cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(
            cancellationToken);
    }
}