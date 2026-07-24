using FaqKnowledgeSearch.Application.Common.Models;
using FaqKnowledgeSearch.Application.Faqs.Admin;
using Microsoft.EntityFrameworkCore;

namespace FaqKnowledgeSearch.Infrastructure.Persistence.Queries;

public sealed class AdminFaqQuery(
    AppDbContext dbContext) : IAdminFaqQuery
{
    public async Task<PagedResult<AdminFaqListItem>> SearchAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = dbContext.Faqs
            .AsNoTracking()
            // IsDeletedは計算プロパティなので、
            // EFクエリではDeletedAtを直接参照する
            .Where(faq => faq.DeletedAt == null);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(faq => faq.UpdatedAt)
            .ThenByDescending(faq => faq.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(faq => new AdminFaqListItem(
                faq.Id,
                faq.Title,
                faq.Body.Length <= 100
                    ? faq.Body
                    : faq.Body.Substring(0, 100) + "…",
                faq.Category.Name,
                faq.IsPublished,
                faq.ViewCount,
                faq.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminFaqListItem>(
            items,
            totalCount,
            pageNumber,
            pageSize);
    }
}