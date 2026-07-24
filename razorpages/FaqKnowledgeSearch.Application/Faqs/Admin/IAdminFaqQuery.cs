using FaqKnowledgeSearch.Application.Common.Models;

namespace FaqKnowledgeSearch.Application.Faqs.Admin;

public interface IAdminFaqQuery
{
    Task<PagedResult<AdminFaqListItem>> SearchAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
